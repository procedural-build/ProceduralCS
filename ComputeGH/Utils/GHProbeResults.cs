using System;
using System.Collections.Generic;
using ComputeCS.Components;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace ComputeCS.Grasshopper
{
    public class GHProbeResults : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the GHProbeResults class.
        /// </summary>
        public GHProbeResults()
          : base("Probe Results", "Probe Results",
              "Loads the probe results from a file",
              "Compute", "Utils")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Folder", "Folder", "Folder path to where to results are", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("U", "U", "Velocity vectors", GH_ParamAccess.tree);
            pManager.AddTextParameter("p", "p", "Pressure", GH_ParamAccess.tree);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string folder = null;

            if (!DA.GetData(0, ref folder)) return;

            var results = ProbeResult.ReadProbeResults(folder);
            var UVectors = ConvertToGHVectors(results["U"]);
            var pValues = ConverToGHValues(results["p"]);

            DA.SetDataTree(0, UVectors);
            DA.SetDataTree(1, pValues);
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
            get { return new Guid("74163c8b-25fd-466f-a56a-d2beeebcaccb"); }
        }
    }
}