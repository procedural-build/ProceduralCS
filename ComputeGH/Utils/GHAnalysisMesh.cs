using System;
using System.Collections.Generic;
using ComputeGH.Properties;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace ComputeGH.Utils
{
    public class GHAnalysisMesh : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the GHAnalysisMesh class.
        /// </summary>
        public GHAnalysisMesh()
          : base("AnalysisMesh", "AnalysisMesh",
              "Create a mesh from a surface, which can be used for analyses",
              "Compute", "Utils")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddSurfaceParameter("Surface", "Surface", "Surface to create the analysis mesh from.", GH_ParamAccess.item);
            pManager.AddNumberParameter("Grid Size", "Grid Size", "Size of the mesh grid. Default is 1.0", GH_ParamAccess.item);
            pManager.AddNumberParameter("Offset", "Offset", "Distance to offset the mesh from the surface along the z-axis. Default is 1.5", GH_ParamAccess.item);

            pManager[1].Optional = true;
            pManager[2].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddMeshParameter("AnalysisMesh", "AnalysisMesh", "Created analysis mesh", GH_ParamAccess.item);
            pManager.AddPointParameter("FaceCenters", "FaceCenters", "Face centers of the created mesh", GH_ParamAccess.list);
            pManager.AddPointParameter("FaceVectors", "FaceVectors", "Face vectors of the created mesh", GH_ParamAccess.list);
            pManager.AddNumberParameter("FaceAreas", "FaceAreas", "Area of each face of the created mesh", GH_ParamAccess.list);
            
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Surface baseSurface = null;
            double gridSize = 1.0;
            double offset = 1.5;

            if (!DA.GetData(0, ref baseSurface)) return;
            DA.GetData(1, ref gridSize);
            DA.GetData(2, ref offset);

            var result = createAnalysisMesh(baseSurface, gridSize, offset);

            DA.SetData(0, result["analysisMesh"]);
            DA.SetData(1, result["faceCenters"]);
            DA.SetData(2, result["faceVectors"]);
            DA.SetData(3, result["faceAreas"]);
        }


        private Dictionary<string, object> createAnalysisMesh(Surface baseSurface, double gridSize, double offset)
        {
            var analysisMesh = CreateMeshFromSurface(baseSurface, gridSize);

            return new Dictionary<string, object> {
                {"analysisMesh", analysisMesh},
                {"faceCenters", null},
                {"faceVectors", null},
                {"faceAreas", null},
            };

        }

        private Mesh CreateMeshFromSurface(Surface surface, double gridSize)
        {
            var meshParams = new MeshingParameters {
                MaximumEdgeLength = gridSize,
                MinimumEdgeLength = gridSize,
                GridAspectRatio = 1
            };

            try
            {
               return Mesh.CreateFromSurface(surface, meshParams);
            }
            catch {
                throw new Exception("Error in converting Brep to Mesh");
            }

            
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return Resources.IconMesh;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("be364a6b-0339-4afa-8c42-2d1a0d031028"); }
        }
    }
}