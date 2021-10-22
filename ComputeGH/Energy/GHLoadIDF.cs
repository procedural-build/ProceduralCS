using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using ComputeGH.Grasshopper.Utils;
using ComputeGH.Properties;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace ComputeCS.Grasshopper
{
    public class GHLoadIDF: PB_Component
    {
        /// <summary>
        /// Initializes a new instance of the GHRecolorMesh class.
        /// </summary>
        public GHLoadIDF()
            : base("Load IDF File", "Load IDF",
                "Load a IDF file into Grasshopper from a local path",
                "Compute", "Energy")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("File Path", "Path", "File path to load the IDF from", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Load", "Load", "Load IDF file", GH_ParamAccess.item);

            pManager[1].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddMeshParameter("Geometry", "Geometry", "Geometry loaded from IDF file", GH_ParamAccess.list);
            pManager.AddMeshParameter("Zones", "Zones", "Zones loaded from IDF file", GH_ParamAccess.list);
            pManager.AddTextParameter("Properties", "Properties", "Properties of elements loaded from IDF file", GH_ParamAccess.item);
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

                (geometry, zones, properties) = Import.LoadIDFFromPath(filePath);
            }

           
            DA.SetDataList(0, geometry);
            DA.SetDataList(1, zones);
            DA.SetData(2, properties);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override Bitmap Icon => Resources.IconMesh;

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("314cf72a-92e6-41f4-8c9d-3bcd9b91aafe");

        private List<Mesh> geometry;
        private List<Mesh> zones;
        private string properties;
    }
}