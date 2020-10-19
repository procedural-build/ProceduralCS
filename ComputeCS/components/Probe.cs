using System;
using System.Collections.Generic;
using System.Text;
using ComputeCS.types;
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

            var sampleSets = GenerateSampleSet(points, names);

            var taskQueryParams = new Dictionary<string, object>
            {
                {"name", "Probe"},
                {"parent", parentTask.UID},
            };
            if (simulationTask != null)
            {
                taskQueryParams.Add("dependent_on", simulationTask.UID);
            }

            var task = CreateThresholdTask(tokens, inputData.Url, project.UID, taskQueryParams, caseDir, cpus,
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
                else if (subTask.Config.ContainsKey("cmd") && (string) subTask.Config["cmd"] == "wind_tunnel")
                {
                    return subTask;
                }
                else if (subTask.Config.ContainsKey("commands"))
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
            List<List<List<double>>> points,
            List<string> names
        )
        {
            var index = 0;
            var sampleSets = new List<Dictionary<string, object>>();
            foreach (var name in names)
            {
                sampleSets.Add(
                    new Dictionary<string, object>()
                    {
                        {"name", name},
                        {"points", points[index]}
                    }
                );
                index++;
            }

            return sampleSets;
        }

        public static Task CreateThresholdTask(
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
                commands.Add("decomposePar -time :10000");
                commands.Add("postProcess -func internalCloud");
                commands.Add("reconstructPar");
            }
            else {
                commands.Add("postProcess -func internalCloud");
            }

            var task = new GenericViewSet<Task>(
                tokens,
                url,
                $"/api/project/{projectId}/task/"
            ).GetOrCreate(
                taskQueryParams,
                new Dictionary<string, object>
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
                },
                create
            );

            return task;
        }
    }
}