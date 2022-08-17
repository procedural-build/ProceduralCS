using ComputeCS.Components;
using ComputeGH.Grasshopper.Utils;
using ComputeGH.Properties;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;

namespace ComputeGH.Radiation
{
    public class RadiationProbesResult
    {
        public string Value { get; set; }
        public Exception Errors { get; set; }
    }
    public class GHRadiationProbe : PB_TaskCapableComponent<RadiationProbesResult>
    {
        /// <summary>
        /// Initializes a new instance of the GHProbe class.
        /// </summary>
        public GHRadiationProbe()
            : base("Probe Radiation", "Probe Radiation",
                "Probe radiation case to get the results in the desired points",
                "Compute", "Radiation")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Input", "Input", "Input from previous Compute Component", GH_ParamAccess.item);
            pManager.AddMeshParameter("Mesh", "Mesh", "Input your analysis mesh here, if you wish to visualize your results in the browser",
                GH_ParamAccess.tree);
            pManager.AddPointParameter("Points", "Points", "Probe points where you want to get radiation values.",
                GH_ParamAccess.tree);
            pManager.AddVectorParameter("Normals", "Normals", "Normals to the probe points given above.",
                GH_ParamAccess.tree);
            pManager.AddTextParameter("Names", "Names",
                "Give names to each branch of in the points tree. The names can later be used to identify the points.",
                GH_ParamAccess.list);
            pManager.AddBooleanParameter("Create", "Create",
                "Whether to create a new probe task, if one doesn't exist. If the Probe task already exists, then this component will create a new task config, that will run after the previous config is finished.",
                GH_ParamAccess.item, false);

            pManager[1].Optional = true;
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

        public override Task<RadiationProbesResult> CreateTask(IGH_DataAccess DA)
        {
            string inputJson = null;
            var mesh = new GH_Structure<GH_Mesh>();
            var points = new GH_Structure<GH_Point>();
            var normals = new GH_Structure<GH_Vector>();
            var names = new List<string>();
            var create = false;

            if (!DA.GetData(0, ref inputJson)) return DefaultTask();
            if (!DA.GetDataTree(1, out mesh)) return DefaultTask();
            if (!DA.GetDataTree(2, out points)) return DefaultTask();
            if (!DA.GetDataTree(3, out normals)) return DefaultTask();
            if (!DA.GetDataList(4, names))
            {
                for (var i = 0; i < points.Branches.Count; i++)
                {
                    names.Add($"set{i}");
                }
            }
            DA.GetData(5, ref create);

            return Task.Run(() => DoProbe(inputJson, mesh, points, normals, names, create));
        }

        public override void SetOutputData(IGH_DataAccess DA, RadiationProbesResult result)
        {
            Message = "";

            if (result.Errors != null && result.Errors.Message.Contains("No object found"))
            {
                Message = "No Probe Task found.";
                return;
            }
            else if (result.Errors != null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, result.Errors.Message);
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
        protected override Bitmap Icon => Resources.IconMesh;

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("ca0e9bd3-46a9-4cc7-b379-07b43d27e46a");

        private RadiationProbesResult DoProbe(
            string inputJson,
            GH_Structure<GH_Mesh> mesh,
            GH_Structure<GH_Point> points,
            GH_Structure<GH_Vector> normals,
            List<string> names,
            bool create)
        {
            try
            {
                return new RadiationProbesResult
                {
                    Value = Probe.RadiationProbes(
                        inputJson,
                        Geometry.ConvertPointsToList(points),
                        Geometry.ConvertPointsToList(normals),
                        names,
                        Export.MeshToObj(mesh, names),
                        create
                    )
                };
            }
            catch (Exception e)
            {
                return new RadiationProbesResult
                {
                    Errors = e
                };
            }
        }
    }
}