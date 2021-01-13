using System;
using System.Collections.Generic;
using ComputeGH.Grasshopper.Utils;
using ComputeGH.Properties;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
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
            pManager.AddSurfaceParameter("Surface", "Surface", "Surface to create the analysis mesh from.", GH_ParamAccess.list);
            pManager.AddNumberParameter("Grid Size", "Grid Size", "Size of the mesh grid. Default is 1.0", GH_ParamAccess.item);
            pManager.AddBrepParameter("Exclude", "Exclude", "Breps that should be cut out from the analysis surface",
                GH_ParamAccess.list);
            pManager.AddNumberParameter("Offset", "Offset", "Distance to offset the mesh from the surface. Default is 1.5", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Offset Direction", "Offset Direction",
                "Direction the surface should be offset in. Default is z", GH_ParamAccess.item, 2);

            pManager[1].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;
            pManager[4].Optional = true;
            
            AddNamedValues(pManager[4] as Param_Integer, OffsetDirection);
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

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddMeshParameter("AnalysisMesh", "AnalysisMesh", "Created analysis mesh", GH_ParamAccess.tree);
            pManager.AddPointParameter("FaceCenters", "FaceCenters", "Face centers of the created mesh", GH_ParamAccess.tree);
            pManager.AddPointParameter("FaceNormals", "FaceNormals", "Face Normals of the created mesh", GH_ParamAccess.tree);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var baseSurfaces = new List<Surface>();
            var gridSize = 1.0;
            var excludeGeometry = new List<Brep>();
            var offset = 1.5;
            var offsetDirection = 2;

            if (!DA.GetDataList(0, baseSurfaces)) return;
            DA.GetData(1, ref gridSize);
            DA.GetDataList(2, excludeGeometry);
            DA.GetData(3, ref offset);
            DA.GetData(4, ref offsetDirection);
            
            var result = Geometry.CreateAnalysisMesh(
                baseSurfaces, gridSize, excludeGeometry, offset, OffsetDirection[offsetDirection]
                );

            DA.SetDataTree(0, result["analysisMesh"]);
            DA.SetDataTree(1, result["faceCenters"]);
            DA.SetDataTree(2, result["faceNormals"]);
        }

        private static readonly List<string> OffsetDirection = new List<string>
        {
            "x",
            "y",
            "z",
            "normal",
        };

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.IconMesh;

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("be364a6b-0339-4afa-8c42-2d1a0d031028");
    }
}