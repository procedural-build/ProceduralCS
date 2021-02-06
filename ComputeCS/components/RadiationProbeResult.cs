using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ComputeCS.Components
{
    public static class RadiationProbeResult
    {
        public static Dictionary<string, Dictionary<string, IEnumerable<object>>> ReadResults(string folder)
        {
            var data = new Dictionary<string, Dictionary<string, IEnumerable<object>>>();
            foreach (var file in Directory.GetFiles(folder))
            {
                if (file.EndsWith(".da"))
                {
                    var values = ReadMetricData(file);
                    var name = Path.GetFileName(file).Replace(".da", "");
                    if (!data.ContainsKey(name))
                    {
                        data.Add(name, new Dictionary<string, IEnumerable<object>>());
                    }
                    data[name].Add("daylight_autonomy", values);
                }
                if (file.EndsWith(".cda"))
                {
                    var values = ReadMetricData(file);
                    var name = Path.GetFileName(file).Replace(".cda", "");
                    if (!data.ContainsKey(name))
                    {
                        data.Add(name, new Dictionary<string, IEnumerable<object>>());
                    }
                    data[name].Add("continuous_daylight_autonomy", values);
                }
                if (file.EndsWith(".sda"))
                {
                    var values = ReadStatisticData(file);
                    var name = Path.GetFileName(file).Replace(".sda", "");
                    if (!data.ContainsKey(name))
                    {
                        data.Add(name, new Dictionary<string, IEnumerable<object>>());
                    }
                    data[name].Add("spatial_daylight_autonomy", values);
                }
                if (file.EndsWith(".udi-a"))
                {
                    var values = ReadMetricData(file);
                    var name = Path.GetFileName(file).Replace(".udi-a", "");
                    if (!data.ContainsKey(name))
                    {
                        data.Add(name, new Dictionary<string, IEnumerable<object>>());
                    }
                    data[name].Add("useful_daylight_illuminances_low", values);
                }
                if (file.EndsWith(".udi-b"))
                {
                    var values = ReadMetricData(file);
                    var name = Path.GetFileName(file).Replace(".udi-b", "");
                    if (!data.ContainsKey(name))
                    {
                        data.Add(name, new Dictionary<string, IEnumerable<object>>());
                    }
                    data[name].Add("useful_daylight_illuminances_between", values);
                }
                if (file.EndsWith(".udi-c"))
                {
                    var values = ReadMetricData(file);
                    var name = Path.GetFileName(file).Replace(".udi-c", "");
                    if (!data.ContainsKey(name))
                    {
                        data.Add(name, new Dictionary<string, IEnumerable<object>>());
                    }
                    data[name].Add("useful_daylight_illuminances_high", values);
                }
                else if (file.EndsWith(".svf"))
                {
                    var values = ReadFactorData(file);
                    var name = Path.GetFileName(file).Replace(".svf", "");
                    if (!data.ContainsKey(name))
                    {
                        data.Add(name, new Dictionary<string, IEnumerable<object>>());
                    }
                    data[name].Add("sky_view_factor", values);
                }
                else if (file.EndsWith(".df"))
                {
                    var values = ReadFactorData(file);
                    var name = Path.GetFileName(file).Replace(".df", "");
                    if (!data.ContainsKey(name))
                    {
                        data.Add(name, new Dictionary<string, IEnumerable<object>>());
                    }
                    data[name].Add("daylight_factor", values);
                }
                else if (file.EndsWith(".avg"))
                {
                    var values = ReadStatisticData(file);
                    var name = Path.GetFileName(file).Replace(".avg", "");
                    if (!data.ContainsKey(name))
                    {
                        data.Add(name, new Dictionary<string, IEnumerable<object>>());
                    }
                    data[name].Add("average", values);
                }
                else if (file.EndsWith(".min"))
                {
                    var values = ReadStatisticData(file);
                    var name = Path.GetFileName(file).Replace(".min", "");
                    if (!data.ContainsKey(name))
                    {
                        data.Add(name, new Dictionary<string, IEnumerable<object>>());
                    }
                    data[name].Add("minimum", values);
                }
                else if (file.EndsWith(".max"))
                {
                    var values = ReadStatisticData(file);
                    var name = Path.GetFileName(file).Replace(".max", "");
                    if (!data.ContainsKey(name))
                    {
                        data.Add(name, new Dictionary<string, IEnumerable<object>>());
                    }
                    data[name].Add("maximum", values);
                }
                else if (file.EndsWith(".std"))
                {
                    var values = ReadStatisticData(file);
                    var name = Path.GetFileName(file).Replace(".std", "");
                    if (!data.ContainsKey(name))
                    {
                        data.Add(name, new Dictionary<string, IEnumerable<object>>());
                    }
                    data[name].Add("standard_deviation", values);
                }
                else if (file.EndsWith(".sum"))
                {
                    var values = ReadStatisticData(file);
                    var name = Path.GetFileName(file).Replace(".sum", "");
                    if (!data.ContainsKey(name))
                    {
                        data.Add(name, new Dictionary<string, IEnumerable<object>>());
                    }
                    data[name].Add("sum", values);
                }
            }

            return data;
        }

        private static IEnumerable<object> ReadMetricData(string file)
        {
            var lines = File.ReadAllLines(file);

            return (from line in lines where !line.StartsWith("#") select double.Parse(line)*100).Cast<object>();
        }
        
        private static IEnumerable<object> ReadStatisticData(string file)
        {
            var lines = File.ReadAllLines(file);

            return (from line in lines where !line.StartsWith("#") select double.Parse(line)).Cast<object>();
        }
        
        private static IEnumerable<object> ReadFactorData(string file)
        {
            var lines = File.ReadAllLines(file);

            return lines.Select(elem => (object)double.Parse(elem));
        }
    }
}