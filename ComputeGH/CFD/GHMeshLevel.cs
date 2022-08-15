//using System;
//using System.Collections.Generic;
//using System.Drawing;
//using System.Linq;
//using ComputeCS.types;
//using ComputeGH.Grasshopper.Utils;
//using ComputeGH.Properties;
//using Grasshopper.Kernel;
//using Grasshopper.Kernel.Types;

//namespace ComputeCS.Grasshopper
//{
//    public class CFDMeshLevel : PB_Component
//    {
//        public CFDMeshLevel() : base("CFD Mesh Level", "Mesh Level",
//            "Defines a CFD Mesh Level for the resolution on the surface of the meshes.", "Compute", "Mesh")
//        {
//        }

//        protected override void RegisterInputParams(GH_InputParamManager pManager)
//        {
//            pManager.AddMeshParameter("Meshes", "Meshes", "Meshes to apply mesh level to.", GH_ParamAccess.list);
//            pManager.AddIntegerParameter("Mesh Levels", "Levels",
//                "Surface Mesh Level. Set the mesh level on the surface of the meshes. " +
//                "The first number provided will be the Min Level and the second will be the Max Level. " +
//                "If only one number is provided then Min and Max will be set as the same.",
//                GH_ParamAccess.list);
//            pManager.AddNumberParameter("Mesh Resolution", "Resolution",
//                "Resolution of the meshes in meters. This input is used only if Levels doesn't have a input.",
//                GH_ParamAccess.item);

//            pManager[1].Optional = true;
//            pManager[2].Optional = true;
//        }

//        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
//        {
//            pManager.AddMeshParameter("Meshes", "Meshes", "Meshes with level applied", GH_ParamAccess.list);
//        }

//        protected override Bitmap Icon => Resources.IconMeshLevel;

//        public override Guid ComponentGuid => new Guid("57bbca4d-9a11-4797-b90a-a5e735b9f803");

//        protected override void SolveInstance(IGH_DataAccess DA)
//        {
//            var meshes = new List<GH_Mesh>();
//            var meshLevels = new List<int>();
//            double? resolution = null;

//            if (!DA.GetDataList(0, meshes)) return;

//            DA.GetDataList(1, meshLevels);
//            DA.GetData(2, ref resolution);


//            if (meshLevels.Count == 0 && resolution == null)
//            {
//                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Please provide a Mesh Level or a Resolution");
//            }

//            meshes = meshes.Select(mesh => SetMeshLevel(mesh, meshLevels, resolution)).ToList();

//            DA.SetDataList(0, meshes);
//        }

//        private static GH_Mesh SetMeshLevel(GH_Mesh mesh, List<int> meshLevels, double? resolution)
//        {
//            var levelDetails = new MeshLevelDetails
//            {
//                Resolution = resolution,
//                Level = meshLevels.Count > 0? new MeshLevels {Min = meshLevels.First(), Max = meshLevels.Last()}: null
//            };
//            Geometry.setUserString(
//                mesh,
//                "ComputeMeshLevels",
//                levelDetails.ToJson()
//            );
//            return mesh;
//        }
//    }
//}