using System;
using System.Collections.Generic;
using System.Drawing;
using ComputeCS.types;
using ComputeGH.Grasshopper.Utils;
using ComputeGH.Properties;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace ComputeCS.Grasshopper
{
    public class CFDDomain : GH_Component
    {
        public CFDDomain() : base("CFD Domain", "Domain", "Create a CFD Domain", "Compute", "Mesh")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Objects", "Objects", "Objects to include in the CFD domain",
                GH_ParamAccess.list);
            pManager.AddIntegerParameter("Domain Type", "Domain Type",
                "Domain type. Choose between: 0: 3D domain, 1: 2D domain.", GH_ParamAccess.item, 0);
            pManager.AddNumberParameter("Cell Size", "Cell Size", "Cell Size", GH_ParamAccess.item);
            pManager.AddBooleanParameter("z0", "z0", "Base z=0", GH_ParamAccess.item, true);
            pManager.AddBooleanParameter("Center XY", "Center XY", "centerXY", GH_ParamAccess.item, true);
            pManager.AddBooleanParameter("Square", "Square", "Whether the domain should be square or not.",
                GH_ParamAccess.item, true);
            pManager.AddNumberParameter("XY Scale", "XY Scale", "Scale in the X and Y directions", GH_ParamAccess.item,
                1.25);
            pManager.AddNumberParameter("XY Offset", "XY Offset", "Offset in the X and Y directions",
                GH_ParamAccess.item, 1.0);
            pManager.AddNumberParameter("Z Scale", "Z Scale", "Scale in the Z direction", GH_ParamAccess.item, 2.0);
            pManager.AddPlaneParameter("Plane", "Plane",
                "Plane for creating a 2D domain. Is not used if you create a 3D Domain", GH_ParamAccess.item);

            pManager[3].Optional = true;
            pManager[4].Optional = true;
            pManager[5].Optional = true;
            pManager[6].Optional = true;
            pManager[7].Optional = true;
            pManager[8].Optional = true;
            pManager[9].Optional = true;

            AddNamedValues(pManager[1] as Param_Integer, DomainTypes);
        }

        private static void AddNamedValues(Param_Integer param, List<string> values)
        {
            var index = 0;
            foreach (var value in values)
            {
                param.AddNamedValue(value, index);
                index++;
            }
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Output", "Output", "Output", GH_ParamAccess.item);
            pManager.AddTextParameter("Info", "Info",
                "Info\nCell estimation is based on an equation developed by Alexander Jacobson", GH_ParamAccess.item);
            pManager.AddBoxParameter("Bounding Box", "Bounding Box", "Bounding boxes representing the domain",
                GH_ParamAccess.item);
            pManager.AddGenericParameter("Mesh", "Mesh", "Mesh", GH_ParamAccess.list);
        }

        protected override Bitmap Icon => Resources.IconRectDomain;

        public override Guid ComponentGuid => new Guid("12aa93b6-fc8e-417c-9c8a-200d59e39a21");

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var geometry = new List<IGH_GeometricGoo>();
            var domainType = 0;
            var cellSize = 1.0;
            var z0 = false;
            var centerXY = true;
            var xyScale = 1.2;
            var xyOffset = 10.0;
            var zScale = 2.0;
            var square = true;
            var plane = new Plane();

            if (!DA.GetDataList(0, geometry))
            {
                return;
            }

            DA.GetData(1, ref domainType);

            if (!DA.GetData(2, ref cellSize))
            {
                return;
            }

            DA.GetData(3, ref z0);
            DA.GetData(4, ref centerXY);
            DA.GetData(5, ref square);
            DA.GetData(6, ref xyScale);
            DA.GetData(7, ref xyOffset);
            DA.GetData(8, ref zScale);
            DA.GetData(9, ref plane);

            // Create Bounding Box
            var bbs = new List<BoundingBox>();
            foreach (var element in geometry)
            {
                bbs.Add(element.Boundingbox);
            }

            var bb = Domain.getMultiBoundingBox(bbs, cellSize, z0, centerXY, xyScale, xyOffset, zScale, square);
            if (domainType == 1)
            {
                bb = Domain.Create2DDomain(bb, plane, cellSize);
            }

            // Construct Surface Dict

            var surfaces = GetSurfaces(geometry);

            var refinementRegions = GetRefinementRegions(geometry);

            var locationInMesh = Domain.GetLocationInMesh(new Box(bb));
            var cellEstimation = Convert.ToInt32(Domain.EstimateCellCount(geometry, cellSize, xyScale, zScale));
            var outputs = new Inputs
            {
                Mesh = new CFDMesh
                {
                    BaseMesh = new BaseMesh
                    {
                        Type = "simpleBox",
                        CellSize = cellSize,
                        BoundingBox = new Dictionary<string, object>
                        {
                            {
                                "min", new List<double>
                                {
                                    bb.Min.X, bb.Min.Y, bb.Min.Z
                                }
                            },
                            {
                                "max", new List<double>
                                {
                                    bb.Max.X, bb.Max.Y, bb.Max.Z
                                }
                            }
                        },
                        Parameters = new Dictionary<string, string>
                        {
                            {"square", Convert.ToString(square)},
                            {"z0", Convert.ToString(z0)}
                        }
                    },
                    SnappyHexMesh = new SnappyHexMesh
                    {
                        Surfaces = surfaces,
                        Overrides = new SnappyHexMeshOverrides
                        {
                            CastellatedMeshControls = new CastellatedMeshControls
                            {
                                LocationInMesh = new List<double>
                                {
                                    locationInMesh.X, locationInMesh.Y, locationInMesh.Z
                                }
                            }
                        },
                        RefinementRegions = refinementRegions
                    },
                    CellEstimate = cellEstimation
                }
            };

            DA.SetData(0, outputs.ToJson());
            DA.SetData(1,
                $"Estimated number of cells: {cellEstimation}\nThis is only an estimation and will probably be correct within a +/-10% margin.");
            DA.SetData(2, bb);
            DA.SetDataList(3, geometry);
        }

        private static readonly List<string> DomainTypes = new List<string>
        {
            "3D", "2D"
        };

        private static List<RefinementRegion> GetRefinementRegions(List<IGH_GeometricGoo> meshes)
        {
            var refinementRegions = new List<RefinementRegion>();
            foreach (var mesh in meshes)
            {
                var regionName = Geometry.getUserString(mesh, "ComputeName");
                var refinementDetails = Geometry.getUserString(mesh, "ComputeRefinementRegion");
                if (regionName == null || refinementDetails == null)
                {
                    continue;
                }

                refinementRegions.Add(
                    new RefinementRegion
                    {
                        Name = regionName,
                        Details = new RefinementDetails().FromJson(refinementDetails)
                    }
                );
            }

            return refinementRegions;
        }

        private static Dictionary<string, object> GetSurfaces(List<IGH_GeometricGoo> meshes)
        {
            var surfaces = new Dictionary<string, object>();
            foreach (var mesh in meshes)
            {
                var surfaceName = Geometry.getUserString(mesh, "ComputeName");
                var minLevel = Geometry.getUserString(mesh, "ComputeMeshMinLevel");
                var maxLevel = Geometry.getUserString(mesh, "ComputeMeshMaxLevel");

                if (surfaceName == null || minLevel == null)
                {
                    continue;
                }

                surfaces.Add(
                    surfaceName, new Dictionary<string, object>
                    {
                        {
                            "level", new Dictionary<string, string>
                            {
                                {"min", minLevel},
                                {"max", maxLevel},
                            }
                        }
                    }
                );
            }

            return surfaces;
        }
    }
}