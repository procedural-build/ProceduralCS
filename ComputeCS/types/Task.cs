using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComputeCS.types
{
    public class Task
    {
        public string UID;
        public string Name;
        public string Status;
        public string Started;
        public string Stopped;
        public string Created;
        public Dictionary<string, string> Config;
        public string ClusterBaseDir;
        public string Parent;
        public string MetaJson;
        public string Project;
        public string DependentOn;
    }
}
