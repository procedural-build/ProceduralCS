using System.Collections.Generic;
using System.Linq;
using ComputeCS.Exceptions;
using ComputeCS.types;

namespace ComputeCS.Tasks
{
    public static class Mesh
    {
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
            Dictionary<string, object> overrides,
            bool create
        )
        {
            if (overrides != null && overrides.ContainsKey("keep_mesh") && (bool) overrides["keep_mesh"])
            {
                create = false;
            }

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

            try
            {
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
            catch (NoObjectFoundException error)
            {
                if (overrides != null && overrides.ContainsKey("keep_mesh") && (bool) overrides["keep_mesh"])
                {
                    // if keep_mesh is in the overrides then we most likely are in the situation of a
                    // mesh independence study and the user want to reuse the mesh from the study. There will therefore not be any Mesh task to find.
                    return null;
                }

                throw new NoObjectFoundException(error.Message);
            }
            
        }

        public static List<Task> CreateMeshIndependenceTasks(
            AuthTokens tokens,
            string url,
            string projectId,
            string parentTaskId,
            string actionTaskId,
            bool includeSetSet,
            CFDSolution solution,
            CFDMesh meshConfig,
            bool create
            )
        {
            var overrides = solution.Overrides;
            var cpus = solution.CPUs;
            var path = $"/api/project/{projectId}/task/";
            var parentIndependenceTask = Utils.CreateParent(
                tokens,
                url,
                path,
                "Mesh Independence Study",
                new Dictionary<string, object> {{"parent", parentTaskId}},
                create);
            var tasks = new List<Task> {parentIndependenceTask};

            var cellSizes = GetBaseCellSizes(meshConfig, overrides);
            var duplicateBaseMesh = (BaseMesh) meshConfig.BaseMesh.Clone();
            var duplicateSolution = (CFDSolution) solution.Clone();
            duplicateSolution.CaseType = "SimpleCase";
            
            foreach (var cellSize in cellSizes)
            {
                var parentTask = Utils.CreateParent(
                    tokens,
                    url,
                    path,
                    $"Cell Size {cellSize}",
                    new Dictionary<string, object> {{"parent", parentIndependenceTask.UID}, {"case_dir", $"independence/mesh_{cellSize}/foam"}, {"show_meta_data", true}},
                    create);
                tasks.Add(parentTask);
                
                // create action task
                duplicateBaseMesh.CellSize = cellSize;
                var actionTask = Action.CreateIndependenceActionTask(
                    tokens,
                    url,
                    path,
                    parentTask.UID,
                    actionTaskId,
                    duplicateBaseMesh,
                    create
                );
                tasks.Add(actionTask);
                
                // create mesh
                var meshTask = CreateMeshTask(tokens, url, projectId, parentTask.UID, actionTask.UID, cpus,
                    includeSetSet, overrides, create);
                tasks.Add(meshTask);
                
                // create solver task
                var cfdTask = CFD.CreateSimpleCase(tokens, url, path, parentTask.UID, meshTask.UID, duplicateSolution, create);
                tasks.Add(cfdTask);
            }
            
            return tasks;
        }

        public static List<int> GetBaseCellSizes(CFDMesh meshConfig, Dictionary<string, object> overrides)
        {
            if (overrides != null && overrides.ContainsKey("mesh_independence"))
            {
                var meshIndependenceOverrides = overrides["mesh_independence"].ToDictionary();
                if (meshIndependenceOverrides.ContainsKey("cell_sizes"))
                {
                    return ((object[]) meshIndependenceOverrides["cell_sizes"]).Cast<int>().ToList();
                }
            }
            
            var defaultSize = (int)meshConfig.BaseMesh.CellSize;
            var smallSize = defaultSize - 4;
            var largeSize = defaultSize + 4;
            if (defaultSize > 4) return new List<int> {smallSize, defaultSize, largeSize};
            
            smallSize = (int)(defaultSize / 2);
            largeSize = defaultSize + smallSize;

            return new List<int> {smallSize, defaultSize, largeSize};
        }
    }
}