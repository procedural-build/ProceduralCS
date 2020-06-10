using Newtonsoft.Json;

namespace ComputeCS.types
{
    public class Inputs : SerializeBase<Inputs>
    {
        public Task Task;
        public Project Project;
        public AuthTokens Auth;
        public string Url;
        public CFDMesh Mesh;
        public CFDSolution CFDSolution;
    }
}