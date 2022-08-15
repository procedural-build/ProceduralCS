//using System;
//using System.Collections.Generic;
//using System.Drawing;
//using System.IO;
//using ComputeGH.Grasshopper.Utils;
//using ComputeGH.Properties;
//using Grasshopper.Kernel;
//using Grasshopper.Kernel.Types;
//using Grasshopper3D = Grasshopper;

//namespace ComputeCS.Grasshopper
//{
//    public class GHSaveMesh : PB_Component
//    {
//        /// <summary>
//        /// Initializes a new instance of the GHSaveMesh class.
//        /// </summary>
//        public GHSaveMesh()
//            : base("Save Mesh", "Save Mesh",
//                "Save a mesh to a local file.",
//                "Compute", "Geometry")
//        {
//        }

//        /// <summary>
//        /// Registers all the input parameters for this component.
//        /// </summary>
//        protected override void RegisterInputParams(GH_InputParamManager pManager)
//        {
//            pManager.AddMeshParameter("Mesh", "Mesh", "Mesh to save", GH_ParamAccess.item);
//            pManager.AddTextParameter("File Path", "Path", "File path to save the mesh to", GH_ParamAccess.item);
//            pManager.AddTextParameter("File Name", "Name", "File name to use for the mesh", GH_ParamAccess.item);
//            pManager.AddBooleanParameter("Save", "Save", "Save mesh", GH_ParamAccess.item);

//            pManager[3].Optional = true;
//        }

//        /// <summary>
//        /// Registers all the output parameters for this component.
//        /// </summary>
//        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
//        {
//            pManager.AddTextParameter("Mesh Path", "Path", "Full path for the mesh file",
//                GH_ParamAccess.item);
//        }

//        /// <summary>
//        /// This is the method that actually does the work.
//        /// </summary>
//        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
//        protected override void SolveInstance(IGH_DataAccess DA)
//        {
//            var mesh = new GH_Mesh();
//            var filePath = "";
//            var fileName = "";
//            var save = false;

//            if ((!DA.GetData(0, ref mesh))) return;

//            if ((!DA.GetData(1, ref filePath))) return;

//            if (!DA.GetData(2, ref fileName)) return;

//            DA.GetData(3, ref save);

//            if (save)
//            {
//                meshPath = CreateMeshPath(filePath, fileName);
//                Export.MeshToObjFile(new List<GH_Mesh> {mesh}, meshPath);
//            }
//            DA.SetData(0, meshPath);

//        }

//        private static string CreateMeshPath(string filePath, string fileName)
//        {
//            if (!fileName.EndsWith(".obj"))
//            {
//                fileName += ".obj";
//            }

//            return Path.Combine(filePath, fileName);
//        } 

//        /// <summary>
//        /// Provides an Icon for the component.
//        /// </summary>
//        protected override Bitmap Icon => Resources.IconMesh;

//        /// <summary>
//        /// Gets the unique ID for this component. Do not change this ID after release.
//        /// </summary>
//        public override Guid ComponentGuid => new Guid("ac42ea6d-f18b-4734-a7d1-28caaf243ba5");

//        private string meshPath;
//    }
//}