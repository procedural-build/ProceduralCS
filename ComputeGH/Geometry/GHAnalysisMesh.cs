//using System;
//using System.Collections.Generic;
//using ComputeGH.Grasshopper.Utils;
//using System.Linq;
//using System.Threading;
//using ComputeCS.Grasshopper;
//using ComputeCS.utils.Cache;
//using ComputeCS.utils.Queue;
//using ComputeGH.Properties;
//using Grasshopper;
//using Grasshopper.Kernel;
//using Grasshopper.Kernel.Parameters;
//using Newtonsoft.Json;
//using Rhino;
//using Rhino.Geometry;

//namespace ComputeGH.Utils
//{
//    public class GHAnalysisMesh : PB_Component
//    {
//        /// <summary>
//        /// Initializes a new instance of the GHAnalysisMesh class.
//        /// </summary>
//        public GHAnalysisMesh()
//            : base("AnalysisMesh", "AnalysisMesh",
//                "Create a mesh from a surface, which can be used for analyses",
//                "Compute", "Geometry")
//        {
//        }

//        /// <summary>
//        /// Registers all the input parameters for this component.
//        /// </summary>
//        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
//        {
//            pManager.AddBrepParameter("Surface", "Surface", "Surface to create the analysis mesh from.",
//                GH_ParamAccess.list);
//            pManager.AddNumberParameter("Grid Size", "Grid Size", "Size of the mesh grid. Default is 1.0",
//                GH_ParamAccess.item);
//            pManager.AddBrepParameter("Exclude", "Exclude", "Breps that should be cut out from the analysis surface",
//                GH_ParamAccess.list);
//            pManager.AddNumberParameter("Offset", "Offset",
//                "Distance to offset the mesh from the surface. Default is 1.5", GH_ParamAccess.item);
//            pManager.AddIntegerParameter("Offset Direction", "Offset Direction",
//                "Direction the surface should be offset in. Default is z", GH_ParamAccess.item, 2);

//            pManager[1].Optional = true;
//            pManager[2].Optional = true;
//            pManager[3].Optional = true;
//            pManager[4].Optional = true;

//            AddNamedValues(pManager[4] as Param_Integer, OffsetDirection);
//        }

//        private static void AddNamedValues(Param_Integer param, List<string> values)
//        {
//            var index = 0;
//            foreach (var value in values)
//            {
//                param.AddNamedValue(value, index);
//                index++;
//            }
//        }

//        /// <summary>
//        /// Registers all the output parameters for this component.
//        /// </summary>
//        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
//        {
//            pManager.AddMeshParameter("AnalysisMesh", "AnalysisMesh", "Created analysis mesh", GH_ParamAccess.tree);
//            pManager.AddPointParameter("FaceCenters", "FaceCenters", "Face centers of the created mesh",
//                GH_ParamAccess.tree);
//            pManager.AddVectorParameter("FaceNormals", "FaceNormals", "Face Normals of the created mesh",
//                GH_ParamAccess.tree);
//        }

//        /// <summary>
//        /// This is the method that actually does the work.
//        /// </summary>
//        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
//        protected override void SolveInstance(IGH_DataAccess DA)
//        {
//            var baseSurfaces = new List<Brep>();
//            var gridSize = 1.0;
//            var excludeGeometry = new List<Brep>();
//            var offset = 1.5;
//            var offsetDirection = 2;

//            if (!DA.GetDataList(0, baseSurfaces)) return;
//            DA.GetData(1, ref gridSize);
//            DA.GetDataList(2, excludeGeometry);
//            DA.GetData(3, ref offset);
//            DA.GetData(4, ref offsetDirection);

//            var cacheKey =
//                JsonConvert.SerializeObject(baseSurfaces) +
//                gridSize.ToString() +
//                JsonConvert.SerializeObject(excludeGeometry) +
//                offset.ToString() + offsetDirection.ToString();
//            var cachedValues = StringCache.getCache(cacheKey);
//            const string queueName = "analysisMesh";
//            DA.DisableGapLogic();


//            if (cachedValues == null)
//            {
//                StringCache.setCache(InstanceGuid.ToString(), "");

//                // Get queue lock
//                var queueLock = StringCache.getCache(queueName);
//                if (queueLock != "true")
//                {
//                    StringCache.setCache(queueName, "true");
//                    StringCache.setCache(queueName + "progress", "Loading...");

//                    QueueManager.addToQueue(queueName, () =>
//                    {
//                        try
//                        {
//                            meshResult = Geometry.CreateAnalysisMesh(
//                                baseSurfaces, gridSize, excludeGeometry, offset, OffsetDirection[offsetDirection]
//                            );

//                            StringCache.setCache(queueName + "progress", "Done");
//                            StringCache.setCache(cacheKey, meshResult.GetHashCode().ToString());
//                        }
//                        catch (Exception e)
//                        {
//                            StringCache.setCache(InstanceGuid.ToString(), e.Message);
//                            StringCache.setCache(cacheKey, "error");
//                            StringCache.setCache(queueName + "progress", "");
//                        }

//                        ExpireSolutionThreadSafe(true);
//                        Thread.Sleep(2000);
//                        StringCache.setCache(queueName, "");
//                    });
//                    ExpireSolutionThreadSafe(true);
//                }
//            }

//            Message = StringCache.getCache(queueName + "progress");

//            // Handle Errors
//            var errors = StringCache.getCache(InstanceGuid.ToString());
//            if (!string.IsNullOrEmpty(errors))
//            {
//                throw new Exception(errors);
//            }

//            if (meshResult != null)
//            {
//                DA.SetDataTree(0, meshResult["analysisMesh"]);
//                DA.SetDataTree(1, meshResult["faceCenters"]);
//                DA.SetDataTree(2, meshResult["faceNormals"]);
//            }
//        }

//        private static readonly List<string> OffsetDirection = new List<string>
//        {
//            "x",
//            "y",
//            "z",
//            "normal",
//        };

//        /// <summary>
//        /// Provides an Icon for the component.
//        /// </summary>
//        protected override System.Drawing.Bitmap Icon => Resources.IconMesh;

//        /// <summary>
//        /// Gets the unique ID for this component. Do not change this ID after release.
//        /// </summary>
//        public override Guid ComponentGuid => new Guid("be364a6b-0339-4afa-8c42-2d1a0d031028");

//        private Dictionary<string, DataTree<object>> meshResult;
//    }
//}