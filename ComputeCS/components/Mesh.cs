using System.Collections.Generic;
using ComputeCS.types;

namespace ComputeCS.Components
{
    public static class Mesh
    {
        public static Dictionary<string, object> Setup(
            string inputJson,
            string type,
            double cellSize,
            List<List<int>> boundingBox,
            Dictionary<string, string> parameters,
            Dictionary<string, object> defaultSurface,
            List<Dictionary<string, object>> surfaces,
            Dictionary<string, object> overrides = null
        )
        {
            var inputData = SerializeIO.InputsFromJson(inputJson);
            var mesh = new types.Mesh
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
            var output = SerializeIO.OutputToJson(
                inputData,
                null,
                null,
                null,
                null,
                mesh
            );

            return new Dictionary<string, object>
            {
                {"out", output}
            };
        }
    }
}