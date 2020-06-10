using System;
using System.Collections.Generic;
using ComputeCS.types;
using Newtonsoft.Json;
using System.Reflection;

namespace ComputeCS
{

    public class SerializeBase<T> {

        private void SetAttributes(Dictionary<string, object> overrides = null) {
            /* Programmatically set class attributes from ket,value pairs (Pythonic)
            */
            foreach (KeyValuePair<string, object> item in overrides) {
                PropertyInfo propertyInfo = this.GetType().GetProperty(item.Key);
                propertyInfo.SetValue(this, item.Value);
            }
        }

        public T FromJson(string inputData) {
            return JsonConvert.DeserializeObject<T>(inputData);
        }

        public string ToJson(Dictionary<string, object> overrides = null) {

            if (overrides != null) {
                SetAttributes(overrides);
            }

            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }
}