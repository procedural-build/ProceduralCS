using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Grasshopper.Kernel;
using Rhino.Geometry;
using ComputeCS.types;
using System.IO;
using ComputeGH.Properties;
using ComputeCS.utils.Cache;
using ComputeCS.utils.Queue;

namespace ComputeCS.Grasshopper
{
    public class Download : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the computeLogin class.
        /// </summary>
        public Download()
          : base("Download", "Download",
              "Download files or folders from Compute",
              "Compute", "Utils")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Input", "Input", "Input from previous Compute Component", GH_ParamAccess.item);
            pManager.AddTextParameter("Download Path", "Download Path", "The path from Compute to download. You can chose both a file or a folder to download.", GH_ParamAccess.item);
            pManager.AddTextParameter("Local Path", "Local Path", "The local path where to you want the download content to be stored.", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Reload", "Reload", "Redownload the content from Compute", GH_ParamAccess.item);
            
            pManager[3].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Path", "Path", "If the download succeded then this will give you the path it was downloaded to.", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string input = null;
            string downloadPath = null;
            string localPath = null;
            bool reload = false;

            if (!DA.GetData(0, ref input)) return;
            if (!DA.GetData(1, ref downloadPath)) return;
            if (!DA.GetData(2, ref localPath)) return;
            DA.GetData(3, ref reload);

            // Get Cache to see if we already did this
            var cacheKey = input + downloadPath;
            var cachedValues = StringCache.getCache(cacheKey);
            DA.DisableGapLogic();

            if (cachedValues == null || reload == true)
            {
                PollDownloadContent(input, downloadPath, localPath, reload, cacheKey);
            }

            if (!Directory.Exists(localPath))
            {
                Directory.CreateDirectory(localPath);
            }
            
            var newPath = Path.Combine(localPath, downloadPath.Split('/').Last());
            if (cachedValues == "True" || Directory.Exists(newPath))
            {
                DA.SetData(0, newPath);
            }
            else
            {
                DA.SetData(0, null);
            }

            // Handle Errors
            var errors = StringCache.getCache(this.InstanceGuid.ToString());
            if (errors != null)
            {
                throw new Exception(errors);
            }

        }

        private void PollDownloadContent(
            string inputJson,
            string downloadPath,
            string localPath,
            bool reload,
            string cacheKey
            )
        {
            var queueName = "Download";

            // Get queue lock
            var queueLock = StringCache.getCache(queueName);
            var downloaded = false;
            var inputData = new Inputs().FromJson(inputJson);


            if (queueLock != "true" && inputData.Task != null)
            {
                StringCache.setCache(queueName, "true");
                StringCache.setCache(cacheKey, null);
                QueueManager.addToQueue(queueName, () => {
                    try
                    {
                        while (!downloaded)
                        {
                            downloaded = Components.DownloadContent.Download(inputJson, downloadPath, localPath, reload);
                            StringCache.setCache(cacheKey, downloaded.ToString());
                            if (!downloaded)
                            {
                                Thread.Sleep(60000);
                            }
                        }

                    }
                    catch (Exception e)
                    {
                        StringCache.AppendCache(this.InstanceGuid.ToString(), e.ToString() + "\n");
                        StringCache.setCache(cacheKey, "error");
                    }
                    StringCache.setCache(queueName, "");
                    ExpireSolutionThreadSafe(true);
                });
                
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
                if (System.Environment.GetEnvironmentVariable("RIDER") == "true")
                {
                    return null;
                }
                return Resources.IconFolder;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("64d78c45-6eda-41e3-a52f-97ff06ddff0a"); }
        }
    }
}
