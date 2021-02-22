using System.Collections.Generic;

namespace ComputeCS.types
{
    public class ProbeOverrides : SerializeBase<ProbeOverrides>
    {
        public List<string> Exclude = null;
        public List<string> Include = null;
        public double? Distance = 0.1;
    }
}