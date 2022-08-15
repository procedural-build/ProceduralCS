using ComputeGH.Grasshopper.Utils;
using ComputeGH.Properties;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace ComputeCS.Grasshopper
{
    public class SetNames : PB_Component
    {
        public SetNames() : base("Set Names", "Set Names", "Set the Compute Name of Objects", "Compute", "Geometry")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Meshes", "Meshes", "Meshes", GH_ParamAccess.list);
            pManager.AddTextParameter("Names", "Names", "Names", GH_ParamAccess.list);

            pManager[1].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddMeshParameter("Meshes", "Meshes", "Meshes with naming applied.", GH_ParamAccess.list);
            pManager.AddTextParameter("Names", "Names", "List of names applied.", GH_ParamAccess.list);
        }

        protected override Bitmap Icon => Resources.IconSetName;

        public override Guid ComponentGuid => new Guid("42064b04-7f26-4a93-b90e-e6111c075b51");

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var ghObjs = new List<IGH_GeometricGoo>();
            var names = new List<string>();

            if (!DA.GetDataList(0, ghObjs))
            {
                return;
            }

            DA.GetDataList(1, names);

            // If the number of names is less than the number of objects then pad it out
            if ((names.Count > 0) & (ghObjs.Count > names.Count))
            {
                var shortFall = ghObjs.Count - names.Count;
                var lastName = names.Last();
                var padLen = ((int)Math.Log10(ghObjs.Count)) + 1;
                if (padLen < 3)
                {
                    padLen = 3;
                }

                names[names.Count - 1] = lastName + "." + 0.ToString().PadLeft(padLen);
                for (var i = 0; i < shortFall; i++)
                {
                    names.Add(lastName + "." + (i + 1).ToString().PadLeft(padLen));
                }
            }

            var ghNames = new List<string>();
            for (var i = 0; i < ghObjs.Count(); i++)
            {
                var name = names.Count() >= ghObjs.Count() ? names[i] : "";

                if (!ghObjs[i].IsValid)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"Mesh {ghObjs[i]} is not a valid Mesh. {ghObjs[i]} is element {i} in the input list");
                }

                name = Geometry.fixName(name);
                Geometry.setUserString(ghObjs[i], "ComputeName", name);
                ghNames.Add(name);
            }

            DA.SetDataList(0, ghObjs);
            DA.SetDataList(1, ghNames);
        }
    }
}