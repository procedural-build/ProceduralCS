using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

            if (!File.Exists(epwFile))
            {
                return null;
            }
            var epwName = Path.GetFileName(epwFile);

            if (create)
            {
                var epwFileContent = File.ReadAllBytes(epwFile);
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

        public static Dictionary<string, Dictionary<string, object>> ReadThresholdResults(
            string folder)
        {
            var data = new Dictionary<string, Dictionary<string, object>>();
            var resultFileType = new List<string>
            {
                "dining", "sitting", "Uav", "Uav_std", "walkthru"
            };

            foreach (var file in Directory.GetFiles(folder))
            {
                
                if (resultFileType.Contains(Path.GetExtension(file).Replace(".", "")))
                {
                    var fileName = Path.GetFileName(file).Split('.');
                    var extension = fileName[1];
                    var patchName = fileName[0];
                    var values = ReadThresholdData(file);

                    if (!data.ContainsKey(extension))
                    {
                        data.Add(extension, new Dictionary<string, object>());
                    }

                    data[extension].Add(patchName, values);
                }
            }

            return data;
        }

        private static List<double> ReadThresholdData(string file)
        {
            var data = new List<double>();
            var lines = File.ReadAllLines(file);
            foreach (var line in lines)
            {
                var data_ = line.Split(',').Skip(1).Select(x => double.Parse(x)).ToList();
                data.Add(data_.Average());
                
            }

            return data;
        }
    }
}