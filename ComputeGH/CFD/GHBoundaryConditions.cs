using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Grasshopper.Kernel;
using ComputeGH.Properties;

namespace ComputeCS.Grasshopper
{
    public class cfdBoundaryCondition : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public cfdBoundaryCondition() : base("CFD Boundary Condition", "CFD BC",
            "Defines a CFD Boundary Condition", "Compute", "CFD")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Names", "Names", "The names of the boundary condition", GH_ParamAccess.list);
            pManager.AddTextParameter("Preset", "Preset", "Presets of the boundary condition", GH_ParamAccess.item,
                "wall");
            pManager.AddTextParameter("Overrides", "Overrides", "Optional overrides to apply to the presets",
                GH_ParamAccess.item, "");

            pManager[1].Optional = true;
            pManager[2].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Boundary Condition", "BC", "Boundary Condition",
                GH_ParamAccess.item);
        }

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                // You can add image files to your project resources and access them like this:
                return Resources.IconBoundaryCondition;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("9fee205d-133f-4fc7-8d46-367765308909"); }
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var names = new List<string>();
            var overrides = "";
            var preset = "";

            DA.GetDataList(0, names);
            if (!DA.GetData(1, ref preset))
            {
                return;
            }

            if (!DA.GetData(2, ref overrides))
            {
                return;
            }

            Dictionary<string, object> boundaryConditions = new Dictionary<string, object>();
            Dictionary<string, string> overrides_ = null;
            try
            {
                overrides_ = JsonConvert.DeserializeObject<Dictionary<string, string>>(overrides);
            }
            catch (JsonReaderException e)
            {
            }

            foreach (string name in names)
            {
                Dictionary<string, object> thisBC = new Dictionary<string, object>();

                if (preset.Length > 0)
                {
                    thisBC.Add("preset", preset);
                }

                if (overrides_ != null)
                {
                    thisBC.Add("overrides", overrides_);
                }
                else if (overrides.Length > 0)
                {
                    thisBC.Add("overrides", overrides);
                }

                boundaryConditions.Add(name, thisBC);
            }

            DA.SetData(0, JsonConvert.SerializeObject(boundaryConditions, Formatting.Indented));
        }
    }
}