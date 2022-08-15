//using System;
//using System.Collections.Generic;
//using System.Drawing;
//using System.Linq;
//using ComputeCS.types;
//using ComputeGH.Grasshopper.Utils;
//using ComputeGH.Properties;
//using Grasshopper.Kernel;
//using Grasshopper.Kernel.Parameters;
//using Grasshopper.Kernel.Types;
//using Rhino.Geometry;

//namespace ComputeCS.Grasshopper
//{
//    public class CFDDomain : PB_Component
//    {
//        public CFDDomain() : base("CFD Domain", "Domain", "Create a CFD Domain", "Compute", "Mesh")
//        {
//        }

//        protected override void RegisterInputParams(GH_InputParamManager pManager)
//        {
//            pManager.AddMeshParameter("Meshes", "Meshes", "Meshes to include in the CFD domain",
//                GH_ParamAccess.list);
//            pManager.AddIntegerParameter("Domain Type", "Domain Type",
//                "Domain type. Choose between: 0: 3D domain, 1: 2D domain.", GH_ParamAccess.item, 0);
//            pManager.AddNumberParameter("Cell Size", "Cell Size", "Base Cell Size throughout the domain.", GH_ParamAccess.item);
//            pManager.AddBooleanParameter("Align Bottom", "Align Bottom", "Sets whether the bottom of the domain should align with the bottom of the geometry or the domain should have the same center as the geometry.", GH_ParamAccess.item, true);
//            pManager.AddBooleanParameter("Center XY", "Center XY", "centerXY", GH_ParamAccess.item, true);
//            pManager.AddBooleanParameter("Square", "Square", "Whether the domain should be square or not.",
//                GH_ParamAccess.item, true);
//            pManager.AddNumberParameter("XY Scale", "XY Scale", "Scale in the X and Y directions", GH_ParamAccess.item,
//                3);
//            pManager.AddNumberParameter("XY Offset", "XY Offset", "Offset in the X and Y directions",
//                GH_ParamAccess.item, 1.0);
//            pManager.AddNumberParameter("Z Scale", "Z Scale", "Scale in the Z direction. Recommended value is 5m", GH_ParamAccess.item, 5.0);
//            pManager.AddPlaneParameter("Plane", "Plane",
//                "Plane for creating a 2D domain. Is not used if you create a 3D Domain", GH_ParamAccess.item);

//            pManager[3].Optional = true;
//            pManager[4].Optional = true;
//            pManager[5].Optional = true;
//            pManager[6].Optional = true;
//            pManager[7].Optional = true;
//            pManager[8].Optional = true;
//            pManager[9].Optional = true;

//            AddNamedValues(pManager[1] as Param_Integer, DomainTypes);
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

//        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
//        {
//            pManager.AddTextParameter("Output", "Output", "Output", GH_ParamAccess.item);
//            pManager.AddTextParameter("Info", "Info",
//                "Info\nCell estimation is based on an equation developed by Alexander Jacobson", GH_ParamAccess.item);
//            pManager.AddBoxParameter("Bounding Box", "Bounding Box", "Bounding boxes representing the domain",
//                GH_ParamAccess.item);
//            pManager.AddMeshParameter("Mesh", "Mesh", "Mesh", GH_ParamAccess.list);
//        }

//        protected override Bitmap Icon => Resources.IconRectDomain;

//        public override Guid ComponentGuid => new Guid("12aa93b6-fc8e-417c-9c8a-200d59e39a21");

//        protected override void SolveInstance(IGH_DataAccess DA)
//        {
//            var geometry = new List<IGH_GeometricGoo>();
//            var domainType = 0;
//            var cellSize = 1.0;
//            var z0 = false;
//            var centerXY = true;
//            var xyScale = 1.2;
//            var xyOffset = 10.0;
//            var zScale = 2.0;
//            var square = true;
//            var plane = new Plane();

//            if (!DA.GetDataList(0, geometry))
//            {
//                return;
//            }

//            DA.GetData(1, ref domainType);

//            if (!DA.GetData(2, ref cellSize))
//            {
//                return;
//            }

//            if (cellSize <= 0)
//            {
//                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, $"Cell Size has to be larger that 0. Given Cell Size was: {cellSize}");
//            }

//            DA.GetData(3, ref z0);
//            DA.GetData(4, ref centerXY);
//            DA.GetData(5, ref square);
//            DA.GetData(6, ref xyScale);
//            DA.GetData(7, ref xyOffset);
//            DA.GetData(8, ref zScale);
//            DA.GetData(9, ref plane);

//            // Create Bounding Box
//            var bbs = geometry.Select(element => element.Boundingbox).ToList();

