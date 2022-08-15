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
//using Grasshopper.Kernel.Parameters;
//using Rhino;

//namespace ComputeGH.Radiation
//{
//    public class GHDaylightMetrics : PB_Component
//    {
//        public GHDaylightMetrics() : base("Daylight Metrics", "Daylight Metrics",
//            "Create a task to compute Daylight Metrics, such as cDA, sDA, DA or UDI", "Compute", "Radiation")
//        {
//        }

//        /// <summary>
//        /// Registers all the input parameters for this component.
//        /// </summary>
//        protected override void RegisterInputParams(GH_InputParamManager pManager)
//        {
//            pManager.AddTextParameter("Input", "Input", "Input from previous Compute Component", GH_ParamAccess.item);
//            pManager.AddIntegerParameter("Preset", "Preset", "Select a Daylight Metric", GH_ParamAccess.item,
//                0);
//            pManager.AddTextParameter("Overrides", "Overrides",
//                "Optional overrides to apply to the presets.\n" +
//                "The overrides should be given in the following JSON format:\n" +
//                "{\n" +
//                "    \"threshold\": 500,\n" +
//                "    \"work_hours\": [8, 16],\n" +
//                "    \"work_days\": [0, 5],\n" +
//                "    \"selected_hours\": [],\n" +
//                "    \"file_extension\": \".res\"\n" +
//                " }\n" +
//                "Here shown with the defaults.\n" +
//                "\"selected_hours\" is a list of true/false values that represents whether or not that hour should be included in the calculation.\n" +
//                "In case it is given then it will override the \"work_hours\" and \"work_days\".",
//                GH_ParamAccess.item, "");
//            pManager.AddIntegerParameter("CPUs", "CPUs",
//                "CPUs to use. Valid choices are:\n1, 2, 4, 8, 16, 18, 24, 36, 48, 64, 72, 96.",
//                GH_ParamAccess.item, 1);
//            pManager.AddTextParameter("Case Directory", "Case Dir",
//                "Folder to save results in on the Compute server. Default is metrics", GH_ParamAccess.item, "metrics");
//            pManager.AddBooleanParameter("Create", "Create", "Run the case on Procedural Compute",
//                GH_ParamAccess.item, false);

//            pManager[1].Optional = true;
//            pManager[2].Optional = true;
//            pManager[3].Optional = true;
//            pManager[4].Optional = true;
//            pManager[5].Optional = true;

//            AddNamedValues(pManager[1] as Param_Integer, Presets);
//        }

//        private static void AddNamedValues(Param_Integer param, List<string> values)
//        {
//            var index = 0;
//            foreach (var value in values)
//            {
//                param.AddNamedValue(value, index);
//                index++;
//            }
//        }

//        /// <summary>
//        /// Registers all the output parameters for this component.
//        /// </summary>
//        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
//        {
//            pManager.AddTextParameter("Output", "Output", "Output", GH_ParamAccess.list);
//        }

//        /// <summary>
//        /// Provides an Icon for every component that will be visible in the User Interface.
//        /// Icons need to be 24x24 pixels.
//        /// </summary>
//        protected override Bitmap Icon => Resources.IconBoundaryCondition;

//        /// <summary>
//        /// Each component must have a unique Guid to identify it. 
//        /// It is vital this Guid doesn't change otherwise old ghx files 
//        /// that use the old ID will partially fail during loading.
//        /// </summary>
//        public override Guid ComponentGuid => new Guid("60e4b10a-52f9-4e50-853a-d9c4512d13cc");

//        /// <summary>
//        /// This is the method that actually does the work.
//        /// </summary>
//        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
//        /// to store data in output parameters.</param>
//        protected override void SolveInstance(IGH_DataAccess DA)
//        {
//            string inputJson = null;
//            var overrides = "";
//            var caseDir = "metrics";
//            var preset_ = 0;
//            var compute = false;
//            var cpus = 1;

//            if (!DA.GetData(0, ref inputJson)) return;
//            if (inputJson == "error") return;
//            if (!DA.GetData(1, ref preset_))
//            {
//                return;
//            }

//            var preset = Presets[preset_];

//            if (!DA.GetData(2, ref overrides))
//            {
//                return;
//            }

//            DA.GetData(3, ref cpus);
//            DA.GetData(4, ref caseDir);
//            DA.GetData(5, ref compute);

//            // Get Cache to see if we already did this
//            var cacheKey = inputJson;
//            var cachedValues = StringCache.getCache(cacheKey);
//            DA.DisableGapLogic();

//            if (cachedValues == null || compute)
//            {
//                var queueName = "daylightMetric" + cacheKey;
//                StringCache.setCache(InstanceGuid.ToString(), "");

//                // Get queue lock
//                var queueLock = StringCache.getCache(queueName);
//                if (queueLock != "true")
//                {
//                    StringCache.setCache(queueName, "true");
//                    QueueManager.addToQueue(queueName, () =>
//                    {
//                        try
//                        {
//                            cachedValues = DaylightMetrics.Create(inputJson, overrides, preset,
//                                ComponentUtils.ValidateCPUs(cpus), caseDir, compute);
//                            StringCache.setCache(cacheKey, cachedValues);
//                            if (compute)
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
//                var outputs = cachedValues;
//                DA.SetData(0, outputs);
//                Message = "";
//                if (StringCache.getCache(cacheKey + "create") == "true")
//                {
//                    Message = "Task Created";
//                }
//            }
//        }

//        private static readonly List<string> Presets = new List<string>
//        {
//            "daylight_autonomy",
//            "spatial_daylight_autonomy",
//            "continuous_daylight_autonomy",
//            "useful_daylight_illuminances",
//            "statistics",
//            "seasonal_statistics"
//        };
//    }
//}