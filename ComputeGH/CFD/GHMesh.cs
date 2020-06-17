using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Rhino.Geometry;
using Newtonsoft.Json;
using ComputeCS.types;

namespace ComputeCS.Grasshopper
{
    public class ComputeMesh : GH_Component
    {

        /// <summary>
        /// Initializes a new instance of the computeLogin class.
        /// </summary>
        public ComputeMesh()
          : base("Compute Mesh", "Mesh",
              "Create the Mesh Parameters for a CFD Case",
              "Compute", "Mesh")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Input", "Input", "Input from previous Compute Component", GH_ParamAccess.item);
            pManager.AddTextParameter("Domain", "Domain", "", GH_ParamAccess.item);
            pManager.AddTextParameter("Default Surfaces", "Default Surfaces", "", GH_ParamAccess.item);
            pManager.AddTextParameter("Overrides", "Overrides", "", GH_ParamAccess.list);

            pManager[2].Optional = true;
            pManager[3].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Output", "Output", "Output", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string inputJson = null;
            string domain = null;
            Dictionary<string, object> defaultSurfaces = new Dictionary<string, object> {
                { "Plane", new Dictionary<string, object>{
                        { "level", new Dictionary<string, string>{
                                { "min", "3"},
                                { "max", "3"},
                        }
                        }
                }
                }
            };
            Dictionary<string, object> overrides = new Dictionary<string, object>();

            if (!DA.GetData(0, ref inputJson)) return;
            if (!DA.GetData(1, ref domain)) return;
            


            var outputs = Components.Mesh.Setup(
                inputJson,
                domain,
                defaultSurfaces,
                overrides
            );

            DA.SetData(0, outputs["out"]);

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
                return null;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("c53c4297-c151-4c24-95bb-0f89f2c875f1"); }
        }
    }
}