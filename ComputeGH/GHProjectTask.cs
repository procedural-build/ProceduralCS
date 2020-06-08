using System;
using System.Collections.Generic;
using computeGH.core;
using Grasshopper.Kernel;
using Rhino.Geometry;
using Newtonsoft.Json;
using computeGH.types;


namespace ComputeCS.Grasshopper
{
    public class GHProjectTask : GH_ComponentC
    {
        /// <summary>
        /// Initializes a new instance of the computeLogin class.
        /// </summary>
        public computeLogin()
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
            pManager.AddTextParameter("Auth", "Auth", "Authentication from the Compute Login component", GH_ParamAccess.item);
            pManager.AddTextParameter("ProjectName", "ProjectName", "Project Name", GH_ParamAccess.item);
            pManager.AddTextParameter("ProjectNumber", "ProjectNumber", "Project  Number", GH_ParamAccess.item);
            pManager.AddTextParameter("TaskName", "TaskName", "Task Name", GH_ParamAccess.item);
            pManager.AddTextParameter("Create", "Create", "Whether to create a new project/task, if they doesn't exist", GH_ParamAccess.item);
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
            if (!DA.GetData(1, ref projectName) || !DA.GetData(2, ref projectNumber) ) return;
            if (!DA.GetData(3, ref taskName)) return;

            Dictionary<string, object> outputs = ComputeCS.Components.ProjectAndTask.GetOrCreate(
                auth,
                project_name,
                project_number,
                task_name,
                create
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
            get { return new Guid("898478bb-5b4f-4972-951a-d9e71ba0086b"); }
        }
    }
}