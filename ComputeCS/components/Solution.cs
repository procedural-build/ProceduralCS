using System;
using System.Collections.Generic;
using System.Linq;
using ComputeCS.types;
using Newtonsoft.Json;

namespace ComputeCS.Components
{
    public static class CFDSolution
    {
        public static Dictionary<string, object> Setup(
            string inputJson,
            List<int> cpus,
            string solver,
            string caseType,
            List<string> boundaryConditions,
            string iterations,
            int numberOfAngles,
            string overrides = null
        )
        {
            var inputData = new Inputs().FromJson(inputJson);

            // Merge the list of dictionaries to one big dict
            var bcs_ = new List<Dictionary<string, object>>();
            foreach (string bc in boundaryConditions) { bcs_.Add(JsonConvert.DeserializeObject<Dictionary<string, object>>(bc)); }
            
            Dictionary<string, object> bcs = bcs_.SelectMany(x => x).GroupBy(d => d.Key).ToDictionary(x => x.Key, y => y.First().Value);

            // Convert iterations from json to dict
            var iterations_ = JsonConvert.DeserializeObject<Dictionary<string, int>>(iterations);

            // Convert overrides to dict
            Dictionary<string, object> overrides_ = null;
            if (overrides != null)
            {
                overrides_ = JsonConvert.DeserializeObject<Dictionary<string, object>>(overrides);
            }
            
            var solution = new types.CFDSolution
            {
                CPUs = cpus,
                Solver = solver,
                CaseType = caseType,
                BoundaryConditions = bcs,
                Iterations = iterations_,
                Angles = GetAngleListFromNumber(numberOfAngles),
                Overrides = overrides_
            };

            inputData.CFDSolution = solution;
            var output = inputData.ToJson();

            return new Dictionary<string, object>
            {
                {"out", output}
            };
        }

        static List<double> GetAngleListFromNumber(
            int numberOfAngles
        )
        {
            return Enumerable.Range(0, numberOfAngles).Select(index => (double)index * 360 / numberOfAngles).ToList();
        }
    }
}