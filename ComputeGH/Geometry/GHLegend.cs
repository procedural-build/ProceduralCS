//using System;
//using System.Collections.Generic;
//using System.Drawing;
//using System.Linq;
//using ComputeGH.Grasshopper.Utils;
//using ComputeGH.Properties;
//using Grasshopper.Kernel;
//using Rhino.Geometry;

//namespace ComputeGH.Utils
//{
//    public class GHLegend : PB_Component
//    {
//        /// <summary>
//        /// Initializes a new instance of the GHAnalysisMesh class.
//        /// </summary>
//        public GHLegend()
//          : base("Legend", "Legend",
//              "Create a legend.",
//              "Compute", "Geometry")
//        {
//        }

//        /// <summary>
//        /// Registers all the input parameters for this component.
//        /// </summary>
//        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
//        {
//            pManager.AddPointParameter("BasePoint", "BasePoint", "A base point for the legend. The base point will be a the lowest left corner of the mesh.", GH_ParamAccess.item);
//            pManager.AddColourParameter("Colors", "Colors", "Colors for the legend", GH_ParamAccess.list);
//            pManager.AddTextParameter("Values", "Values", "Values for the legend", GH_ParamAccess.list);
//            pManager.AddNumberParameter("Scale", "Scale", "A value to scale the legend by", GH_ParamAccess.item, 1.0);
//            pManager.AddNumberParameter("TextHeight", "TextHeight", "The height of the legend text", GH_ParamAccess.item, 1.0);

//            pManager[3].Optional = true;
//            pManager[4].Optional = true;

//        }

//        /// <summary>
//        /// Registers all the output parameters for this component.
//        /// </summary>
//        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
//        {
//            pManager.AddMeshParameter("Legend", "Legend", "Created legend as mesh", GH_ParamAccess.item);
//            pManager.AddGenericParameter("Text", "Text", "Values as text", GH_ParamAccess.list);
//        }

//        /// <summary>
//        /// This is the method that actually does the work.
//        /// </summary>
//        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
//        protected override void SolveInstance(IGH_DataAccess DA)
//        {
//            var basePoint = new Point3d();
//            var colors = new List<Color>();
//            var values = new List<string>();
//            var scale = 1.0;
//            var textHeight = 1.0;

//            if (!DA.GetData(0, ref basePoint)) return;
//            if (!DA.GetDataList(1, colors)) return;
//            if (!DA.GetDataList(2, values)) return;
//            DA.GetData(3, ref scale);
//            DA.GetData(4, ref textHeight);

//            var (legend, text) = Geometry.CreateLegend(basePoint, colors, values, scale, textHeight);

//            DA.SetData(0, legend);
//            DA.SetDataList(1, text.Select(t => new Types.TextGoo(t)).ToList());
//        }

//        /// <summary>
//        /// Provides an Icon for the component.
//        /// </summary>
//        protected override Bitmap Icon => Resources.IconMesh;

//        /// <summary>
//        /// Gets the unique ID for this component. Do not change this ID after release.
//        /// </summary>
//        public override Guid ComponentGuid => new Guid("04ebd080-c0c6-486e-8b31-9dc4d25d3bae");
//    }
//}