using ComputeCS.Components;
using ComputeCS.Exceptions;
using ComputeCS.utils.Cache;
using ComputeCS.utils.Queue;
using ComputeGH.Grasshopper.Utils;
using ComputeGH.Properties;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;

namespace ComputeGH.Radiation
{
    public class GHRadiationProbe : PB_Component
    {
        /// <summary>
        /// Initializes a new instance of the GHProbe class.
        /// </summary>
        public GHRadiationProbe()
            : base("Probe Radiation", "Probe Radiation",
                "Probe radiation case to get the results in the desired points",
                "Compute", "Radiation")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Input", "Input", "Input from previous Compute Component", GH_ParamAccess.item);
            pManager.AddMeshParameter("Mesh", "Mesh", "Input your analysis mesh here, if you wish to visualize your results in the browser",
                GH_ParamAccess.tree);
            pManager.AddPointParameter("Points", "Points", "Probe points where you want to get radiation values.",
                GH_ParamAccess.tree);
            pManager.AddVectorParameter("Normals", "Normals", "Normals to the probe points given above.",
                GH_ParamAccess.tree);
            pManager.AddTextParameter("Names", "Names",
                "Give names to each branch of in the points tree. The names can later be used to identify the points.",
                GH_ParamAccess.list);
            pManager.AddBooleanParameter("Create", "Create",
                "Whether to create a new probe task, if one doesn't exist. If the Probe task already exists, then this component will create a new task config, that will run after the previous config is finished.",
                GH_ParamAccess.item, false);

            pManager[1].Optional = true;
            pManager[4].Optional = true;
            pManager[5].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Output", "Output", "Output", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string inputJson = null;
            var mesh = new GH_Structure<GH_Mesh>();
            var points = new GH_Structure<GH_Point>();
            var normals = new GH_Structure<GH_Vector>();
            var names = new List<string>();
            var create = false;

            if (!DA.GetData(0, ref inputJson)) return;
            if (!DA.GetDataTree(1, out mesh)) return;
            if (!DA.GetDataTree(2, out points)) return;
            if (!DA.GetDataTree(3, out normals)) return;
            if (!DA.GetDataList(4, names))
            {
                for (var i = 0; i < points.Branches.Count; i++)
                {
                    names.Add($"set{i.ToString()}");
                }
            }

            DA.GetData(5, ref create);



            // Get Cache to see if we already did this
            var cacheKey = string.Join("", points) + string.Join("", names) + inputJson;
            var cachedValues = StringCache.getCache(cacheKey);
            DA.DisableGapLogic();

            if (cachedValues == null || create)
            {
                var queueName = "radiationProbe";

                // Get queue lock
                var queueLock = StringCache.getCache(queueName);
                if (queueLock != "true")
                {
                    StringCache.setCache(queueName, "true");
                    StringCache.setCache(cacheKey, null);
                    QueueManager.addToQueue(queueName, () =>
                    {
                        try
                        {
                            var meshFile = Export.MeshToObj(mesh, names);
                            var results = Probe.RadiationProbes(
                                inputJson,
                                Geometry.ConvertPointsToList(points),
                                Geometry.ConvertPointsToList(normals),
                                names,
                                meshFile,
                                create
                            );
                            cachedValues = results;
                            StringCache.setCache(cacheKey, cachedValues);
                            if (create)
                            {
                                StringCache.setCache(cacheKey + "create", "true");
                            }
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

            // Handle Errors
            var errors = StringCache.getCache(InstanceGuid.ToString());
            if (errors != null)
            {
                if (errors.Contains("No object found"))
                {
                    Message = "No Probe Task found.";
                }
                else
                {
                    throw new Exception(errors);
                }

            }

            // Read from Cache
            Message = "";
            if (cachedValues != null)
            {
                var outputs = cachedValues;
                DA.SetData(0, outputs);
                if (StringCache.getCache(cacheKey + "create") == "true")
                {
                    Message = "Task Created";
                }
            }
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override Bitmap Icon => Resources.IconMesh;

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("ca0e9bd3-46a9-4cc7-b379-07b43d27e46a");
    }
}