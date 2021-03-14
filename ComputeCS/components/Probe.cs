using System;
using System.Collections.Generic;
using System.Linq;
using ComputeCS.types;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;

namespace ComputeCS.Components
{
    public static class Probe
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static string ProbePoints(
            string inputJson,
            List<List<List<double>>> points,
            List<string> fields,
            List<string> names,
            List<int> cpus,
            string dependentOn = "",
            string caseDir = "VWT/",
            string overrides = "",
            bool create = false
        )
        {
            var inputData = new Inputs().FromJson(inputJson);
            var tokens = inputData.Auth;
            var parentTask = inputData.Task;
            var subTasks = inputData.SubTasks;
            var simulationTask = GetSimulationTask(subTasks, dependentOn);
            var project = inputData.Project;

            if (parentTask == null)
            {
                Logger.Info("Did not find any parent task.");
                return null;
            }

            var sampleSets = GenerateSampleSet(names);

            var taskQueryParams = new Dictionary<string, object>
            {
                {"name", "Probe"},
                {"parent", parentTask.UID},
            };
            if (simulationTask != null)
            {
                taskQueryParams.Add("dependent_on", simulationTask.UID);
            }

            if (create)
            {
                UploadPointsFiles(tokens, inputData.Url, parentTask.UID, names, points, caseDir);
            }

            var task = CreateProbeTask(tokens, inputData.Url, project.UID, taskQueryParams, caseDir, cpus,
                sampleSets, fields, overrides, create);
            if (inputData.SubTasks != null)
            {
                inputData.SubTasks.Add(task);
            }
            else
            {
                inputData.SubTasks = new List<Task> {task};
            }

            Logger.Info($"Created probe task: {task.UID}");
            return inputData.ToJson();
        }

        private static Task GetSimulationTask(
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
                    Logger.Info($"Found simulation task: {subTask.UID}");
                    return subTask;
                }

                if (subTask.Name == "SimpleCase")
                {
                    Logger.Info($"Found simulation task: {subTask.UID}");
                    return subTask;
                }

                if (subTask.Config == null)
                {
                    continue;
                }

                if (subTask.Config.ContainsKey("cmd") && (string) subTask.Config["cmd"] == "wind_tunnel")
                {
                    Logger.Info($"Found simulation task: {subTask.UID}");
                    return subTask;
                }

