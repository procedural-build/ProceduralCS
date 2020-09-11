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
                throw new System.Exception("Cannot upload a case without a parent task.");
            }

            if (project == null)
            {
                throw new System.Exception("Cannot upload a case without a project.");
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
            var includeSetSet = inputData.Mesh.BaseMesh.setSetRegions != null;
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
                            {"cpus", solution.CPUs},
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
                                    "reconstructPar -skipZero"
                                }
                            },
                            {"cpus", solution.CPUs},
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
            var commands = new List<string>
            {
                "blockMesh",
                "snappyHexMesh -overwrite",
                "reconstructParMesh -constant -mergeTol 1e-6",
                "!checkMesh -writeSets vtk",
                "foamToSurface -constant surfaceMesh.obj"
            };

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
    }
}