//            var boundingBox = Domain.GetMultiBoundingBox(bbs, cellSize, z0, centerXY, xyScale, xyOffset, zScale, square);
//            if (domainType == 1)
//            {
//                boundingBox = Domain.Create2DDomain(boundingBox, plane, cellSize);
//            }

//            // Construct Surface Dict

//            var surfaces = GetSurfaces(geometry, cellSize);

//            var refinementRegions = GetRefinementRegions(geometry, cellSize);

//            var locationInMesh = Domain.GetLocationInMesh(new Box(boundingBox));
//            var cellEstimation = Convert.ToInt32(Domain.EstimateCellCount(geometry, cellSize, xyScale, zScale));
//            var outputs = new Inputs
//            {
//                Mesh = new CFDMesh
//                {
//                    BaseMesh = new BaseMesh
//                    {
//                        Type = "simpleBox",
//                        CellSize = cellSize,
//                        BoundingBox = new Dictionary<string, object>
//                        {
//                            {
//                                "min", new List<double>
//                                {
//                                    boundingBox.Min.X, boundingBox.Min.Y, boundingBox.Min.Z
//                                }
//                            },
//                            {
//                                "max", new List<double>
//                                {
//                                    boundingBox.Max.X, boundingBox.Max.Y, boundingBox.Max.Z
//                                }
//                            }
//                        },
//                        Parameters = new Dictionary<string, string>
//                        {
//                            {"square", Convert.ToString(square)},
//                            {"z0", Convert.ToString(z0)}
//                        }
//                    },
//                    SnappyHexMesh = new SnappyHexMesh
//                    {
//                        Surfaces = surfaces,
//                        Overrides = new SnappyHexMeshOverrides
//                        {
//                            CastellatedMeshControls = new CastellatedMeshControls
//                            {
//                                LocationInMesh = new List<double>
//                                {
//                                    locationInMesh.X, locationInMesh.Y, locationInMesh.Z
//                                }
//                            }
//                        },
//                        RefinementRegions = refinementRegions
//                    },
//                    CellEstimate = cellEstimation
//                }
//            };

//            DA.SetData(0, outputs.ToJson());
//            DA.SetData(1, GetInfoText(cellEstimation, cellSize, surfaces.Keys.Select(key => surfaces[key].Level.Min)));
//            DA.SetData(2, boundingBox);
//            DA.SetDataList(3, geometry);
//        }

//        private static string GetInfoText(int cellEstimation, double baseCellSize, IEnumerable<int> meshLevels)
//        {
//            var text = $"Estimated number of cells: {cellEstimation}\n" +
//                       $"This is only an estimation and will probably be correct within a +/-10% margin.\n\n" +
//                       $"Base Cell Size is {baseCellSize}m\n";
//            var levels = meshLevels.GroupBy(level => level);
//            foreach (var level in levels)
//            {
//                var resolution = baseCellSize / Math.Pow(2, level.Key);
//                text += $"* There is {level.Count()} meshes with mesh level {level.Key}. They will have a surface resolution of {resolution}m\n";    
//            }

//            return text;
//        }

//        private static readonly List<string> DomainTypes = new List<string>
//        {
//            "3D", "2D"
//        };

//        private static List<RefinementRegion> GetRefinementRegions(List<IGH_GeometricGoo> meshes, double cellSize)
//        {
//            var refinementRegions = new List<RefinementRegion>();
//            foreach (var mesh in meshes)
//            {
//                var regionName = Geometry.getUserString(mesh, "ComputeName");
//                var refinementDetails = Geometry.getUserString(mesh, "ComputeRefinementRegion");
//                if (regionName == null || refinementDetails == null)
//                {
//                    continue;
//                }

//                var detail = new RefinementDetails().FromJson(refinementDetails);
//                detail.CellSize = cellSize;
//                refinementRegions.Add(
//                    new RefinementRegion
//                    {
//                        Name = regionName,
//                        Details = detail
//                    }
//                );
//            }

//            return refinementRegions;
//        }

//        private static Dictionary<string, MeshLevelDetails> GetSurfaces(List<IGH_GeometricGoo> meshes, double cellSize)
//        {
//            var surfaces = new Dictionary<string, MeshLevelDetails>();
//            foreach (var mesh in meshes)
//            {
//                var surfaceName = Geometry.getUserString(mesh, "ComputeName");
//                var meshLevels = Geometry.getUserString(mesh, "ComputeMeshLevels");

//                if (string.IsNullOrEmpty(surfaceName) || string.IsNullOrEmpty(meshLevels))
//                {
//                    continue;
//                }

//                var meshLevel = new MeshLevelDetails().FromJson(meshLevels);
//                meshLevel.CellSize = cellSize;
//                surfaces.Add(
//                    surfaceName, meshLevel
//                );
//            }

//            return surfaces;
//        }
//    }
//}