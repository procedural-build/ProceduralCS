using System;
using System.Collections.Generic;
using System.Linq;
using ComputeCS.types;
using Newtonsoft.Json;
using NLog;

namespace ComputeCS.Tasks
{
    public static class Probe
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

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

        public static Task CreateRadiationProbeTask(
            AuthTokens tokens,
            string url,
            string projectId,
            Dictionary<string, object> taskQueryParams,
            string caseDir,
            List<Dictionary<string, object>> sampleSets,
            bool create
        )
        {
            var config = new Dictionary<string, object>
            {
                {"task_type", "magpy"},
                {"cmd", "radiance.io.tasks.write_radiation_samples_set"},
                {"case_dir", caseDir},
                {"sets", sampleSets},
            };

            var createParams = new Dictionary<string, object>
            {
                {"config", config},
                {"status", "pending"}
            };

            taskQueryParams.Add("project", projectId);

            var task = TaskViews.GetCreateOrUpdateTask(
                tokens,
                url,
                $"/api/task/",
                taskQueryParams,
                createParams,
                create
            );

            if (task.ErrorMessages != null)
            {
                throw new Exception(task.ErrorMessages.First());
            }

            return task;
        }

        public static Task CreateProbeTask(
            AuthTokens tokens,
            string url,
            string projectId,
            Dictionary<string, object> taskQueryParams,
            string caseDir,
            List<int> cpus,
            List<Dictionary<string, object>> sampleSets,
            List<string> fields,
            Dictionary<string, object> overrides,
            string probeCommand = "postProcess -func internalCloud",
            bool create = false
        )
        {
            var commands = new List<string> {"write_sample_set"};
            var nCPUs = 1;
            cpus.ForEach(cpu => nCPUs *= cpu);
            commands.Add(nCPUs > 1 ? $"!{probeCommand}" : probeCommand);

            var config = new Dictionary<string, object>
            {
                {"task_type", "cfd"},
                {"cmd", "pipeline"},
                {"commands", commands},
                {"case_dir", caseDir},
                {"cpus", cpus},
                {"sets", sampleSets},
                {"fields", fields},
            };
            if (overrides != null)
            {
                config.Add("overrides", overrides);
            }

            var createParams = new Dictionary<string, object>
            {
                {
                    "config", config
                }
            };

            taskQueryParams.Add("project", projectId);

            var task = TaskViews.GetCreateOrUpdateTask(
                tokens,
                url,
                $"/api/task/",
                taskQueryParams,
                createParams,
                create
            );
            Logger.Info($"Created probe task: {task.UID}");
            return task;
        }

        public static List<Task> CreateMeshIndependenceProbeTask(
            Inputs inputData,
            List<int> cpus,
            List<Dictionary<string, object>> sampleSets,
            List<string> fields,
            Dictionary<string, object> overrides,
            bool create
        )
        {
            var tokens = inputData.Auth;
            var subTasks = inputData.SubTasks;
            var parentTask = inputData.Task;
            var projectId = inputData.Project.UID;
            var url = inputData.Url;
            var tasks = new List<Task>();
            if (overrides == null)
            {
                overrides = new Dictionary<string, object> {{"set_dir", "/data/VWT"}};
            }
            else
            {
                overrides.Add("set_dir", "VWT");
            }
            // Get Cell Size parent tasks
            var meshIndependenceParent = Tasks.Utils.GetDependentTask(subTasks, "Mesh Independence Study",
                inputData.Url, tokens, parentTask.UID);
            var cellSizes = Tasks.Mesh.GetBaseCellSizes(inputData.Mesh, inputData.CFDSolution.Overrides);
            var cellSizeNames = cellSizes.Select(size => $"Cell Size {size}").ToList();
            var cellSizeTasks = Tasks.Utils.GetDependentTask(subTasks, cellSizeNames,
                inputData.Url, tokens, meshIndependenceParent.UID);
            var probeTasks = new List<string>();
            
            // Get simpleCase tasks
            var caseDirs = new List<string>();
            foreach (var cellSizeTask in cellSizeTasks)
            {
                var simulationTask = Tasks.Utils.GetDependentTask(subTasks, "SimpleCase",
                    inputData.Url, tokens, cellSizeTask.UID);
                if (simulationTask.Config == null)
                {
                    simulationTask = new GenericViewSet<Task>(tokens,
                        url,
                        "/api/task/").Retrieve(simulationTask.UID);
                }
                var caseDir = (string)simulationTask.Config["case_dir"];
                caseDirs.Add($"/data/{caseDir}");
                
                var taskQueryParams = new Dictionary<string, object>
                {
                    {"name", "Probe"},
                    {"parent", cellSizeTask.UID},
                    {"dependent_on", simulationTask.UID}
                };
                var probeTask = CreateProbeTask(tokens, url, projectId, taskQueryParams, caseDir, cpus, sampleSets,
                    fields, overrides, "postProcess -func internalCloud -latestTime", create);
                tasks.Add(probeTask);
                probeTasks.Add(probeTask.UID);
            }

            // Create probe task for each 
            // Create statistics tasks 
            var statisticsTask = CreateProbeStatisticsTask(tokens, url, projectId,  meshIndependenceParent.UID, probeTasks, caseDirs, cpus,
                sampleSets.Select(sampleSet => (string) sampleSet["name"]).ToList(), fields, overrides, create);
            tasks.Add(statisticsTask);
            return tasks;
        }

        public static Task CreateProbeStatisticsTask(
            AuthTokens tokens,
            string url,
            string projectId,
            string parentTaskId,
            List<string> dependentOns,
            List<string> caseDirs,
            List<int> cpus,
            List<string> sampleSets,
            List<string> fields,
            Dictionary<string, object> overrides,
            bool create
        )
        {
            var config = new Dictionary<string, object>
            {
                {"task_type", "cfd"},
                {"cmd", "compute_mesh_independence_result"},
                {"case_dirs", caseDirs},
                {"case_dir", "independence"},
                {"cpus", cpus},
                {"sets", sampleSets},
                {"fields", fields},
            };
            if (overrides != null)
            {
                config.Add("overrides", overrides);
            }

            var createParams = new Dictionary<string, object>
            {
                {
                    "config", config
                },
                {
                    "dependent_on", dependentOns
                }
            };

            var taskQueryParams = new Dictionary<string, object>
            {
                {"name", "Probe Statistics"},
                {"parent", parentTaskId},
                {"project", projectId}
            };

            var task = TaskViews.GetCreateOrUpdateTask(
                tokens,
                url,
                "/api/task/",
                taskQueryParams,
                createParams,
                create
            );
            Logger.Info($"Created probe task: {task.UID}");
            return task;
        }
    }
}