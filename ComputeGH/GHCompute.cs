using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Rhino.Geometry;
using Newtonsoft.Json;
using ComputeCS.types;

namespace ComputeCS.Grasshopper
{
    public class ComputeCompute : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the ComputeCompute class.
        /// </summary>
        public ComputeCompute()
          : base("Compute Solution", "CFD Solution",
              "Create the Solution Parameters for a CFD Case",
              "Compute", "CFD")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Input", "Input", "Input from previous Compute Component", GH_ParamAccess.item);
            pManager.AddTextParameter("Geometry", "Geometry", "Case Geometry", GH_ParamAccess.list);
            pManager.AddTextParameter("Local Path", "Path", "Local Path to write the geometry to. Default is %Temp%", GH_ParamAccess.item);
            pManager.AddTextParameter("Compute", "Compute", "Run the case on Procedural Compute", GH_ParamAccess.item);

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
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
            List<Brep> geometry = null;
            string path = null;
            bool compute = false;



            if (!DA.GetData(0, ref inputJson)) return;
            if (!DA.GetData(1, ref geometry)) return;
            if (!DA.GetData(2, ref path)) return;
            if (!DA.GetData(3, ref compute)) return;

            ExportSTL(geometry, path);


            Dictionary<string, object> outputs = ComputeCS.Components.Compute.Create(
                inputJson,
                path
            );

            DA.SetData(0, inputJson);

        }

        protected void ExportSTL(List<Brep> breps, string path)
        {
            List<Guid> guidList = new List<Guid>();
            Rhino.DocObjects.Tables.ObjectTable ot = Rhino.RhinoDoc.ActiveDoc.Objects;
            for (int i = 0; i < breps.Count; i++)
            {
                if (breps[i] == null || !breps[i].IsValid)
                {
                    this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "No object to bake, or brep not valid, aborted.");
                    return;
                }
                System.Guid guid = ot.AddBrep(breps[i]);
                guidList.Add(guid);
            }
            int nSelected = ot.Select(guidList);
            if (nSelected != guidList.Count)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Not all objects could be selected, aborted.");
                return;
            }

            string cmd = "-_Export " + path + ".stl" + " _Enter";
            Rhino.RhinoApp.RunScript(cmd, false);

            ot.Delete(guidList, true);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return null;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("898478bb-5b4f-4972-951a-d9e71ba0086b"); }
        }
    }
}