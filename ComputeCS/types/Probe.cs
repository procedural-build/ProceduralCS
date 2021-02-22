using System.Collections.Generic;

namespace ComputeCS.types
{
    public class ProbeOverrides : SerializeBase<ProbeOverrides>
    {
        public List<string> Exclude;
        public List<string> Include;
        public double? Distance;
    }
}