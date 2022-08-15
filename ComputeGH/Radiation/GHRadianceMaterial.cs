//using System;
//using System.Collections.Generic;
//using System.Drawing;
//using System.Linq;
//using ComputeCS.types;
//using ComputeGH.Grasshopper.Utils;
//using ComputeGH.Properties;
//using Grasshopper.Kernel;
//using Grasshopper.Kernel.Parameters;
//using Newtonsoft.Json;

//namespace ComputeGH.Radiation
//{
//    public class GHRadianceMaterial : PB_Component
//    {
//        public GHRadianceMaterial() : base("Radiance Material", "Material",
//            "Define and apply a Radiance material to the construction", "Compute", "Radiation")
//        {
//        }

//        /// <summary>
//        /// Registers all the input parameters for this component.
//        /// </summary>
//        protected override void RegisterInputParams(GH_InputParamManager pManager)
//        {
//            pManager.AddTextParameter("Names", "Names",
//                "The names of the meshes to apply this material to.\nIf the name ends with a * it applies to all the geometry that have a name that starts with the name given here.",
//                GH_ParamAccess.list);
//            pManager.AddIntegerParameter("Preset", "Preset",
//                "Material preset.\n" +
//                "All presets can be changed by applying overrides.\n" +
//                "If you pick window as preset, you can only apply the bsdf override.\n" +
//                "If you want to see the preset values, visit https://compute.procedural.build/docs/daylight/materials",
//                GH_ParamAccess.item,
//                0);
//            pManager.AddTextParameter("Overrides", "Overrides",
//                "Optional overrides to apply to the presets.\n" +
//                "If you have chosen window, you should add a override with:\n" +
//                "{\n" +
//                "    \"bsdf_path\": \"BSDF FILE\"\n" +
//                "}.\n" +
//                "If you want to add a window with variable shading given by a schedule then the following override should be applied:\n" +
//                "{\n" +
//                "    \"bsdf_path\": [\n" +
//                "         \"BSDF FILE 1\",\n" +
//                "         \"BSDF FILE 2\",\n" +
//                "    ]\n" +
//                "    \"schedules\": [\n" +
//                "         [true, false, false, ...],\n" +
//                "         [false, true, true, ...],\n" +
//                "    ]\n" +
//                "}.\n" +
//                "The BSDF file can either be given as a path to a local file or \"clear.xml\", which is a default BSDF file Compute provides.\n" +
//                "If you want to override a material, which is not a window preset, the following overrides can be given:\n" +
//                "{\n" +
//                "    \"type\": \"plastic\",\n" +
//                "    \"reals\": [0.2, 0.2, 0.2, 0, 0],\n" +
//                "\n}",
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
//            pManager.AddTextParameter("Material", "Material", "Radiance Material",
//                GH_ParamAccess.list);
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
//        public override Guid ComponentGuid => new Guid("3f70e696-1f64-4272-9ee8-48e635369435");

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

//            var output = new List<string>();
//            try
//            {
//                output = names.Select(name =>
//                    new RadianceMaterial
//                            {Name = name, Preset = preset, Overrides = new MaterialOverrides().FromJson(overrides)}
//                        .ToJson()).ToList();
//            }
//            catch (JsonSerializationException error)
//            {
//                AddRuntimeMessage(GH_RuntimeMessageLevel.Error,
//                    error.InnerException != null ? error.InnerException.Message : error.Message);
//            }

//            DA.SetDataList(0, output);
//        }

//        private static readonly List<string> Presets = new List<string>
//        {
//            "context",
//            "wall",
//            "window",
//            "ceiling",
//            "roof",
//            "floor",
//            "ground"
//        };
//    }
//}