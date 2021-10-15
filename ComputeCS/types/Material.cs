using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace ComputeCS.types
{
    public class RadianceMaterial : SerializeBase<RadianceMaterial>
    {
        public string Name
        {
            get => name;
            set => name = UpdateName(value);
        }
        public string Preset;
        public MaterialOverrides Overrides;

        private string name;

        private static string UpdateName(string _name)
        {
            if (_name.EndsWith("*"))
            {
                return _name;
            }

            return _name + "*";
        }
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
        public List<List<bool>> ScheduleValues;
        public List<string> Schedules;

        public List<string> BSDFPath
        {
            get => bsdfs;
            set => bsdfs = value?.Select(CheckValidFile).ToList();
        }
        private List<string> bsdfs;
        
        [JsonProperty("bsdf")]
        public List<string> BSDF => bsdfs?.Select(Path.GetFileName).ToList();

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