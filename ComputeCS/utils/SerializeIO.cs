using System;
using System.Collections.Generic;
using ComputeCS.types;
using Newtonsoft.Json;
using System.Reflection;
using Newtonsoft.Json.Serialization;

namespace ComputeCS
{

    public class SerializeBase<T> {

        private void SetAttributes(Dictionary<string, object> overrides = null) {
            /* Programmatically set class attributes from ket,value pairs (Pythonic)
            */
            foreach (KeyValuePair<string, object> item in overrides) {
                PropertyInfo propertyInfo = this.GetType().GetProperty(item.Key);
                if (item.Value != null)
                {
                    propertyInfo.SetValue(this, item.Value);
                }

            }
        }

        public T FromJson(string inputData) {
            return JsonConvert.DeserializeObject<T>(inputData, JsonSettings);
        }

        private JsonSerializerSettings JsonSettings = new JsonSerializerSettings
        {
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new SnakeCaseNamingStrategy()
            },
            NullValueHandling = NullValueHandling.Ignore,
            Formatting = Formatting.Indented
        };

        public string ToJson(Dictionary<string, object> overrides = null) {

            if (overrides != null) {
                SetAttributes(overrides);
            }

            return JsonConvert.SerializeObject(this, JsonSettings);
        }
    }
}