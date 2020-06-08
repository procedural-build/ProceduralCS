using System;
using System.Collections.Generic;
using computeGH.core;
using Grasshopper.Kernel;
using Rhino.Geometry;
using Newtonsoft.Json;
using computeGH.types;

namespace ComputeCS.Grasshopper
{
    public class GHMesh : GH_ComponentC
    {
    
        /// <summary>
        /// Initializes a new instance of the computeLogin class.
        /// </summary>
        public computeLogin()
          : base("Compute Mesh, "Mesh",
              "Create the Mesh Parameters for a CFD Case",
              "Compute", "CFD")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Input", "Input", "Input from previous Compute Component", GH_ParamAccess.item);
            pManager.AddTextParameter("Type", "Type", "", GH_ParamAccess.item);
            pManager.AddTextParameter("Cell Size", "Cell Size", "", GH_ParamAccess.item);
            pManager.AddTextParameter("Bounding Box", "Bounding Box", "", GH_ParamAccess.List);
            pManager.AddTextParameter("Parameters", "Parameters", "", GH_ParamAccess.item);
            pManager.AddTextParameter("Default Surfaces", "Default Surfaces", "", GH_ParamAccess.item);
            pManager.AddTextParameter("Surfaces", "Surfaces", "", GH_ParamAccess.List);
            pManager.AddTextParameter("Overrides", "Overrides", "", GH_ParamAccess.List);
            
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Output", "Output", "Output", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string inputJson = null;
            string type;

            double cellSize;

            List<List<int>> boundingBox;

            Dictionary<string, string> params_;

            // Inputs for SnappyHexMesh
            Dictionary<string, object> defaultSurfaces;
            List<Dictionary<string, object>> surfaces;
            List<Dictionary<string, object>> overrides;
                

            if (!DA.GetData(1, ref type)) return;
            if (!DA.GetData(2, ref cellSize)) return;
            if (!DA.GetData(3, ref boundingBox)) return;
            if (!DA.GetData(6, ref surfaces)) return;

            var outputs = ComputeCS.Components.Mesh.Setup(
                inputJson,
                type,
                cellSize,
                boundingBox,
                params_,
                defaultSurfaces,
                surfaces,
                overrides
            );

            DA.SetData(0, outputs["out"]);
            
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return null;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("898478bb-5b4f-4972-951a-d9e71ba0086b"); }
        }
    }
}