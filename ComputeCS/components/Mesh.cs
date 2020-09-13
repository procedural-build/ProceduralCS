using System.Collections.Generic;
using System.Linq;
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
                if (inputData.Mesh.SnappyHexMesh.Overrides == null)
                {
                    inputData.Mesh.SnappyHexMesh.Overrides = overrides;  
                }

                var existingOverrides = inputData.Mesh.SnappyHexMesh.Overrides;
                inputData.Mesh.SnappyHexMesh.Overrides = existingOverrides
                    .Concat(overrides)
                    .ToLookup(x => x.Key, x => x.Value)
                    .ToDictionary(x => x.Key, g => g.First());
                
            }

            if (setSetRegions != null)
            {
                var setSetRegions_ = setSetRegions.Select(region => new setSetRegion().FromJson(region)).ToList();
                inputData.Mesh.BaseMesh.setSetRegions = setSetRegions_;
            }

            return inputData.ToJson();
        }
    }
}