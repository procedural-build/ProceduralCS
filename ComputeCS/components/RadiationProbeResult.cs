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
                if (!file.EndsWith(".da"))
                {
                    continue;
                }
                var values = ReadMetricData(file);
                var name = Path.GetFileName(file).Replace(".da", "");
                data.Add(name, values);
            }

            return data;
        }

        private static IEnumerable<object> ReadMetricData(string file)
        {
            var lines = File.ReadAllLines(file);

            return (from line in lines where !line.StartsWith("#") select line);
        }
    }
}