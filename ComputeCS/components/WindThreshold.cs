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

            if (parentTask == null)
            {
                return null;
            }
            
            dependentOn = null;
            if (postProcessTask != null)
            {
                dependentOn = postProcessTask.UID;
            }

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
                new Dictionary<string, object>
                {
                    {"name", "Wind Thresholds"},
                    {"parent", parentTask.UID},
                    {"dependent_on", dependentOn}
                },
                new Dictionary<string, object>
                {
                    {
                        "config", new Dictionary<string, object>
                        {
                            {"task_type", "cfd"},
                            {"cmd", "run_wind_thresholds"},
                            {"case_dir", "VWT/"},
                            {"epw_file", $"WeatherFiles/{epwName}"},
                            {"patches", patches},
                            {"set_foam_patch_fields", false},
                            {"cpus", cpus},
                        }
                    }
                },
                create
            );

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
            foreach (Task subTask in subTasks)
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
            Dictionary<string, Dictionary<string, object>> data
            )
        {
            var resultTypes = new List<string>
            {
                "dining", "sitting", "walkthru"
            };

            var newData = new Dictionary<string, Dictionary<string, object>>();
            foreach (var resultType in resultTypes)
            {
                foreach (var patchName in data[resultType].Keys)
                {
                    if (!newData.ContainsKey(patchName))
                    {
                        newData.Add(patchName, new Dictionary<string, object>());
                    }

                    newData[patchName].Add(resultType, data[resultType][patchName]);
                }
            }

            var output = new Dictionary<string, Dictionary<string, List<int>>>();
            foreach (var patchKey in newData.Keys)
            {
                output = ComputeLawson(newData[patchKey], patchKey, output);
            }

            return output;
        }

        private static Dictionary<string, Dictionary<string, List<int>>> ComputeLawson(
            Dictionary<string, object> patchValues,
            string patchKey,
            Dictionary<string, Dictionary<string, List<int>>> output
        )
        {
            var seasons = new List<string>
            {
                "winter_morning", "winter_noon", "winter_afternoon", "winter_evenings",
                "spring_morning", "spring_noon", "spring_afternoon", "spring_evenings",
                "summer_morning", "summer_noon", "summer_afternoon", "summer_evenings",
                "fall_morning", "fall_noon", "fall_afternoon", "fall_evenings",
                "yearly"
            };

            // we get data in form {dinning: [point1: [winter, spring, summer, fall], point2: [...], ...], "sitting": [...], ...}
            // return should be:
            // {"winter": [point1, point2, ...], "spring": [point1, point2, ...], ...}
            foreach (var resultKey in patchValues.Keys)
            {
                var pointValues = (List<List<double>>) patchValues[resultKey];
                var pointLenght = pointValues.Count();
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

                        if (output[seasonKey][patchKey].Count() < pointLenght)
                        {
                            output[seasonKey][patchKey].Add(Convert.ToInt32(value > 0.05));
                        }
                        else
                        {
                            output[seasonKey][patchKey][pointIndex] += Convert.ToInt32(value > 0.05);
                        }
                        
                        seasonCounter++;
                    }

                    pointIndex++;
                }
            }

            return output;
        }
    }
}