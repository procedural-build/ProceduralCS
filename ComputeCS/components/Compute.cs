using System.Collections.Generic;
using ComputeCS.types;
using System.IO;

namespace ComputeCS.Components
{
    public static class Compute
    {
        public static string Create(
            string inputJson,
            byte[] geometryFile,
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
                // Upload File to parent task
                new GenericViewSet<Dictionary<string, object>>(
                    tokens,
                    inputData.Url,
                    $"/api/task/{parentTask.UID}/file/foam/constant/triSurface/cfdGeom.stl"
                ).Update(
                    null,
                    new Dictionary<string, object>
                    {
                    {"file", geometryFile}
                    }
                );
            }

            
            // Tasks to Handle MagPy Celery Actions
            // First Action to create Mesh Files
            var actionTask = new GenericViewSet<Task>(
                tokens,
                inputData.Url,
                $"/api/project/{project.UID}/task/"
            ).GetOrCreate(
                new Dictionary<string, object> {
                    {"name", "Actions"},
                    {"parent", parentTask.UID}
                }, 
                new Dictionary<string, object> {
                    {"config", new Dictionary<string, object> {
                        {"task_type", "magpy"},
                        {"cmd", "cfd.io.tasks.write_mesh"},
                        {"base_mesh", inputData.Mesh.BaseMesh},
                        {"snappyhex_mesh", inputData.Mesh.SnappyHexMesh},
                    }}
                }, 
                create
            );

            // Then Action Task to create CFD files
            if (create)
            {
                new GenericViewSet<Task>(
                    tokens,
                    inputData.Url,
                    $"/api/project/{project.UID}/task/"
                ).Update(
                    actionTask.UID,
                    new Dictionary<string, object> {
                        {"name", "Actions"},
                            {"status", "pending"},
                            {"config", new Dictionary<string, object> {
                                {"task_type", "magpy"},
                                {"cmd", "cfd.io.tasks.write_solution"},
                            {"solution", inputData.CFDSolution}
                        }}
                    }
                );
            }

            
            // Task to Handle Meshing
            var meshTask = new GenericViewSet<Task>(
                tokens,
                inputData.Url,
                $"/api/project/{project.UID}/task/"
            ).GetOrCreate(
                new Dictionary<string, object> {
                    {"name", "Mesh"},
                    {"parent", parentTask.UID},
                    {"dependent_on", actionTask.UID}
                }, 
                new Dictionary<string, object> {
                    {"config", new Dictionary<string, object> {
                        {"task_type", "cfd"},
                        {"cmd", "pipeline"},
                        {"commands", new List<string>
                        {
                            "blockMesh",
                            "snappyHexMesh -overwrite",
                            "reconstructParMesh -constant -mergeTol 1e-6",
                            "!checkMesh -writeSets vtk",
                            "foamToSurface -constant surfaceMesh.obj"
                        }},
                        {"cpus", solution.CPUs},
                    }}
                }, 
                create
            );
            
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
                new Dictionary<string, object> {
                    {"name", solution.CaseType},
                    {"parent", parentTask.UID},
                    {"dependent_on", meshTask.UID}
                }, 
                createParams, 
                create
            );

            var tasks = new List<Task> {
                actionTask,
                meshTask,
                cfdTask
            };
            inputData.SubTasks = tasks;
            
            return inputData.ToJson();
        } 
    }
}