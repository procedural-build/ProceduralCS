using System;
using System.Collections.Generic;
using System.Text;
using ComputeCS.types;
using Newtonsoft.Json.Linq;

namespace ComputeCS.Components
{
    public static class WindThreshold
    {
        public static string ComputeWindThresholds(
            string inputJson,
            string epwFile,
            List<string> patches,
            List<int> cpus,
            string dependentOn = "",
            bool create = false
        )
        {
            var inputData = new Inputs().FromJson(inputJson);
            var tokens = inputData.Auth;
            var parentTask = inputData.Task;
            var subTasks = inputData.SubTasks;
            var postProcessTask = GetPostProcessTask(subTasks, dependentOn);
            var project = inputData.Project;
            
            if (parentTask == null) {return null;}

            var epwName = "";

            if (create)
            {
                var epwFileContent = new Byte();
                {
                    // Upload EPW File to parent task
                    new GenericViewSet<Dictionary<string, object>>(
                        tokens,
                        inputData.Url,
                        $"/api/task/{parentTask.UID}/file/WeatherFiles/{epwName}/"
                    ).Update(
                        null,
                        new Dictionary<string, object>
                        {
                            {"file", epwFileContent}
                        }
                    );
                }
            }

            
            var task = new GenericViewSet<Task>(
                tokens,
                inputData.Url,
                $"/api/project/{project.UID}/task/"
            ).GetOrCreate(
                new Dictionary<string, object> {
                    {"name", "Wind Thresholds"},
                    {"parent", parentTask.UID},
                    {"dependent_on", postProcessTask.UID}
                },
                new Dictionary<string, object> {
                    {"config", new Dictionary<string, object> {
                        {"task_type", "cfd"},
                        {"cmd", "run_wind_thresholds"},
                        {"case_dir", "VWT/" },
                        {"epw_file", $"WeatherFiles/{epwName}"},
                        {"patches", patches},
                        {"set_foam_patch_fields", false},
                        {"cpus", cpus },
                    }}
                },
                create
            );

            inputData.SubTasks.Add(task);

            return inputData.ToJson();
        }
        
        private static Task GetPostProcessTask(
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
                if (subTask.Name == "PostProcess")
                {
                    return subTask;
                }
                if (subTask.Config.ContainsKey("commands"))
                {
                    // Have to do this conversion to be able to compare the strings
                    foreach (var cmd in ((JArray) subTask.Config["commands"]).ToObject<List<string>>())
                    {
                        if (cmd.StartsWith("postProcess"))
                        {
                            return subTask;
                        }
                    }
                } 
            }
            return null;
        }
    }
}