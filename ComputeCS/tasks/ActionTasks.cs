using System.Collections.Generic;
using ComputeCS.types;

namespace ComputeCS.Tasks
{
    public static class Action
    {
        public static Task CreateIndependenceActionTask(
            AuthTokens tokens,
            string url,
            string path,
            string parentId,
            string dependentOn,
            BaseMesh meshData,
            bool create
        )
        {
            var actionTask = new GenericViewSet<Task>(
                tokens,
                url,
                path
            ).GetOrCreate(
                new Dictionary<string, object>
                {
                    {"name", "Actions"},
                    {"parent", parentId},
                    {"dependent_on", dependentOn}
                },
                new Dictionary<string, object>
                {
                    {
                        "config", new Dictionary<string, object>
                        {
                            {"task_type", "magpy"},
                            {"cmd", "pipelines.pipeline"},
                            {"commands", new List<string>
                                {
                                    "magpy_copytree", 
                                    "write_mesh"
                                }
                            },
                            {"src", "foam"},
                            {"dst", $"independence/mesh_{meshData.CellSize}/foam"},
                            {"base_mesh", meshData},
                            {"snappyhex_mesh", null}
                        }
                    }
                },
                create
            );

            return actionTask;
        }
        
        public static Task CreateCFDActionTask(
            AuthTokens tokens,
            string url,
            string path,
            string parentId,
            CFDMesh meshData,
            CFDSolution solution,
            int geometryFileSize,
            Dictionary<string, object> overrides,
            bool create
        )
        {
            if (overrides != null && overrides.ContainsKey("keep_mesh") && (bool) overrides["keep_mesh"])
            {
                create = false;
            }
            
           var actionTask = new GenericViewSet<Task>(
                tokens,
                url,
                path
            ).GetOrCreate(
                new Dictionary<string, object>
                {
                    {"name", "Actions"},
                    {"parent", parentId}
                },
                new Dictionary<string, object>
                {
                    {
                        "config", new Dictionary<string, object>
                        {
                            {"task_type", "magpy"},
                            {"cmd", "cfd.io.tasks.write_mesh"},
                            {"base_mesh", meshData.BaseMesh},
                            {"snappyhex_mesh", meshData.SnappyHexMesh.ToDict()},
                        }
                    }
                },
                create
            );

            // Then Action Task to create CFD files
            if (create)
            {
                if (solution != null && geometryFileSize > 0)
                {
                    new GenericViewSet<Task>(
                        tokens,
                        url,
                        path
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

            return actionTask;
        }
    }
}