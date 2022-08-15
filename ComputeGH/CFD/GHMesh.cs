//using System;
//using System.Collections.Generic;
//using System.Drawing;
//using ComputeCS.Components;
//using ComputeGH.Grasshopper.Utils;
//using ComputeGH.Properties;
//using Grasshopper.Kernel;

//namespace ComputeCS.Grasshopper
//{
//    public class ComputeMesh : PB_Component
//    {
//        /// <summary>
//        /// Initializes a new instance of the computeLogin class.
//        /// </summary>
//        public ComputeMesh()
//            : base("Compute Mesh", "Mesh",
//                "Create the Mesh Parameters for a CFD Case",
//                "Compute", "Mesh")
//        {
//        }

//        /// <summary>
//        /// Registers all the input parameters for this component.
//        /// </summary>
//        protected override void RegisterInputParams(GH_InputParamManager pManager)
//        {
//            pManager.AddTextParameter("Input", "Input", "Input from previous Compute Component", GH_ParamAccess.item);
//            pManager.AddTextParameter("Domain", "Domain", "", GH_ParamAccess.item);
//            pManager.AddTextParameter("Default Surface", "Default Surface", "", GH_ParamAccess.item);
//            pManager.AddTextParameter("Overrides", "Overrides",
//                "Takes overrides for SnappyHexMesh in JSON format:\n" +
//                "{\n" +
//                "    \"castellatedMeshControls\": {...},\n" +
//                "    \"snapControls\": {...},\n" +
//                "    \"addLayersControls\": {...},\n" +
//                "    \"meshQualityControls\": {...},\n" +
//                "    \"mergeTolerance\": number\n" +
//                "}" +
//                "\nA full reference of available keys can be found at: https://www.openfoam.com/documentation/guides/latest/doc/guide-meshing-snappyhexmesh-quick-reference.html",
//                GH_ParamAccess.item);
//            pManager.AddTextParameter("setSet", "setSet", "setSet regions", GH_ParamAccess.list);

//            pManager[2].Optional = true;
//            pManager[3].Optional = true;
//            pManager[4].Optional = true;
//        }

//        /// <summary>
//        /// Registers all the output parameters for this component.
//        /// </summary>
//        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
//        {
//            pManager.AddTextParameter("Output", "Output", "Output", GH_ParamAccess.item);
//        }

//        /// <summary>
//        /// This is the method that actually does the work.
//        /// </summary>
//        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
//        protected override void SolveInstance(IGH_DataAccess DA)
//        {
//            string inputJson = null;
//            string domain = null;
//            var defaultSurfaces = new Dictionary<string, object>
//            {
//                {
//                    "Plane", new Dictionary<string, object>
//                    {
//                        {
//                            "level", new Dictionary<string, string>
//                            {
//                                {"min", "3"},
//                                {"max", "3"},
//                            }
//                        }
//                    }
//                }
//            };
//            var overrides = "";
//            var setSets = new List<string>();

//            if (!DA.GetData(0, ref inputJson)) return;
//            if (!DA.GetData(1, ref domain)) return;
//            DA.GetData(2, ref defaultSurfaces);
//            DA.GetData(3, ref overrides);
//            DA.GetDataList(4, setSets);


//            var outputs = Mesh.Setup(
//                inputJson,
//                domain,
//                defaultSurfaces,
//                overrides,
//                setSets
//            );

//            DA.SetData(0, outputs);
//        }

//        /// <summary>
//        /// Provides an Icon for the component.
//        /// </summary>
//        protected override Bitmap Icon => Resources.IconMesh;

//        /// <summary>
//        /// Gets the unique ID for this component. Do not change this ID after release.
//        /// </summary>
//        public override Guid ComponentGuid => new Guid("c53c4297-c151-4c24-95bb-0f89f2c875f1");
//    }
//}