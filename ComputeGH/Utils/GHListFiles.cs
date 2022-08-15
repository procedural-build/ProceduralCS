//using System;
//using System.Drawing;
//using System.Linq;
//using System.Threading;
//using ComputeCS.Components;
//using ComputeCS.Grasshopper;
//using ComputeCS.utils.Cache;
//using ComputeCS.utils.Queue;
//using ComputeGH.Grasshopper.Utils;
//using ComputeGH.Properties;
//using Grasshopper.Kernel;
//using Rhino;

//namespace ComputeGH.Utils
//{
//    public class GHListFiles : PB_Component
//    {
//        /// <summary>
//        /// Initializes a new instance of the GHAnalysisMesh class.
//        /// </summary>
//        public GHListFiles() : base("File List", "File List", "Get a list of the files from a certain task on Compute.",
//              "Compute", "Utils"){}

//        /// <summary>
//        /// Registers all the input parameters for this component.
//        /// </summary>
//        protected override void RegisterInputParams(GH_InputParamManager pManager)
//        {
//            pManager.AddTextParameter("Input", "Input", "Input from previous Compute Component", GH_ParamAccess.item);
//            pManager.AddTextParameter("Exclude Pattern", "Exclude", "Files to exclude. Provide a pattern and the component will exclude them from being listed. If you want to exclude all files that ends with '.txt', then you can do that with: 'txt'", GH_ParamAccess.item);
//            pManager.AddTextParameter("Include Pattern", "Include", "Files to only include. Provide a pattern and the component will on include files that matches that pattern.", GH_ParamAccess.item);
//            pManager.AddBooleanParameter("Reload", "Reload", "Reload the content from Compute", GH_ParamAccess.item);

//            pManager[1].Optional = true;
//            pManager[2].Optional = true;
//            pManager[3].Optional = true;

//        }

//        /// <summary>
//        /// Registers all the output parameters for this component.
//        /// </summary>
//        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
//        {
//            pManager.AddTextParameter("FileList", "FileList", "List of files on the server at the specified path.", GH_ParamAccess.list);
//        }

//        /// <summary>
//        /// This is the method that actually does the work.
//        /// </summary>
//        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
//        protected override void SolveInstance(IGH_DataAccess DA)
//        {
//            var inputJson = "";
//            var exclude = "";
//            var include = "";
//            var rerun = false;

//            if (!DA.GetData(0, ref inputJson)) return;
//            DA.GetData(1, ref exclude);
//            DA.GetData(2, ref include);
//            DA.GetData(3, ref rerun);

//            // Get Cache to see if we already did this
//            var cacheKey = inputJson + "exc" + exclude + "inc" + include;
//            var cachedValues = StringCache.getCache(cacheKey);
//            DA.DisableGapLogic();

//            if (cachedValues == null || rerun == true)
//            {
//                var queueName = "FileList" + cacheKey;

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
//                            var results = FileList.GetFileList(
//                                inputJson,
//                                exclude,
//                                include
//                            );
//                            cachedValues = results;
//                            StringCache.setCache(cacheKey, cachedValues);
//                            StringCache.setCache(InstanceGuid.ToString(), "");
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

//            // Read from Cache
//            if (cachedValues != null)
//            {
//                var outputs = cachedValues.Split(',');
//                if (outputs.Length > 1)
//                {
//                    outputs = outputs.OrderBy(file => file).ToArray();
//                }
//                DA.SetDataList(0, outputs);
//            }

//            HandleErrors();
//        }

//        /// <summary>
//        /// Provides an Icon for the component.
//        /// </summary>
//        protected override Bitmap Icon => Resources.IconFolder;

//        /// <summary>
//        /// Gets the unique ID for this component. Do not change this ID after release.
//        /// </summary>
//        public override Guid ComponentGuid => new Guid("5581eac6-bd0c-4ace-92b9-8f638ec591f4");
//    }
//}