using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComputeCS.types
{
    public class CFDMesh
    {
        public BaseMesh BaseMesh;
        public SnappyHexMesh SnappyHexMesh;
    }

    public class BaseMesh
    {
        public string Type;
        public double CellSize;
        public Dictionary<string, object> BoundingBox;
        public Dictionary<string, string> Parameters;
    }

    public class SnappyHexMesh
    {
        public Dictionary<string, object> Overrides;
        public Dictionary<string, object> DefaultSurface;
        public Dictionary<string, object> Surfaces;
    }
}
