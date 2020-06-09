using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Rhino.Geometry;
using Newtonsoft.Json;
using ComputeCS.types;

namespace ComputeCS.Grasshopper
{
    public class ComputeSolution : GH_Component
    {

        /// <summary>
        /// Initializes a new instance of the computeLogin class.
        /// </summary>
        public ComputeSolution()
          : base("Compute Solution", "CFD Solution",
              "Create the Solution Parameters for a CFD Case",
              "Compute", "CFD")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Input", "Input", "Input from previous Compute Component", GH_ParamAccess.item);
            pManager.AddTextParameter("CPUs", "CPUs", "", GH_ParamAccess.list);
            pManager.AddTextParameter("Solver", "Solver", "", GH_ParamAccess.item);
            pManager.AddTextParameter("Boundary Conditions", "Boundary Conditions", "", GH_ParamAccess.list);
            pManager.AddTextParameter("Iterations", "Iterations", "", GH_ParamAccess.item);
            pManager.AddTextParameter("Number of Angles", "Number of Angles", "", GH_ParamAccess.item);
            pManager.AddTextParameter("Overrides", "Overrides", "", GH_ParamAccess.list);

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
            List<int> cpus = null;

            string solver = null;

            List<Dictionary<string, object>> boundaryConditions = null;

            Dictionary<string, int> iterations = null;

            int numberOfAngles = 1;
            Dictionary<string, object> overrides = new Dictionary<string, object>();


            if (!DA.GetData(1, ref cpus)) return;
            if (!DA.GetData(2, ref solver)) return;
            if (!DA.GetData(3, ref boundaryConditions)) return;
            if (!DA.GetData(4, ref iterations)) return;

            var outputs = ComputeCS.Components.CFDSolution.Setup(
                inputJson,
                cpus,
                solver,
                boundaryConditions,
                iterations,
                numberOfAngles,
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
            get { return new Guid("31b37cda-f5cc-4ffe-86b2-00bd457e4311"); }
        }
    }
}