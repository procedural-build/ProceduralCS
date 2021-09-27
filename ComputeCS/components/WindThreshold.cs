using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ComputeCS.types;
using Newtonsoft.Json.Linq;
using NLog;

namespace ComputeCS.Components
{
    public static class WindThreshold
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static string ComputeWindThresholds(
            string inputJson,
            string epwFile,
            List<string> patches,
            List<string> thresholds,
            List<int> cpus,
            string dependentOn = "Probe",
            bool create = false
        )
        {
            var inputData = new Inputs().FromJson(inputJson);
            var tokens = inputData.Auth;
            var parentTask = inputData.Task;
            var subTasks = inputData.SubTasks;
            var postProcessTask =
                Tasks.Utils.GetDependentTask(subTasks, dependentOn, inputData.Url, tokens, parentTask.UID);
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

            if (create)
            {
                Tasks.Upload.UploadEPWFile(tokens, inputData.Url, parentTask.UID, epwFile);
            }

            var _thresholds = thresholds.Select(threshold => new Thresholds.WindThreshold().FromJson(threshold))
                .ToList();
            
            var epwName = Path.GetFileName(epwFile);
            var taskCreateParams = new Dictionary<string, object>
            {
                {
                    "config", new Dictionary<string, object>
                    {
                        {"task_type", "cfd"},
                        {"cmd", "run_wind_thresholds"},
                        {"case_dir", "VWT/"},
                        {"epw_file", $"weather/{epwName}"},
                        {"patches", patches},
                        {"thresholds", _thresholds},
                        {"set_foam_patch_fields", false},
                        {"cpus", cpus},
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

            Logger.Info($"Created Wind Threshold task: {task.UID}");

            return inputData.ToJson();
        }

        public static Dictionary<string, Dictionary<string, object>> ReadThresholdResults(
            string folder)
        {
            var data = new Dictionary<string, Dictionary<string, object>>();
            var resultFileTypes = ReadThresholdFileTypes(folder);

            foreach (var file in Directory.GetFiles(folder))
            {
                var extension = Path.GetExtension(file).Replace(".", "");
                if (!resultFileTypes.Contains(extension)) continue;

                var patchName = Path.GetFileNameWithoutExtension(file);
                var values = ReadThresholdData(file);

                if (!data.ContainsKey(extension))
                {
                    data.Add(extension, new Dictionary<string, object>());
                }

                data[extension].Add(patchName, values);
            }

            return data;
        }

        public static List<List<double>> ReadThresholdData(string file, int skip=1)
        {
            var lines = File.ReadAllLines(file);
            return lines.Select(line => line.Split(',').Skip(skip).Select(double.Parse).ToList()).ToList();
        }

        public static Dictionary<string, Dictionary<string, List<int>>> LawsonsCriteria(
            Dictionary<string, Dictionary<string, object>> data,
            List<Thresholds.WindThreshold> thresholds
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
            List<Thresholds.WindThreshold> thresholds
        )
        {
            var seasons = ThresholdSeasons();

            // we get data in form {dinning: [point1: [winter, spring, summer, fall], point2: [...], ...], "sitting": [...], ...}
            // return should be:
            // {"winter": [point1, point2, ...], "spring": [point1, point2, ...], ...}
            foreach (var resultKey in patchValues.Keys)
            {
                var thresholdFrequency = thresholds.First(threshold => threshold.Field == resultKey).Value / 100;
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
            return allFiles.Select(Path.GetFileName).ToList();
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