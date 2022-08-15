//using System;
//using System.Collections.Generic;
//using System.Drawing;
//using System.Threading;
//using ComputeCS.Components;
//using ComputeCS.Exceptions;
//using ComputeCS.Grasshopper;
//using ComputeCS.utils.Cache;
//using ComputeCS.utils.Queue;
//using ComputeGH.Grasshopper.Utils;
//using ComputeGH.Properties;
//using Grasshopper.Kernel;
//using Rhino;

//namespace ComputeGH.CFD
//{
//    public class GHWindThresholds : PB_Component
//    {
//        /// <summary>
//        /// Initializes a new instance of the WindThresholds class.
//        /// </summary>
//        public GHWindThresholds()
//            : base("Wind Thresholds", "Wind Thresholds",
//                "Compute the Lawson criteria for a CFD case.",
//                "Compute", "CFD")
//        {
//        }

//        /// <summary>
//        /// Registers all the input parameters for this component.
//        /// </summary>
//        protected override void RegisterInputParams(GH_InputParamManager pManager)
//        {
//            pManager.AddTextParameter("Input", "Input", "Input from the previous Compute Component",
//                GH_ParamAccess.item);
//            pManager.AddTextParameter("EPW File", "EPW File", "Path to where the EPW file is located.",
//                GH_ParamAccess.item);
//            pManager.AddTextParameter("Probe Names", "Probes",
//                "Give names to each branch of in the points tree. The names can later be used to identify the points.",
//                GH_ParamAccess.list);
//            pManager.AddTextParameter("Thresholds", "Thresholds",
//                "Thresholds for different wind comfort categories. Input should be a list JSON formatted strings, " +
//                "with the fields: \"field\" and \"value\", respectively describing the category name and threshold value." +
//                "\nThe default values corresponds to the Lawson 2001 Criteria",
//                GH_ParamAccess.list,
//                new List<string>
//                {
//                    "{\"field\": \"sitting\", \"value\": 4}",
//                    "{\"field\": \"standing\", \"value\": 6}",
//                    "{\"field\": \"strolling\", \"value\": 8}",
//                    "{\"field\": \"business_walking\", \"value\": 10}",
//                    "{\"field\": \"uncomfortable\", \"value\": 10}",
//                    "{\"field\": \"unsafe_frail\", \"value\": 15}",
//                    "{\"field\": \"unsafe_all\", \"value\": 20}"
//                });
//            pManager.AddIntegerParameter("CPUs", "CPUs",
//                "CPUs to use. Valid choices are:\n1, 2, 4, 8, 16, 18, 24, 36, 48, 64, 72, 96", GH_ParamAccess.item, 4);
//            pManager.AddTextParameter("DependentOn", "DependentOn",
//                "By default the probe task is dependent on a wind tunnel task or a task running simpleFoam. If you want it to be dependent on another task. Please supply the name of that task here.",
//                GH_ParamAccess.item, "Probe");
//            pManager.AddBooleanParameter("Create", "Create",
//                "Whether to create a new Wind Threshold task, if one doesn't exist", GH_ParamAccess.item, false);

//            pManager[2].Optional = true;
//            pManager[3].Optional = true;
//            pManager[4].Optional = true;
//            pManager[5].Optional = true;
//            pManager[6].Optional = true;
//        }

//        /// <summary>
//        /// Registers all the output parameters for this component.
//        /// </summary>
//        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
//        {
//            pManager.AddTextParameter("Output", "Output", "Output", GH_ParamAccess.item);
//        }

//        /// <summary>
//        /// This is the method that actually does the work.
//        /// </summary>
//        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
//        protected override void SolveInstance(IGH_DataAccess DA)
//        {
//            string inputJson = null;
//            var epwFile = "";
//            var patches = new List<string>();
//            var thresholds = new List<string>();
//            var cpus = 4;
//            var dependentOn = "Probe";
//            var create = false;

//            if (!DA.GetData(0, ref inputJson)) return;
//            if (inputJson == "error") return;
//            if (!DA.GetData(1, ref epwFile)) return;
//            if (!DA.GetDataList(2, patches))
//            {
//                patches.Add("set1");
//            }

//            DA.GetDataList(3, thresholds);
//            DA.GetData(4, ref cpus);

//            DA.GetData(5, ref dependentOn);
//            DA.GetData(6, ref create);

//            // Get Cache to see if we already did this
//            var cacheKey = string.Join("", patches) + epwFile + inputJson;
//            var cachedValues = StringCache.getCache(cacheKey);
//            DA.DisableGapLogic();

//            if (cachedValues == null || create)
//            {
//                var queueName = "windThreshold";

//                // Get queue lock
//                var queueLock = StringCache.getCache(queueName);
//                if (queueLock != "true")
//                {
//                    StringCache.setCache(queueName, "true");
//                    StringCache.setCache(InstanceGuid.ToString(), "");
//                    StringCache.setCache(cacheKey, null);
//                    QueueManager.addToQueue(queueName, () =>
//                    {
//                        try
//                        {
//                            var results = WindThreshold.ComputeWindThresholds(
//                                inputJson,
//                                epwFile,
//                                patches,
//                                thresholds,
//                                ComponentUtils.ValidateCPUs(cpus),
//                                dependentOn,
//                                create
//                            );
//                            cachedValues = results;
//                            StringCache.setCache(cacheKey, cachedValues);
//                            if (create)
//                            {
//                                StringCache.setCache(cacheKey + "create", "true");
//                            }
//                        }
//                        catch (NoObjectFoundException)
//                        {
//                            StringCache.setCache(cacheKey + "create", "");
//                        }
//                        catch (Exception e)
//                        {
//                            StringCache.setCache(InstanceGuid.ToString(), e.Message);
//                            StringCache.setCache(cacheKey, "error");
//                            StringCache.setCache(cacheKey + "create", "");
//                        }


//                        ExpireSolutionThreadSafe(true);
//                        Thread.Sleep(2000);
//                        StringCache.setCache(queueName, "");
//                    });
//                }
//            }

//            HandleErrors();

//            // Read from Cache
//            if (cachedValues != null)
//            {
//                DA.SetData(0, cachedValues);
//                Message = "";
//                if (StringCache.getCache(cacheKey + "create") == "true")
//                {
//                    Message = "Task Created";
//                }
//            }
//        }

//        /// <summary>
//        /// Provides an Icon for the component.
//        /// </summary>
//        protected override Bitmap Icon => Resources.IconSolver;

//        /// <summary>
//        /// Gets the unique ID for this component. Do not change this ID after release.
//        /// </summary>
//        public override Guid ComponentGuid => new Guid("235182ea-f739-4bea-be24-907de80e58fa");
//    }
//}