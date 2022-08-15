using System;
using System.Collections.Generic;

namespace ComputeGH.Grasshopper.Utils
{
    public static class ComponentUtils
    {
        public static List<int> ValidateCPUs(int cpus)
        {
            if (cpus == 1)
            {
                return new List<int> { 1, 1, 1 };
            }
            if (cpus == 2)
            {
                return new List<int> { 2, 1, 1 };
            }
            if (cpus == 4)
            {
                return new List<int> { 2, 2, 1 };
            }
            if (cpus == 8)
            {
                return new List<int> { 4, 2, 1 };
            }
            if (cpus == 16)
            {
                return new List<int> { 4, 4, 1 };
            }
            if (cpus == 18)
            {
                return new List<int> { 6, 3, 1 };
            }
            if (cpus == 24)
            {
                return new List<int> { 6, 4, 1 };
            }
            if (cpus == 36)
            {
                return new List<int> { 6, 6, 1 };
            }
            if (cpus == 48)
            {
                return new List<int> { 12, 4, 1 };
            }
            if (cpus == 64)
            {
                return new List<int> { 8, 8, 1 };
            }
            if (cpus == 72)
            {
                return new List<int> { 12, 6, 1 };
            }
            if (cpus == 96)
            {
                return new List<int> { 12, 8, 1 };
            }

            throw new Exception($"Number of CPUs ({cpus}) were not valid. Valid choices are: 1, 2, 4, 8, 16, 18, 24, 36, 48, 64, 72, 96");
        }
    }
}