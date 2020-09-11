using System;
using System.Drawing;
using ComputeCS.Components;
using ComputeCS.utils.Cache;
using ComputeGH.Properties;
using Grasshopper.Kernel;
using Rhino;

namespace ComputeCS.Grasshopper
{
    public class GHUploadFile : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the computeLogin class.
        /// </summary>
        public GHUploadFile()
            : base("Upload File", "Upload File",
                "Upload a file to Compute",
                "Compute", "Utils")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Input", "Input", "Input from previous Compute Component", GH_ParamAccess.item);
            pManager.AddTextParameter("Upload Path", "Upload Path", "The path on Compute you want to write to.",
                GH_ParamAccess.item);
            pManager.AddTextParameter("Text", "Text", "The text you want to write.", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Upload", "Upload", "Upload the file to Compute", GH_ParamAccess.item);

            pManager[3].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Url", "Url",
                "Url to the file on Compute. You can copy this url directly into your browser to see the file on Compute.",
                GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string input = null;
            string uploadPath = null;
            string text = null;
            var upload = false;

            if (!DA.GetData(0, ref input)) return;
            if (!DA.GetData(1, ref uploadPath)) return;
            if (!DA.GetData(2, ref text)) return;
            DA.GetData(3, ref upload);

            // Get Cache to see if we already did this
            var cacheKey = input + uploadPath + text;
            var cachedValues = StringCache.getCache(cacheKey);
            DA.DisableGapLogic();

            var response = string.Empty;
            if (cachedValues == null || upload == true)
            {
                const string queueName = "upload";

                // Get queue lock
                var queueLock = StringCache.getCache(queueName);
                if (queueLock != "true")
                {
                    try
                    {
                        cachedValues = UploadFile.UploadTextFile(input, uploadPath, text, upload);
                        StringCache.setCache(cacheKey, cachedValues);
                        StringCache.setCache(this.InstanceGuid.ToString(), "");
                        if (upload)
                        {
                            StringCache.setCache(cacheKey + "create", "true");
                        }
                    }
                    catch (Exception e)
                    {
                        StringCache.AppendCache(this.InstanceGuid.ToString(), e.Message);
                        StringCache.setCache(cacheKey, "error");
                        StringCache.setCache(cacheKey + "create", "");
                    }

                    StringCache.setCache(queueName, "");
                    ExpireSolutionThreadSafe(true);
                }
            }


            // Handle Errors
            var errors = StringCache.getCache(this.InstanceGuid.ToString());
            if (!string.IsNullOrEmpty(errors))
            {
                throw new Exception(errors);
            }

            // Read from Cache
            if (cachedValues != null)
            {
                DA.SetData(0, cachedValues);
                Message = "";
                if (StringCache.getCache(cacheKey + "create") == "true")
                {
                    Message = "File Uploaded";
                }
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
        protected override Bitmap Icon
        {
            get
            {
                if (Environment.GetEnvironmentVariable("RIDER") == "true")
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
            get { return new Guid("bd8c3373-c4c7-4267-a03b-13c803d7678c"); }
        }
    }
}