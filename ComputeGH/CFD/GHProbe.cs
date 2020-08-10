using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using ComputeCS;
using Rhino.Geometry;
using ComputeCS.utils.Cache;
using ComputeCS.utils.Queue;

namespace ComputeCS.Grasshopper
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
            pManager.AddPointParameter("Points", "Points", "Points to create the sample sets from.", GH_ParamAccess.list);
            pManager.AddTextParameter("Fields", "Fields", "", GH_ParamAccess.list);
            pManager.AddIntegerParameter("CPUs", "CPUs", "CPUs to use. Default is [1, 1, 1]", GH_ParamAccess.list);
            pManager.AddTextParameter("DependentOn", "DependentOn", "By default the probe task is dependent on a wind tunnel task or a task running simpleFoam. If you want it to be dependent on another task. Please supply the name of that task here.", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Create", "Create", "Whether to create a new probe task, if one doesn't exist", GH_ParamAccess.item, false);

            pManager[3].Optional = true;
            pManager[4].Optional = true;
            pManager[5].Optional = true;
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
            var points = new List<Point3d>();
            var fields = new List<string>();
            var cpus = new List<int>();
            string dependentOn = null;
            var create = false;

            if (!DA.GetData(0, ref inputJson)) return;
            if (!DA.GetDataList(1, points)) return;
            if (!DA.GetDataList(2, fields)) return;
            if (!DA.GetDataList(3, cpus)) { cpus = new List<int> { 1, 1, 1 }; }
            DA.GetData(4, ref dependentOn);
            DA.GetData(5, ref create);

            var convertedPoints = new List<List<double>>();

            foreach (Point3d point in points) {
                convertedPoints.Add(new List<double> { point.X, point.Y, point.Z });
            };

            // Get Cache to see if we already did this
            var cacheKey = string.Join("", points) + string.Join("", fields);
            var cachedValues = StringCache.getCache(cacheKey);
            DA.DisableGapLogic();

            if (cachedValues == null && create)
            {
                var queueName = "probe";

                // Get queue lock
                var queueLock = StringCache.getCache(queueName);
                if (queueLock != "true")
                {
                    StringCache.setCache(queueName, "true");
                    StringCache.setCache(cacheKey, null);
                    QueueManager.addToQueue(queueName, () => {
                        try
                        {
                            var results = ComputeCS.Components.Probe.ProbePoints(
                                inputJson,
                                convertedPoints,
                                fields,
                                cpus,
                                dependentOn,
                                create
                            );
                            cachedValues = results;
                            StringCache.setCache(cacheKey, cachedValues);

                        }
                        catch (Exception e)
                        {
                            StringCache.AppendCache(this.InstanceGuid.ToString(), e.ToString() + "\n");
                        }
                        StringCache.setCache(queueName, "");
                        ExpireSolutionThreadSafe(true);
                    });
                  
                }

            }

            // Read from Cache
            string outputs = null;
            if (cachedValues != null)
            {
                outputs = cachedValues;
                DA.SetData(0, outputs);
            }

            // Handle Errors
            var errors = StringCache.getCache(this.InstanceGuid.ToString());
            if (errors != null)
            {
                throw new Exception(errors);
            }
        }

        private void ExpireSolutionThreadSafe(bool recompute = false)
        {
            var delegated = new ExpireSolutionDelegate(ExpireSolution);
            Rhino.RhinoApp.InvokeOnUiThread(delegated, recompute);
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