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

            if (!File.Exists(epwFile) && (method == "three_phase" || method == "solar_radiation"))
            {
                throw new FileNotFoundException($"EPW file: {epwFile} does not exist!");
            }
            
            var solution = new types.RadiationSolution
            {
                CPUs = cpus,
                Method = method,
                CaseType = caseType,
                Materials = materials_,
                EPWFile = epwFile,
                Overrides = overrides!=null? new RadiationSolutionOverrides().FromJson(overrides): null,
            };

            inputData.RadiationSolution = solution;
            return inputData.ToJson();
        }
    }
}