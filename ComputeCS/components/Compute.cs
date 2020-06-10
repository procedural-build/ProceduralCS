using System.Collections.Generic;
using ComputeCS.types;
using System.IO;

namespace ComputeCS.Components
{
    public static class Compute
    {
        public static Dictionary<string, object> Create(
            string inputJson,
            MemoryStream geometryFile
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

            // Upload File to parent task
            new GenericViewSet<string>(
                tokens,
                $"{inputData.Url}/api/task/{parentTask.UID}/file/constant/triSurface/cfdGeom.stl/"
            ).Update(
                null,
                new Dictionary<string, object>
                {
                    {"file", geometryFile}
                }
            );
            
            // Tasks to Handle MagPy Celery Actions
            // First Action to create Mesh Files
            var actionTask = new GenericViewSet<Task>(
                tokens, 
                $"{inputData.Url}/api/project/{project.UID}/task/"
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
                true
            );
            // Then Action Task to create CFD files
            new GenericViewSet<Task>(
                tokens, 
                $"{inputData.Url}/api/project/{project.UID}/task/"
            ).GetOrCreate(
                new Dictionary<string, object> {
                    {"name", "Actions"},
                    {"parent", parentTask.UID}
                }, 
                new Dictionary<string, object> {
                    {"config", new Dictionary<string, object> {
                        {"task_type", "magpy"},
                        {"cmd", "cfd.io.tasks.write_solution"},
                        {"solution", inputData.CFDSolution}
                    }}
                }, 
                true
            );
            
            // Task to Handle Meshing
            var meshTask = new GenericViewSet<Task>(
                tokens, 
                $"{inputData.Url}/api/project/{project.UID}/task/"
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
                            "snappyHesMesh -overwrite",
                            "reconstructParMesh -constant -mergeTol 1e-6",
                            "!checkMesh -writeSets vtk"
                        }},
                        {"cpus", solution.CPUs},
                    }}
                }, 
                true
            );
            
            // Task to Handle CFD
            Dictionary<string, object> createParams;

            if (solution.Solver.ToLower() == "windtunnel")
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
            new GenericViewSet<Task>(
                tokens, 
                $"{inputData.Url}/api/project/{project.UID}/task/"
            ).GetOrCreate(
                new Dictionary<string, object> {
                    {"name", solution.Solver},
                    {"parent", parentTask.UID},
                    {"dependent_on", meshTask.UID}
                }, 
                createParams, 
                true
            );
            
            return new Dictionary<string, object>
            {
                {"out", inputData.ToJson()}
            };
        } 
    }
}