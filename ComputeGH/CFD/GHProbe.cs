using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using ComputeCS;

namespace ComputeGH.CFD
{
    public class GHProbe : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the GHProbe class.
        /// </summary>
        public GHProbe()
          : base("Probe Points", "Probe Points",
              "Probe Points to get result",
              "Compute", "CFD")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Input", "Input", "Input from previous Compute Component", GH_ParamAccess.item);
            pManager.AddTextParameter("SampleSets", "SampleSets", "", GH_ParamAccess.item);
            pManager.AddTextParameter("Fields", "Fields", "", GH_ParamAccess.item);
            pManager.AddTextParameter("DependentOn", "DependentOn", "By default the probe task is dependent on a wind tunnel task or a task running simpleFoam. If you want it to be dependent on another task. Please supply the name of that task here.", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Create", "Create", "Whether to create a new probe task, if one doesn't exist", GH_ParamAccess.item, false);

            pManager[3].Optional = true;
            pManager[4].Optional = true;
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
            string sampleSets = null;
            string fields = null;
            string dependentOn = null;
            bool create = false;

            if (!DA.GetData(0, ref inputJson)) return;
            if (!DA.GetData(1, ref sampleSets)) return;
            if (!DA.GetData(2, ref fields)) return;
            DA.GetData(3, ref dependentOn);
            DA.GetData(4, ref create);

            var outputs = ComputeCS.Components.Probe.ProbePoints(
                inputJson,
                sampleSets,
                fields,
                dependentOn,
                create
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
            get { return new Guid("2f0bdda2-f7eb-4fc7-8bf0-a2fd5a787493"); }
        }
    }
}