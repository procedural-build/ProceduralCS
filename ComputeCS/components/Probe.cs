using System.Collections.Generic;
using ComputeCS.Tasks;
using ComputeCS.types;
using Newtonsoft.Json;
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
            Dictionary<string, byte[]> meshFiles,
            string dependentOn = "VirtualWindTunnel",
            string caseDir = "VWT/",
            string overrides = "",
            bool create = false
        )
        {
            var inputData = new Inputs().FromJson(inputJson);
            var tokens = inputData.Auth;
            var parentTask = inputData.Task;
            var subTasks = inputData.SubTasks;
            var simulationTask = Tasks.Utils.GetDependentTask(subTasks, dependentOn,
                inputData.Url, tokens, parentTask.UID);
            var project = inputData.Project;
            var probeConfig = new ProbeConfig
            {
                Overrides = JsonConvert.DeserializeObject<Dictionary<string, object>>(overrides),
                Fields = fields,
                SampleSets = GenerateSampleSet(names)
            };
            inputData.ProbeConfig = probeConfig;
            
            if (parentTask.UID == null)
            {
                Logger.Info("Did not find any parent task.");
                return null;
            }

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
                Upload.UploadPointsFiles(tokens, inputData.Url, parentTask.UID, names, points, caseDir);
                Upload.UploadMeshFile(tokens, inputData.Url, parentTask.UID, meshFiles, "geometry");
            }

            if (inputData.SubTasks == null)
            {
                inputData.SubTasks = new List<Task>();
            }
            
            if (inputData.CFDSolution?.CaseType.ToLower() == "meshindependencestudy" || (probeConfig.Overrides != null && probeConfig.Overrides.ContainsKey("mesh_independence_study") && (bool)probeConfig.Overrides["mesh_independence_study"]))
            {
                probeConfig.MeshIndependenceStudy = true;
                var tasks = Tasks.Probe.CreateMeshIndependenceProbeTask(inputData, cpus, probeConfig.SampleSets, fields, probeConfig.Overrides, create);
                inputData.SubTasks.AddRange(tasks);
            }
            else
            {
                var task = Tasks.Probe.CreateProbeTask(tokens, inputData.Url, project.UID, taskQueryParams, caseDir,
                    cpus,
                    probeConfig.SampleSets, fields, probeConfig.Overrides, create: create);
                inputData.SubTasks.Add(task);
            }
            
            return inputData.ToJson();
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
                Upload.UploadPointsFiles(tokens, inputData.Url, parentTask.UID, names, points, uploadDir);
                Upload.UploadPointsFiles(tokens, inputData.Url, parentTask.UID, names, normals, uploadDir, "nls");
                Upload.UploadMeshFile(tokens, inputData.Url, parentTask.UID, meshFiles, uploadDir);
            }

            var task = Tasks.Probe.CreateRadiationProbeTask(tokens, inputData.Url, project.UID, taskQueryParams, caseDir,
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
        
    }
}