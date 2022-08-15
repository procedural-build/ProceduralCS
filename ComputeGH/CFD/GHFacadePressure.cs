//using System;
//using System.Collections.Generic;
//using System.Drawing;
//using System.Threading;
//using ComputeCS.Components;
//using ComputeCS.Exceptions;
//using ComputeCS.utils.Cache;
//using ComputeCS.utils.Queue;
//using ComputeGH.Grasshopper.Utils;
//using ComputeGH.Properties;
//using Grasshopper.Kernel;

//namespace ComputeGH.CFD
//{
//    public class GHFacadePressure : PB_Component
//    {
//        /// <summary>
//        /// Initializes a new instance of the WindThresholds class.
//        /// </summary>
//        public GHFacadePressure()
//            : base("Facade Pressure", "Facade Pressure",
//                "Calculate pressure coefficients to use in fx. EnergyPlus airflow networks",
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
//            pManager.AddTextParameter("Probe Names", "Probes",
//                "Give names to each branch of in the points tree. The names can later be used to identify the points.",
//                GH_ParamAccess.list);
//            pManager.AddIntegerParameter("CPUs", "CPUs",
//                "CPUs to use. Valid choices are:\n1, 2, 4, 8, 16, 18, 24, 36, 48, 64, 72, 96", GH_ParamAccess.item, 4);
//            pManager.AddTextParameter("DependentOn", "DependentOn",
//                "By default the Pressure Coefficient task is dependent on a Probe task. If you want it to be dependent on another task. Please supply the name of that task here.",
//                GH_ParamAccess.item, "Probe");
//            pManager.AddTextParameter("Overrides", "Overrides", "Optional overrides to apply to the presets.",
//                GH_ParamAccess.item, "");
//            pManager.AddBooleanParameter("Create", "Create",
//                "Whether to create a new Wind Threshold task, if one doesn't exist", GH_ParamAccess.item, false);

//            pManager[2].Optional = true;
//            pManager[3].Optional = true;
//            pManager[4].Optional = true;
//            pManager[5].Optional = true;
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
//            var probes = new List<string>();
//            var cpus = 4;
//            var dependentOn = "Probe";
//            var overrides = "";
//            var create = false;

//            if (!DA.GetData(0, ref inputJson)) return;
//            if (inputJson == "error") return;
//            if (!DA.GetDataList(1, probes)) return;
//            DA.GetData(2, ref cpus);
//            DA.GetData(3, ref dependentOn);
//            DA.GetData(4, ref overrides);
//            DA.GetData(5, ref create);

//            // Get Cache to see if we already did this
//            var cacheKey = string.Join("", probes) + dependentOn + inputJson + overrides;
//            var cachedValues = StringCache.getCache(cacheKey);
//            DA.DisableGapLogic();

//            if (cachedValues == null || create)
//            {
//                const string queueName = "pressureCoefficients";

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
//                            cachedValues = PressureCoefficient.CreatePressureCoefficientTask(
//                                inputJson,
//                                probes,
//                                ComponentUtils.ValidateCPUs(cpus),
//                                dependentOn,
//                                overrides,
//                                create
//                            );
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
//                            StringCache.AppendCache(InstanceGuid.ToString(), e.Message + "\n");
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

//            Message = "";

//            // Read from Cache
//            if (cachedValues != null)
//            {
//                DA.SetData(0, cachedValues);
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
//        public override Guid ComponentGuid => new Guid("5d97e2d0-70ac-4683-bd16-86b529c60420");
//    }
//}