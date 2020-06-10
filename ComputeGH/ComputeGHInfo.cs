using System;
using System.Drawing;
using Grasshopper.Kernel;

namespace ComputeGH
{
    public class ComputeGHInfo : GH_AssemblyInfo
    {
        public override string Name
        {
            get
            {
                return "Compute";
            }
        }
        public override Bitmap Icon
        {
            get
            {
                //Return a 24x24 pixel bitmap to represent this GHA library.
                return null;
            }
        }
        public override string Description
        {
            get
            {
                //Return a short string describing the purpose of this GHA library.
                return "Grasshopper Client for Procedural Compute";
            }
        }
        public override Guid Id
        {
            get
            {
                return new Guid("7b46d4ec-a9f3-428a-a838-6c5de9bb8d96");
            }
        }

        public override string AuthorName
        {
            get
            {
                //Return a string identifying you or your company.
                return "Procedural Build";
            }
        }
        public override string AuthorContact
        {
            get
            {
                //Return a string representing your preferred contact details.
                return "info@procedural.build";
            }
        }
    }
}
