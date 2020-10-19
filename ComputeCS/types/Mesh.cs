using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ComputeCS.types
{
    public class CFDMesh
    {
        public BaseMesh BaseMesh;
        [JsonProperty("snappyhex_mesh")]
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
    
    public class SnappyHexMesh :  SerializeBase<SnappyHexMesh>
    {
        public Dictionary<string, object> Overrides;
        public Dictionary<string, object> DefaultSurface;
        public Dictionary<string, object> Surfaces;
        
        [JsonProperty("refinementRegions")]
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

    public class CastellatedMeshControls : SerializeBase<CastellatedMeshControls>
    {
        [JsonProperty("locationInMesh")]
        public List<double> LocationInMesh;
    }
}
