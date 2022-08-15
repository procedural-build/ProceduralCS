//using System;
//using System.Diagnostics;
//using System.Drawing;
//using ComputeCS.utils.Cache;
//using ComputeCS.utils.Queue;
//using ComputeGH.Grasshopper.Utils;
//using ComputeGH.Properties;
//using Grasshopper.Kernel;

//namespace ComputeGH.Utils
//{
//    public class GHDownloadEPW : PB_Component
//    {
//        /// <summary>
//        /// Initializes a new instance of the GHDownloadEPW class.
//        /// </summary>
//        public GHDownloadEPW()
//            : base("Download EPW", "Download EPW",
//                "Download an EPW file from EnergyPlus' website. This component will open your web browser directly to EnergyPlus' website so you can download an EPW file.",
//                "Compute", "Utils")
//        {
//        }

//        /// <summary>
//        /// Registers all the input parameters for this component.
//        /// </summary>
//        protected override void RegisterInputParams(GH_InputParamManager pManager)
//        {
//            pManager.AddBooleanParameter("Open Browser", "Open Browser",
//                "Connect a button to lauch your webbrowser onto the EnergyPlus website", GH_ParamAccess.item);
//        }

//        /// <summary>
//        /// Registers all the output parameters for this component.
//        /// </summary>
//        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
//        {
//        }

//        /// <summary>
//        /// This is the method that actually does the work.
//        /// </summary>
//        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
//        protected override void SolveInstance(IGH_DataAccess DA)
//        {
//            var launch = false;

//            if (!DA.GetData(0, ref launch)) return;
//            DA.DisableGapLogic();
//            const string url = "https://energyplus.net/weather";
//            if (launch)
//            {
//                QueueManager.addToQueue(url, () =>
//                {
//                    try
//                    {
//                        Process.Start(url);
//                    }
//                    catch (Exception e)
//                    {
//                        StringCache.setCache(InstanceGuid.ToString(), e.Message);
//                    }
//                });
//            }

//            var errors = StringCache.getCache(InstanceGuid.ToString());
//            if (errors != null)
//            {
//                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, errors);
//            }
//        }

//        /// <summary>
//        /// Provides an Icon for the component.
//        /// </summary>
//        protected override Bitmap Icon => Resources.IconFolder;

//        /// <summary>
//        /// Gets the unique ID for this component. Do not change this ID after release.
//        /// </summary>
//        public override Guid ComponentGuid => new Guid("fdb4b28d-727a-44b5-9676-2e3a447c4610");
//    }
//}