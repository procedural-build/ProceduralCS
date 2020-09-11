using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComputeCS.types
{
    public class CFDSolution
    {
        public List<int> CPUs;
        public string Solver;
        public string CaseType;
        public Dictionary<string, object> BoundaryConditions;
        public Dictionary<string, int> Iterations;
        public List<double> Angles;
        public Dictionary<string, object> Overrides;
        public List<Dictionary<string, object>> Files;
    }
}
