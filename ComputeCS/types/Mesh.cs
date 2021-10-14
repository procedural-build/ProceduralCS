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
        [JsonProperty("snappyhex_mesh")] public SnappyHexMesh SnappyHexMesh;

        public int CellEstimate;
    }

    public class BaseMesh : ICloneable
    {
        public string Type;
        public double CellSize;
        public Dictionary<string, object> BoundingBox;
        public Dictionary<string, string> Parameters;
        public List<setSetRegion> setSetRegions;

        public object Clone()
        {
            return MemberwiseClone();
        }
    }

    public class SnappyHexMesh : SerializeBase<SnappyHexMesh>
    {
        public SnappyHexMeshOverrides Overrides;
        public Dictionary<string, object> DefaultSurface;
        public Dictionary<string, MeshLevelDetails> Surfaces;

        [JsonProperty("refinementRegions")] public List<RefinementRegion> RefinementRegions;
    }

    public class RefinementRegion : SerializeBase<RefinementRegion>
    {
        public string Name;
        public RefinementDetails Details;
    }


    public class RefinementDetails : SerializeBase<RefinementDetails>
    {
        public string Mode;

        public string Levels
        {
            get => GetLevels();
            set => levels = value;
        }
        public double? Resolution;
        public double? CellSize; 
        private string levels;

        private string GetLevels()
        {
            if (!string.IsNullOrEmpty(levels) || !(CellSize > 0) || !(Resolution > 0)) return levels;
            var level = (int) (Math.Log10((double) CellSize / (double) Resolution) / Math.Log10(2));
            return $"(( {level} {level} ))";
        }
    }
    
    public class MeshLevelDetails : SerializeBase<MeshLevelDetails>
    {
        public double? Resolution;
        public double? CellSize;
        public MeshLevels Level     
        {
            get => GetLevel();
            set => level = value;
        }
        
        private MeshLevels level;

        private MeshLevels GetLevel()
        {
            if (level != null || !(CellSize > 0) || !(Resolution > 0)) return level;
            var _level = (int) (Math.Log10((double) CellSize / (double) Resolution) / Math.Log10(2));
            return new MeshLevels {Min = _level, Max = _level};
        }
    }

    public class MeshLevels : SerializeBase<MeshLevels>
    {
        public int Min;
        public int Max;
    }

    public class setSetRegion : SerializeBase<setSetRegion>
    {
        public string Name;
        public List<bool> Locations;
        public List<double> KeepPoint;
        public bool CellZone;
    }

    public class SnappyHexMeshOverrides : SerializeBase<SnappyHexMeshOverrides>
    {
        [JsonProperty("castellatedMeshControls")]
        public CastellatedMeshControls CastellatedMeshControls;

        [JsonProperty("snapControls")] public SnapControls SnapControls;
        [JsonProperty("addLayersControls")] public AddLayersControls AddLayersControls;
        [JsonProperty("meshQualityControls")] public MeshQualityControls MeshQualityControls;
        [JsonProperty("mergeTolerance")] public double? MergeTolerance;

        public void Merge(SnappyHexMeshOverrides mergeWith)
        {
            var overrideFields = mergeWith.GetType().GetFields();
            foreach (var field in overrideFields)
            {
                var value = field.GetValue(mergeWith);
                if (value == null){continue;}
                if (value is double)
                {
                    field.SetValue(this, value);
                }
                else
                {
                    var deepFields = value.GetType().GetFields();
                    foreach (var deepField in deepFields)
                    {
                        var deepValueMerge = deepField.GetValue(value);
                        var deepFieldThis = field.GetValue(this) != null?field.GetValue(this).GetType().GetField(deepField.Name): null;
                        var deepValueThis = deepFieldThis != null? deepFieldThis.GetValue(field.GetValue(this)): null;
                        if (deepValueMerge != null)
                        {
                            deepField.SetValue(value, deepValueMerge);
                        }
                        if (deepValueThis != null)
                        {
                            deepField.SetValue(value, deepValueThis);
                        }
                    }
                    field.SetValue(this, value);
                }
            }
        }
    }

    public class CastellatedMeshControls : SerializeBase<CastellatedMeshControls>
    {
        [JsonProperty("locationInMesh")] public List<double> LocationInMesh;
        [JsonProperty("maxGlobalCells")] public uint? MaxGlobalCells;
        [JsonProperty("nCellsBetweenLevels")] public uint? NCellsBetweenLevels;
        [JsonProperty("minRefinementCells")] public uint? MinRefinementCells;
        [JsonProperty("resolveFeatureAngle")] public double? ResolveFeatureAngle;

        [JsonProperty("allowFreeStandingZoneFaces")]
        public string AllowFreeStandingZoneFaces;
    }

    public class SnapControls : SerializeBase<SnapControls>
    {
        [JsonProperty("nSmoothPatch")] public uint? NSmoothPatch;
        [JsonProperty("tolerance")] public double? Tolerance;
        [JsonProperty("nSolveIter")] public uint? NSolveIter;
        [JsonProperty("nRelaxIter")] public uint? NRelaxIter;
        [JsonProperty("nFeatureSnapIter")] public uint? NFeatureSnapIter;
        [JsonProperty("implicitFeatureSnap")] public string ImplicitFeatureSnap;
        [JsonProperty("explicitFeatureSnap")] public string ExplicitFeatureSnap;

        [JsonProperty("multiRegionFeatureSnap")]
        public string MultiRegionFeatureSnap;
    }

    public class AddLayersControls : SerializeBase<AddLayersControls>
    {
        [JsonProperty("relativeSizes")] public string RelativeSizes;
        [JsonProperty("expansionRatio")] public double? ExpansionRatio;
        [JsonProperty("finalLayerThickness")] public double? FinalLayerThickness;
        [JsonProperty("minThickness")] public double? MinThickness;
        [JsonProperty("nGrow")] public double? NGrow;
        [JsonProperty("featureAngle")] public double? FeatureAngle;
        [JsonProperty("nRelaxIter")] public uint? NRelaxIter;

        [JsonProperty("nSmoothSurfaceNormals")]
        public uint? NSmoothSurfaceNormals;

        [JsonProperty("nSmoothNormals")] public uint? NSmoothNormals;
        [JsonProperty("nSmoothThickness")] public uint? NSmoothThickness;

        [JsonProperty("maxFaceThicknessRatio")]
        public double? MaxFaceThicknessRatio;

        [JsonProperty("maxThicknessToMedialRatio")]
        public double? MaxThicknessToMedialRatio;

        [JsonProperty("minMedianAxisAngle")] public double? MinMedianAxisAngle;

        [JsonProperty("nBufferCellsNoExtrude")]
        public uint? NBufferCellsNoExtrude;

        [JsonProperty("nLayerIter")] public uint? NLayerIter;
    }

    public class MeshQualityControls : SerializeBase<MeshQualityControls>
    {
        [JsonProperty("maxNonOrtho")] public uint? MaxNonOrtho;
        [JsonProperty("maxBoundarySkewness")] public uint? MaxBoundarySkewness;
        [JsonProperty("maxInternalSkewness")] public uint? MaxInternalSkewness;
        [JsonProperty("maxConcave")] public uint? MaxConcave;
        [JsonProperty("minFlatness")] public double? MinFlatness;
        [JsonProperty("minVol")] public double? MinVol;
        [JsonProperty("minTetQuality")] public double? MinTetQuality;
        [JsonProperty("minArea")] public double? MinArea;
        [JsonProperty("minTwist")] public string MinTwist;
        [JsonProperty("minDeterminant")] public double? MinDeterminant;
        [JsonProperty("minFaceWeight")] public double? MinFaceWeight;
        [JsonProperty("minTriangleTwist")] public double? MinTriangleTwist;
        [JsonProperty("nSmoothScale")] public uint? NSmoothScale;
        [JsonProperty("errorReduction")] public double? ErrorReduction;
    }

    public class RadiationMesh : SerializeBase<RadiationMesh>
    {
        public List<string> MeshIds;
        public List<List<double>> Points;
        public List<List<double>> Normals;
    }
}