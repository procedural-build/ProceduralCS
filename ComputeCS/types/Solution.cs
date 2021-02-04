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
        [JsonProperty("epw_file")] public string EPWFile;
        public RadiationSolutionOverrides Overrides;
        public Dictionary<string, int> Probes;
    }

    public class RadiationSolutionOverrides : SerializeBase<RadiationSolutionOverrides>
    {
        public uint AmbientBounces;
        public uint AmbientDivisions;
        public double AmbientAccuracy;
        public uint AmbientSamples;
        public uint AmbientResolution;
        public double DirectCertainty;
        public double DirectSampling;
        public double DirectThreshold;
        public uint LimitReflections;
        public double LimitRayWeight;
        public uint Samples;
        public uint SamplingThreshold;
        public uint SecondaryPresampling;
        public uint SecondaryRelay;
        public uint ReinhartDivisions;
    }
}
