using System;
using System.Collections.Generic;
using ComputeCS.Grasshopper.Utils;
using ComputeGH.Properties;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;

namespace ComputeGH.Grasshopper
{
    public class GetNames : GH_Component
    {
        public GetNames() : base("getNames", "getNames", "Get the Compute Name of Objects", "Compute", "Utils")
        {
        }
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("objs", "objs", "objs", GH_ParamAccess.list);
        }
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("names", "names", "names", GH_ParamAccess.list);
        }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //return Resources.IconGetName;
                return null;
            }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("5199183e-aa10-41f7-ba63-df1bb96d141c"); }
        }
        protected override void SolveInstance(IGH_DataAccess DA)
        {

            List<IGH_GeometricGoo> ghObjs = new List<IGH_GeometricGoo>();
            List<string> ids = new List<string>();

            if (!DA.GetDataList(0, ghObjs)) { return; }

            foreach (IGH_GeometricGoo ghObj in ghObjs)
            {
                string refID = "";
                if (ghObj.IsReferencedGeometry) { refID = ghObj.ReferenceID.ToString(); }
                ids.Add(Geometry.getOrSetUserString(ghObj, "ComputeName", Geometry.fixName(refID)));
            }

            DA.SetDataList(0, ids);
        }
    }
}