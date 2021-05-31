﻿using System;
using System.Collections.Generic;
using ComputeCS.utils.Cache;

namespace ComputeGH.Grasshopper.Utils
{
    public static class ComponentUtils
    {
        public static List<int> ValidateCPUs(int cpus)
        {
            if (cpus == 1)
            {
                return new List<int> {1, 1, 1};
            }
            if (cpus == 2)
            {
                return new List<int> {2, 1, 1};
            }
            if (cpus == 4)
            {
                return new List<int> {2, 2, 1};
            }
            if (cpus == 8)
            {
                return new List<int> {4, 2, 1};
            }
            if (cpus == 16)
            {
                return new List<int> {4, 4, 1};
            }
            if (cpus == 18)
            {
                return new List<int> {6, 3, 1};
            }
            if (cpus == 24)
            {
                return new List<int> {6, 4, 1};
            }
            if (cpus == 36)
            {
                return new List<int> {6, 6, 1};
            }
            if (cpus == 48)
            {
                return new List<int> {12, 4, 1};
            }
            if (cpus == 64)
            {
                return new List<int> {8, 8, 1};
            }
            if (cpus == 72)
            {
                return new List<int> {12, 6, 1};
            }
            if (cpus == 96)
            {
                return new List<int> {12, 8, 1};
            }

            throw new Exception($"Number of CPUs ({cpus}) were not valid. Valid choices are: 1, 2, 4, 8, 16, 18, 24, 36, 48, 64, 72, 96");
        }

        public static Tuple<string, string> BlockingComponent(string cacheKey, string instanceGuid)
        {
            if (string.IsNullOrEmpty(cacheKey) || string.IsNullOrEmpty(instanceGuid))
            {
                return new Tuple<string, string>("", "");
            }
            
            var cachedValues = StringCache.getCache(cacheKey);
            var errors = StringCache.getCache(instanceGuid);

            if (StringCache.getCache("compute.blocking") != "true")
                return new Tuple<string, string>(cachedValues, errors);
            
            var startTime = DateTime.Now;
            while (string.IsNullOrEmpty(cachedValues) && string.IsNullOrEmpty(errors))
            {
                cachedValues = StringCache.getCache(cacheKey);
                errors = StringCache.getCache(instanceGuid);
                if (DateTime.Now - startTime > TimeSpan.FromSeconds(10))
                {
                    break;
                }
            }

            return new Tuple<string, string>(cachedValues, errors);
        }
    }
}