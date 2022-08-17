using ComputeCS.types;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ComputeCS.Components
{
    public static class RadiationProbeResult
    {
        public static Dictionary<string, Dictionary<string, IEnumerable<object>>> ReadResults(IEnumerable<DownloadFile> files)
        {
            var data = new Dictionary<string, Dictionary<string, IEnumerable<object>>>();
            foreach (var file in files)
            {
                var filePathUnix = file.FilePathUnix;
                var name = Path.GetFileNameWithoutExtension(filePathUnix);

                if (!data.ContainsKey(name))
                {
                    data.Add(name, new Dictionary<string, IEnumerable<object>>());
                }

                if (file.FilePathUnix.EndsWith(".da"))
                {
                    var values = ReadMetricData(file);
                    data[name].Add("daylight_autonomy", values);
                }
                else if (file.FilePathUnix.EndsWith(".cda"))
                {
                    var values = ReadMetricData(file);
                    data[name].Add("continuous_daylight_autonomy", values);
                }
                else if (file.FilePathUnix.EndsWith(".sda"))
                {
                    var values = ReadStatisticData(file);
                    data[name].Add("spatial_daylight_autonomy", values);
                }
                else if (file.FilePathUnix.EndsWith(".udi-a"))
                {
                    var values = ReadMetricData(file);
                    data[name].Add("useful_daylight_illuminances_low", values);
                }
                else if (file.FilePathUnix.EndsWith(".udi-b"))
                {
                    var values = ReadMetricData(file);
                    data[name].Add("useful_daylight_illuminances_between", values);
                }
                else if (file.FilePathUnix.EndsWith(".udi-c"))
                {
                    var values = ReadMetricData(file);
                    data[name].Add("useful_daylight_illuminances_high", values);
                }
                else if (file.FilePathUnix.EndsWith(".svf"))
                {
                    var values = ReadFactorData(file);
                    data[name].Add("sky_view_factor", values);
                }
                else if (file.FilePathUnix.EndsWith(".df"))
                {
                    var values = ReadFactorData(file);
                    data[name].Add("daylight_factor", values);
                }
                else if (file.FilePathUnix.EndsWith(".avg"))
                {
                    var values = ReadStatisticData(file);
                    data[name].Add("average", values);
                }
                else if (file.FilePathUnix.EndsWith(".min"))
                {
                    var values = ReadStatisticData(file);
                    data[name].Add("minimum", values);
                }
                else if (file.FilePathUnix.EndsWith(".max"))
                {
                    var values = ReadStatisticData(file);
                    data[name].Add("maximum", values);
                }
                else if (file.FilePathUnix.EndsWith(".std"))
                {
                    var values = ReadStatisticData(file);
                    data[name].Add("standard_deviation", values);
                }
                else if (file.FilePathUnix.EndsWith(".sum"))
                {
                    var values = ReadStatisticData(file);
                    data[name].Add("sum", values);
                }
            }
            return data;
        }

        private static IEnumerable<object> ReadMetricData(DownloadFile file)
        {
            var lines = ReadAllLines(file);

            return (from line in lines where !line.StartsWith("#") select double.Parse(line) * 100).Cast<object>();
        }

        private static IEnumerable<object> ReadStatisticData(DownloadFile file)
        {
            var lines = ReadAllLines(file);

            return (from line in lines where !line.StartsWith("#") select double.Parse(line)).Cast<object>();
        }

        private static IEnumerable<object> ReadFactorData(DownloadFile file)
        {
            var lines = ReadAllLines(file);

            return lines.Select(elem => (object)double.Parse(elem));
        }

        private static IEnumerable<string> ReadAllLines(DownloadFile file)
        {
            using (StringReader reader = new StringReader(System.Text.Encoding.Default.GetString(file.Content)))
            {
                string line = reader.ReadLine();

                while (line != null)
                {
                    yield return line;
                    line = reader.ReadLine();
                }
            }
        }
    }
}