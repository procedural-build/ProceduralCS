using System;
using System.Collections.Generic;
using System.Drawing;
using ComputeCS.Components;
using ComputeGH.Properties;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using ComputeCS.GrasshopperUtils;

namespace ComputeCS.Grasshopper
{
    public class ComputeSolution : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the computeLogin class.
        /// </summary>
        public ComputeSolution()
            : base("Compute Solution", "CFD Solution",
                "Create the Solution Parameters for a CFD Case",
                "Compute", "CFD")
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
            pManager.AddIntegerParameter("Solver", "Solver", "Select which OpenFOAM solver to use.",
                GH_ParamAccess.item, 0);
            pManager.AddIntegerParameter("Case Type", "Case Type",
                "Available Options: SimpleCase, VirtualWindTunnel, MeshOnly\n" +
                "SimplyCase runs a standard CFD case with the geometry specified. This is often used for a single wind angle or in case of indoor studies.\n" +
                "VirtualWindTunnel rotates and runs the CFD the number of time you specifies it in the \"Number of Angles\" input. This is used for outdoor comfort studies.\n" +
                "MeshOnly only runs the meshing part. You can run this first. Check the mesh and then pick either SimpleCase or VirtualWindTunnel depending on your needs.",
                GH_ParamAccess.item, 0);
            pManager.AddTextParameter("Boundary Conditions", "Boundary Conditions", 
                "This should be a list of boundary conditions generated with the Compute Boundary Conditions components.", 
                GH_ParamAccess.list);
            pManager.AddIntegerParameter("Iterations", "Iterations",
                "Number of iterations to run.",
                GH_ParamAccess.item, 100);
            pManager.AddNumberParameter("Number of Angles", "Number of Angles",
                "Number of Angles. Only used for Virtual Wind Tunnel. Default is 16\n" +
                "If a single number is given then the component will interpret it as the number of angles that should be ran.\n" +
                "If a list of numbers is given then that is the angles that will be run.",
                GH_ParamAccess.list, 16);
            pManager.AddTextParameter("Overrides", "Overrides",
                "Takes overrides in JSON format: \n" +
                "{\n\t\"setup\": [...],\n\t\"fields\": [...],\n\t\"presets\": [...],\n\t\"caseFiles\": [...]\n}",
                GH_ParamAccess.item);

            pManager[2].Optional = true;
            pManager[3].Optional = true;
            pManager[5].Optional = true;
            pManager[6].Optional = true;
            pManager[7].Optional = true;

            AddNamedValues(pManager[2] as Param_Integer, _solvers);
            AddNamedValues(pManager[3] as Param_Integer, _caseTypes);
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

            var solver = 0;
            var caseType = 0;
            var boundaryConditions = new List<string>();

            var iterations = 100;

            var numberOfAngles = new List<double>();
            string overrides = null;


            if (!DA.GetData(0, ref inputJson)) return;
            if (!DA.GetData(1, ref cpus)) return;
            DA.GetData(2, ref solver);
            DA.GetData(3, ref caseType);
            if (!DA.GetDataList(4, boundaryConditions)) return;
            DA.GetData(5, ref iterations);
            DA.GetDataList(6, numberOfAngles);
            DA.GetData(7, ref overrides);
            var _iterations = $"{{'init': {iterations}}}";

            var outputs = CFDSolution.Setup(
                inputJson,
                ComponentUtils.ValidateCPUs(cpus),
                _solvers[solver],
                _caseTypes[caseType],
                boundaryConditions,
                _iterations,
                numberOfAngles,
                overrides
            );


            DA.SetData(0, outputs);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override Bitmap Icon => Resources.IconSolver;

        private static List<string> _solvers = new List<string>
        {
            "simpleFoam",
            "potentialFoam",
            "pisoFoam",
            "buoyantSimpleFoam",
            "buoyantPimpleFoam",
            "buoyantBoussinesqSimpleFoam",
            "buoyantBoussinesqPimpleFoam"
        };

        private static readonly List<string> _caseTypes = new List<string>
        {
            "SimpleCase", "VirtualWindTunnel", "MeshOnly"
        };
        
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("31b37cda-f5cc-4ffe-86b2-00bd457e4311");
    }
}