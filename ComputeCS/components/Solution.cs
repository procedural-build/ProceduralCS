using System;
using System.Collections.Generic;
using System.Linq;
using ComputeCS.types;
using Newtonsoft.Json;

namespace ComputeCS.Components
{
    public static class CFDSolution
    {
        public static string Setup(
            string inputJson,
            List<int> cpus,
            string solver,
            string caseType,
            List<string> boundaryConditions,
            string iterations,
            List<List<double>> numberOfAngles,
            string overrides = null,
            List<string> files = null
        )
        {
            var inputData = new Inputs().FromJson(inputJson);

            // Merge the list of dictionaries to one big dict
            var bcs_ = new List<Dictionary<string, object>>();
            foreach (var bc in boundaryConditions) { bcs_.Add(JsonConvert.DeserializeObject<Dictionary<string, object>>(bc)); }
            
            var bcs = bcs_.SelectMany(x => x).GroupBy(d => d.Key).ToDictionary(x => x.Key, y => y.First().Value);

            // Convert iterations from json to dict
            var _iterations = JsonConvert.DeserializeObject<Dictionary<string, int>>(iterations);

            // Convert overrides to dict
            Dictionary<string, object> _overrides = null;
            if (overrides != null)
            {
                _overrides = JsonConvert.DeserializeObject<Dictionary<string, object>>(overrides);
            }
            
            // Convert overrides to dict
            var _files = new List<Dictionary<string, object>>();
            if (files != null)
            {
                _files.AddRange(files.Select(file => JsonConvert.DeserializeObject<Dictionary<string, object>>(file)));
            }
            
            var solution = new types.CFDSolution
            {
                CPUs = cpus,
                Solver = solver,
                CaseType = caseType,
                BoundaryConditions = bcs,
                Iterations = _iterations,
                Angles = caseType == "VirtualWindTunnel"? GetAngleListFromNumber(numberOfAngles, _overrides): null,
                Overrides = _overrides,
                Files = _files
            };

            inputData.CFDSolution = solution;
            return inputData.ToJson();
        }

        private static List<object> GetAngleListFromNumber(
            List<List<double>> angles,
            Dictionary<string, object> overrides
        )
        {
            if (overrides != null && overrides.ContainsKey("single_angle") && (bool) overrides["single_angle"])
            {
                return new List<object> {angles.First().First()};
            }
            
            switch (angles.Count)
            {
                case 1 when angles.First().Count == 1:
                {
                    var numberOfAngles = (int)angles.First().First();
                    return Enumerable.Range(0, numberOfAngles).Select(index => (object)(index * 360 / numberOfAngles)).ToList();
                }
                case 1:
                    return angles.First().Select(angle => (object)angle).ToList();
            }

            return angles.Select(angle => (object)angle).ToList();

        }
        
    }
}