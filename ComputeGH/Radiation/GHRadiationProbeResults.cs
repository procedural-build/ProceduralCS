using ComputeCS.Components;
using ComputeCS.types;
using ComputeGH.Grasshopper.Utils;
using ComputeGH.Params;
using ComputeGH.Properties;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace ComputeGH.Radiation
{
    public class RadiationProbeResults
    {
        public DataTree<object> Info { get; set; }
        public DataTree<object> ProbeResults { get; set; }
        public Exception Errors { get; set; }
    }
    public class GHRadiationProbeResults : PB_TaskCapableComponent<RadiationProbeResults>
    {
        /// <summary>
        /// Initializes a new instance of the GHProbeResults class.
        /// </summary>
        public GHRadiationProbeResults()
            : base("Radiation Probe Results", "Radiation Results",
                "Loads the probe results from a file",
                "Compute", "Radiation")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddParameter(new DownloadFileParam(), "Downloaded files", "Files", "File downloaded from Procedural compute to retrieve results from", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Info", "Info", "Description of the outputs", GH_ParamAccess.tree);
            pManager.AddNumberParameter("Metric", "Metric", "Result Metric",
                GH_ParamAccess.tree);
        }

        private static DataTree<object> ConvertToDataTree(Dictionary<string, Dictionary<string, IEnumerable<object>>> data)
        {
            var output = new DataTree<object>();
            var patchCounter = 0;
            foreach (var patchKey in data.Keys)
            {
                var metricCounter = 0;
                foreach (var metricKey in data[patchKey].Keys)
                {
                    var path = new GH_Path(new int[] { patchCounter, metricCounter });
                    output.AddRange(data[patchKey][metricKey], path);
                    metricCounter++;
                }
                patchCounter++;
            }

            return output;
        }

        private static DataTree<object> UpdateInfo(Dictionary<string, Dictionary<string, IEnumerable<object>>> data)
        {
            var info = "Patch Names:\n";
            var i = 0;

            foreach (var key in data.Keys)
            {
                info += $"{{{i};*}} is {key}\n";
                i++;
            }

            var j = 0;
            info += "\nMetrics:\n";
            var output = new DataTree<object>();
            var patchKey = data.Keys.ToList().First();
            foreach (var metric in data[patchKey].Keys.ToList())
            {
                info += $"{{*;{j}}} is {ComputeCS.Utils.SnakeCaseToHumanCase(metric)}\n";
                output.Add(metric, new GH_Path(1));
                j++;
            }

            output.Add(info, new GH_Path(0));
            return output;
        }

        public override Task<RadiationProbeResults> CreateTask(IGH_DataAccess DA)
        {
            var files = new List<DownloadFile>();

            if (!DA.GetDataList(0, files)) return DefaultTask();

            return System.Threading.Tasks.Task.Run(() => GetRadiationProbeResults(files));
        }

        public override void SetOutputData(IGH_DataAccess DA, RadiationProbeResults result)
        {
            if (result.Errors != null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, result.Errors.Message);
                return;
            }

            DA.SetDataTree(0, result.Info);
            DA.SetDataTree(1, result.ProbeResults);
        }


        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override Bitmap Icon => Resources.IconMesh;

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("1728a9d5-868d-4e17-aede-6d346233c9d3");

        private RadiationProbeResults GetRadiationProbeResults(List<DownloadFile> files)
        {
            try
            {
                var results = RadiationProbeResult.ReadResults(files);
                return new RadiationProbeResults
                {
                    ProbeResults = ConvertToDataTree(results),
                    Info = UpdateInfo(results),
                };
            }
            catch (Exception e)
            {
                return new RadiationProbeResults
                {
                    Errors = e
                };
            }
        }
    }
}