                if (subTask.Config.ContainsKey("commands"))
                {
                    // Have to do this conversion to be able to compare the strings
                    if (((JArray) subTask.Config["commands"]).ToObject<List<string>>()[0] == "simpleFoam")
                    {
                        Logger.Info($"Found simulation task: {subTask.UID}");
                        return subTask;
                    }
                }
            }

            Logger.Info($"Did not find any simulation task!");
            return null;
        }

        public static List<Dictionary<string, object>> GenerateSampleSet(
            List<string> names,
            bool withNormals = false
        )
        {
            var sampleSets = new List<Dictionary<string, object>>();
            foreach (var name in names)
            {
                var sample = new Dictionary<string, object>()
                {
                    {"name", name},
                    {"file", $"{name}.pts"}
                };
                if (withNormals)
                {
                    sample.Add("normals", $"{name}.nls");
                }

                sampleSets.Add(sample);
            }

            Logger.Info($"Generated sample sets for {names} with normal? {withNormals}");
            Logger.Debug($"Generated sample set: {sampleSets}");
            return sampleSets;
        }

        public static void UploadPointsFiles(
            AuthTokens tokens,
            string url,
            string taskId,
            List<string> names,
            List<List<List<double>>> points,
            string caseDir,
            string fileExtension = "pts"
        )
        {
            var index = 0;
            foreach (var name in names)
            {
                Logger.Info($"Uploading probes from set {name} to the server");
                new GenericViewSet<Dictionary<string, object>>(
                    tokens,
                    url,
                    $"/api/task/{taskId}/file/{caseDir}/{name}.{fileExtension}"
                ).Update(
                    null,
                    new Dictionary<string, object>
                    {
                        {"file", points[index]}
                    }
                );
                index++;
            }
        }

        public static Task CreateProbeTask(
            AuthTokens tokens,
            string url,
            string projectId,
            Dictionary<string, object> taskQueryParams,
            string caseDir,
            List<int> cpus,
            List<Dictionary<string, object>> sampleSets,
            List<string> fields,
            string overrides,
            bool create
        )
        {
            var commands = new List<string> {"write_sample_set"};
            var nCPUs = 1;
            cpus.ForEach(cpu => nCPUs *= cpu);
            if (nCPUs > 1)
            {
                //commands.Add("decomposePar -time :10000");
                commands.Add("!postProcess -func internalCloud");
                //commands.Add("reconstructPar");
            }
            else
            {
                commands.Add("postProcess -func internalCloud");
            }


            var config = new Dictionary<string, object>
            {
                {"task_type", "cfd"},
                {"cmd", "pipeline"},
                {"commands", commands},
                {"case_dir", caseDir},
                {"cpus", cpus},
                {"sets", sampleSets},
                {"fields", fields},
            };
            if (!string.IsNullOrEmpty(overrides))
            {
                var json = JsonConvert.DeserializeObject<Dictionary<string, string>>(overrides);
                config.Add("overrides", json);
            }

            var createParams = new Dictionary<string, object>
            {
                {
                    "config", config
                }
            };

            taskQueryParams.Add("project", projectId);

            var task = Tasks.GetCreateOrUpdateTask(
                tokens,
                url,
                $"/api/task/",
                taskQueryParams,
                createParams,
                create
            );

            return task;
        }

        public static string RadiationProbes(
            string inputJson,
            List<List<List<double>>> points,
            List<List<List<double>>> normals,
            List<string> names,
            Dictionary<string, byte[]> meshFiles,
            bool create = false
        )
        {
            var inputData = new Inputs().FromJson(inputJson);
            var tokens = inputData.Auth;
            var parentTask = inputData.Task;
            var project = inputData.Project;
            var uploadDir = "geometry";
            var caseDir = "probes";

            if (parentTask == null)
            {
                return null;
            }

            var sampleSets = GenerateSampleSet(names, true);

            var taskQueryParams = new Dictionary<string, object>
            {
                {"name", "Probe"},
                {"parent", parentTask.UID},
            };

            if (create)
            {
                UploadPointsFiles(tokens, inputData.Url, parentTask.UID, names, points, uploadDir);
                UploadPointsFiles(tokens, inputData.Url, parentTask.UID, names, normals, uploadDir, "nls");
                UploadMeshFile(tokens, inputData.Url, parentTask.UID, meshFiles, uploadDir);
            }

            var task = CreateRadiationProbeTask(tokens, inputData.Url, project.UID, taskQueryParams, caseDir,
                sampleSets, create);

            inputData.SubTasks = new List<Task> {task};
            var probes = new Dictionary<string, int>();
            for (var i = 0; i < names.Count; i++)
            {
                probes.Add(names[i], points[i].Count);
            }

            inputData.RadiationSolution.Probes = probes;

            return inputData.ToJson();
        }

        public static void UploadMeshFile(
            AuthTokens tokens,
            string url,
            string taskId,
            Dictionary<string, byte[]> meshFiles,
            string caseDir = "geometry"
        )
        {
            foreach (var name in meshFiles.Keys)
            {
                Logger.Info($"Uploading mesh: {name} to the server");
                var upload = new GenericViewSet<Dictionary<string, object>>(
                    tokens,
                    url,
                    $"/api/task/{taskId}/file/{caseDir}/{name}.obj"
                ).Update(
                    null,
                    new Dictionary<string, object>
                    {
                        {"file", meshFiles[name]}
                    }
                );
                if (upload.ContainsKey("errors"))
                {
                    Logger.Error(upload["error"]);
                    throw new Exception($"Got error while uploading mesh to server: {upload["error"]}");
                }

                Logger.Debug($"Uploaded {caseDir}/{name}.obj to server");
            }
        }

        public static Task CreateRadiationProbeTask(
            AuthTokens tokens,
            string url,
            string projectId,
            Dictionary<string, object> taskQueryParams,
            string caseDir,
            List<Dictionary<string, object>> sampleSets,
            bool create
        )
        {
            var config = new Dictionary<string, object>
            {
                {"task_type", "magpy"},
                {"cmd", "radiance.io.tasks.write_radiation_samples_set"},
                {"case_dir", caseDir},
                {"sets", sampleSets},
            };

            var createParams = new Dictionary<string, object>
            {
                {"config", config},
                {"status", "pending"}
            };

            taskQueryParams.Add("project", projectId);

            var task = Tasks.GetCreateOrUpdateTask(
                tokens,
                url,
                $"/api/task/",
                taskQueryParams,
                createParams,
                create
            );

            if (task.ErrorMessages != null)
            {
                throw new Exception(task.ErrorMessages.First());
            }

            return task;
        }
    }
}