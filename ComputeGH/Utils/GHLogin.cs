using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Rhino.Geometry;
using Newtonsoft.Json;
using ComputeCS.types;


namespace ComputeCS.Grasshopper
{
    public class ComputeLogin : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the computeLogin class.
        /// </summary>
        public ComputeLogin()
          : base("Compute Login", "Login",
              "Login to Procedural Compute",
              "Compute", "Utils")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("username", "username", "User Name", GH_ParamAccess.item);
            pManager.AddTextParameter("password", "password", "Password", GH_ParamAccess.item);
            pManager.AddTextParameter("computeURL", "url", "URL for Compute", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("auth", "auth", "auth", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string username = null;
            string password = null;
            string url = null;

            if (!DA.GetData(0, ref username)) return;
            if (!DA.GetData(1, ref password)) return;
            if (!DA.GetData(2, ref url)) return;

            var client = new ComputeClient(url);
            var tokens = client.Auth(username, password);
            var output = SerializeIO.OutputToJson(new Inputs
            {
                Auth = tokens,
                Url = url
            });

            DA.SetData(0, output);

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
            get { return new Guid("3bcb2f6e-7c64-41b2-8086-6b1ddd6e80ee"); }
        }
    }
}