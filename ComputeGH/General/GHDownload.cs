using ComputeCS.Components;
using ComputeGH.Grasshopper.Utils;
using ComputeGH.Params;
using ComputeGH.Properties;
using Grasshopper.Kernel;
using System;
using System.Drawing;
using System.Threading.Tasks;

namespace ComputeCS.Grasshopper
{
    public class DownloadResult
    {
        public DownloadContentResponse Value { get; set; }
        public Exception Errors { get; set; }
    }
    public class Download : PB_TaskCapableComponent<DownloadResult>
    {
        /// <summary>
        /// Initializes a new instance of the computeLogin class.
        /// </summary>
        public Download()
          : base("Download", "Download",
              "Download files or folders from Compute. This component will return the files found on the Compute server.",
              "Compute", "General")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Input", "Input", "Input from previous Compute Component", GH_ParamAccess.item);
            pManager.AddTextParameter("Download Path", "Download Path", "The path from Compute to download. You can chose both a file or a folder to download.", GH_ParamAccess.item);
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

            pManager[2].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddParameter(new DownloadFileParam(), "Downloaded files", "Files", "The files downloaded from Procedural computs", GH_ParamAccess.list);
        }

        public override Task<DownloadResult> CreateTask(IGH_DataAccess DA)
        {
            string input = null;
            string downloadPath = null;
            var overrides = "";

            if (!DA.GetData(0, ref input)) return DefaultTask();
            if (input == "error") return DefaultTask();
            if (!DA.GetData(1, ref downloadPath)) return DefaultTask();
            DA.GetData(2, ref overrides);

            return Task.Run(() => DoDownload(input, downloadPath, overrides));
        }

        public override void SetOutputData(IGH_DataAccess DA, DownloadResult result)
        {
            if (result.Errors != null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, result.Errors.Message);
                return;
            }
            else if (!result.Value.FilesFoundForTask)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Found no files on Compute server");
                return;
            }

            DA.SetDataList(0, result.Value.Files);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override Bitmap Icon => Resources.IconFolder;

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("64d78c45-6eda-41e3-a52f-97ff06ddff0a");

        private DownloadResult DoDownload(string inputJson, string downloadPath, string overrides)
        {
            try
            {
                return new DownloadResult
                {
                    Value = DownloadContent.Download(inputJson, downloadPath, overrides)
                };
            }
            catch (Exception e)
            {
                return new DownloadResult
                {
                    Errors = e
                };
            }
        }
    }
}
