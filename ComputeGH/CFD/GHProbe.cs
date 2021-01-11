using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using ComputeCS.Components;
using ComputeCS.GrasshopperUtils;
using ComputeCS.utils.Cache;
using ComputeCS.utils.Queue;
using ComputeGH.Properties;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino;

namespace ComputeCS.Grasshopper
{
    public class GHProbe : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the GHProbe class.
        /// </summary>
        public GHProbe()
            : base("Probe Points", "Probe Points",
                "Probe the CFD case to get the results in the desired points",
                "Compute", "CFD")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Input", "Input", "Input from previous Compute Component", GH_ParamAccess.item);
            pManager.AddPointParameter("Points", "Points", "Points to create the sample sets from.",
                GH_ParamAccess.tree);
            pManager.AddTextParameter("Names", "Names",
                "Give names to each branch of in the points tree. The names can later be used to identify the points.",
                GH_ParamAccess.list);
            pManager.AddTextParameter("Fields", "Fields", "Choose which fields to probe. Default is U",
                GH_ParamAccess.list);
            pManager.AddIntegerParameter("CPUs", "CPUs", "CPUs to use. Valid choices are:\n1, 2, 4, 8, 16, 18, 24, 36, 48, 64, 72, 96. \nIn most cases it is not advised to use more CPUs than 1, as the time it takes to decompose and reconstruct the case will exceed the speed-up gained by multiprocessing the probing.", GH_ParamAccess.item, 1);
            pManager.AddTextParameter("DependentOn", "DependentOn",
                "By default the probe task is dependent on a wind tunnel task or a task running simpleFoam. If you want it to be dependent on another task. Please supply the name of that task here.",
                GH_ParamAccess.item);
            pManager.AddTextParameter("Case Directory", "Case Dir", "Folder to probe on the Compute server. Default is VWT", GH_ParamAccess.item);
            pManager.AddTextParameter("Overrides", "Overrides",
                "Takes overrides in JSON format: \n" +
                "{\n\t\"setup\": [...],\n\t\"fields\": [...],\n\t\"presets\": [...],\n\t\"caseFiles\": [...]\n}",
                GH_ParamAccess.item);
            pManager.AddBooleanParameter("Create", "Create", "Whether to create a new probe task, if one doesn't exist. If the Probe task already exists, then this component will create a new task config, that will run after the previous config is finished.",
                GH_ParamAccess.item, false);

            pManager[2].Optional = true;
            pManager[3].Optional = true;
            pManager[4].Optional = true;
            pManager[5].Optional = true;
            pManager[6].Optional = true;
            pManager[7].Optional = true;
            pManager[8].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
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
            var points = new GH_Structure<GH_Point>();
            var names = new List<string>();
            var fields = new List<string>();
            var cpus = 1;
            string dependentOn = null;
            var caseDir = "VWT";
            var overrides = "";
            var create = false;

            if (!DA.GetData(0, ref inputJson)) return;
            if (!DA.GetDataTree(1, out points)) return;
            if (!DA.GetDataList(2, names))
            {
                names.Add("set1");
            }

            if (!DA.GetDataList(3, fields))
            {
                fields.Add("U");
            }

            DA.GetData(4, ref cpus);
            DA.GetData(5, ref dependentOn);
            DA.GetData(6, ref caseDir);
            DA.GetData(7, ref overrides);
            DA.GetData(8, ref create);

            var convertedPoints = ConvertToPointList(points);
            caseDir = caseDir.TrimEnd('/');

            // Get Cache to see if we already did this
            var cacheKey = string.Join("", points) + string.Join("", fields);
            var cachedValues = StringCache.getCache(cacheKey);
            DA.DisableGapLogic();

            if (cachedValues == null || create)
            {
                var queueName = "probe";

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
                            var results = Probe.ProbePoints(
                                inputJson,
                                convertedPoints,
                                fields,
                                names,
                                ComponentUtils.ValidateCPUs(cpus),
                                dependentOn,
                                caseDir,
                                overrides,
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


                        ExpireSolutionThreadSafe(true);
                        Thread.Sleep(2000);
                        StringCache.setCache(queueName, "");
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

        private void ExpireSolutionThreadSafe(bool recompute = false)
        {
            var delegated = new ExpireSolutionDelegate(ExpireSolution);
            RhinoApp.InvokeOnUiThread(delegated, recompute);
        }

        /// <summary>
        /// Converts a Grasshopper data tree with points into a list of lists
        /// </summary>
        /// <param name="pointTree"></param>
        /// <returns></returns>
        private static List<List<List<double>>> ConvertToPointList(GH_Structure<GH_Point> pointTree)
        {
            var convertedPoints = new List<List<List<double>>>();
            foreach (var branch in pointTree.Branches)
            {
                var branchPoints = new List<List<double>>();
                foreach (var point in branch)
                {
                    branchPoints.Add(new List<double> {point.Value.X, point.Value.Y, point.Value.Z});
                }

                ;
                convertedPoints.Add(branchPoints);
            }

            return convertedPoints;
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override Bitmap Icon => Resources.IconMesh;

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("2f0bdda2-f7eb-4fc7-8bf0-a2fd5a787493");
    }
}