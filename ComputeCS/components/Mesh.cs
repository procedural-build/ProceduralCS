using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ComputeCS.types;
using Newtonsoft.Json;

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
                List<double> locationInMesh = null;
                if (inputData.Mesh.SnappyHexMesh.Overrides.ContainsKey("castellatedMeshControls"))
                {
                    
                    var castellatedMeshControls = new CastellatedMeshControls().FromJson(inputData.Mesh.SnappyHexMesh.Overrides["castellatedMeshControls"].ToString());
                    locationInMesh = castellatedMeshControls.LocationInMesh;
                }
                
                foreach (var setSetRegion in setSetRegions_)
                {
                    if (setSetRegion.KeepPoint == null)
                    {
                        if (locationInMesh == null)
                        {
                            throw new Exception("Could not find castellatedMeshControls.locationInMesh in SnappyHexMesh.Overrides. You need to provide locationInMesh if you haven't specified the keep point for the setSet regions.");
                        }
                        setSetRegion.KeepPoint = locationInMesh;
                    }
                }
                inputData.Mesh.BaseMesh.setSetRegions = setSetRegions_;
            }

            return inputData.ToJson();
        }
    }
}