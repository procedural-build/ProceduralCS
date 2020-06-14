using System;
using System.Collections.Generic;

namespace ComputeCS.utils.Cache
{
    public static class StringArrayCache
    {
        private static Dictionary<string, string[]> cache = new Dictionary<string, string[]>();
        private static Object setCacheLock = new Object();

        public static void setCache(string key, string[] valueList)
        {
            lock (setCacheLock) {
                cache[key] = valueList;
            }
        }

        public static string[] getCache(string key)
        {
            if (cache.ContainsKey(key)) { return cache[key]; }
            return null;
        }

        public static bool ContainsKey(string key) {
            return cache.ContainsKey(key);
        }
    }

    public static class StringCache
    {
        private static Dictionary<string, string> cache = new Dictionary<string, string>();
        private static Object setCacheLock = new Object();

        public static void setCache(string key, string valueString)
        {
            lock (setCacheLock) {
                cache[key] = valueString;
            }
        }

        public static string getCache(string key)
        {
            if (cache.ContainsKey(key)) { return cache[key]; }
            return null;
        }

        public static bool ContainsKey(string key) {
            return cache.ContainsKey(key);
        }

        public static void AppendCache(string key, string valueString) {
            string existingString = getCache(key);
            if (existingString == null) { existingString = ""; }
            setCache(key, existingString + "\n" + valueString);
        }
    }
}
