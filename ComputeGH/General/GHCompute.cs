using ComputeCS.Components;
using ComputeGH.Grasshopper.Utils;
using ComputeGH.Properties;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ComputeCS.Grasshopper
{
    public class ComputeResult
    {
        public Dictionary<string, double> TimeEstimate { get; set; }
        public string Value { get; set; }
        public Exception Errors { get; set; }
    }
    public class ComputeCompute : PB_TaskCapableComponent<ComputeResult>
    {
        /// <summary>
        /// Initializes a new instance of the ComputeCompute class.
        /// </summary>
        public ComputeCompute()
            : base("Compute", "Compute",
                "Upload and compute the CFD Case",
                "Compute", "General")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Input", "Input", "Input from previous Compute Component", GH_ParamAccess.item);
            pManager.AddMeshParameter("Mesh", "Mesh", "Case Geometry as a list of meshes", GH_ParamAccess.list);
            pManager.AddTextParameter("Folder", "Folder",
                "If running a HoneyBee case, connect the path of the case to here.", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Compute", "Compute", "Run the case on Procedural Compute",
                GH_ParamAccess.item, false);

            pManager[1].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Info", "Info",
                "Info\nCell estimation is based on an equation developed by Alexander Jacobson", GH_ParamAccess.item);
            pManager.AddTextParameter("Output", "Output", "Output", GH_ParamAccess.list);
        }

        private static string Info(Dictionary<string, double> estimations)
        {
            if (estimations == null || estimations.Count == 0)
            {
                return "Not enough information to calculate time and cost estimation";
            }

            var info = $"Estimated number of cells: {estimations["cells"]}\n" +
                       $"Estimated time to run: {Math.Round(estimations["time"], 2)} minutes\n" +
                       $"Estimated cost: {Math.Round(estimations["cost"], 2)} credits";
            return info;
        }

        public override Task<ComputeResult> CreateTask(IGH_DataAccess DA)
        {
            string inputJson = null;
            var geometry = new List<GH_Mesh>();
            var compute = false;
            var folder = string.Empty;

            if (!DA.GetData(0, ref inputJson)) return DefaultTask();
            if (inputJson == "error") return DefaultTask();

            DA.GetDataList(1, geometry);
            DA.GetData(2, ref folder);
            DA.GetData(3, ref compute);

            if (!string.IsNullOrEmpty(folder))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "folder input is not allowed");
                return DefaultTask();
            }

            return Task.Run(() => DoCompute(inputJson, geometry, folder, compute));
        }

        public override void SetOutputData(IGH_DataAccess DA, ComputeResult result)
        {
            Message = "";

            if (result.Errors != null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, result.Errors.Message);
                return;
            }

            var computes = false;
            DA.GetData(3, ref computes);
            Message = computes ? "Task Created" : "";

            DA.SetData(0, Info(result.TimeEstimate));
            DA.SetData(1, result.Value);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override Bitmap Icon => Resources.IconRun;

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("898478bb-5b4f-4972-951a-d9e71ba0086b");

        private ComputeResult DoCompute(string inputJson, List<GH_Mesh> geometry, string folder, bool compute)
        {
            try
            {
                return new ComputeResult
                {
                    TimeEstimate = Compute.GetTaskEstimates(inputJson),
                    Value = RunOnCompute(inputJson, geometry, folder, compute)
                };
            }
            catch (Exception e)
            {
                return new ComputeResult
                {
                    Errors = e
                };
            }
        }

        private string RunOnCompute(string inputJson, List<GH_Mesh> geometry, string folder, bool compute)
        {
            if (FolderContainsEnergyPlus(folder))
            {
                return Compute.CreateEnergyPlus(
                    inputJson,
                    folder,
                    compute
                );
            }

            else if (FolderContainsRadiance(folder))
            {
                return Compute.CreateRadiance(
                    inputJson,
                    folder,
                    compute
                );
            }
            else if (inputJson.Contains("radiation_solution"))
            {
                return Compute.Create(
                     inputJson,
                     Export.STLObject(geometry),
                     "Probe",
                     compute
                 );
            }
            else
            {
                return Compute.Create(
                    inputJson,
                    Export.STLObject(geometry),
                    Export.RefinementRegionsToSTL(geometry),
                    compute
                );
            }
        }

        private static bool FolderContainsRadiance(string folder)
        {
            if (string.IsNullOrEmpty(folder))
            {
                return false;
            }

            var files = Directory.GetFiles(folder);
            return files.Any(file => file.ToLower().EndsWith(".rad"));
        }

        private static bool FolderContainsEnergyPlus(string folder)
        {
            if (string.IsNullOrEmpty(folder))
            {
                return false;
            }

            var files = Directory.GetFiles(folder);
            return files.Any(file => file.ToLower().EndsWith(".idf"));
        }
    }
}