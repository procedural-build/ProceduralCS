using System;
using System.Collections.Generic;
using ComputeCS.types;
using System.IO;
using System.Linq;
using System.Threading;
using Newtonsoft.Json;

namespace ComputeCS.Components
{
    public static class Compute
    {
        public static string Create(
            string inputJson,
            byte[] geometryFile,
            List<Dictionary<string, byte[]>> refinementRegions,
            bool create
        )
        {
            var inputData = new Inputs().FromJson(inputJson);
            var tokens = inputData.Auth;
            var parentTask = inputData.Task;
            var project = inputData.Project;
            var solution = inputData.CFDSolution;

            if (parentTask == null)
            {
                return null;
                //throw new System.Exception("Cannot upload a case without a parent task.");
            }

            if (project == null)
            {
                return null;
                //throw new System.Exception("Cannot upload a case without a project.");
            }

            if (create)
            {
                UploadGeometry(tokens, inputData.Url, parentTask.UID, geometryFile, "foam/constant/triSurface",
                    "cfdGeom", refinementRegions);
            }


            // Tasks to Handle MagPy Celery Actions
            // First Action to create Mesh Files
            var actionTask = new GenericViewSet<Task>(
                tokens,
                inputData.Url,
                $"/api/project/{project.UID}/task/"
            ).GetOrCreate(
                new Dictionary<string, object>
                {
                    {"name", "Actions"},
                    {"parent", parentTask.UID}
                },
                new Dictionary<string, object>
                {
                    {
                        "config", new Dictionary<string, object>
                        {
                            {"task_type", "magpy"},
                            {"cmd", "cfd.io.tasks.write_mesh"},
                            {"base_mesh", inputData.Mesh.BaseMesh},
                            {"snappyhex_mesh", inputData.Mesh.SnappyHexMesh.ToDict()},
                        }
                    }
                },
                create
            );

            // Then Action Task to create CFD files
            if (create)
            {
                if (solution != null && geometryFile.Length > 0)
                {
                    new GenericViewSet<Task>(
                        tokens,
                        inputData.Url,
                        $"/api/project/{project.UID}/task/"
                    ).Update(
                        actionTask.UID,
                        new Dictionary<string, object>
                        {
                            {"name", "Actions"},
                            {"status", "pending"},
                            {
                                "config", new Dictionary<string, object>
                                {
                                    {"task_type", "magpy"},
                                    {"cmd", "cfd.io.tasks.write_solution"},
                                    {"solution", solution}
                                }
                            }
                        }
                    );
                }
                else
                {
                    // TODO - We need to handle if there is no solution given. Then we need to create the controlDict so people can do the meshing.
                }
            }


            // Task to Handle Meshing
            var includeSetSet = inputData.Mesh.BaseMesh.setSetRegions.Count > 0;
            var cpus = solution.CPUs;
            var meshTask = CreateMeshTask(tokens, inputData.Url, project.UID, parentTask.UID, actionTask.UID, cpus,
                includeSetSet, create);

            // Task to Handle CFD
            var createParams = new Dictionary<string, object>();

            if (solution.CaseType.ToLower() == "virtualwindtunnel")
            {
                createParams = new Dictionary<string, object>
                {
                    {
                        "config", new Dictionary<string, object>
                        {
                            {"task_type", "cfd"},
                            {"cmd", "wind_tunnel"},
                            {"commands", solution.Angles},
                            {"cpus", cpus},
                            {"iterations", solution.Iterations},
                            {"solver", solution.Solver}
                        }
                    }
                };
            }
            else if (solution.CaseType.ToLower() == "simplecase")
            {
                createParams = new Dictionary<string, object>
                {
                    {
                        "config", new Dictionary<string, object>
                        {
                            {"task_type", "cfd"},
                            {"cmd", "pipeline"},
                            {
                                "commands", new List<string>
                                {
                                    solution.Solver,
                                    "reconstructPar -noZero"
                                }
                            },
                            {"cpus", cpus},
                            {"iterations", solution.Iterations}
                        }
                    }
                };
            }

            var tasks = new List<Task>
            {
                actionTask,
                meshTask,
            };

            if (createParams.ContainsKey("config"))
            {
                var cfdTask = new GenericViewSet<Task>(
                    tokens,
                    inputData.Url,
                    $"/api/project/{project.UID}/task/"
                ).GetOrCreate(
                    new Dictionary<string, object>
                    {
                        {"name", solution.CaseType},
                        {"parent", parentTask.UID},
                        {"dependent_on", meshTask.UID}
                    },
                    createParams,
                    create
                );
                tasks.Add(cfdTask);
            }

            inputData.SubTasks = tasks;

            return inputData.ToJson();
        }

