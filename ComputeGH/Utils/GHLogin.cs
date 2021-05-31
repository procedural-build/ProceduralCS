using System;
using System.Drawing;
using System.Linq;
using System.Threading;
using ComputeCS.types;
using ComputeCS.utils.Cache;
using ComputeCS.utils.Queue;
using ComputeGH.Grasshopper.Utils;
using ComputeGH.Properties;
using Grasshopper.Kernel;
using Rhino;

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
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Username", "Username", "Username", GH_ParamAccess.item);
            pManager.AddTextParameter("Password", "Password", "Password", GH_ParamAccess.item);
            pManager.AddTextParameter("Compute URL", "URL", "URL for Compute", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Retry", "Retry", "Force retry to connect to Compute", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Blocking", "Blocking", "Make Compute components blocking throughout the canvas.", GH_ParamAccess.item, false);

            pManager[3].Optional = true;
            pManager[4].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Auth", "Auth", "Auth", GH_ParamAccess.item);
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
            var retry = false;
            var blocking = false;

            if (!DA.GetData(0, ref username)) return;
            if (!DA.GetData(1, ref password)) return;
            if (!DA.GetData(2, ref url)) return;
            DA.GetData(3, ref retry);
            DA.GetData(4, ref blocking);

            var client = new ComputeClient(url);

            if (retry)
            {
                StringCache.ClearCache();
            }
            if (blocking)
            {
                StringCache.setCache("compute.blocking", "true");
            }

            //Async Execution
            var cacheKey = username + password + url;
            var cachedValues = StringCache.getCache(cacheKey);
            DA.DisableGapLogic();
            if (cachedValues == null)
            {
                const string queueName = "login";
                // Get queue lock
                var queueLock = StringCache.getCache(queueName);
                if (queueLock != "true")
                {
                    StringCache.setCache(queueName, "true");
                    QueueManager.addToQueue(queueName, () =>
                    {
                        try
                        {
                            var results = client.Auth(username, password);

                            if (results.ErrorMessages != null)
                            {
                                StringCache.setCache(cacheKey, "error");
                                throw new Exception(results.ErrorMessages.First());
                            }
                            if (results.ErrorMessages == null)
                            {
                                StringCache.ClearCache();
                                cachedValues = results.ToJson();
                                StringCache.setCache(cacheKey, cachedValues);
                            }
                        }
                        catch (Exception e)
                        {
                            StringCache.setCache(InstanceGuid.ToString(), e.Message);
                        }

                        if (StringCache.getCache("compute.blocking") == "true") return;
                        ExpireSolutionThreadSafe(true);
                        Thread.Sleep(2000);
                        StringCache.setCache(queueName, "");
                    });
                }
            }

            var instanceId = InstanceGuid.ToString();
            string errors;
            (cachedValues, errors) = ComponentUtils.BlockingComponent(cacheKey, instanceId);


            // Read from Cache
            if (errors != null)
            {
                if (errors.Contains("(401) Unauthorized"))
                {
                    errors = "Could not login with the provided credentials. Try again.";
                }

                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, errors);
            }

            var tokens = new AuthTokens();
            if (cachedValues != null)
            {
                tokens = tokens.FromJson(cachedValues);
                var output = new Inputs
                {
                    Auth = tokens,
                    Url = url
                };
                DA.SetData(0, output.ToJson());
            }
            
        }

        private void ExpireSolutionThreadSafe(bool recompute = false)
        {
            var delegated = new ExpireSolutionDelegate(ExpireSolution);
            RhinoApp.InvokeOnUiThread(delegated, recompute);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override Bitmap Icon => Resources.IconLicense;

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("3bcb2f6e-7c64-41b2-8086-6b1ddd6e80ee");
    }
}