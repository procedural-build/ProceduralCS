using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using ComputeCS.Tasks;
using ComputeCS.types;
using Action = ComputeCS.Tasks.Action;
using CFD = ComputeCS.Tasks.CFD;

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

            if (create && !(solution.Overrides != null && solution.Overrides.ContainsKey("keep_mesh") &&
                            (bool) solution.Overrides["keep_mesh"]))
            {
                Upload.UploadGeometry(tokens, inputData.Url, parentTask.UID, geometryFile,
                    "foam/constant/triSurface",
                    "cfdGeom", refinementRegions);
            }

            // Tasks to Handle MagPy Celery Actions
            // First Action to create Mesh Files
            var actionTask = Action.CreateCFDActionTask(tokens, inputData.Url,
                $"/api/project/{project.UID}/task/", parentTask.UID, inputData.Mesh, solution,
                geometryFile.Length, solution.Overrides, create);

            // Task to Handle Meshing
            var includeSetSet = inputData.Mesh.BaseMesh.setSetRegions.Count > 0;

            var tasks = new List<Task>
            {
                actionTask,
            };

            if (solution.CaseType.ToLower() == "meshindependencestudy")
            {
                var meshIndependenceTasks = Tasks.Mesh.CreateMeshIndependenceTasks(tokens, inputData.Url, project.UID,
                    parentTask.UID, actionTask.UID, includeSetSet, solution, inputData.Mesh, create);
                tasks.AddRange(meshIndependenceTasks);
            }
            else
            {
                var meshTask = Tasks.Mesh.CreateMeshTask(tokens, inputData.Url, project.UID, parentTask.UID,
                    actionTask.UID,
                    solution.CPUs,
                    includeSetSet, solution.Overrides, create);
                if (meshTask != null)
                {
                    tasks.Add(meshTask);    
                }

                var cfdTask = new Task();
                if (solution.CaseType.ToLower() == "virtualwindtunnel")
                {
                    cfdTask = CFD.CreateVWTCase(tokens,
                        inputData.Url,
                        $"/api/project/{project.UID}/task/",
                        parentTask.UID,
                        meshTask?.UID,
                        solution,
                        create
                    );
                }
                else if (solution.CaseType.ToLower() == "simplecase")
                {
                    cfdTask = CFD.CreateSimpleCase(tokens,
                        inputData.Url,
                        $"/api/project/{project.UID}/task/",
                        parentTask.UID,
                        meshTask.UID,
                        solution,
                        create
                    );
                }

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
            var probeTask = Tasks.Probe.GetProbeTask(subTasks, dependentOn);
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
                Upload.UploadGeometry(tokens, inputData.Url, parentTask.UID, geometryFile, "geometry",
                    "radianceGeom");
                switch (solution.Method)
                {
                    case "three_phase":
                        Upload.UploadEPWFile(tokens, inputData.Url, parentTask.UID, solution.EPWFile);
                        Upload.UploadBSDFFile(tokens, inputData.Url, parentTask.UID, solution.Materials);
                        break;
                    case "solar_radiation":
                    case "mean_radiant_temperature":
                        Upload.UploadEPWFile(tokens, inputData.Url, parentTask.UID, solution.EPWFile);
                        break;
                }
            }

            // Create Action Task
            var actionTask = RadianceCompute.CreateRadianceActionTask(tokens, inputData.Url, parentTask, probeTask,
                project, solution, caseDir, create);
            inputData.SubTasks.Add(actionTask);

            if (solution.Method == "mean_radiant_temperature")
            {
                var rayTracingTask = Tasks.Utils.CreateParent(tokens, inputData.Url,
                    $"/api/project/{project.UID}/task/",
                    "Ray Tracing",
                    new Dictionary<string, object> {{"parent", parentTask.UID}}, create);

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
    }
}