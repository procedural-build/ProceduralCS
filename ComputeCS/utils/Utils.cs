using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        
        public static IDictionary<string, object> ToDictionary(this object source)
        {
            return source.ToDictionary<object>();
        }

        public static IDictionary<string, T> ToDictionary<T>(this object source)
        {
            if (source == null) ThrowExceptionWhenSourceArgumentIsNull();

            var dictionary = new Dictionary<string, T>();
            foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(source))
            {
                object value = property.GetValue(source);
                if (IsOfType<T>(value))
                {
                    dictionary.Add(property.Name, (T)value);
                }
            }
            return dictionary;
        }

        private static bool IsOfType<T>(object value)
        {
            return value is T;
        }

        private static void ThrowExceptionWhenSourceArgumentIsNull()
        {
            throw new NullReferenceException("Unable to convert anonymous object to a dictionary. The source anonymous object is null.");
        }
    }
}