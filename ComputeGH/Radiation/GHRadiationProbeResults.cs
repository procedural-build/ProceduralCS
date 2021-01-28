using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using ComputeCS.Components;
using ComputeCS.Grasshopper;
using ComputeCS.utils.Cache;
using ComputeCS.utils.Queue;
using ComputeGH.Properties;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Rhino;
using Rhino.Geometry;

namespace ComputeGH.Radiation
{
    public class GHRadiationProbeResults : GH_Component
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
            pManager.AddTextParameter("Info", "Info", "Description of the outputs", GH_ParamAccess.item);
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
                            info = UpdateInfo(results.Keys.ToList());
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

            // Handle Errors
            var errors = StringCache.getCache(InstanceGuid.ToString());
            if (!string.IsNullOrEmpty(errors))
            {
                throw new Exception(errors);
            }

            if (info != null)
            {
                DA.SetData(0, info);
            }

            if (probeResults != null)
            {
                DA.SetDataTree(1, probeResults);
            }

            Message = StringCache.getCache(cacheKey + "progress");
        }

        private void ExpireSolutionThreadSafe(bool recompute = false)
        {
            var delegated = new ExpireSolutionDelegate(ExpireSolution);
            RhinoApp.InvokeOnUiThread(delegated, recompute);
        }


        private static DataTree<object> ConvertToDataTree(Dictionary<string, IEnumerable<object>> data)
        {
            var output = new DataTree<object>();
            var patchCounter = 0;
            foreach (var patchKey in data.Keys)
            {
                var path = new GH_Path(patchCounter);
                output.AddRange(data[patchKey], path);
                patchCounter++;
            }

            return output;
        }

        private static string UpdateInfo(List<string> keys)
        {
            var info = "Patch Names:\n";
            var i = 0;
            foreach (var key in keys)
            {
                info += $"{{{i};*}} is {key}\n";
                i++;
            }

            return info;
        }


        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override Bitmap Icon => Resources.IconMesh;

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("1728a9d5-868d-4e17-aede-6d346233c9d3");

        private string info;
        private DataTree<object> probeResults;
    }
}