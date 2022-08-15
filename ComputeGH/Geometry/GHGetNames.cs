using ComputeGH.Grasshopper.Utils;
using ComputeGH.Properties;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace ComputeGH.Grasshopper
{
    public class GetNames : PB_Component
    {
        public GetNames() : base("Get Names", "Get Names", "Get the Compute Name of Meshes", "Compute", "Geometry")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Meshes", "Meshes", "Meshes to get name from", GH_ParamAccess.list);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Names", "Names", "Names of meshes", GH_ParamAccess.list);
        }

        protected override Bitmap Icon => Resources.IconGetName;

        public override Guid ComponentGuid => new Guid("5199183e-aa10-41f7-ba63-df1bb96d141c");

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var ghObjs = new List<IGH_GeometricGoo>();
            var ids = new List<string>();

            if (!DA.GetDataList(0, ghObjs))
            {
                return;
            }

            foreach (var ghObj in ghObjs)
            {
                var refId = "";
                if (ghObj.IsReferencedGeometry)
                {
                    refId = ghObj.ReferenceID.ToString();
                }

                ids.Add(Geometry.getOrSetUserString(ghObj, "ComputeName", Geometry.fixName(refId)));
            }

            DA.SetDataList(0, ids);
        }
    }
}