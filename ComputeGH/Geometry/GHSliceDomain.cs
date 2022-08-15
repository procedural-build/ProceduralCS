//using System;
//using System.Collections.Generic;
//using ComputeGH.Grasshopper.Utils;
//using ComputeGH.Properties;
//using Grasshopper.Kernel;
//using Grasshopper.Kernel.Parameters;
//using Rhino.Geometry;

//namespace ComputeGH.Utils
//{
//    public class GHSliceDomain : PB_Component
//    {
//        /// <summary>
//        /// Initializes a new instance of the GHAnalysisMesh class.
//        /// </summary>
//        public GHSliceDomain()
//          : base("SliceDomain", "SliceDomain",
//              "Create an analysis mesh by slicing a domain box",
//              "Compute", "Geometry")
//        {
//        }

//        /// <summary>
//        /// Registers all the input parameters for this component.
//        /// </summary>
//        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
//        {
//            pManager.AddBoxParameter("Domain", "Domain", "Domain to create the slice from.", GH_ParamAccess.item);
//            pManager.AddNumberParameter("Location", "Location", "Location in the domain to slice at", GH_ParamAccess.item);
//            pManager.AddNumberParameter("Grid Size", "Grid Size", "Size of the mesh grid. Default is 1.0", GH_ParamAccess.item);
//            pManager.AddBrepParameter("Exclude", "Exclude", "Breps that should be cut out from the analysis surface",
//                GH_ParamAccess.list);
//            pManager.AddIntegerParameter("Slice Direction", "Slice Direction",
//                "Direction the domain should be sliced in. Default is x", GH_ParamAccess.item, 0);

//            pManager[2].Optional = true;
//            pManager[3].Optional = true;
//            pManager[4].Optional = true;

//            AddNamedValues(pManager[4] as Param_Integer, SliceDirection);
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
//            pManager.AddPointParameter("FaceCenters", "FaceCenters", "Face centers of the created mesh", GH_ParamAccess.tree);
//            pManager.AddVectorParameter("FaceNormals", "FaceNormals", "Face Normals of the created mesh", GH_ParamAccess.tree);

//        }

//        /// <summary>
//        /// This is the method that actually does the work.
//        /// </summary>
//        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
//        protected override void SolveInstance(IGH_DataAccess DA)
//        {
//            var domain = new Box();
//            var location = 0.0;
//            var gridSize = 1.0;
//            var excludeGeometry = new List<Brep>();
//            var sliceDirection = 2;

//            if (!DA.GetData(0, ref domain)) return;
//            if (!DA.GetData(1, ref location)) return;
//            DA.GetData(2, ref gridSize);
//            DA.GetDataList(3, excludeGeometry);
//            DA.GetData(4, ref sliceDirection);

//            var result = Geometry.CreatedMeshFromSlicedDomain(
//                domain, location, gridSize, excludeGeometry, SliceDirection[sliceDirection]
//                );

//            DA.SetDataTree(0, result["analysisMesh"]);
//            DA.SetDataTree(1, result["faceCenters"]);
//            DA.SetDataTree(2, result["faceNormals"]);
//        }

//        private static readonly List<string> SliceDirection = new List<string>
//        {
//            "x",
//            "y",
//            "z",
//        };

//        /// <summary>
//        /// Provides an Icon for the component.
//        /// </summary>
//        protected override System.Drawing.Bitmap Icon => Resources.IconRectDomain;

//        /// <summary>
//        /// Gets the unique ID for this component. Do not change this ID after release.
//        /// </summary>
//        public override Guid ComponentGuid => new Guid("fe327b56-efd8-4c22-b61e-1232295370ae");
//    }
//}