using System;
using System.Collections.Generic;
using ComputeCS.types;
using System.IO;
using System.Linq;

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
                UploadGeometry(tokens, inputData.Url, parentTask.UID, geometryFile, refinementRegions);
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
                            {"snappyhex_mesh", inputData.Mesh.SnappyHexMesh},
                        }
                    }
                },
                create
            );

            // Then Action Task to create CFD files
            if (create)
            {
                if (solution != null)
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
            Dictionary<string, object> createParams;

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
                            {"iterations", solution.Iterations}
                        }
                    }
                };
            }
            else
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

            var tasks = new List<Task>
            {
                actionTask,
                meshTask,
                cfdTask
            };
            inputData.SubTasks = tasks;

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
            cpus.ForEach(cpu => nCPUs = nCPUs * cpu);
            var commands = new List<string>
            {
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
            List<Dictionary<string, byte[]>> refinementRegions
        )
        {
            // Upload File to parent task
            new GenericViewSet<Dictionary<string, object>>(
                tokens,
                url,
                $"/api/task/{taskId}/file/foam/constant/triSurface/cfdGeom.stl"
            ).Update(
                null,
                new Dictionary<string, object>
                {
                    {"file", geometryFile}
                }
            );

            if (refinementRegions != null)
            {
                foreach (var refinementRegion in refinementRegions)
                {
                    var fileName = refinementRegion.Keys.First();
                    var file = refinementRegion[fileName];
                    new GenericViewSet<Dictionary<string, object>>(
                        tokens,
                        url,
                        $"/api/task/{taskId}/file/foam/constant/triSurface/{fileName}.stl"
                    ).Update(
                        null,
                        new Dictionary<string, object>
                        {
                            {"file", file}
                        }
                    );
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
            return new List<int>{batFiles, 1, 1};
        }

        public static Dictionary<string, double> GetTaskEstimates(string inputJson)
        {
            var inputData = new Inputs().FromJson(inputJson);
            var tokens = inputData.Auth;
            var solution = inputData.CFDSolution;
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

            var totalTime = meshEstimate.Time + prepareEstimate.Time + angleEstimate.Time;
            var totalCost = meshEstimate.Cost + prepareEstimate.Cost + angleEstimate.Cost * solution.Angles.Count;;
            
            return new Dictionary<string, double>
            {
                {"cost", totalCost},
                {"time", totalTime},
                {"cells", cells}
            };
        }
    }
}