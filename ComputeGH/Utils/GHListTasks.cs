//using System;
//using System.Activities;
//using System.Collections.Generic;
//using System.Drawing;
//using System.Linq;
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

//namespace ComputeGH.Utils
//{
//    public class GHListTasks : PB_Component
//    {
//                /// <summary>
//        /// Initializes a new instance of the computeLogin class.
//        /// </summary>
//        public GHListTasks()
//            : base("List Project Tasks", "List Tasks",
//                "List the tasks under a specific project on Procedural Compute",
//                "Compute", "Utils")
//        {
//        }

//        /// <summary>
//        /// Registers all the input parameters for this component.
//        /// </summary>
//        protected override void RegisterInputParams(GH_InputParamManager pManager)
//        {
//            pManager.AddTextParameter("Auth", "Auth", "Authentication from the Compute Login component",
//                GH_ParamAccess.item);
//            pManager.AddTextParameter("ProjectName", "ProjectName", "Project Name", GH_ParamAccess.item);
//            pManager.AddIntegerParameter("ProjectNumber", "ProjectNumber", "Project  Number", GH_ParamAccess.item);
//            pManager.AddBooleanParameter("Exclude Children", "Exclude Children", "If true only parent task will be returned", GH_ParamAccess.item, true);
//            pManager.AddBooleanParameter("Retry", "Retry", "Try again", GH_ParamAccess.item, false);

//            pManager[2].Optional = true;
//            pManager[3].Optional = true;
//            pManager[4].Optional = true;
//        }

//        /// <summary>
//        /// Registers all the output parameters for this component.
//        /// </summary>
//        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
//        {
//            pManager.AddTextParameter("Output", "Output", "Output", GH_ParamAccess.list);
//        }

//        /// <summary>
//        /// This is the method that actually does the work.
//        /// </summary>
//        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
//        protected override void SolveInstance(IGH_DataAccess DA)
//        {
//            string auth = null;
//            string projectName = null;
//            int? projectNumber = null;
//            var excludeChildren = true;
//            var refresh = false;

//            if (!DA.GetData(0, ref auth)) return;
//            if (!DA.GetData(1, ref projectName)) return;
//            DA.GetData(2, ref projectNumber);
//            DA.GetData(3, ref excludeChildren);
//            DA.GetData(4, ref refresh);

//            // Get Cache to see if we already did this
//            var cacheKey = projectName + projectNumber + excludeChildren;
//            var cachedValues = StringCache.getCache(cacheKey);
//            DA.DisableGapLogic();

//            if (cachedValues == null || refresh == true)
//            {
//                var queueName = "ListTasks" + cacheKey;

//                // Get queue lock
//                var queueLock = StringCache.getCache(queueName);
//                if (queueLock != "true")
//                {
//                    StringCache.setCache(queueName, "true");
//                    StringCache.setCache(cacheKey, null);
//                    QueueManager.addToQueue(queueName, () =>
//                    {
//                        try
//                        {
//                            cachedValues = ProjectAndTask.GetTasks(
//                                auth,
//                                projectName,
//                                projectNumber,
//                                excludeChildren
//                            );
//                            StringCache.setCache(cacheKey, cachedValues);
//                            StringCache.setCache(this.InstanceGuid.ToString(), "");
//                        }
//                        catch (NoObjectFoundException)
//                        {
//                            StringCache.setCache(cacheKey + "create", "");
//                        }
//                        catch (Exception e)
//                        {
//                            StringCache.setCache(InstanceGuid.ToString(), e.Message);
//                            StringCache.setCache(cacheKey, "error");
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
//                var outputs = cachedValues.Split(';');
//                if (outputs.Length > 1)
//                {
//                    outputs = outputs.OrderBy(task => task).ToArray();
//                }
//                DA.SetDataList(0, outputs);
//            }
//        }

//        /// <summary>
//        /// Provides an Icon for the component.
//        /// </summary>
//        protected override Bitmap Icon => Resources.IconFolder;

//        /// <summary>
//        /// Gets the unique ID for this component. Do not change this ID after release.
//        /// </summary>
//        public override Guid ComponentGuid => new Guid("ca0f366b-df11-46d3-8351-219eb01a627a");
//    }
//}