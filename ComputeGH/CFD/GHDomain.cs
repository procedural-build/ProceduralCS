using System;
using System.Collections.Generic;
using ComputeCS.Grasshopper.Utils;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using ComputeCS.types;


namespace ComputeCS.Grasshopper
{
    public class cfdDomain : GH_Component
    {
        public cfdDomain() : base("CFD Domain", "cfdDomain", "Create a CFD Domainn", "Compute", "Mesh")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("objs", "objs", "objs", GH_ParamAccess.list);
            pManager.AddNumberParameter("cellSize", "cellSize", "Cell Size", GH_ParamAccess.item);
            pManager.AddBooleanParameter("z0", "z0", "Base z=0", GH_ParamAccess.item, true);
            pManager.AddBooleanParameter("centerXY", "centerXY", "centerXY", GH_ParamAccess.item, true);
            pManager.AddBooleanParameter("square", "square", "square", GH_ParamAccess.item, true);
            pManager.AddNumberParameter("xyScale", "xyScale", "xyScale", GH_ParamAccess.item, 1.25);
            pManager.AddNumberParameter("xyOffset", "xyOffset", "xyOffset", GH_ParamAccess.item, 1.0);
            pManager.AddNumberParameter("zScale", "zScale", "zScale", GH_ParamAccess.item, 2.0);

            pManager[2].Optional = true;
            pManager[3].Optional = true;
            pManager[4].Optional = true;
            pManager[5].Optional = true;
            pManager[6].Optional = true;
            pManager[7].Optional = true;
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Output", "Output", "Output", GH_ParamAccess.item);
            pManager.AddBoxParameter("Bounding Box", "Bounding Box", "BoundingBox", GH_ParamAccess.item);
            pManager.AddGenericParameter("objs", "objs", "objs", GH_ParamAccess.list);
        }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return null; //ghODSResources.IconRectDomain;
            }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("12aa93b6-fc8e-417c-9c8a-200d59e39a21"); }
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {

            List<IGH_GeometricGoo> geometry = new List<IGH_GeometricGoo>();
            double cellSize = 1.0;
            bool z0 = false;
            bool centerXY = true;
            double xyScale = 1.2;
            double xyOffset = 10;
            double zScale = 2.0;
            bool square = true;

            if (!DA.GetDataList(0, geometry)) { return; }
            if (!DA.GetData(1, ref cellSize)) { return; }
            if (!DA.GetData(2, ref z0)) { return; }
            if (!DA.GetData(3, ref centerXY)) { return; }
            if (!DA.GetData(4, ref square)) { return; }
            if (!DA.GetData(5, ref xyScale)) { return; }
            if (!DA.GetData(6, ref xyOffset)) { return; }
            if (!DA.GetData(7, ref zScale)) { return; }

            // Create Bounding Box
            List<BoundingBox> bbs = new List<BoundingBox>();
            foreach (IGH_GeometricGoo o in geometry) { bbs.Add(o.Boundingbox); }
            BoundingBox bb = Domain.getMultiBoundingBox(bbs, cellSize, z0, centerXY, xyScale, xyOffset, zScale, square);

            // Construct Surface Dict
            
            Dictionary<string, object> surfaces = new Dictionary<string, object>();
            foreach (IGH_GeometricGoo mesh in geometry) {
                surfaces.Add(
                Geometry.getUserString(mesh, "ComputeName"), new Dictionary<string, object>
                    {
                        { "level", new Dictionary<string, string>{
                                { "min", Geometry.getUserString(mesh, "ComputeMeshMinLevel")},
                                { "max", Geometry.getUserString(mesh, "ComputeMeshMaxLevel")},
                        }
                        }
                    }                
                );
            }

            var locationInMesh = Domain.getLocationInMesh(new Box(bb));

            var outputs = new Inputs {
                Mesh = new CFDMesh {
                    BaseMesh = new BaseMesh
                    {
                        Type = "simpleBox",
                        CellSize = cellSize,
                        BoundingBox = new Dictionary<string, object> {
                        {"min", new List<double>{
                            bb.Min.X, bb.Min.Y, bb.Min.Z
                        } },
                        {"max", new List<double>{
                            bb.Max.X, bb.Max.Y, bb.Max.Z
                        } }
                        },
                        Parameters = new Dictionary<string, string>
                        {
                            {"square", Convert.ToString(square) },
                            {"z0", Convert.ToString(z0) }
                        }
                    },
                    SnappyHexMesh = new SnappyHexMesh
                    {
                        Surfaces = surfaces,
                        Overrides = new Dictionary<string, object>
                        {
                            { "castellatedMeshControls", new Dictionary<string, object>
                            {
                                { "locationInMesh", new List<double>
                                {
                                    locationInMesh.X, locationInMesh.Y, locationInMesh.Z
                                }
                                }
                            }
                            }
                        }
                    }
                }
            };

            DA.SetData(0, outputs.ToJson());
            DA.SetData(1, bb);
            DA.SetDataList(2, geometry);
        }
    }
}