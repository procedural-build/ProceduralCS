﻿using System;
using System.Collections.Generic;
using ComputeCS.types;
using ComputeGH.Properties;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace ComputeGH.Grasshopper
{
    public class CFDsetSet : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public CFDsetSet() : base("CFD setSet", "setSet", "Description", "Compute", "CFD"){}

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Names", "Names", "Mesh names", GH_ParamAccess.list);
            pManager.AddBooleanParameter("Location", "Location", "This input takes a list of false and true. Inside, Surface, Outside", GH_ParamAccess.list, new List<bool>(){ true, true, false });
            pManager.AddPointParameter("Keep Point", "Keep Point", "Keep Point", GH_ParamAccess.item, new Point3d { X = 0, Y = 0, Z = 0});

            pManager[1].Optional = true;
            pManager[2].Optional = true;

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Output", "Output", "setSet Regions", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var names = new List<string>();
            var location = new List<bool>();
            var keepPoint = new Point3d();

            if (!DA.GetDataList(0, names)) return;
            DA.GetData(1, ref location);
            DA.GetData(2, ref keepPoint);

            var outputs = new List<string>();
            foreach (var name in names)
            {
                outputs.Add(new setSetRegion
                {
                    Name = name,
                    Locations = location,
                    KeepPoint = new List<double>() { keepPoint.X, keepPoint.Y, keepPoint.Z }
                }.ToJson());
            }
 

            DA.SetData(0, outputs);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                if (System.Environment.GetEnvironmentVariable("RIDER") == "true")
                {
                    return null;
                }
                return Resources.IconMesh;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("8f622be6-22e4-4607-b280-527abe200921"); }
        }
    }
}