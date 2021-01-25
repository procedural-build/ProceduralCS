using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using ComputeCS.types;
using ComputeGH.Properties;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Newtonsoft.Json;

namespace ComputeGH.Radiation
{
    public class GHRadianceMaterial: GH_Component
    {
        public GHRadianceMaterial() : base("Radiance Material", "Material",
            "Define and apply a Radiance material to the construction", "Compute", "Radiation")
        {
        }
        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Names", "Names", "The names of the meshes to apply this material to.", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Preset", "Preset", "Presets of the material", GH_ParamAccess.item,
                0);
            pManager.AddTextParameter("Overrides", "Overrides", "Optional overrides to apply to the presets",
                GH_ParamAccess.item, "");

            pManager[1].Optional = true;
            pManager[2].Optional = true;

            AddNamedValues(pManager[1] as Param_Integer, Presets);
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
            pManager.AddTextParameter("Material", "Material", "Radiance Material",
                GH_ParamAccess.list);
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
        public override Guid ComponentGuid => new Guid("3f70e696-1f64-4272-9ee8-48e635369435");

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var names = new List<string>();
            var overrides = "";
            var preset_ = 0;

            DA.GetDataList(0, names);
            if (!DA.GetData(1, ref preset_))
            {
                return;
            }

            var preset = Presets[preset_];

            if (!DA.GetData(2, ref overrides))
            {
                return;
            }

            var output = names.Select(name =>
                new RadianceMaterial {Name = name, Preset = preset, Overrides = JsonConvert.DeserializeObject<Dictionary<string, object>>(overrides)}.ToJson()).ToList();

            DA.SetDataList(0, output);
        }

        private static readonly List<string> Presets = new List<string>
        {
            "context",
            "wall",
            "window",
            "ceiling",
            "roof",
            "floor",
            "ground"
        };
    }
}