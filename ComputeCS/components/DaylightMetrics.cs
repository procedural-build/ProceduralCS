using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            string caseDir,
            bool create
        )
        {
            var inputData = new Inputs().FromJson(inputJson);
            var tokens = inputData.Auth;
            var parentTask = inputData.Task;
            var project = inputData.Project;
            var solution = inputData.RadiationSolution;
            var subTasks = inputData.SubTasks;
            var daylightTask = GetDaylightTask(subTasks, Utils.SnakeCaseToHumanCase(solution.Method));
            
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
                {"name", Utils.SnakeCaseToHumanCase(preset)},
                {"parent", parentTask.UID},
                {"project", project.UID}
            };
            if (daylightTask != null)
            {
                taskQueryParams.Add("dependent_on", daylightTask.UID);
            }

            var taskCreateParams = new Dictionary<string, object>
            {
                {
                    "config", new Dictionary<string, object>
                    {
                        {"task_type", "radiance"},
                        {"cmd", $"{preset}"},
                        {"overrides", _overrides},
                        {"cpus", cpus},
                        {"case_dir", caseDir},
                        {"result_names", solution.Probes.Keys}
                    }
                }
            };
            
            // First Action to create Mesh Files
            var metricTask = Tasks.GetCreateOrUpdateTask(
                tokens,
                inputData.Url,
                $"/api/task/",
                taskQueryParams,
                taskCreateParams,
                create
            );
            if (metricTask.ErrorMessages != null && metricTask.ErrorMessages.Count > 0)
            {
                if (create == false && metricTask.ErrorMessages.First() == "No object found")
                {
                    // pass
                }
                else
                {
                    throw new Exception(metricTask.ErrorMessages.First());    
                }
                
            }
            
            inputData.SubTasks.Add(metricTask);

            return inputData.ToJson();
        }
        
        public static Task GetDaylightTask(
            List<Task> subTasks,
            string dependentName = ""
        )
        {
            if (subTasks == null)
            {
                return null;
            }
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