        /// <summary>
        /// This method is used for creating Radiance cases on Compute
        /// </summary>
        public static string Create(
            string inputJson,
            byte[] geometryFile,
            string dependentOn,
            bool create
        )
        {
            var inputData = new Inputs().FromJson(inputJson);
            var tokens = inputData.Auth;
            var parentTask = inputData.Task;
            var project = inputData.Project;
            var solution = inputData.RadiationSolution;
            var subTasks = inputData.SubTasks;
            var probeTask = GetProbeTask(subTasks, dependentOn);
            const string caseDir = "objects";

            if (parentTask == null)
            {
                return null;
            }

            if (project == null)
            {
                return null;
            }

            if (create)
            {
                UploadGeometry(tokens, inputData.Url, parentTask.UID, geometryFile, "geometry", "radianceGeom");
                switch (solution.Method)
                {
                    case "three_phase":
                        UploadEPWFile(tokens, inputData.Url, parentTask.UID, solution.EPWFile);
                        UploadBSDFFile(tokens, inputData.Url, parentTask.UID, solution.Materials);
                        break;
                    case "solar_radiation":
                    case "mean_radiant_temperature":
                        UploadEPWFile(tokens, inputData.Url, parentTask.UID, solution.EPWFile);
                        break;
                }
            }

            // Create Action Task
            var actionTask = RadianceCompute.CreateRadianceActionTask(tokens, inputData.Url, parentTask, probeTask,
                project, solution, caseDir, create);
            inputData.SubTasks.Add(actionTask);

            if (solution.Method == "mean_radiant_temperature")
            {
                var rayTracingTask = Tasks.CreateParent(tokens, inputData.Url, $"/api/project/{project.UID}/task/",
                    "Ray Tracing",
                    new Dictionary<string, object>{{"parent", parentTask.UID}}, create);

                // Create Sky View Task
                solution.Method = "sky_view_factor";
                var skyViewTask = RadianceCompute.CreateRadianceTask(
                    tokens,
                    inputData.Url,
                    rayTracingTask,
                    actionTask,
                    project,
                    solution,
                    caseDir,
                    create
                );
                inputData.SubTasks.Add(skyViewTask);

                // Create Solar Radiation Task
                solution.Method = "solar_radiation";
                
                var solarRadiationTask = RadianceCompute.CreateRadianceTask(
                    tokens,
                    inputData.Url,
                    rayTracingTask,
                    actionTask,
                    project,
                    solution,
                    caseDir,
                    create
                );
                inputData.SubTasks.Add(solarRadiationTask);
                actionTask = rayTracingTask;
                solution.Method = "mean_radiant_temperature";
            }

            var radianceTask = RadianceCompute.CreateRadianceTask(
                tokens,
                inputData.Url,
                parentTask,
                actionTask,
                project,
                solution,
                "results",
                create
            );
            inputData.SubTasks.Add(radianceTask);

            return inputData.ToJson();
        }

        /// <summary>
        /// Create a Mesh Task
        /// </summary>
        public static Task CreateMeshTask(
            AuthTokens tokens,
            string url,
            string projectId,
            string taskId,
            string actionTaskId,
            List<int> cpus,
            bool includeSetSet,
            bool create
        )
        {
            var nCPUs = 1;
            cpus.ForEach(cpu => nCPUs *= cpu);
            var commands = new List<string>
            {
                "remove_processor_directories",
                "blockMesh",
                "snappyHexMesh -overwrite"
            };
            if (nCPUs > 1)
            {
                commands.Add("reconstructParMesh -constant -mergeTol 1e-6");
            }

            commands.Add("!checkMesh -writeSets vtk");
            commands.Add("foamToSurface -constant surfaceMesh.obj");

            if (includeSetSet)
            {
                commands.Add("!setSet -batch zones.setSet");
            }

            var meshTask = new GenericViewSet<Task>(
                tokens,
                url,
                $"/api/project/{projectId}/task/"
            ).GetOrCreate(
                new Dictionary<string, object>
                {
                    {"name", "Mesh"},
                    {"parent", taskId},
                    {"dependent_on", actionTaskId}
                },
                new Dictionary<string, object>
                {
                    {
                        "config", new Dictionary<string, object>
                        {
                            {"task_type", "cfd"},
                            {"cmd", "pipeline"},
                            {"commands", commands},
                            {"cpus", cpus},
                        }
                    }
                },
                create
            );
            return meshTask;
        }

