using System;
using System.Collections.Generic;
using ComputeCS.Grasshopper.Utils;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;


namespace ComputeCS.Grasshopper
{
    public class cfdRefinementRegion : GH_Component
    {
        public cfdRefinementRegion() : base("cfdRefinementRegion", "cfdRefinementRegion", "Defines a CFD Refinement Region", "Compute", "Mesh")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("meshes", "meshes", "meshes", GH_ParamAccess.list);
            pManager.AddTextParameter("location", "location", "Location (inside|outside)", GH_ParamAccess.item, "inside");
            pManager.AddTextParameter("levels", "levels", "Refinement Levels", GH_ParamAccess.item, "((1 1))");

            pManager[1].Optional = true;
            pManager[2].Optional = true;
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddMeshParameter("refinementRegions", "refinementRegions", "refinementRegions", GH_ParamAccess.list);
        }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return null; //ghODSResources.IconRefinementRegion;
            }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("902b5e4a-9ca6-417c-9a38-d2e97b650d40"); }
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            
            List<GH_Mesh> meshes = new List<GH_Mesh>();
            string location = "inside";
            string refLevels = "(( 1 3 ))";


            DA.GetDataList(0, meshes);
            if (!DA.GetData(1, ref location)) { return; }
            if (!DA.GetData(2, ref refLevels)) { return; }

            // Get a list of object references in the Rhino model
            foreach (GH_Mesh mesh in meshes)
            {
                Geometry.setUserString(mesh, "ComputeRefinementRegion", string.Format("mode {0}; levels {1};", location, refLevels));
            }

            DA.SetDataList(0, meshes);
        }
    }
}