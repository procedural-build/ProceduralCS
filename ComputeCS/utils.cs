using System;
using System.Collections.Generic;

namespace ComputeCS
{
    static class Utils {

        public static string DictToQueryString(Dictionary<string, object> dict)
        {
            string query_string = "";
            foreach (string key in dict.Keys) {
                string var_str = Convert.ToString(dict[key]);
                if (var_str != "") {
                    query_string += $"&{key}={var_str}";
                }
            }
            query_string = (query_string != "") ? $"?{query_string.Substring(1)}" : "";
            return query_string;
        }
    }
}