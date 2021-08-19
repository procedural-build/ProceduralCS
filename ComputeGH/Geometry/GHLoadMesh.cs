using System;
using System.Drawing;
using System.IO;
using System.Linq;
using ComputeGH.Grasshopper.Utils;
using ComputeGH.Properties;
using Grasshopper.Kernel;
using Rhino.Geometry;
using Grasshopper3D = Grasshopper;

namespace ComputeCS.Grasshopper
{
    public class GHLoadMesh : PB_Component
    {
        /// <summary>
        /// Initializes a new instance of the GHRecolorMesh class.
        /// </summary>
        public GHLoadMesh()
            : base("Load Mesh", "Load Mesh",
                "Load a mesh into Grasshopper from a local file",
                "Compute", "Geometry")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("File Path", "Path", "File path to load the mesh from", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Load", "Load", "Load mesh", GH_ParamAccess.item);

            pManager[1].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "Mesh", "Loaded mesh", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var filePath = "";
            var load = false;


            if ((!DA.GetData(0, ref filePath))) return;

            DA.GetData(1, ref load);

            if (load)
            {
                if (!File.Exists(filePath))
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"Could not find file {filePath}. Please provide a valid file path.");
                    return;
                }

                meshes = Import.LoadMeshFromPath(filePath, null, null).First().Value;
            }

           
            DA.SetData(0, meshes);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override Bitmap Icon => Resources.IconMesh;

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("2748f8d3-9526-4643-b1ec-b33d20e2f8b3");

        private Mesh meshes;
    }
}