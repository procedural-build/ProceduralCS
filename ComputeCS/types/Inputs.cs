using Newtonsoft.Json;
using System.Collections.Generic;

namespace ComputeCS.types
{
    public class Inputs : SerializeBase<Inputs>
    {
        public Task Task;
        public List<Task> SubTasks;
        public Project Project;
        public AuthTokens Auth;
        public string Url;
        public CFDMesh Mesh;
        public CFDSolution CFDSolution;
    }
}