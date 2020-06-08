using System;
using System.Collections.Generic;
using computeGH.core;
using Grasshopper.Kernel;
using Rhino.Geometry;
using Newtonsoft.Json;
using computeGH.types;

namespace ComputeCS.Grasshopper
{
    public class computeLogin : GH_ComponentC
    {
        /// <summary>
        /// Initializes a new instance of the computeLogin class.
        /// </summary>
        public computeLogin()
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

            client = new ComputeClient(url);
            var tokens = client.Auth(user.username, user.password);
            var output = SerializeIO.OutputToJson(new Inputs {
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
            get { return new Guid("898478bb-5b4f-4972-951a-d9e71ba0086b"); }
        }
    }
}