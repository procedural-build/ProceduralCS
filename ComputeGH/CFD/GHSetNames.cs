using System;
using System.Collections.Generic;
using System.Linq;
using ComputeCS.Grasshopper.Utils;
using ComputeGH.Properties;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;


namespace ComputeCS.Grasshopper
{
    public class SetNames : GH_Component
    {
        public SetNames() : base("setNames", "setNames", "Set the Compute Name of Objects", "Compute", "Utils")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("objs", "objs", "objs", GH_ParamAccess.list);
            pManager.AddTextParameter("names", "names", "names", GH_ParamAccess.list);

            pManager[1].Optional = true;
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("objs", "objs", "objs", GH_ParamAccess.list);
        }

        protected override System.Drawing.Bitmap Icon
        {
            get { return Resources.IconSetName; }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("42064b04-7f26-4a93-b90e-e6111c075b51"); }
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<IGH_GeometricGoo> ghObjs = new List<IGH_GeometricGoo>();
            List<string> names = new List<string>();
            string name;

            if (!DA.GetDataList(0, ghObjs))
            {
                return;
            }

            DA.GetDataList(1, names);

            // If the number of names is less than the number of objects then pad it out
            if ((names.Count > 0) & (ghObjs.Count > names.Count))
            {
                int shortFall = ghObjs.Count - names.Count;
                string lastName = names.Last();
                int padLen = ((int) Math.Log10(ghObjs.Count)) + 1;
                if (padLen < 3)
                {
                    padLen = 3;
                }

                names[names.Count - 1] = lastName + "." + 0.ToString().PadLeft(padLen);
                for (int i = 0; i < shortFall; i++)
                {
                    names.Add(lastName + "." + (i + 1).ToString().PadLeft(padLen));
                }
            }

            for (int i = 0; i < ghObjs.Count(); i++)
            {
                if (names.Count() >= ghObjs.Count())
                {
                    name = names[i];
                }
                else
                {
                    name = "";
                }

                Geometry.setUserString(ghObjs[i], "ComputeName", Geometry.fixName(name));
            }

            DA.SetDataList(0, ghObjs);
        }
    }
}