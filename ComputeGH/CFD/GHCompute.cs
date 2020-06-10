using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Rhino.Geometry;
using Newtonsoft.Json;
using ComputeCS.types;

namespace ComputeCS.Grasshopper
{
    public class ComputeCompute : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the ComputeCompute class.
        /// </summary>
        public ComputeCompute()
          : base("Compute", "Compute",
              "Upload and compute the CFD Case",
              "Compute", "CFD")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Input", "Input", "Input from previous Compute Component", GH_ParamAccess.item);
            pManager.AddBrepParameter("Geometry", "Geometry", "Case Geometry", GH_ParamAccess.list);
            pManager.AddTextParameter("Local Path", "Path", "Local Path to write the geometry to. Default is %Temp%", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Compute", "Compute", "Run the case on Procedural Compute", GH_ParamAccess.item, false);

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
            List<Brep> geometry = new List<Brep>();
            string path = null;
            bool compute = false;



            if (!DA.GetData(0, ref inputJson)) return;
            if (!DA.GetDataList(1, geometry)) return;
            if (!DA.GetData(2, ref path)) return;
            if (!DA.GetData(3, ref compute)) return;
  
            /*Dictionary<string, object> outputs = ComputeCS.Components.Compute.Create(
                inputJson,
                path
            );*/

            DA.SetData(0, inputJson);

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