        public static void UploadGeometry(
            AuthTokens tokens,
            string url,
            string taskId,
            byte[] geometryFile,
            string caseFolder,
            string fileName,
            List<Dictionary<string, byte[]>> refinementRegions = null
        )
        {
            // Upload File to parent task
            var geometryUpload = new GenericViewSet<Dictionary<string, object>>(
                tokens,
                url,
                $"/api/task/{taskId}/file/{caseFolder}/{fileName}.stl"
            ).Update(
                null,
                new Dictionary<string, object>
                {
                    {"file", geometryFile}
                }
            );
            if (geometryUpload.ContainsKey("error_messages"))
            {
                Console.Write(geometryUpload["error_messages"]);
            }

            if (refinementRegions != null)
            {
                foreach (var refinementRegion in refinementRegions)
                {
                    var refinementFileName = refinementRegion.Keys.First();
                    var file = refinementRegion[refinementFileName];
                    var refinementUpload = new GenericViewSet<Dictionary<string, object>>(
                        tokens,
                        url,
                        $"/api/task/{taskId}/file/foam/constant/triSurface/{refinementFileName}.stl"
                    ).Update(
                        null,
                        new Dictionary<string, object>
                        {
                            {"file", file}
                        }
                    );
                    if (refinementUpload.ContainsKey("error_messages"))
                    {
                        Console.Write(refinementUpload["error_messages"]);
                    }
                }
            }
        }

        public static string CreateEnergyPlus(string inputJson, string folder, bool compute)
        {
            var inputData = new Inputs().FromJson(inputJson);
            var tokens = inputData.Auth;
            var parentTask = inputData.Task;
            var project = inputData.Project;

            if (parentTask == null)
            {
                return null;
                //throw new System.Exception("Cannot upload a case without a parent task.");
            }

            if (project == null)
            {
                return null;
                //throw new System.Exception("Cannot upload a case without a project.");
            }

            if (compute)
            {
                var files = Directory.GetFiles(folder);
                if (!files.Any(file => file.ToLower().EndsWith(".idf")))
                {
                    throw new Exception($"Case folder should contain an idf");
                }

                if (!files.Any(file => file.ToLower().EndsWith(".epw")))
                {
                    throw new Exception($"Case folder should contain an epw");
                }

                foreach (var file in files)
                {
                    var fileInfo = new FileInfo(file);
                    var text = File.ReadAllBytes(file);
                    var response = new GenericViewSet<Dictionary<string, object>>(
                        tokens,
                        inputData.Url,
                        $"/api/task/{parentTask.UID}/file/foam/${fileInfo.Name}"
                    ).Update(
                        null,
                        new Dictionary<string, object>
                        {
                            {"file", text}
                        }
                    );
                }
            }

            var createParams = new Dictionary<string, object>
            {
                {
                    "config", new Dictionary<string, object>
                    {
                        {"task_type", "energyplus"},
                        {"cmd", "run_honeybee_energyplus"},
                        {"cpus", new List<int> {1, 1, 1}}
                    }
                },
                {"status", "pending"}
            };

            var energyTask = new GenericViewSet<Task>(
                tokens,
                inputData.Url,
                $"/api/project/{project.UID}/task/"
            ).GetOrCreate(
                new Dictionary<string, object>
                {
                    {"name", "EnergyPlus"},
                    {"parent", parentTask.UID},
                },
                createParams,
                compute
            );

            var tasks = new List<Task>
            {
                energyTask
            };
            inputData.SubTasks = tasks;

            return inputData.ToJson();
        }

