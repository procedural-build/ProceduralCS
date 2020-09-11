using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Grasshopper;
using Rhino.Geometry;
using Newtonsoft.Json;
using ComputeCS.types;
using ComputeCS.utils.Cache;
using ComputeCS.utils.Queue;
using ComputeGH.Properties;

namespace ComputeCS.Grasshopper
{
    public delegate void ExpireSolutionDelegate(bool recompute);

    public class ComputeProjectTask : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the computeLogin class.
        /// </summary>
        public ComputeProjectTask()
            : base("Get or Create Project and Task", "Project and Task",
                "Get or Create a project and/or a parent Task on Procedural Compute",
                "Compute", "Utils")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Auth", "Auth", "Authentication from the Compute Login component",
                GH_ParamAccess.item);
            pManager.AddTextParameter("ProjectName", "ProjectName", "Project Name", GH_ParamAccess.item);
            pManager.AddIntegerParameter("ProjectNumber", "ProjectNumber", "Project  Number", GH_ParamAccess.item);
            pManager.AddTextParameter("TaskName", "TaskName", "Task Name", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Create", "Create",
                "Whether to create a new project/task, if they doesn't exist", GH_ParamAccess.item, false);

            pManager[2].Optional = true;
            pManager[4].Optional = true;
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
            string auth = null;
            string projectName = null;
            int? projectNumber = null;
            string taskName = null;
            bool create = false;

            if (!DA.GetData(0, ref auth)) return;
            if (!DA.GetData(1, ref projectName) || !DA.GetData(2, ref projectNumber)) return;
            if (!DA.GetData(3, ref taskName)) return;
            DA.GetData(4, ref create);

            // Get Cache to see if we already did this
            var cacheKey = projectName + taskName;
            var cachedValues = StringCache.getCache(cacheKey);
            DA.DisableGapLogic();

            if (cachedValues == null || create == true)
            {
                var queueName = "ProjectAndTask";

                // Get queue lock
                var queueLock = StringCache.getCache(queueName);
                if (queueLock != "true")
                {
                    StringCache.setCache(queueName, "true");
                    StringCache.setCache(cacheKey, null);
                    QueueManager.addToQueue(queueName, () =>
                    {
                        try
                        {
                            var results = Components.ProjectAndTask.GetOrCreate(
                                auth,
                                projectName,
                                (int) projectNumber,
                                taskName,
                                create
                            );
                            cachedValues = results;
                            StringCache.setCache(cacheKey, cachedValues);
                            StringCache.setCache(this.InstanceGuid.ToString(), "");
                            if (create)
                            {
                                StringCache.setCache(cacheKey + "create", "true");
                            }
                        }
                        catch (Exception e)
                        {
                            StringCache.AppendCache(this.InstanceGuid.ToString(), e.Message + "\n");
                            StringCache.setCache(cacheKey, "error");
                            StringCache.setCache(cacheKey + "create", "");
                        }

                        StringCache.setCache(queueName, "");
                        ExpireSolutionThreadSafe(true);
                    });
                }
            }

            // Read from Cache
            if (cachedValues != null)
            {
                var outputs = cachedValues;
                DA.SetData(0, outputs);
                Message = "";
                if (StringCache.getCache(cacheKey + "create") == "true")
                {
                    Message = "Task Created";
                }
            }

            // Handle Errors
            var errors = StringCache.getCache(this.InstanceGuid.ToString());
            if (!string.IsNullOrEmpty(errors))
            {
                if (errors.Contains("No object found"))
                {
                    errors = "Could not find the desired project. Click create to create a new project.";
                }

                throw new Exception(errors);
            }
        }

        private void ExpireSolutionThreadSafe(bool recompute = false)
        {
            var delegated = new ExpireSolutionDelegate(ExpireSolution);
            Rhino.RhinoApp.InvokeOnUiThread(delegated, recompute);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get {                 if (System.Environment.GetEnvironmentVariable("RIDER") == "true")
                {
                    return null;
                }
                return Resources.IconFolder; }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("74e6ee44-9879-4769-83bb-3f2cdeb8dd7a"); }
        }
    }
}