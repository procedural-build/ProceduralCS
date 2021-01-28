using System.Collections.Generic;
using System.IO;
using ComputeCS.types;
using Newtonsoft.Json;

namespace ComputeCS.Components
{
    public static class DaylightMetrics
    {
        public static string Create(
            string inputJson, 
            string overrides, 
            string preset,
            List<int> cpus,
            bool create
        )
        {
            var inputData = new Inputs().FromJson(inputJson);
            var tokens = inputData.Auth;
            var parentTask = inputData.Task;
            var project = inputData.Project;
            var solution = inputData.RadiationSolution;
            var subTasks = inputData.SubTasks;
            var daylightTask = GetDaylightTask(subTasks, solution.Method);
            const string caseDir = "metrics";
            
            var _overrides = JsonConvert.DeserializeObject<Dictionary<string, object>>(overrides);
            
            if (parentTask == null)
            {
                return null;
            }

            if (project == null)
            {
                return null;
            }

            var taskQueryParams = new Dictionary<string, object>
            {
                {"name", "Daylight Metrics"},
                {"parent", parentTask.UID},
            };
            if (daylightTask != null)
            {
                taskQueryParams.Add("dependent_on", daylightTask.UID);
            }
            
            // First Action to create Mesh Files
            var metricTask = new GenericViewSet<Task>(
                tokens,
                inputData.Url,
                $"/api/project/{project.UID}/task/"
            ).GetOrCreate(
                taskQueryParams,
                new Dictionary<string, object>
                {
                    {
                        "config", new Dictionary<string, object>
                        {
                            {"task_type", "radiance"},
                            {"cmd", $"{preset}"},
                            {"overrides", _overrides},
                            {"cpus", cpus},
                            {"case_dir", caseDir},
                        }
                    }
                },
                create
            );
            inputData.SubTasks.Add(metricTask);

            return inputData.ToJson();
        }
        
        public static Task GetDaylightTask(
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
            }

            return null;
        }
    }
}