        public static string CreateRadiance(string inputJson, string folder, bool compute)
        {
            var inputData = new Inputs().FromJson(inputJson);
            var tokens = inputData.Auth;
            var parentTask = inputData.Task;
            var project = inputData.Project;

            if (parentTask == null)
            {
                return null;
                //throw new System.Exception("Cannot upload a case without a parent task.");
            }

            if (project == null)
            {
                return null;
                //throw new System.Exception("Cannot upload a case without a project.");
            }

            if (compute)
            {
                var files = Directory.GetFiles(folder);
                if (!files.Any(file => file.ToLower().EndsWith(".rad")))
                {
                    throw new Exception("Case folder should contain an .rad file");
                }

                foreach (var file in files)
                {
                    var fileInfo = new FileInfo(file);
                    var text = File.ReadAllBytes(file);
                    var response = new GenericViewSet<Dictionary<string, object>>(
                        tokens,
                        inputData.Url,
                        $"/api/task/{parentTask.UID}/file/foam/{fileInfo.Name}"
                    ).Update(
                        null,
                        new Dictionary<string, object>
                        {
                            {"file", text}
                        }
                    );
                    if (response.ContainsKey("error_messages"))
                    {
                        throw new Exception(
                            $"Got the following error, while trying to upload: {file}: {response["error_messages"]}");
                    }

                    Thread.Sleep(100);
                }
            }

            var cpus = GetRadianceCPUs(folder);
            var createParams = new Dictionary<string, object>
            {
                {
                    "config", new Dictionary<string, object>
                    {
                        {"task_type", "radiance"},
                        {"cmd", "run_honeybee_radiance"},
                        {"cpus", cpus}
                    }
                },
                {"status", "pending"}
            };

            var radianceTask = new GenericViewSet<Task>(
                tokens,
                inputData.Url,
                $"/api/project/{project.UID}/task/"
            ).GetOrCreate(
                new Dictionary<string, object>
                {
                    {"name", "Radiance"},
                    {"parent", parentTask.UID},
                },
                createParams,
                compute
            );

            var tasks = new List<Task>
            {
                radianceTask
            };
            inputData.SubTasks = tasks;

            return inputData.ToJson();
        }

        private static List<int> GetRadianceCPUs(string folder)
        {
            var files = Directory.GetFiles(folder);
            var batFiles = 0;
            foreach (var file in files)
            {
                if (file.ToLower().EndsWith("_rad.bat"))
                {
                    batFiles++;
                }
            }

            return new List<int> {batFiles, 1, 1};
        }

        public static Dictionary<string, double> GetTaskEstimates(string inputJson)
        {
            var inputData = new Inputs().FromJson(inputJson);
            var tokens = inputData.Auth;
            var solution = inputData.CFDSolution;

            if (solution == null)
            {
                return new Dictionary<string, double>();
            }

            var cells = inputData.Mesh.CellEstimate;
            var iterations = solution.Iterations["init"];


            var nCPUs = 1;
            solution.CPUs.ForEach(cpu => nCPUs *= cpu);

            var meshEstimate = new GenericViewSet<TaskEstimate>(
                tokens,
                inputData.Url,
                $"/api/task/estimation/"
            ).Create(
                new Dictionary<string, object>
                {
                    {"cells", cells},
                    {"cpus", nCPUs},
                    {"task_type", "mesh"}
                }
            );

            var totalTime = meshEstimate.Time;
            var totalCost = meshEstimate.Cost;

            if (solution.CaseType == "VirtualWindTunnel")
            {
                var angleEstimate = new GenericViewSet<TaskEstimate>(
                    tokens,
                    inputData.Url,
                    $"/api/task/estimation/"
                ).Create(
                    new Dictionary<string, object>
                    {
                        {"cells", cells},
                        {"cpus", nCPUs},
                        {"task_type", "vwt_angle"},
                        {"iterations", iterations}
                    }
                );
                var prepareEstimate = new GenericViewSet<TaskEstimate>(
                    tokens,
                    inputData.Url,
                    $"/api/task/estimation/"
                ).Create(
                    new Dictionary<string, object>
                    {
                        {"cells", cells},
                        {"cpus", nCPUs},
                        {"task_type", "prepare"},
                        {"iterations", iterations}
                    }
                );

                totalTime += prepareEstimate.Time + angleEstimate.Time;
                totalCost += prepareEstimate.Cost + angleEstimate.Cost * solution.Angles.Count;
                ;
            }

            return new Dictionary<string, double>
            {
                {"cost", totalCost},
                {"time", totalTime},
                {"cells", cells}
            };
        }

