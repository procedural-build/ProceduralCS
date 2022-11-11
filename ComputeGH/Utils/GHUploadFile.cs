using ComputeCS.Components;
using ComputeGH.Grasshopper.Utils;
using ComputeGH.Properties;
using Grasshopper.Kernel;
using System;
using System.Drawing;
using System.Threading.Tasks;

namespace ComputeCS.Grasshopper
{
    public class UploadFileResult
    {
        public string Url { get; set; }
        public Exception Errors { get; set; }
    }
    public class GHUploadFile : PB_TaskCapableComponent<UploadFileResult>
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

        public override Task<UploadFileResult> CreateTask(IGH_DataAccess DA)
        {
            string input = null;
            string uploadPath = null;
            string text = null;
            var upload = false;

            if (!DA.GetData(0, ref input)) return DefaultTask();
            if (!DA.GetData(1, ref uploadPath)) return DefaultTask();
            if (!DA.GetData(2, ref text)) return DefaultTask();
            DA.GetData(3, ref upload);

            return Task.Run(() => DoUpload(input, uploadPath, text, upload));
        }

        public override void SetOutputData(IGH_DataAccess DA, UploadFileResult result)
        {
            if (result.Errors != null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, result.Errors.Message);
                return;
            }
            DA.SetData(0, result.Url);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override Bitmap Icon => Resources.IconFolder;

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("bd8c3373-c4c7-4267-a03b-13c803d7678c");

        private UploadFileResult DoUpload(string inputJson, string uploadPath, string text, bool upload)
        {
            try
            {
                return new UploadFileResult
                {
                    Url = UploadFile.UploadTextFile(inputJson, uploadPath, text, upload)
                };
            }
            catch (Exception e)
            {
                return new UploadFileResult
                {
                    Errors = e
                };
            }
        }
    }
}