using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using ComputeCS.types;
using ComputeGH.Grasshopper.Utils;
using ComputeGH.Properties;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace ComputeGH.Radiation
{
    public class GHRadiationMesh : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the computeLogin class.
        /// </summary>
        public GHRadiationMesh()
            : base("Radiation Mesh", "Mesh",
                "Collect the mesh objects for a radiation Case",
                "Compute", "Radiation")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Meshes", "Meshes", "A list of mesh objects from Compute Set Name", GH_ParamAccess.list);
            pManager.AddPointParameter("Probe Points", "Probe Points", "Probe points where you want to get radiation values.", GH_ParamAccess.list);
            pManager.AddVectorParameter("Probe Points", "Probe Normals", "Normals to the probe points given above.", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Output", "Output", "Output", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var meshes = new List<IGH_GeometricGoo>();
            var points = new List<Point3d>();
            var normals = new List<Vector3d>();

            DA.GetDataList(0, meshes);
            DA.GetDataList(1, points);
            DA.GetDataList(2, normals);


            var outputs = new RadiationMesh
            {
                MeshIds = Geometry.GetObjRefStrings(meshes),
                Points = points.Select(point => new List<double>{point.X, point.Y, point.Z}).ToList(),
                Normals = normals.Select(normal => new List<double>{normal.X, normal.Y, normal.Z}).ToList()
            }.ToJson();

            DA.SetData(0, outputs);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override Bitmap Icon => Resources.IconMesh;

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("ba3f203f-3c7b-4669-b35f-3bacb2e3eba9");
    }
}