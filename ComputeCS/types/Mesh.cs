﻿using System;
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
        public List<setSetRegion> setSetRegions;
    }

    public class SnappyHexMesh
    {
        public Dictionary<string, object> Overrides;
        public Dictionary<string, object> DefaultSurface;
        public Dictionary<string, object> Surfaces;
        public List<RefinementRegion> RefinementRegions;
    }

    public class RefinementRegion : SerializeBase<RefinementRegion>
    {
        public string Name;
        public RefinementDetails Details;
    }


    public class RefinementDetails : SerializeBase<RefinementDetails>
    {
        public string Mode;
        public string Levels;
    }

    public class setSetRegion : SerializeBase<setSetRegion>
    {
        public string Name;
        public List<bool> Locations;
        public List<double> KeepPoint;
    }
}
