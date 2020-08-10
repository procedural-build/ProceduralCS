using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using Newtonsoft.Json;
using ComputeCS.types;
using ComputeCS.Grasshopper.Utils;
using ComputeCS.utils.Cache;
using ComputeCS.utils.Queue;

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
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Input", "Input", "Input from previous Compute Component", GH_ParamAccess.item);
            pManager.AddMeshParameter("Mesh", "Mesh", "Case Geometry as a list of meshes", GH_ParamAccess.list);
            pManager.AddBooleanParameter("Compute", "Compute", "Run the case on Procedural Compute", GH_ParamAccess.item, false);

            pManager[2].Optional = true;


        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
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
            List<GH_Mesh> geometry = new List<GH_Mesh>();
            bool compute = false;



            if (!DA.GetData(0, ref inputJson)) return;
            if (!DA.GetDataList(1, geometry)) return;
            DA.GetData(2, ref compute);

            // Get Cache to see if we already did this
            var cacheKey = inputJson;
            var cachedValues = StringCache.getCache(cacheKey);
            DA.DisableGapLogic();

            if (cachedValues == null && compute)
            {
                var queueName = "compute";

                // Get queue lock
                var queueLock = StringCache.getCache(queueName);
                if (queueLock != "true")
                {
                    StringCache.setCache(queueName, "true");
                    QueueManager.addToQueue(queueName, () => {
                        try
                        {
                            var geometryFile = Export.STLObject(geometry);
                            var results = Components.Compute.Create(
                                inputJson,
                                geometryFile,
                                compute
                            );
                            cachedValues = results;
                            StringCache.setCache(cacheKey, cachedValues);

                        }
                        catch (Exception e)
                        {
                            StringCache.AppendCache(this.InstanceGuid.ToString(), e.ToString() + "\n");
                        }
                        StringCache.setCache(queueName, "");
                        ExpireSolutionThreadSafe(true);
                    });
                    
                }

            }


            // Read from Cache
            string outputs = null;
            if (cachedValues != null)
            {
                outputs = cachedValues;
                DA.SetData(0, outputs);
            }

            // Handle Errors
            var errors = StringCache.getCache(this.InstanceGuid.ToString());
            if (errors != null)
            {
                throw new Exception(errors);
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
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return null;
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