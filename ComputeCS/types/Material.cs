using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace ComputeCS.types
{
    public class RadianceMaterial : SerializeBase<RadianceMaterial>
    {
        public string Name;
        public string Preset;
        public MaterialOverrides Overrides;
    }

    public class MaterialOverrides : SerializeBase<MaterialOverrides>
    {
        public List<double> Reals;
        public List<int> Integers;
        public List<string> Strings;
        public string Identifier;
        public string Type;
        public string Modifier;
        public string Normal;

        public string BSDFPath
        {
            get => bsdf;
            set => bsdf = CheckValidFile(value);
        }
        private string bsdf;
        
        [JsonProperty("bsdf")]
        public string BSDF => Path.GetFileName(bsdf);

        private static string CheckValidFile(string path)
        {
            if (!File.Exists(path) && path != "clear.xml")
            {
                throw new FileNotFoundException($"Could not find {path}. Please provide a valid BSDF file.");
            }

            return path;
        }
    }
}