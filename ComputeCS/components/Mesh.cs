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
        /*
        string type,
        double cellSize,
        List<List<int>> boundingBox,
        Dictionary<string, string> parameters,
        Dictionary<string, object> defaultSurface,
        List<Dictionary<string, object>> surfaces,
        Dictionary<string, object> overrides = null
        */
        )
        {
            var inputData = new Inputs().FromJson(inputJson);
            var domainData = new Inputs().FromJson(domain);
            inputData.Mesh = domainData.Mesh;
            inputData.Mesh.SnappyHexMesh.DefaultSurface = defaultSurface;
            inputData.Mesh.SnappyHexMesh.Overrides = overrides;

            /*var mesh = new types.Mesh
            {
                BaseMesh = new BaseMesh
                {
                    Type = type,
                    CellSize = cellSize,
                    BoundingBox = boundingBox,
                    Parameters = parameters,
                },
                SnappyHexMesh = new SnappyHexMesh
                {
                    Overrides = overrides,
                    DefaultSurface = defaultSurface,
                    Surfaces = surfaces
                }
            };
            */
            var output = inputData.ToJson();
            
            return new Dictionary<string, object>
            {
                {"out", output}
            };
        }
    }
}