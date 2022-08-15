using ComputeGH.Types;
using Grasshopper.Kernel;
using System;

namespace ComputeGH.Params
{
    public class DownloadFileParam : GH_Param<PB_DownloadFile>
    {
        public override GH_Exposure Exposure => GH_Exposure.hidden;

        public DownloadFileParam()
          : base(
            new GH_InstanceDescription(
              "Download File", "File", "A File downloaded from Procedural Compute", "Compute", "Params"
            )
          )
        { }

        public override Guid ComponentGuid
        {
            get { return new Guid("{105AD250-67F0-4075-AF3C-69FD35CBEC09}"); }
        }
    }
}
