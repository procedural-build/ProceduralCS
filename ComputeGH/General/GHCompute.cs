using ComputeCS.Components;
using ComputeCS.Exceptions;
using ComputeCS.utils.Cache;
using ComputeCS.utils.Queue;
using ComputeGH.Grasshopper.Utils;
using ComputeGH.Properties;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;

namespace ComputeCS.Grasshopper
{
    public class ComputeCompute : PB_Component
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

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string inputJson = null;
            var geometry = new List<GH_Mesh>();
            var compute = false;
            var folder = string.Empty;

            if (!DA.GetData(0, ref inputJson)) return;
            if (inputJson == "error") return;

            DA.GetDataList(1, geometry);
            DA.GetData(2, ref folder);

            DA.GetData(3, ref compute);

            // Get Cache to see if we already did this
            var cacheKey = inputJson;
            var cachedValues = StringCache.getCache(cacheKey);
            DA.DisableGapLogic();

            if (cachedValues == null || compute)
            {
                var queueName = "compute" + cacheKey;
                StringCache.setCache(InstanceGuid.ToString(), "");

                // Get queue lock
                var queueLock = StringCache.getCache(queueName);
                if (queueLock != "true")
                {
                    StringCache.setCache(queueName, "true");
                    QueueManager.addToQueue(queueName, () =>
                    {
                        try
                        {
                            try
                            {
                                TimeEstimate = Compute.GetTaskEstimates(inputJson);
                            }
                            catch (Exception)
                            {
                            }

                            RunOnCompute(inputJson, geometry, folder, cacheKey, compute);
                        }
                        catch (NoObjectFoundException)
                        {
                            StringCache.setCache(cacheKey + "create", "");
                        }
                        catch (Exception e)
                        {
                            StringCache.setCache(InstanceGuid.ToString(), e.Message);
                            StringCache.setCache(cacheKey, "error");
                            StringCache.setCache(cacheKey + "create", "");
                        }

                        ExpireSolutionThreadSafe(true);
                        Thread.Sleep(2000);
                        StringCache.setCache(queueName, "");
                    });
                }
            }

            HandleErrors();

            Message = "";

            // Read from Cache
            if (cachedValues != null)
            {
                DA.SetData(0, Info(TimeEstimate));
                DA.SetData(1, cachedValues);

                if (StringCache.getCache(cacheKey + "create") == "true")
                {
                    Message = "Tasks Created";
                }
            }
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

        private void RunCFD(string inputJson, List<GH_Mesh> geometry, string cacheKey, bool compute)
        {
            var geometryFile = Export.STLObject(geometry);
            var refinementGeometry = Export.RefinementRegionsToSTL(geometry);
            var results = Compute.Create(
                inputJson,
                geometryFile,
                refinementGeometry,
                compute
            );
            StringCache.setCache(cacheKey, results);
            StringCache.setCache(InstanceGuid.ToString(), "");
            if (compute)
            {
                StringCache.setCache(cacheKey + "create", "true");
            }
        }

        private void RunRadiance(string inputJson, List<GH_Mesh> geometry, string cacheKey, bool compute)
        {
            var geometryFile = Export.STLObject(geometry);
            var results = Compute.Create(
                inputJson,
                geometryFile,
                "Probe",
                compute
            );
            StringCache.setCache(cacheKey, results);
            StringCache.setCache(InstanceGuid.ToString(), "");
            if (compute)
            {
                StringCache.setCache(cacheKey + "create", "true");
            }
        }

        private void RunEnergyPlus(string inputJson, string folder, string cacheKey, bool compute)
        {
            var results = Compute.CreateEnergyPlus(
                inputJson,
                folder,
                compute
            );
            StringCache.setCache(cacheKey, results);
            StringCache.setCache(this.InstanceGuid.ToString(), "");
            if (compute)
            {
                StringCache.setCache(cacheKey + "create", "true");
            }
        }

        private void RunHoneybeeRadiance(string inputJson, string folder, string cacheKey, bool compute)
        {
            var results = Compute.CreateRadiance(
                inputJson,
                folder,
                compute
            );
            StringCache.setCache(cacheKey, results);
            StringCache.setCache(this.InstanceGuid.ToString(), "");
            if (compute)
            {
                StringCache.setCache(cacheKey + "create", "true");
            }
        }

        private void RunOnCompute(string inputJson, List<GH_Mesh> geometry, string folder, string cacheKey,
            bool compute)
        {
            if (FolderContainsEnergyPlus(folder))
            {
                RunEnergyPlus(inputJson, folder, cacheKey, compute);
            }

            else if (FolderContainsRadiance(folder))
            {
                RunHoneybeeRadiance(inputJson, folder, cacheKey, compute);
            }
            else if (inputJson.Contains("radiation_solution"))
            {
                RunRadiance(inputJson, geometry, cacheKey, compute);
            }
            else
            {
                RunCFD(inputJson, geometry, cacheKey, compute);
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

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override Bitmap Icon => Resources.IconRun;

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("898478bb-5b4f-4972-951a-d9e71ba0086b");

        private Dictionary<string, double> TimeEstimate;
    }
}