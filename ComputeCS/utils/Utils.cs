using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;


namespace ComputeCS
{
    public static class Utils
    {
        public static string DictToQueryString(Dictionary<string, object> dict)
        {
            string query_string = "";
            foreach (string key in dict.Keys)
            {
                string var_str = Convert.ToString(dict[key]);
                if (var_str != "")
                {
                    query_string += $"&{key}={var_str}";
                }
            }

            query_string = (query_string != "") ? $"?{query_string.Substring(1)}" : "";
            return query_string;
        }

        public static Dictionary<string, string> DeserializeJsonString(string json_string)
        {
            return JsonConvert.DeserializeObject<Dictionary<string, string>>(json_string);
        }

        public static string SnakeCaseToHumanCase(string name)
        {
            return string.Join(" ",
                name.Split('_').Select(word => word.First().ToString().ToUpper() + word.Substring(1)));
        }
    }
}