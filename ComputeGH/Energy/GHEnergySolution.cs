using System;
using System.Collections.Generic;
using System.Drawing;
using ComputeCS.Components;
using ComputeGH.Grasshopper.Utils;
using ComputeGH.Properties;
using Grasshopper.Kernel;

namespace ComputeGH.Energy
{
    public class GHEnergySolution : PB_Component
    {
        public GHEnergySolution() : base("EnergyPlus Solution", "Energy Solution",
            "Define and apply an EnergyPlus construction from materials", "Compute", "Energy")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Input", "Input", "Input from previous Compute Component",
                GH_ParamAccess.item);
            pManager.AddTextParameter("EPW File", "EPW File",
                "Path to where the EPW file is located. Only used for ThreePhase and Solar Radiation",
                GH_ParamAccess.item);
            pManager.AddTextParameter("Buildings", "Buildings",
                "This should be a list of buildings generated with the EnergyPlus Building components.",
                GH_ParamAccess.list);
            pManager.AddTextParameter("Zones", "Zones",
                "This should be a list of zones generated with the EnergyPlus Zone components.",
                GH_ParamAccess.list);
            pManager.AddTextParameter("Loads", "Loads",
                "This should be a list of loads generated with the EnergyPlus Load components.",
                GH_ParamAccess.list);
            pManager.AddTextParameter("Constructions", "Constructions",
                "This should be a list of constructions generated with the EnergyPlus Construction components.",
                GH_ParamAccess.list);
            pManager.AddTextParameter("Materials", "Materials",
                "This should be a list of materials generated with the EnergyPlus Material components.",
                GH_ParamAccess.list);
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
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Output", "Output", "Output", GH_ParamAccess.item);
        }

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override Bitmap Icon => Resources.IconBoundaryCondition;

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid => new Guid("b9f6bcfa-a002-4849-8001-b3bd882c095d");

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string inputJson = null;
            var epwFile = "";
            var buildings = new List<string>();
            var zones = new List<string>();
            var loads = new List<string>();
            var constructions = new List<string>();
            var materials = new List<string>();

            string overrides = null;


            if (!DA.GetData(0, ref inputJson)) return;
            if (inputJson == "error") return;
            DA.GetData(1, ref epwFile);
            DA.GetDataList(2, buildings);
            DA.GetDataList(3, zones);
            DA.GetDataList(4, loads);
            DA.GetDataList(5, constructions);
            DA.GetDataList(6, materials);
            DA.GetData(7, ref overrides);

            try
            {
                var outputs = EnergySolution.Setup(
                    inputJson,
                    buildings,
                    zones,
                    loads,
                    constructions,
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
    }
}