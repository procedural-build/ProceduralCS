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
            List<string> thresholds,
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

            if (parentTask?.UID == null)
            {
                return null;
            }

            var taskQueryParams = new Dictionary<string, object>
            {
                {"name", "Wind Thresholds"},
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
                var epwFileContent = File.ReadAllBytes(epwFile);
                {
                    // Upload EPW File to parent task
                    var uploadTask = new GenericViewSet<Dictionary<string, object>>(
                        tokens,
                        inputData.Url,
                        $"/api/task/{parentTask.UID}/file/WeatherFiles/{epwName}"
                    ).Update(
                        null,
                        new Dictionary<string, object>
                        {
                            {"file", epwFileContent}
                        }
                    );
                    if (uploadTask.ContainsKey("error_messages"))
                    {
                        throw new Exception(uploadTask["error_messages"].ToString());
                    }

                }
            }


            var _thresholds = thresholds.Select(threshold => new WindThresholds.Threshold().FromJson(threshold))
                .ToList();

            var taskCreateParams = new Dictionary<string, object>
            {
                {
                    "config", new Dictionary<string, object>
                    {
                        {"task_type", "cfd"},
                        {"cmd", "run_wind_thresholds"},
                        {"case_dir", "VWT/"},
                        {"epw_file", $"WeatherFiles/{epwName}"},
                        {"patches", patches},
                        {"thresholds", _thresholds},
                        {"set_foam_patch_fields", false},
                        {"cpus", cpus},
                    }
                }
            };

            var task = Tasks.GetCreateOrUpdateTask(tokens, inputData.Url, "/api/task/", taskQueryParams,
                taskCreateParams, create);

            if (task.ErrorMessages != null)
            {
                throw new Exception(task.ErrorMessages[0]);
            }

            inputData.SubTasks.Add(task);

            return inputData.ToJson();
        }

        private static Task GetPostProcessTask(
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

                if (subTask.Name == "PostProcess")
                {
                    return subTask;
                }
                
                if (subTask.Config == null){continue;}

                if (subTask.Config.ContainsKey("commands"))
                {
                    // Have to do this conversion to be able to compare the strings
                    try
                    {
                        if (((JArray) subTask.Config["commands"]).ToObject<List<string>>()
                            .Any(cmd => cmd.StartsWith("postProcess")))
                        {
                            return subTask;
                        }
                    }
                    catch (Exception e)
                    {
                        Console.Write(e.Message);
                    }
                }
            }

            return null;
        }

        public static Dictionary<string, Dictionary<string, object>> ReadThresholdResults(
            string folder)
        {
            var data = new Dictionary<string, Dictionary<string, object>>();
            var resultFileTypes = ReadThresholdFileTypes(folder);

            foreach (var file in Directory.GetFiles(folder))
            {
                if (resultFileTypes.Contains(Path.GetExtension(file).Replace(".", "")))
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

        private static List<List<double>> ReadThresholdData(string file)
        {
            var data = new List<List<double>>();
            var lines = File.ReadAllLines(file);
            foreach (var line in lines)
            {
                data.Add(line.Split(',').Skip(1).Select(x => double.Parse(x)).ToList());
            }

            return data;
        }

        public static Dictionary<string, Dictionary<string, List<int>>> LawsonsCriteria(
            Dictionary<string, Dictionary<string, object>> data,
            List<WindThresholds.Threshold> thresholds
        )
        {

            var newData = new Dictionary<string, Dictionary<string, object>>();
            foreach (var threshold in thresholds)
            {
                foreach (var patchName in data[threshold.Field].Keys)
                {
                    if (!newData.ContainsKey(patchName))
                    {
                        newData.Add(patchName, new Dictionary<string, object>());
                    }

                    newData[patchName].Add(threshold.Field, data[threshold.Field][patchName]);
                }
            }

            var output = new Dictionary<string, Dictionary<string, List<int>>>();
            foreach (var patchKey in newData.Keys)
            {
                output = ComputeLawson(newData[patchKey], patchKey, output, thresholds);
            }

            return output;
        }

        private static Dictionary<string, Dictionary<string, List<int>>> ComputeLawson(
            Dictionary<string, object> patchValues,
            string patchKey,
            Dictionary<string, Dictionary<string, List<int>>> output,
            List<WindThresholds.Threshold> thresholds
        )
        {
            var seasons = ThresholdSeasons();

            // we get data in form {dinning: [point1: [winter, spring, summer, fall], point2: [...], ...], "sitting": [...], ...}
            // return should be:
            // {"winter": [point1, point2, ...], "spring": [point1, point2, ...], ...}
            foreach (var resultKey in patchValues.Keys)
            {
                var thresholdFrequency = thresholds.First(threshold => threshold.Field == resultKey).Value/100;
                var pointValues = (List<List<double>>) patchValues[resultKey];
                var pointLength = pointValues.Count();
                var pointIndex = 0;
                foreach (var pointValue in pointValues)
                {
                    var seasonCounter = 0;
                    foreach (var value in pointValue)
                    {
                        var seasonKey = seasons[seasonCounter];
                        if (!output.ContainsKey(seasonKey))
                        {
                            output.Add(seasonKey, new Dictionary<string, List<int>>());
                        }

                        if (!output[seasonKey].ContainsKey(patchKey))
                        {
                            output[seasonKey].Add(patchKey, new List<int>());
                        }

                        if (output[seasonKey][patchKey].Count() < pointLength)
                        {
                            output[seasonKey][patchKey].Add(Convert.ToInt32(value > thresholdFrequency));
                        }
                        else
                        {
                            output[seasonKey][patchKey][pointIndex] += Convert.ToInt32(value > thresholdFrequency);
                        }

                        seasonCounter++;
                    }

                    pointIndex++;
                }
            }

            return output;
        }

        private static List<string> ReadThresholdFileTypes(string folder)
        {
            var allFiles = Directory.GetFiles(Path.Combine(folder, "0")).ToList();
            return allFiles.Select(file => Path.GetFileName(file)).ToList();
        }

        public static List<string> ThresholdSeasons()
        {
            return new List<string>
            {
                "winter_morning", "winter_noon", "winter_afternoon", "winter_evenings",
                "spring_morning", "spring_noon", "spring_afternoon", "spring_evenings",
                "summer_morning", "summer_noon", "summer_afternoon", "summer_evenings",
                "fall_morning", "fall_noon", "fall_afternoon", "fall_evenings",
                "yearly"
            };
        }
        
    }
}