using System;
using System.Drawing;
using System.Linq;
using Grasshopper.Kernel;

namespace ComputeGH
{
    public class ComputeGHInfo : GH_AssemblyInfo
    {
        public override string Name
        {
            get
            {
                return "ProceduralCS";
            }
        }

        public override string Version
        {
            //get { return NextVersion(); }
            get { return "2020.9.3"; }
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

        private static string GetCurrentVersion()
        {
            return "0.0.0";
        }
        private static string GetNextBuildVersion()
        {
            var currentVersion = GetCurrentVersion().Split('.').Select(x => Convert.ToInt32(x)).ToList();
            var date = new DateTime().ToString("yyyy.m").Split('.').Select(x => Convert.ToInt32(x)).ToList();
            if (date[0] > currentVersion[0] || date[1] > currentVersion[1])
            {
                return "0";
            }
            return (currentVersion[2] + 1).ToString();
        }

        private static string NextVersion()
        {
            var buildNumber = GetNextBuildVersion();
            var date = new DateTime().ToString("yyyy.m");
            return $"{date}.${buildNumber}";
        }
    }
}
