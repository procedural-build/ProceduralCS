using ComputeCS.Components;
using ComputeGH.Grasshopper.Utils;
using ComputeGH.Properties;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using System;
using System.Collections.Generic;
using System.Drawing;


namespace ComputeGH.Radiation
{
    public class GHRadiationSolution : PB_Component
    {
        public GHRadiationSolution()
            : base("Radiation Solution", "Radiation Solution",
                "Create the Solution Parameters for a Radiation Case",
                "Compute", "Radiation")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Input", "Input", "Input from previous Compute Component",
                GH_ParamAccess.item);
            pManager.AddIntegerParameter("CPUs", "CPUs",
                "Number of CPUs to run the simulation across. Valid choices are:\n" +
                "1, 2, 4, 8, 16, 18, 24, 36, 48, 64, 72, 96",
                GH_ParamAccess.item, 4);
            pManager.AddIntegerParameter("Method", "Method", "Select which Radiance Method to use.",
                GH_ParamAccess.item, 0);
            //pManager.AddIntegerParameter("Case Type", "Case Type", "Available Options: Grid, Image", GH_ParamAccess.item, 0);
            pManager.AddTextParameter("Materials", "Materials",
                "This should be a list of materials generated with the Radiance Material components.",
                GH_ParamAccess.list);
            pManager.AddTextParameter("EPW File", "EPW File",
                "Path to where the EPW file is located. Only used for ThreePhase and Solar Radiation",
                GH_ParamAccess.item);
            pManager.AddTextParameter("Overrides", "Overrides",
                "Accepts the following overrides with defaults in JSON format: \n" +
                "{\n" +
                "    \"ambient_bounces\": 4,\n" +
                "    \"ambient_divisions\": 5000,\n" +
                "    \"limit_ray_weight\": 0.0002,\n" +
                "    \"samples\": 1000,\n" +
                "    \"reinhart_divisions\": 1,\n" +
                "    \"keep_tmp\": false\n" +
                "    \"suppress_warnings\": false\n" +
                "    \"sun_only\": false\n" +
                "    \"keep\": {\n" +
                "        \"all\": false,\n" +
                "        \"view\": false,\n" +
                "        \"daylight\": false,\n" +
                "        \"sky\": false\n" +
                "    }\n" +
                "    \"suppress_warnings\": false,\n" +
                "    \"octree_resolution\": 16384\n" +
                "}",
                GH_ParamAccess.item);

            pManager[1].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;
            pManager[4].Optional = true;
            pManager[5].Optional = true;


            AddNamedValues(pManager[2] as Param_Integer, Methods);
            //AddNamedValues(pManager[3] as Param_Integer, CaseTypes);
        }

        private static void AddNamedValues(Param_Integer param, List<string> values)
        {
            var index = 0;
            foreach (var value in values)
            {
                param.AddNamedValue(value, index);
                index++;
            }
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
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
            var cpus = 4;
            var method = 0;
            var caseType = 0;
            var materials = new List<string>();
            var epwFile = "";
            string overrides = null;


            if (!DA.GetData(0, ref inputJson)) return;
            if (inputJson == "error") return;
            DA.GetData(1, ref cpus);
            DA.GetData(2, ref method);
            //DA.GetData(3, ref caseType);
            if (!DA.GetDataList(3, materials)) return;
            if (!DA.GetData(4, ref epwFile) && (method <= 1)) return;
            DA.GetData(5, ref overrides);

            try
            {
                var outputs = RadiationSolution.Setup(
                    inputJson,
                    ComponentUtils.ValidateCPUs(cpus),
                    Methods[method],
                    CaseTypes[caseType].ToLower(),
                    materials,
                    epwFile,
                    overrides
                );
                DA.SetData(0, outputs);
            }
            catch (Exception error)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, error.Message);
            }

        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override Bitmap Icon => Resources.IconSolver;

        private static readonly List<string> Methods = new List<string>
        {
            "three_phase",
            "solar_radiation",
            "daylight_factor",
            "sky_view_factor",
            "mean_radiant_temperature"
        };

        private static readonly List<string> CaseTypes = new List<string>
        {
            "Grid", "Image"
        };

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("b3b6da6f-35c2-44d6-933e-621b666963d9");
    }
}