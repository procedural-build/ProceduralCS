using System.Collections.Generic;

namespace ComputeCS.types
{
    public class Task : SerializeBase<Task>
    {
        public string UID;
        public string Name;
        public string Status;
        public string Started;
        public string Stopped;
        public string Created;
        public Dictionary<string, object> Config;
        public string ClusterBaseDir;
        public Task Parent;
        public Dictionary<string, object> MetaJson;
        public string Project;
        public Task DependentOn;
        public List<string> ErrorMessages = null;
    }

    public class TaskFile : SerializeBase<TaskFile>
    {
        public string File;
        public string Hash;
        public List<string> ErrorMessages = null;
    }
}
