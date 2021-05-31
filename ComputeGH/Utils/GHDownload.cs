using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using ComputeCS.Components;
using ComputeCS.types;
using ComputeCS.utils.Cache;
using ComputeCS.utils.Queue;
using ComputeGH.Properties;
using Grasshopper.Kernel;
using Rhino;

namespace ComputeCS.Grasshopper
{
    public class Download : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the computeLogin class.
        /// </summary>
        public Download()
          : base("Download", "Download",
              "Download files or folders from Compute. This component will keep polling the Compute server until the files are available. If you reload this component it will check if the files on the server matches the local files and download them if needed.",
              "Compute", "Utils")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Input", "Input", "Input from previous Compute Component", GH_ParamAccess.item);
            pManager.AddTextParameter("Download Path", "Download Path", "The path from Compute to download. You can chose both a file or a folder to download.", GH_ParamAccess.list);
            pManager.AddTextParameter("Local Path", "Local Path", "The local path where to you want the download content to be stored.", GH_ParamAccess.item);
            pManager.AddTextParameter("Overrides", "Overrides", 
                "Optional overrides to apply to the download.\n" +
                "The overrides lets you exclude or include files from the server in the provided path, that you want to download.\n" +
                "If you want to exclude all files that ends with '.txt', then you can do that with: {\"exclude\": [\".txt\"]}\n" +
                "The overrides takes a JSON formatted string as follows:\n" +
                "{\n" +
                "    \"exclude\": List[string],\n" +
                "    \"include\": List[string],\n" +
                "\n}",
                GH_ParamAccess.item, "");
            pManager.AddBooleanParameter("Reload", "Reload", "Redownload the content from Compute", GH_ParamAccess.item);
            
            pManager[3].Optional = true;
            pManager[4].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Path", "Path", "If the download succeeded then this will give you the path it was downloaded to.", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string input = null;
            var downloadPaths = new List<string>();
            string localPath = null;
            var overrides = "";
            var reload = false;

            if (!DA.GetData(0, ref input)) return;
            if (!DA.GetDataList(1, downloadPaths)) return;
            if (!DA.GetData(2, ref localPath)) return;
            DA.GetData(3, ref overrides);
            DA.GetData(4, ref reload);

            if (!Directory.Exists(localPath))
            {
                Directory.CreateDirectory(localPath);
            }
            
            // Get Cache to see if we already did this
            foreach (var downloadPath in downloadPaths)
            {
                var cacheKey = input + downloadPath;
                var cachedValues = StringCache.getCache(cacheKey);
                DA.DisableGapLogic();

                if (cachedValues == null || reload)
                {
                    PollDownloadContent(input, downloadPath, localPath, overrides, reload, cacheKey);
                }                
            }

            var cacheKeys = downloadPaths.Select(path => input + path);
            
            if (Directory.Exists(localPath) && cacheKeys.All(cacheKey => StringCache.getCache(cacheKey) == "True"))
            {
                DA.SetData(0, localPath);
            }
            else
            {
                DA.SetData(0, null);
            }

            // Handle Errors
            var errors = StringCache.getCache(InstanceGuid.ToString());
            if (!string.IsNullOrEmpty(errors))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, errors);
            }
            
            Message = StringCache.getCache(InstanceGuid.ToString() + "progress");

        }

        private void PollDownloadContent(
            string inputJson,
            string downloadPath,
            string localPath,
            string overrides,
            bool reload,
            string cacheKey
            )
        {
            var queueName = "Download" + cacheKey;

            // Get queue lock
            var queueLock = StringCache.getCache(queueName);
            var downloaded = false;
            var inputData = new Inputs().FromJson(inputJson);
            var instanceId = InstanceGuid.ToString();
            if (reload)
            {
                StringCache.setCache(instanceId, "");
            }

            if (queueLock != "true" && inputData.Task != null)
            {
                StringCache.setCache(queueName, "true");
                StringCache.setCache(cacheKey, null);
                QueueManager.addToQueue(queueName, () => {
                    try
                    {
                        while (!downloaded)
                        {
                            StringCache.setCache(instanceId + "progress", "Downloading...");
                            ExpireSolutionThreadSafe(true);
                            
                            downloaded = DownloadContent.Download(inputJson, downloadPath, localPath, overrides);
                            StringCache.setCache(cacheKey, downloaded.ToString());
                            
                            if (!downloaded)
                            {
                                StringCache.setCache(instanceId + "progress", "Waiting for results...");
                                ExpireSolutionThreadSafe(true);
                                Thread.Sleep(60000);
                            }
                            else { StringCache.setCache(instanceId + "progress", "Downloaded files");}
                            ExpireSolutionThreadSafe(true);
                        }

                    }
                    catch (Exception e)
                    {
                        StringCache.setCache(instanceId, e.Message);
                        StringCache.setCache(cacheKey, "error");
                    }
                    ExpireSolutionThreadSafe(true);
                    Thread.Sleep(2000);
                    StringCache.setCache(queueName, "");
                });
                
            }
        }

        private void ExpireSolutionThreadSafe(bool recompute = false)
        {
            var delegated = new ExpireSolutionDelegate(ExpireSolution);
            RhinoApp.InvokeOnUiThread(delegated, recompute);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override Bitmap Icon => Resources.IconFolder;

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("64d78c45-6eda-41e3-a52f-97ff06ddff0a");
    }
}
