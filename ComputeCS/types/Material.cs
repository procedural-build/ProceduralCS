using System.Collections.Generic;
using Newtonsoft.Json;

namespace ComputeCS.types
{
    public class RadianceMaterial : SerializeBase<RadianceMaterial>
    {
        public string Name;
        public string Preset;
        public Dictionary<string, object> Overrides;
    }
}