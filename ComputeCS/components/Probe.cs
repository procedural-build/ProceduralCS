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
            List<List<double>> points,
            List<string> fields,
            List<int> cpus,
            string dependentOn = "",
            bool create = false
        )
        {
            var inputData = new Inputs().FromJson(inputJson);
            var tokens = inputData.Auth;
            var parentTask = inputData.Task;
            var subTasks = inputData.SubTasks;
            var simulationTask = GetSimulationTask(subTasks, dependentOn);
            var project = inputData.Project;

            if (parentTask == null) {return null;}
            if (simulationTask == null) {return null;}

            var fieldsOpenFoamFormat = String.Join(" ", fields);
            var sampleSets = GenerateSampleSet(points);

            var task = new GenericViewSet<Task>(
                tokens,
                inputData.Url,
                $"/api/project/{project.UID}/task/"
            ).GetOrCreate(
                new Dictionary<string, object> {
                    {"name", "PostProcess"},
                    {"parent", parentTask.UID},
                    {"dependent_on", simulationTask.UID}
                },
                new Dictionary<string, object> {
                    {"config", new Dictionary<string, object> {
                        {"task_type", "cfd"},
                        {"cmd", "pipeline"},
                        {"commands", new List<string>{ "write_sample_set", $"postProcess -fields '({fieldsOpenFoamFormat})' -func internalCloud"} },
                        {"case_dir", "VWT/" },
                        {"cpus", cpus },
                        {"sets", sampleSets },
                        {"fields", fields},
                    }}
                },
                create
            );

            inputData.SubTasks.Add(task);

            return inputData.ToJson();
        }

        private static Task GetSimulationTask(
            List<Task> subTasks,
            string dependentName = ""
        )
        {

            foreach (Task subTask in subTasks)
            {
                if (dependentName == subTask.Name)
                {
                    return subTask;
                }
                else if ((string)subTask.Config["cmd"] == "wind_tunnel")
                {
                    return subTask;
                }
                else if (subTask.Config.ContainsKey("commands"))
                {
                    // Have to do this conversion to be able to compare the strings
                    if (((JArray)subTask.Config["commands"]).ToObject<List<string>>()[0] == "simpleFoam")
                    {
                        return subTask;
                    }
                } 
            }
            return null;
        }

        public static List<Dictionary<string, object>> GenerateSampleSet(
            List<List<double>> points
            )
        {
            var sampleSet = new Dictionary<string, object> {
                { "name", "set1" },
                { "points", points}
            };

            return new List<Dictionary<string, object>> { sampleSet };
        }
    }
}
