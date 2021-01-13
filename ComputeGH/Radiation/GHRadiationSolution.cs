using System;
using System.Collections.Generic;
using System.Drawing;
using ComputeCS.Components;
using ComputeGH.Grasshopper.Utils;
using ComputeGH.Properties;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Types;


namespace ComputeGH.Radiation
{
    public class GHRadiationSolution : GH_Component
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
            pManager.AddMeshParameter("Objects", "Objects", "Mesh objects to include in simulation",
                GH_ParamAccess.list);
            pManager.AddIntegerParameter("CPUs", "CPUs",
                "Number of CPUs to run the simulation across. Valid choices are:\n" +
                "1, 2, 4, 8, 16, 18, 24, 36, 48, 64, 72, 96",
                GH_ParamAccess.item, 4);
            pManager.AddIntegerParameter("Method", "Method", "Select which Radiance Method to use.",
                GH_ParamAccess.item, 0);
            pManager.AddIntegerParameter("Case Type", "Case Type",
                "Available Options: Grid, Image",  
                GH_ParamAccess.item, 0);
            pManager.AddTextParameter("Materials", "Materials", 
                "This should be a list of materials generated with the Radiance Material components.", 
                GH_ParamAccess.list);
            pManager.AddTextParameter("Overrides", "Overrides",
                "Takes overrides in JSON format: \n" +
                "{\n\t\"setup\": [...],\n\t\"fields\": [...],\n\t\"presets\": [...],\n\t\"caseFiles\": [...]\n}",
                GH_ParamAccess.item);

            pManager[2].Optional = true;
            pManager[3].Optional = true;
            pManager[4].Optional = true;
            pManager[5].Optional = true;
            pManager[6].Optional = true;

            AddNamedValues(pManager[3] as Param_Integer, Methods);
            AddNamedValues(pManager[4] as Param_Integer, CaseTypes);
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
            var meshes = new List<IGH_GeometricGoo>();
            var cpus = 4;
            var method = 0;
            var caseType = 0;
            var materials = new List<string>();
            string overrides = null;


            if (!DA.GetData(0, ref inputJson)) return;
            if (!DA.GetDataList(1, meshes)) return;
            DA.GetData(2, ref cpus);
            DA.GetData(3, ref method);
            DA.GetData(4, ref caseType);
            if (!DA.GetDataList(5, materials)) return;
            DA.GetData(6, ref overrides);

            var outputs = RadiationSolution.Setup(
                inputJson,
                Geometry.GetMeshIds(meshes),
                ComponentUtils.ValidateCPUs(cpus),
                Methods[method],
                CaseTypes[caseType],
                materials,
                overrides
            );
            
            DA.SetData(0, outputs);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override Bitmap Icon => Resources.IconSolver;

        private static readonly List<string> Methods = new List<string>
        {
            "DaylightFactor",
            "3Phase",
            "5Phase",
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