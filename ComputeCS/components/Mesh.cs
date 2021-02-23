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
            string overrides = null,
            List<string> setSetRegions = null
        )
        {
            var inputData = new Inputs().FromJson(inputJson);
            var domainData = new Inputs().FromJson(domain);
           
                
            inputData.Mesh = domainData.Mesh;
            inputData.Mesh.SnappyHexMesh.DefaultSurface = defaultSurface;

            if (!string.IsNullOrEmpty(overrides))
            {
                var _overrides = new SnappyHexMeshOverrides().FromJson(overrides);
                if (inputData.Mesh.SnappyHexMesh.Overrides == null)
                {
                    inputData.Mesh.SnappyHexMesh.Overrides = _overrides;  
                }

                inputData.Mesh.SnappyHexMesh.Overrides.Merge(_overrides);
            }

            if (setSetRegions != null)
            {
                var _setSetRegions = setSetRegions.Select(region => new setSetRegion().FromJson(region)).ToList();
                var locationInMesh = inputData.Mesh.SnappyHexMesh.Overrides.CastellatedMeshControls.LocationInMesh;
                
                foreach (var setSetRegion in _setSetRegions.Where(setSetRegion => setSetRegion.KeepPoint == null))
                {
                    if (locationInMesh == null)
                    {
                        throw new Exception("Could not find castellatedMeshControls.locationInMesh in SnappyHexMesh.Overrides. You need to provide locationInMesh if you haven't specified the keep point for the setSet regions.");
                    }
                    setSetRegion.KeepPoint = locationInMesh;
                }
                inputData.Mesh.BaseMesh.setSetRegions = _setSetRegions;
            }

            return inputData.ToJson();
        }
    }
}