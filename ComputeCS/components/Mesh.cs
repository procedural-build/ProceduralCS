using System.Collections.Generic;
using ComputeCS.types;

namespace ComputeCS.Components
{
    public static class Mesh
    {
        public static Dictionary<string, object> Setup(
            string inputJson,
            string domain,
            Dictionary<string, object> defaultSurface,
            Dictionary<string, object> overrides = null
        )
        {
            var inputData = new Inputs().FromJson(inputJson);
            var domainData = new Inputs().FromJson(domain);
           
                
            inputData.Mesh = domainData.Mesh;
            inputData.Mesh.SnappyHexMesh.DefaultSurface = defaultSurface;

            var output = inputData.ToJson();
            
            return new Dictionary<string, object>
            {
                {"out", output}
            };
        }
    }
}