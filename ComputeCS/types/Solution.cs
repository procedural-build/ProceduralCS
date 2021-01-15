using System.Collections.Generic;
using Newtonsoft.Json;

namespace ComputeCS.types
{
    public class CFDSolution
    {
        [JsonProperty("cpus")] public List<int> CPUs;
        public string Solver;
        public string CaseType;
        public Dictionary<string, object> BoundaryConditions;
        public Dictionary<string, int> Iterations;
        public List<double> Angles;
        public Dictionary<string, object> Overrides;
        public List<Dictionary<string, object>> Files;
    }

    public class RadiationSolution
    {
        [JsonProperty("cpus")] public List<int> CPUs;

        public string Method;
        public string CaseType;
        public List<RadianceMaterial> Materials;
        public Dictionary<string, object> Overrides;
    }
}