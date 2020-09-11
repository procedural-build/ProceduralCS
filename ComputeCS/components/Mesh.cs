using System.Collections.Generic;
using ComputeCS.types;

namespace ComputeCS.Components
{
    public static class Mesh
    {
        public static string Setup(
            string inputJson,
            string domain,
            Dictionary<string, object> defaultSurface,
            Dictionary<string, object> overrides = null,
            List<string> setSetRegions = null
        )
        {
            var inputData = new Inputs().FromJson(inputJson);
            var domainData = new Inputs().FromJson(domain);
           
                
            inputData.Mesh = domainData.Mesh;
            inputData.Mesh.SnappyHexMesh.DefaultSurface = defaultSurface;

            if (overrides != null)
            {
                inputData.Mesh.SnappyHexMesh.Overrides = overrides;    
            }

            if (setSetRegions != null)
            {
                var setSetRegions_ = new List<setSetRegion>();
                foreach (var region in setSetRegions)
                {
                    setSetRegions_.Add(new setSetRegion().FromJson(region));
                }
                inputData.Mesh.BaseMesh.setSetRegions = setSetRegions_;
            }

            return inputData.ToJson();
        }
    }
}