using ComputeCS.Components;
using ComputeCS.utils.Cache;
using ComputeCS.utils.Queue;
using ComputeGH.Grasshopper.Utils;
using ComputeGH.Properties;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;

namespace ComputeGH.Radiation
{
    public class GHRadiationProbeResults : PB_Component
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
            pManager.AddTextParameter("Folder", "Folder", "Folder path to where to results are", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Rerun", "Rerun", "Rerun this component.", GH_ParamAccess.item);

            pManager[1].Optional = true;
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

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string folder = null;
            var refresh = false;

            if (!DA.GetData(0, ref folder)) return;
            DA.GetData(1, ref refresh);

            // Get Cache to see if we already did this
            var cacheKey = folder;
            var cachedValues = StringCache.getCache(cacheKey);
            DA.DisableGapLogic();

            if (cachedValues == null || refresh)
            {
                const string queueName = "radiationProbeResults";
                StringCache.setCache(InstanceGuid.ToString(), "");

                // Get queue lock
                var queueLock = StringCache.getCache(queueName);
                if (queueLock != "true")
                {
                    StringCache.setCache(queueName, "true");
                    StringCache.setCache(cacheKey + "progress", "Loading...");

                    QueueManager.addToQueue(queueName, () =>
                    {
                        try
                        {
                            var results = RadiationProbeResult.ReadResults(folder);
                            probeResults = ConvertToDataTree(results);
                            info = UpdateInfo(results);
                            StringCache.setCache(cacheKey + "progress", "Done");
                            StringCache.setCache(cacheKey, "results");
                        }
                        catch (Exception e)
                        {
                            StringCache.setCache(InstanceGuid.ToString(), e.Message);
                            StringCache.setCache(cacheKey, "error");
                            StringCache.setCache(cacheKey + "progress", "");
                        }

                        ExpireSolutionThreadSafe(true);
                        Thread.Sleep(2000);
                        StringCache.setCache(queueName, "");
                    });
                    ExpireSolutionThreadSafe(true);
                }
            }

            HandleErrors();

            if (info != null)
            {
                DA.SetDataTree(0, info);
            }

            if (probeResults != null)
            {
                DA.SetDataTree(1, probeResults);
            }

            Message = StringCache.getCache(cacheKey + "progress");
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


        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override Bitmap Icon => Resources.IconMesh;

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("1728a9d5-868d-4e17-aede-6d346233c9d3");

        private DataTree<object> info;
        private DataTree<object> probeResults;
    }
}