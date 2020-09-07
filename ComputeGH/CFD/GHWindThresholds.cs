﻿using System;
using System.Collections.Generic;
using ComputeCS.Grasshopper;
using ComputeCS.utils.Cache;
using ComputeCS.utils.Queue;
using ComputeGH.Properties;
using Grasshopper.Kernel;

namespace ComputeGH.CFD
{
    public class GHWindThresholds : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the WindThresholds class.
        /// </summary>
        public GHWindThresholds()
            : base("Wind Thresholds", "Wind Thresholds",
                "Compute the Lawson criteria for a CFD case.",
                "Compute", "CFD")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Input", "Input", "Input from the previous Compute Component",
                GH_ParamAccess.item);
            pManager.AddTextParameter("EPW File", "EPW Files", "Path to where the EPW file is located.",
                GH_ParamAccess.item);
            pManager.AddTextParameter("Patch Names", "Patch Names",
                "Give names to each branch of in the points tree. The names can later be used to identify the points.",
                GH_ParamAccess.list);
            pManager.AddIntegerParameter("CPUs", "CPUs", "CPUs to use. Default is [1, 1, 1]", GH_ParamAccess.list);
            pManager.AddTextParameter("DependentOn", "DependentOn",
                "By default the probe task is dependent on a wind tunnel task or a task running simpleFoam. If you want it to be dependent on another task. Please supply the name of that task here.",
                GH_ParamAccess.item);
            pManager.AddBooleanParameter("Create", "Create",
                "Whether to create a new Wind Threshold task, if one doesn't exist", GH_ParamAccess.item, false);

            pManager[2].Optional = true;
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
            var epwFile = "";
            var patches = new List<string>();
            var cpus = new List<int>();
            string dependentOn = null;
            var create = false;

            if (!DA.GetData(0, ref inputJson)) return;
            if (!DA.GetData(1, ref epwFile)) return;
            if (!DA.GetDataList(2, patches))
            {
                patches.Add("set1");
            }

            if (!DA.GetDataList(3, cpus))
            {
                cpus = new List<int> {1, 1, 1};
            }

            DA.GetData(4, ref dependentOn);
            DA.GetData(5, ref create);

            // Get Cache to see if we already did this
            var cacheKey = string.Join("", patches) + epwFile;
            var cachedValues = StringCache.getCache(cacheKey);
            DA.DisableGapLogic();

            if (cachedValues == null || create)
            {
                var queueName = "windThreshold";

                // Get queue lock
                var queueLock = StringCache.getCache(queueName);
                if (queueLock != "true")
                {
                    StringCache.setCache(queueName, "true");
                    StringCache.setCache(cacheKey, null);
                    QueueManager.addToQueue(queueName, () =>
                    {
                        try
                        {
                            var results = ComputeCS.Components.WindThreshold.ComputeWindThresholds(
                                inputJson,
                                epwFile,
                                patches,
                                cpus,
                                dependentOn,
                                create
                            );
                            cachedValues = results;
                            StringCache.setCache(cacheKey, cachedValues);
                            if (create)
                            {
                                StringCache.setCache(cacheKey + "create", "true");
                            }
                        }
                        catch (Exception e)
                        {
                            StringCache.AppendCache(this.InstanceGuid.ToString(), e.ToString() + "\n");
                            StringCache.setCache(cacheKey, "error");
                            StringCache.setCache(cacheKey + "create", "");
                        }

                        StringCache.setCache(queueName, "");
                        ExpireSolutionThreadSafe(true);
                    });
                }
            }

            // Handle Errors
            var errors = StringCache.getCache(this.InstanceGuid.ToString());
            if (errors != null)
            {
                throw new Exception(errors);
            }

            // Read from Cache
            string outputs = null;
            if (cachedValues != null)
            {
                outputs = cachedValues;
                DA.SetData(0, outputs);
                Message = "";
                if (StringCache.getCache(cacheKey + "create") == "true")
                {
                    Message = "Task Created";
                }
            }
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get { return Resources.IconSolver; }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("235182ea-f739-4bea-be24-907de80e58fa"); }
        }

        private void ExpireSolutionThreadSafe(bool recompute = false)
        {
            var delegated = new ExpireSolutionDelegate(ExpireSolution);
            Rhino.RhinoApp.InvokeOnUiThread(delegated, recompute);
        }
    }
}