        public static void UploadEPWFile(
            AuthTokens tokens,
            string url,
            string taskId,
            string epwFile
        )
        {
            var epwFileContent = File.ReadAllBytes(epwFile);
            var epwName = Path.GetFileName(epwFile);
            {
                // Upload EPW File to parent task
                var uploadTask = new GenericViewSet<Dictionary<string, object>>(
                    tokens,
                    url,
                    $"/api/task/{taskId}/file/weather/{epwName}"
                ).Update(
                    null,
                    new Dictionary<string, object>
                    {
                        {"file", epwFileContent}
                    }
                );
                if (uploadTask.ContainsKey("error_messages"))
                {
                    throw new Exception(uploadTask["error_messages"].ToString());
                }
            }
        }

        public static Task GetProbeTask(
            List<Task> subTasks,
            string dependentName = ""
        )
        {
            foreach (var subTask in subTasks)
            {
                if (string.IsNullOrEmpty(subTask.UID))
                {
                    continue;
                }

                if (dependentName == subTask.Name)
                {
                    return subTask;
                }

                if (subTask.Name == "Probe")
                {
                    return subTask;
                }
            }

            return null;
        }

        public static void UploadBSDFFile(
            AuthTokens tokens,
            string url,
            string taskId,
            List<RadianceMaterial> materials
        )
        {
            foreach (var material in materials)
            {
                if (material.Overrides?.BSDF == null) continue;
                var bsdfIndex = 0;
                foreach (var bsdfPath in material.Overrides.BSDFPath)
                {
                    if (bsdfPath == "clear.xml")
                    {
                        bsdfIndex++;
                        continue;
                    }

                    var bsdfFileContent = File.ReadAllBytes(bsdfPath);

                    var uploadTask = new GenericViewSet<Dictionary<string, object>>(
                        tokens,
                        url,
                        $"/api/task/{taskId}/file/bsdf/{material.Overrides.BSDF[bsdfIndex]}"
                    ).Update(
                        null,
                        new Dictionary<string, object>
                        {
                            {"file", bsdfFileContent}
                        }
                    );
                    if (uploadTask.ContainsKey("error_messages"))
                    {
                        throw new Exception(uploadTask["error_messages"].ToString());
                    }

                    bsdfIndex++;
                }

                if (material.Overrides.ScheduleValues != null)
                {
                    UploadScheduleFiles(tokens, url, taskId, material);
                }
            }
        }

        public static void UploadScheduleFiles(
            AuthTokens tokens,
            string url,
            string taskId,
            RadianceMaterial material
        )
        {
            var scheduleIndex = 0;
            foreach (var schedule in material.Overrides.ScheduleValues)
            {
                var stream = new MemoryStream();
                using (var memStream = new StreamWriter(stream))
                {
                    memStream.Write(
                        $"{JsonConvert.SerializeObject(schedule)}");
                }

                var scheduleContent = stream.ToArray();
                var scheduleName = $"{material.Name.Replace("*", "")}_{scheduleIndex}.json";
                if (material.Overrides.Schedules == null)
                {
                    material.Overrides.Schedules = new List<string>();
                }

                material.Overrides.Schedules.Add(scheduleName);
                var uploadTask = new GenericViewSet<Dictionary<string, object>>(
                    tokens,
                    url,
                    $"/api/task/{taskId}/file/schedules/{scheduleName}"
                ).Update(
                    null,
                    new Dictionary<string, object>
                    {
                        {"file", scheduleContent}
                    }
                );
                if (uploadTask.ContainsKey("error_messages"))
                {
                    throw new Exception(uploadTask["error_messages"].ToString());
                }

                scheduleIndex++;
            }

            material.Overrides.ScheduleValues = null;
        }
    }
}