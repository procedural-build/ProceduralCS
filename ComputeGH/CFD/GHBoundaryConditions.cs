//using System;
//using System.Collections.Generic;
//using System.Drawing;
//using ComputeGH.Grasshopper.Utils;
//using ComputeGH.Properties;
//using Grasshopper.Kernel;
//using Grasshopper.Kernel.Parameters;
//using Newtonsoft.Json;

//namespace ComputeCS.Grasshopper
//{
//    public class cfdBoundaryCondition : PB_Component
//    {
//        /// <summary>
//        /// Each implementation of GH_Component must provide a public 
//        /// constructor without any arguments.
//        /// Category represents the Tab in which the component will appear, 
//        /// Subcategory the panel. If you use non-existing tab or panel names, 
//        /// new tabs/panels will automatically be created.
//        /// </summary>
//        public cfdBoundaryCondition() : base("CFD Boundary Condition", "CFD BC",
//            "Defines a CFD Boundary Condition", "Compute", "CFD")
//        {
//        }

//        /// <summary>
//        /// Registers all the input parameters for this component.
//        /// </summary>
//        protected override void RegisterInputParams(GH_InputParamManager pManager)
//        {
//            pManager.AddTextParameter("Names", "Names", "The names of the boundary condition", GH_ParamAccess.list);
//            pManager.AddIntegerParameter("Preset", "Preset", "Presets of the boundary condition", GH_ParamAccess.item,
//                0);
//            pManager.AddTextParameter("Overrides", "Overrides", "Optional overrides to apply to the presets",
//                GH_ParamAccess.item, "");

//            pManager[1].Optional = true;
//            pManager[2].Optional = true;

//            AddNamedValues(pManager[1] as Param_Integer, Presets);
//        }

//        private static void AddNamedValues(Param_Integer param, List<string> values)
//        {
//            var index = 0;
//            foreach (var value in values)
//            {
//                param.AddNamedValue(value, index);
//                index++;
//            }
//        }

//        /// <summary>
//        /// Registers all the output parameters for this component.
//        /// </summary>
//        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
//        {
//            pManager.AddTextParameter("Boundary Condition", "BC", "Boundary Condition",
//                GH_ParamAccess.item);
//        }

//        /// <summary>
//        /// Provides an Icon for every component that will be visible in the User Interface.
//        /// Icons need to be 24x24 pixels.
//        /// </summary>
//        protected override Bitmap Icon => Resources.IconBoundaryCondition;

//        /// <summary>
//        /// Each component must have a unique Guid to identify it. 
//        /// It is vital this Guid doesn't change otherwise old ghx files 
//        /// that use the old ID will partially fail during loading.
//        /// </summary>
//        public override Guid ComponentGuid => new Guid("9fee205d-133f-4fc7-8d46-367765308909");

//        /// <summary>
//        /// This is the method that actually does the work.
//        /// </summary>
//        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
//        /// to store data in output parameters.</param>
//        protected override void SolveInstance(IGH_DataAccess DA)
//        {
//            var names = new List<string>();
//            var overrides = "";
//            var preset_ = 0;

//            DA.GetDataList(0, names);
//            if (!DA.GetData(1, ref preset_))
//            {
//                return;
//            }

//            var preset = Presets[preset_];

//            if (!DA.GetData(2, ref overrides))
//            {
//                return;
//            }

//            var boundaryConditions = new Dictionary<string, object>();
//            Dictionary<string, string> overrides_ = null;
//            try
//            {
//                overrides_ = JsonConvert.DeserializeObject<Dictionary<string, string>>(overrides);
//            }
//            catch (JsonReaderException)
//            {
//            }

//            foreach (var name in names)
//            {
//                var boundaryCondition = new Dictionary<string, object>();

//                if (preset.Length > 0)
//                {
//                    boundaryCondition.Add("preset", preset);
//                }

//                if (overrides_ != null)
//                {
//                    boundaryCondition.Add("overrides", overrides_);
//                }
//                else if (overrides.Length > 0)
//                {
//                    boundaryCondition.Add("overrides", overrides);
//                }

//                boundaryConditions.Add(name, boundaryCondition);
//            }

//            DA.SetData(0, JsonConvert.SerializeObject(boundaryConditions, Formatting.Indented));
//        }

//        private static readonly List<string> Presets = new List<string>
//        {
//            "wall",
//            "wallSlip",
//            "fixedVelocity",
//            "fixedPressure",
//            "fixedPressureOutOnly",
//            "atmBoundaryLayer",
//        };
//    }
//}