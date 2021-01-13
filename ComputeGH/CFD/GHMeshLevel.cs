﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using ComputeGH.Grasshopper.Utils;
using ComputeGH.Properties;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;

namespace ComputeCS.Grasshopper
{
    public class CFDMeshLevel : GH_Component
    {
        public CFDMeshLevel() : base("CFD Mesh Level", "Mesh Level", "Defines a CFD Mesh Level", "Compute", "Mesh")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Objects", "Objects", "Objects", GH_ParamAccess.list);
            pManager.AddNumberParameter("Min Level", "Min Level", "Minimum Level", GH_ParamAccess.item);
            pManager.AddNumberParameter("Max Level", "Max Level", "Maximum Level", GH_ParamAccess.item, 0);

            pManager[2].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Level", "Level", "Level", GH_ParamAccess.list);
        }

        protected override Bitmap Icon => Resources.IconMeshLevel;

        public override Guid ComponentGuid => new Guid("57bbca4d-9a11-4797-b90a-a5e735b9f803");

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var ghObjs = new List<IGH_GeometricGoo>();
            double minLevel = 2;
            double maxLevel = 2;

            if (!DA.GetDataList(0, ghObjs))
            {
                return;
            }

            if (!DA.GetData(1, ref minLevel))
            {
                return;
            }

            if (!DA.GetData(2, ref maxLevel))
            {
                return;
            }

            if (maxLevel < minLevel)
            {
                maxLevel = minLevel;
            }

            // Get a list of object references in the Rhino model
            for (var i = 0; i < ghObjs.Count(); i++)
            {
                Geometry.setUserString(ghObjs[i], "ComputeMeshMinLevel", minLevel.ToString());
                Geometry.setUserString(ghObjs[i], "ComputeMeshMaxLevel", maxLevel.ToString());
            }

            DA.SetDataList(0, ghObjs);
        }
    }
}