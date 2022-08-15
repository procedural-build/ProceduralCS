//using System;
//using System.Collections.Generic;
//using System.Drawing;
//using System.Linq;
//using ComputeCS.types;
//using ComputeGH.Grasshopper.Utils;
//using ComputeGH.Properties;
//using Grasshopper.Kernel;
//using Grasshopper.Kernel.Types;
//using Rhino.Geometry;

//namespace ComputeCS.Grasshopper
//{
//    public class CFDRadialRefinement : PB_Component
//    {
//        public CFDRadialRefinement() : base("Radial Refinement Region", "Radial Refinement", "Defines a radial CFD Refinement Region that grows.",
//            "Compute", "Mesh")
//        {
//        }

//        protected override void RegisterInputParams(GH_InputParamManager pManager)
//        {
//            pManager.AddPointParameter("Center Point", "Center", "Center point for the refinement region", GH_ParamAccess.item);
//            pManager.AddNumberParameter("Radius", "Radius", "Radii of the refinement regions", GH_ParamAccess.list);
//            pManager.AddNumberParameter("Height", "Height", "Height of the refinement regions", GH_ParamAccess.list);
//            pManager.AddIntegerParameter("Mesh Level", "Level", 
//                "Mesh Level of the refinement regions. The value given here is the mesh level at the inner most refinement region.\n" +
//                "Every refinement region after that will have a mesh level is that 1 lower than the previous.", 
//                GH_ParamAccess.item);
//            pManager.AddNumberParameter("Resolution", "Resolution", 
//                "Resolution of each of the refinement regions in meters.\n" +
//                "This input is used only if Level doesn't have a input.", GH_ParamAccess.list
//                );

//            pManager[3].Optional = true;
//            pManager[4].Optional = true;
//        }

//        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
//        {
//            pManager.AddMeshParameter("Refinement Regions", "Regions", "Created refinement regions",
//                GH_ParamAccess.list);
//        }

//        protected override Bitmap Icon => Resources.IconRefinementRegion;

//        public override Guid ComponentGuid => new Guid("2b15f056-46d8-4325-a510-0dfc06eb2e09");

//        protected override void SolveInstance(IGH_DataAccess DA)
//        {
//            var centerPoint = new Point3d();
//            var radii = new List<double>();
//            var heights = new List<double>();
//            int? meshLevel = null;
//            var resolutions = new List<double>();


//            if (!DA.GetData(0, ref centerPoint)) return;
//            if (!DA.GetDataList(1, radii)) return;
//            if (!DA.GetDataList(2, heights)) return;
//            DA.GetData(3, ref meshLevel);
//            DA.GetDataList(4, resolutions);

//            if (meshLevel == null && resolutions.Count == 0)
//            {
//                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Please provide a Mesh Level or a list of Resolutions");
//            }

//            var refinementMeshes = CreateRadialRefinementRegions(centerPoint, radii, heights, meshLevel, resolutions);

//            DA.SetDataList(0, refinementMeshes);
//        }

//        private static List<GH_Mesh> CreateRadialRefinementRegions(Point3d centerPoint, List<double> radii, List<double> heights, int? meshLevel, List<double> resolutions)
//        {
//            var meshes = new List<GH_Mesh>();
//            var index = 0;
//            foreach (var mesh in radii.Select(radius => CreateRefinementMesh(centerPoint, radius, index, heights, meshLevel, resolutions)))
//            {
//                meshes.Add(mesh);
//                index++;
//            }

//            return meshes;
//        }

//        private static GH_Mesh CreateRefinementMesh(Point3d centerPoint, double radius, int index, List<double> heights, int? meshLevel, List<double> resolutions)
//        {
//            var height = index < heights.Count ? heights[index] : heights.Last();
//            var mesh = CreateMesh(centerPoint, radius, height);

//            if (meshLevel != null)
//            {
//                meshLevel = meshLevel - index <= 1 ? 1 : meshLevel - index;

//                var refinementRegion = new RefinementDetails
//                {
//                    Mode = "inside",
//                    Levels = $"(( {meshLevel} {meshLevel} ))"
//                };
//                Geometry.setUserString(
//                    mesh,
//                    "ComputeRefinementRegion",
//                    refinementRegion.ToJson()
//                );
//            }
//            else
//            {
//                var resolution = index < resolutions.Count ? resolutions[index] : resolutions.Last();
//                var refinementRegion = new RefinementDetails
//                {
//                    Mode = "inside",
//                    Resolution = resolution
//                };
//                Geometry.setUserString(
//                    mesh,
//                    "ComputeRefinementRegion",
//                    refinementRegion.ToJson()
//                );
//            }

//            Geometry.setUserString(
//                mesh,
//                "ComputeName",
//                $"Refinement_X{centerPoint.X}_Y{centerPoint.Y}_Z{centerPoint.Z}_R{radius}"
//            );
//            return mesh;
//        }

//        private static GH_Mesh CreateMesh(Point3d centerPoint, double radius, double height)
//        {
//            var circle = new Circle(centerPoint, radius);
//            var extrusion = Extrusion.Create(
//                circle.ToNurbsCurve(), 
//                height,
//                true
//                );

//            return new GH_Mesh(Mesh.CreateFromSurface(extrusion));
//        }
//    }
//}