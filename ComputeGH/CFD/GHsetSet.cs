//using System;
//using System.Collections.Generic;
//using System.Linq;
//using ComputeCS.types;
//using ComputeGH.Grasshopper.Utils;
//using ComputeGH.Properties;
//using Grasshopper.Kernel;
//using Rhino.Geometry;

//namespace ComputeGH.Grasshopper
//{
//    public class CFDsetSet : PB_Component
//    {
//        /// <summary>
//        /// Initializes a new instance of the MyComponent1 class.
//        /// </summary>
//        public CFDsetSet() : base("CFD setSet", "setSet", "Description", "Compute", "CFD"){}

//        /// <summary>
//        /// Registers all the input parameters for this component.
//        /// </summary>
//        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
//        {
//            pManager.AddTextParameter("Names", "Names", "Mesh names", GH_ParamAccess.list);
//            pManager.AddBooleanParameter("Location", "Location", "This input takes a list of false and true. Inside, Surface, Outside", GH_ParamAccess.list, new List<bool>(){ true, true, false });
//            pManager.AddPointParameter("Keep Point", "Keep Point", "Keep Point. If no point is provided, the point is set to the same as the SnappyHexMesh keep point.", GH_ParamAccess.item);
//            pManager.AddBooleanParameter("isCellZone", "isCellZone", 
//                "Whether or not this setSet region should be treated as cellZone.\n" +
//                "Default is false, which means that the region will be treated as a cellSet", 
//                GH_ParamAccess.item, false);

//            pManager[1].Optional = true;
//            pManager[2].Optional = true;
//            pManager[3].Optional = true;

//        }

//        /// <summary>
//        /// Registers all the output parameters for this component.
//        /// </summary>
//        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
//        {
//            pManager.AddTextParameter("Output", "Output", "setSet Regions", GH_ParamAccess.item);
//        }

//        /// <summary>
//        /// This is the method that actually does the work.
//        /// </summary>
//        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
//        protected override void SolveInstance(IGH_DataAccess DA)
//        {
//            var names = new List<string>();
//            var location = new List<bool>();
//            var keepPoint = new Point3d();
//            var isCellZone = false;
//            List<double> keepList = null;

//            if (!DA.GetDataList(0, names)) return;
//            DA.GetDataList(1, location);
//            if (DA.GetData(2, ref keepPoint))
//            {
//                keepList = new List<double>() {keepPoint.X, keepPoint.Y, keepPoint.Z};
//            }

//            DA.GetData(3, ref isCellZone);

//            var outputs = names.Select(name => new setSetRegion {Name = name, Locations = location, KeepPoint = keepList, CellZone = isCellZone}.ToJson()).ToList();

//            DA.SetDataList(0, outputs);
//        }

//        /// <summary>
//        /// Provides an Icon for the component.
//        /// </summary>
//        protected override System.Drawing.Bitmap Icon => Resources.IconMesh;

//        /// <summary>
//        /// Gets the unique ID for this component. Do not change this ID after release.
//        /// </summary>
//        public override Guid ComponentGuid => new Guid("8f622be6-22e4-4607-b280-527abe200921");
//    }
//}