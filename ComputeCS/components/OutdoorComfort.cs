using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ComputeCS.types;
using NLog;

namespace ComputeCS.Components
{
    public static class OutdoorComfort
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static string CreateComfortTask(
            string inputJson,
            string epwFile,
            string method,
            List<string> probes,
            List<string> thresholds,
            List<int> cpus,
            string dependentOn = "Probe",
            bool create = false)
        {
            var inputData = new Inputs().FromJson(inputJson);
            var tokens = inputData.Auth;
            var parentTask = inputData.Task;
            var subTasks = inputData.SubTasks;
            var postProcessTask =
                TaskUtils.GetDependentTask(subTasks, dependentOn, inputData.Url, tokens, parentTask.UID);
            var project = inputData.Project;

            if (parentTask?.UID == null)
            {
                return null;
            }

            var taskQueryParams = new Dictionary<string, object>
            {
                {"name", Utils.SnakeCaseToHumanCase(method)},
                {"parent", parentTask.UID},
                {"project", project.UID}
            };

            if (postProcessTask != null)
            {
                taskQueryParams.Add("dependent_on", postProcessTask.UID);
            }

            if (!File.Exists(epwFile))
            {
                throw new Exception("A .epw file is needed to proceed!");
            }

            var epwName = Path.GetFileName(epwFile);

            if (create)
            {
                Compute.UploadEPWFile(tokens, inputData.Url, parentTask.UID, epwFile);
            }


            var _thresholds = thresholds.Select(threshold => new WindThresholds.Threshold().FromJson(threshold))
                .ToList();

            var taskCreateParams = new Dictionary<string, object>
            {
                {
                    "config", new Dictionary<string, object>
                    {
                        {"task_type", "radiance"},
                        {"cmd", "outdoor_comfort"},
                        {"case_dir", "results"},
                        {"epw_file", epwName},
                        {"probes", probes},
                        {"thresholds", _thresholds},
                        {"cpus", cpus},
                        {"comfort_index", method},
                        {"wind_dir", "urbanComfort"}
                    }
                }
            };

            var task = Tasks.GetCreateOrUpdateTask(tokens, inputData.Url, "/api/task/", taskQueryParams,
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

            Logger.Info($"Created Outdoor Comfort task: {task.UID}");

            return inputData.ToJson();
        }

        public static string ReadComfortResults(
            string folder
        )
        {
            return "";
        }
    }
}