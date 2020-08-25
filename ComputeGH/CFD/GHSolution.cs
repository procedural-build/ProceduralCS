using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Rhino.Geometry;
using Newtonsoft.Json;
using ComputeCS.types;
using ComputeGH.Properties;

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
            pManager.AddIntegerParameter("CPUs", "CPUs", "", GH_ParamAccess.list);
            pManager.AddTextParameter("Solver", "Solver", "", GH_ParamAccess.item);
            pManager.AddTextParameter("Case Type", "Case Type", "Avaible Options: SimpleCase, VirtualWindTunnel", GH_ParamAccess.item, "SimpleCase");
            pManager.AddTextParameter("Boundary Conditions", "Boundary Conditions", "", GH_ParamAccess.list);
            pManager.AddTextParameter("Iterations", "Iterations", "", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Number of Angles", "Number of Angles", "Number of Angles. Default is 16", GH_ParamAccess.item, 16);
            pManager.AddTextParameter("Overrides", "Overrides", "", GH_ParamAccess.item);

            pManager[2].Optional = true;
            pManager[3].Optional = true;
            pManager[6].Optional = true;
            pManager[7].Optional = true;

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
            List<int> cpus = new List<int>();

            string solver = "simpleFoam";
            string caseType = "simpleCase";
            List<string> boundaryConditions = new List<string>();

            string iterations = null;

            int numberOfAngles = 16;
            string overrides = null;


            if (!DA.GetData(0, ref inputJson)) return;
            if (!DA.GetDataList(1, cpus)) return;
            DA.GetData(2, ref solver);
            DA.GetData(3, ref caseType);
            if (!DA.GetDataList(4, boundaryConditions)) return;
            if (!DA.GetData(5, ref iterations)) return;
            DA.GetData(6, ref numberOfAngles);
            DA.GetData(7, ref overrides);

            var outputs = Components.CFDSolution.Setup(
                inputJson,
                cpus,
                solver,
                caseType,
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
                return null; // Resources.IconSolver;
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