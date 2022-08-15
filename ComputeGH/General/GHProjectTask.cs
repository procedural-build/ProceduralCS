using ComputeCS.Components;
using ComputeGH.Grasshopper.Utils;
using ComputeGH.Properties;
using Grasshopper.Kernel;
using System;
using System.Activities;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace ComputeCS.Grasshopper
{
    public class ProjectTaskResult
    {
        public string Value { get; set; }
        public Exception Errors { get; set; }
    }
    public class ComputeProjectTask : PB_TaskCapableComponent<ProjectTaskResult>
    {
        /// <summary>
        /// Initializes a new instance of the computeLogin class.
        /// </summary>
        public ComputeProjectTask()
            : base("Get or Create Project and Task", "Project and Task",
                "Get or Create a project and/or a parent Task on Procedural Compute",
                "Compute", "General")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Auth", "Auth", "Authentication from the Compute Login component",
                GH_ParamAccess.item);
            pManager.AddTextParameter("ProjectName", "ProjectName", "Project Name", GH_ParamAccess.item);
            pManager.AddIntegerParameter("ProjectNumber", "ProjectNumber", "Project  Number", GH_ParamAccess.item);
            pManager.AddTextParameter("TaskName", "TaskName", "Task Name", GH_ParamAccess.item);
            pManager.AddTextParameter("Overrides", "Overrides",
                "Takes overrides in JSON format: \n" +
                "{\n" +
                "    \"company\": companyId,\n" +
                "    \"copy_from\": [\n" +
                "        {\n" +
                "            \"task\": taskId,\n" +
                "            \"files\": [fileName1, fileName2]\n" +
                "        }\n" +
                "    ],\n" +
                "    \"nest_with\": parentNameToNestWith,\n" +
                "    \"comment\": commentHTMLFormattedText\n" +
                "}",
                GH_ParamAccess.item);
            pManager.AddBooleanParameter("Create", "Create",
                "Whether to create a new project/task, if they doesn't exist", GH_ParamAccess.item, false);

            pManager[2].Optional = true;
            pManager[4].Optional = true;
            pManager[5].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Output", "Output", "Output", GH_ParamAccess.item);
        }

        private void ValidateName(string name)
        {
            var illegalCharacters = new List<string> { "?", "&", "/", "%", "#", "!", "+" };
            if (illegalCharacters.Any(name.Contains))
            {
                throw new ValidationException(
                    $"{name} contains illegal characters. " +
                    $"A name cannot include any on the following characters: {string.Join(", ", illegalCharacters)}"
                );
            }
        }

        public override Task<ProjectTaskResult> CreateTask(IGH_DataAccess DA)
        {
            string auth = null;
            string projectName = null;
            int? projectNumber = null;
            string taskName = null;
            string overrides = null;
            var create = false;

            if (!DA.GetData(0, ref auth)) return DefaultTask();
            if (!DA.GetData(1, ref projectName)) return DefaultTask();
            DA.GetData(2, ref projectNumber);
            if (!DA.GetData(3, ref taskName)) return DefaultTask();
            DA.GetData(4, ref overrides);
            DA.GetData(5, ref create);

            ValidateName(taskName);
            ValidateName(projectName);

            return Task.Run(() =>
                GetProjectTaskResult(auth, projectName, projectNumber, taskName, overrides, create));
        }
        public override void SetOutputData(IGH_DataAccess DA, ProjectTaskResult result)
        {
            if (result.Errors != null)
            {
                var errorMessage = result.Errors.Message;
                var messageLevel = GH_RuntimeMessageLevel.Error;
                if (result.Errors.Message.Contains("No object found"))
                {
                    errorMessage = "Could not find the desired project. Click create to create a new project.";
                    messageLevel = GH_RuntimeMessageLevel.Warning;
                }
                AddRuntimeMessage(messageLevel, errorMessage);
                return;
            }

            var create = false;
            DA.GetData(5, ref create);
            Message = create ? "Task Created" : "";

            DA.SetData(0, result.Value);
        }


        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override Bitmap Icon => Resources.IconFolder;

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("74e6ee44-9879-4769-83bb-3f2cdeb8dd7a");

        private static ProjectTaskResult GetProjectTaskResult(string auth, string projectName, int? projectNumber, string taskName, string overrides, bool create)
        {
            try
            {
                return new ProjectTaskResult
                {
                    Value = ProjectAndTask.GetOrCreate(
                        auth,
                        projectName,
                        projectNumber,
                        taskName,
                        overrides,
                        create
                    )
                };
            }
            catch (Exception e)
            {
                return new ProjectTaskResult { Errors = e };
            }
        }

    }
}