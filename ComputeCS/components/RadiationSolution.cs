using System.Collections.Generic;
using System.IO;
using System.Linq;
using ComputeCS.types;
using Newtonsoft.Json;

namespace ComputeCS.Components
{
    public static class RadiationSolution
    {
        public static string Setup(
            string inputJson,
            List<int> cpus,
            string method,
            string caseType,
            List<string> materials,
            string epwFile,
            string overrides = null
        )
        {
            var inputData = new Inputs().FromJson(inputJson);

            var materials_ = materials.Select(material => new RadianceMaterial().FromJson(material)).ToList();
            // Convert overrides to dict
            Dictionary<string, object> overrides_ = null;
            if (overrides != null)
            {
                overrides_ = JsonConvert.DeserializeObject<Dictionary<string, object>>(overrides);
            }

            if (!File.Exists(epwFile))
            {
                throw new FileNotFoundException($"{epwFile} does not exist!");
            }
            
            var solution = new types.RadiationSolution
            {
                CPUs = cpus,
                Method = method,
                CaseType = caseType,
                Materials = materials_,
                EPWFile = epwFile,
                Overrides = overrides_,
            };

            inputData.RadiationSolution = solution;
            return inputData.ToJson();
        }
    }
}