using System;
using System.Collections.Generic;
using ComputeCS.types;
using Newtonsoft.Json;
using NLog;

namespace ComputeCS.Components
{
    public class PressureCoefficient
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static string CreatePressureCoefficientTask(
            string inputJson,
            List<string> probeNames,
            List<int> cpus,
            string dependentOn,
            string overrides,
            bool create
        )
        {
            var inputData = new Inputs().FromJson(inputJson);
            var tokens = inputData.Auth;
            var parentTask = inputData.Task;
            var subTasks = inputData.SubTasks;
            var postProcessTask =
                Tasks.Utils.GetDependentTask(subTasks, dependentOn, inputData.Url, tokens, parentTask.UID);
            var project = inputData.Project;
            var overrideDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(overrides);

            if (parentTask?.UID == null)
            {
                return null;
            }

            var taskQueryParams = new Dictionary<string, object>
            {
                {"name", "Pressure Coefficient"},
                {"parent", parentTask.UID},
                {"project", project.UID}
            };

            if (postProcessTask != null)
            {
                taskQueryParams.Add("dependent_on", postProcessTask.UID);
            }

            var taskCreateParams = new Dictionary<string, object>
            {
                {
                    "config", new Dictionary<string, object>
                    {
                        {"task_type", "cfd"},
                        {"cmd", "calculate_pressure_coefficients"},
                        {"case_dir", "VWT"},
                        {"probes", probeNames},
                        {"cpus", cpus},
                        {"overrides", overrideDict}
                    }
                }
            };

            var task = TaskViews.GetCreateOrUpdateTask(tokens, inputData.Url, "/api/task/", taskQueryParams,
                taskCreateParams, create);

            if (task.ErrorMessages != null)
            {
                throw new Exception(task.ErrorMessages[0]);
            }

            if (inputData.SubTasks != null)
            {
                inputData.SubTasks.Add(task);
            }
            else
            {
                inputData.SubTasks = new List<Task> {task};
            }

            Logger.Info($"Created Pressure Coefficient task: {task.UID}");

            return inputData.ToJson();
        }
    }
}