using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ComputeCS.Components
{
    public static class RadiationProbeResult
    {
        public static Dictionary<string, IEnumerable<object>> ReadResults(string folder)
        {
            var data = new Dictionary<string, IEnumerable<object>>();
            foreach (var file in Directory.GetFiles(folder))
            {
                if (file.EndsWith(".da"))
                {
                    var values = ReadMetricData(file);
                    var name = Path.GetFileName(file).Replace(".da", "");
                    data.Add(name, values);
                }
                else if (file.EndsWith(".svf"))
                {
                    var values = ReadSkyViewFactorData(file);
                    var name = Path.GetFileName(file).Replace(".svf", "");
                    data.Add(name, values);
                }

            }

            return data;
        }

        private static IEnumerable<object> ReadMetricData(string file)
        {
            var lines = File.ReadAllLines(file);

            return (from line in lines where !line.StartsWith("#") select double.Parse(line)*100).Cast<object>();
        }
        
        private static IEnumerable<object> ReadSkyViewFactorData(string file)
        {
            var lines = File.ReadAllLines(file);

            return lines.Select(elem => (object)double.Parse(elem));
        }
    }
}