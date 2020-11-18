using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ComputeCS.types;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ComputeCS.Components
{
    public static class Probe
    {
        public static string ProbePoints(
            string inputJson,
            List<List<List<double>>> points,
            List<string> fields,
            List<string> names,
            List<int> cpus,
            string dependentOn = "",
            string caseDir = "VWT/",
            bool create = false
        )
        {
            var inputData = new Inputs().FromJson(inputJson);
            var tokens = inputData.Auth;
            var parentTask = inputData.Task;
            var subTasks = inputData.SubTasks;
            var simulationTask = GetSimulationTask(subTasks, dependentOn);
            var project = inputData.Project;

            if (parentTask == null)
            {
                return null;
            }

            var sampleSets = GenerateSampleSet(names);

            var taskQueryParams = new Dictionary<string, object>
            {
                {"name", "Probe"},
                {"parent", parentTask.UID},
            };
            if (simulationTask != null)
            {
                taskQueryParams.Add("dependent_on", simulationTask.UID);
            }

            if (create)
            {
                UploadPointsFiles(tokens, inputData.Url, parentTask.UID, names, points, caseDir);
            }
            
            var task = CreateProbeTask(tokens, inputData.Url, project.UID, taskQueryParams, caseDir, cpus,
                sampleSets, fields, create);
            inputData.SubTasks.Add(task);

            return inputData.ToJson();
        }

        private static Task GetSimulationTask(
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

                if (subTask.Name == "SimpleCase")
                {
                    return subTask;
                }

                if (subTask.Config.ContainsKey("cmd") && (string) subTask.Config["cmd"] == "wind_tunnel")
                {
                    return subTask;
                }

                if (subTask.Config.ContainsKey("commands"))
                {
                    // Have to do this conversion to be able to compare the strings
                    if (((JArray) subTask.Config["commands"]).ToObject<List<string>>()[0] == "simpleFoam")
                    {
                        return subTask;
                    }
                }
            }

            return null;
        }

        public static List<Dictionary<string, object>> GenerateSampleSet(
            List<string> names
        )
        {
            var sampleSets = new List<Dictionary<string, object>>();
            foreach (var name in names)
            {
                sampleSets.Add(
                    new Dictionary<string, object>()
                    {
                        {"name", name},
                        {"file", $"{name}.pts"}
                    }
                );
            }

            return sampleSets;
        }

        public static void UploadPointsFiles(
            AuthTokens tokens,
            string url,
            string taskId,
            List<string> names,
            List<List<List<double>>> points,
            string caseDir
        )
        {
            var index = 0;
            foreach (var name in names)
            {
                new GenericViewSet<Dictionary<string, object>>(
                    tokens,
                    url,
                    $"/api/task/{taskId}/file/{caseDir}/{name}.pts"
                ).Update(
                    null,
                    new Dictionary<string, object>
                    {
                        {"file", points[index]}
                    }
                );
                index++;
            }
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
            bool create
        )
        {
            var commands = new List<string> {"write_sample_set"};
            var nCPUs = 1;
            cpus.ForEach(cpu => nCPUs *= cpu);
            if (nCPUs > 1)
            {
                //commands.Add("decomposePar -time :10000");
                commands.Add("!postProcess -func internalCloud");
                //commands.Add("reconstructPar");
            }
            else
            {
                commands.Add("postProcess -func internalCloud");
            }

            var createParams = new Dictionary<string, object>
            {
                {
                    "config", new Dictionary<string, object>
                    {
                        {"task_type", "cfd"},
                        {"cmd", "pipeline"},
                        {"commands", commands},
                        {"case_dir", caseDir},
                        {"cpus", cpus},
                        {"sets", sampleSets},
                        {"fields", fields},
                    }
                }
            };

            taskQueryParams.Add("project", projectId);
            
            var task = GetCreateOrUpdateTask(
                tokens,
                url,
                $"/api/task/",
                taskQueryParams,
                createParams,
                create
            );

            return task;
        }

        public static Task GetCreateOrUpdateTask(
            AuthTokens tokens,
            string url,
            string path,
            Dictionary<string, object> queryParams,
            Dictionary<string, object> createParams,
            bool create
        )
        {
            try
            {
                var task = new GenericViewSet<Task>(
                    tokens,
                    url,
                    path
                ).GetByQueryParams(
                    queryParams
                );
                if (task != null && create)
                {
                    if (task.Status == "failed" || task.Status == "failed")
                    {
                        createParams.Add("status", "pending");
                    }
                    task = new GenericViewSet<Task>(
                        tokens,
                        url,
                        path
                    ).PartialUpdate(
                        task.UID,
                        createParams,
                        new Dictionary<string, object>{{"runAs", "last"}}
                    );
                }

                return task;
            }
            catch (ArgumentException err)
            {
                if (create)
                {
                    // Merge the query_params with create_params
                    if (createParams == null)
                    {
                        createParams = queryParams;
                    }
                    else
                    {
                        createParams = queryParams
                            .Union(createParams)
                            .ToDictionary(s => s.Key, s => s.Value);
                    }

                    // Create the object
                    return new GenericViewSet<Task>(
                        tokens,
                        url,
                        path
                    ).Create(createParams);
                }

                return new Task {ErrorMessages = new List<string> {err.Message}};
            }
        }
    }
}