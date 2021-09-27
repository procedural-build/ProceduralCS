using System.Collections.Generic;

namespace ComputeCS.types
{
    public class ProbeResultOverrides : SerializeBase<ProbeResultOverrides>
    {
        public List<string> Exclude;
        public List<string> Include;
        public double? Distance;
        public List<string> Outputs;
    }
    
    public class ProbeConfig : SerializeBase<ProbeConfig>
    {
        public List<Dictionary<string, object>> SampleSets;
        public Dictionary<string, object> Overrides;
        public List<string> Fields;
        public bool MeshIndependenceStudy;
    }
}