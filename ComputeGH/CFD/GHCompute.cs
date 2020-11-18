﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using ComputeCS.Components;
using ComputeCS.Grasshopper.Utils;
using ComputeCS.types;
using ComputeCS.utils.Cache;
using ComputeCS.utils.Queue;
using ComputeGH.Properties;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino;

namespace ComputeCS.Grasshopper
{
    public class ComputeCompute : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the ComputeCompute class.
        /// </summary>
        public ComputeCompute()
          : base("Compute", "Compute",
              "Upload and compute the CFD Case",
              "Compute", "CFD")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Input", "Input", "Input from previous Compute Component", GH_ParamAccess.item);
            pManager.AddMeshParameter("Mesh", "Mesh", "Case Geometry as a list of meshes", GH_ParamAccess.list);
            pManager.AddTextParameter("Folder", "Folder", "If running a HoneyBee case, connect the path of the case to here.", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Compute", "Compute", "Run the case on Procedural Compute", GH_ParamAccess.item, false);

            pManager[1].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;


        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
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

            DA.GetDataList(1, geometry);
            DA.GetData(2, ref folder);
            
            DA.GetData(3, ref compute);

            // Get Cache to see if we already did this
            var cacheKey = inputJson;
            var cachedValues = StringCache.getCache(cacheKey);
            DA.DisableGapLogic();

            if (cachedValues == null || compute)
            {
                const string queueName = "compute";
                StringCache.setCache(this.InstanceGuid.ToString(), "");

                // Get queue lock
                var queueLock = StringCache.getCache(queueName);
                if (queueLock != "true")
                {
                    StringCache.setCache(queueName, "true");
                    QueueManager.addToQueue(queueName, () => {
                        try
                        {
                            RunOnCompute(inputJson, geometry, folder, cacheKey, compute);
                        }
                        catch (Exception e)
                        {
                            StringCache.setCache(this.InstanceGuid.ToString(), e.Message);
                            StringCache.setCache(cacheKey, "error");
                            StringCache.setCache(cacheKey + "create", "");
                        }

                        ExpireSolutionThreadSafe(true);
                        Thread.Sleep(2000);
                        StringCache.setCache(queueName, "");
                    });
                    
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
                var outputs = cachedValues;
                DA.SetData(0, outputs);
                Message = "";
                if (StringCache.getCache(cacheKey + "create") == "true"){Message = "Tasks Created";}
            }
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
            StringCache.setCache(this.InstanceGuid.ToString(), "");
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
        
        private void RunRadiance(string inputJson, string folder, string cacheKey, bool compute)
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
            if (geometry.Count != 0)
            {
                RunCFD(inputJson, geometry, cacheKey, compute);
            }

            else if (FolderContainsEnergyPlus(folder))
            {
                RunEnergyPlus(inputJson, folder, cacheKey, compute);
            }
            
            else if (FolderContainsRadiance(folder))
            {
                RunRadiance(inputJson, folder, cacheKey, compute);
            }

            else
            {
                throw new Exception("Unknown Case type!");
            }
        }

        private static bool FolderContainsRadiance(string folder)
        {
            var files = Directory.GetFiles(folder);
            return files.Any(file => file.ToLower().EndsWith(".rad"));
        }

        private static bool FolderContainsEnergyPlus(string folder)
        {
            var files = Directory.GetFiles(folder);
            return files.Any(file => file.ToLower().EndsWith(".idf"));
        }

        private string UpdateInputs(string inputJson)
        {
            var inputData = new Inputs().FromJson(inputJson);
            if (inputData.CFDSolution == null)
            {
                foreach (var param in Params.Input)
                {
                    if (param.Name == "_")
                    param.Name = "Folder";
                    param.NickName = "Folder";
                    param.Access = GH_ParamAccess.item;
                    param.Description = "Path to case folder";
                    Params.OnParametersChanged();
                    ExpireSolution(true);
                }

                return "folder";
            }
            else
            {
                foreach (var param in Params.Input.Where(param => param.Name == "Folder"))
                {
                    param.Name = "Mesh";
                    param.NickName = "Mesh";
                    param.Access = GH_ParamAccess.list;
                    param.Description = "Case Geometry as a list of meshes";
                    Params.OnParametersChanged();
                    ExpireSolution(true);
                }

                return "mesh";
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
                return Resources.IconRun;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("898478bb-5b4f-4972-951a-d9e71ba0086b"); }
        }
    }
}