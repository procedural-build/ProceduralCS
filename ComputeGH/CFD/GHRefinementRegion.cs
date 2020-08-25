using System;
using System.Collections.Generic;
using ComputeCS.Grasshopper.Utils;
using ComputeGH.Properties;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;


namespace ComputeCS.Grasshopper
{
    public class cfdRefinementRegion : GH_Component
    {
        public cfdRefinementRegion() : base("Refinement Region", "Refinement Region", "Defines a CFD Refinement Region",
            "Compute", "Mesh")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("meshes", "meshes", "meshes", GH_ParamAccess.list);
            pManager.AddTextParameter("location", "location", "Location (inside|outside)", GH_ParamAccess.item,
                "inside");
            pManager.AddIntegerParameter("minLevel", "minLevel", "Minimum Refinement Level", GH_ParamAccess.item, 1);
            pManager.AddIntegerParameter("maxLevel", "maxLevel", "Maximum Refinement Level", GH_ParamAccess.item, 1);

            pManager[1].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddMeshParameter("RefinementRegions", "RefinementRegions", "RefinementRegions",
                GH_ParamAccess.list);
        }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return null; //Resources.IconRefinementRegion;
            }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("902b5e4a-9ca6-417c-9a38-d2e97b650d40"); }
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var meshes = new List<GH_Mesh>();
            var location = "inside";
            var minLevel = 1;
            var maxLevel = 1;


            DA.GetDataList(0, meshes);
            if (!DA.GetData(1, ref location))
            {
                return;
            }

            DA.GetData(2, ref minLevel);
            DA.GetData(2, ref maxLevel);

            var refLevels = $"(( {minLevel} {maxLevel}))";
            
            // Get a list of object references in the Rhino model
            foreach (var mesh in meshes)
            {
                Geometry.setUserString(
                    mesh,
                    "ComputeRefinementRegion",
                    $"mode {location}; levels {refLevels};"
                );
            }

            DA.SetDataList(0, meshes);
        }
    }
}