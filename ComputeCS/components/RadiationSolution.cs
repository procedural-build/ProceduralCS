using System.Collections.Generic;
using System.Linq;
using ComputeCS.types;
using Newtonsoft.Json;

namespace ComputeCS.Components
{
    public static class RadiationSolution
    {
        public static string Setup(
            string inputJson,
            List<string> meshes,
            List<int> cpus,
            string method,
            string caseType,
            List<string> materials,
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
            
            var solution = new types.RadiationSolution
            {
                CPUs = cpus,
                MeshIds = meshes,
                Method = method,
                CaseType = caseType,
                Materials = materials_,
                Overrides = overrides_,
            };

            inputData.RadiationSolution = solution;
            return inputData.ToJson();
        }
    }
}