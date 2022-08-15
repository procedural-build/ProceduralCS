using ComputeCS.types;
using ComputeGH.Grasshopper.Utils;
using ComputeGH.Properties;
using Grasshopper.Kernel;
using System;
using System.Drawing;
using System.Threading.Tasks;

namespace ComputeCS.Grasshopper
{
    public class LoginResult
    {
        public Inputs Value { get; set; }
        public Exception Errors { get; set; }
    }
    public class ComputeLogin : PB_TaskCapableComponent<LoginResult>
    {

        /// <summary>
        /// Initializes a new instance of the computeLogin class.
        /// </summary>
        public ComputeLogin()
            : base("Compute Login", "Login",
                "Login to Procedural Compute",
                "Compute", "General")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Username", "Username", "Username", GH_ParamAccess.item);
            pManager.AddTextParameter("Password", "Password", "Password", GH_ParamAccess.item);
            pManager.AddTextParameter("Compute URL", "URL", "URL for Compute", GH_ParamAccess.item, "https://compute.procedural.build");
            pManager.AddBooleanParameter("Retry", "Retry", "Force retry to connect to Compute", GH_ParamAccess.item);

            pManager[2].Optional = true;
            pManager[3].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Auth", "Auth", "Auth", GH_ParamAccess.item);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override Bitmap Icon => Resources.IconLicense;

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("3bcb2f6e-7c64-41b2-8086-6b1ddd6e80ee");

        public override Task<LoginResult> CreateTask(IGH_DataAccess DA)
        {
            string username = null;
            string password = null;
            var url = "https://compute.procedural.build";
            var retry = false;

            if (!DA.GetData(0, ref username)) return DefaultTask();
            if (!DA.GetData(1, ref password)) return DefaultTask();
            DA.GetData(2, ref url);
            DA.GetData(3, ref retry);

            return System.Threading.Tasks.Task.Run(
                () => DoAuth(url, username, password), CancelToken);
        }

        public override void SetOutputData(IGH_DataAccess DA, LoginResult result)
        {
            if (result.Errors != null)
            {
                var errorMessage = result.Errors.Message.Contains("(401) Unauthorized")
                    ? "Could not login with the provided credentials. Try again."
                    : result.Errors.Message;

                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, errorMessage);
                return;
            }

            DA.SetData(0, result.Value.ToJson());
        }

        private LoginResult DoAuth(string url, string username, string password)
        {
            var client = new ComputeClient(url);
            try
            {
                return new LoginResult
                {
                    Value = new Inputs
                    {
                        Auth = client.Auth(username, password),
                        Url = url
                    }
                };
            }
            catch (Exception e)
            {
                return new LoginResult
                {
                    Errors = e
                };
            };
        }
    }
}