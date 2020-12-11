using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using ComputeGH.Properties;
using Grasshopper.Kernel;

namespace ComputeGH
{
    public class ComputeGHInfo : GH_AssemblyInfo
    {
        public override string Name
        {
            get { return "ProceduralCS"; }
        }

        public override string Version => NextVersion();

        //get { return Resources.Version; }
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
            get { return new Guid("7b46d4ec-a9f3-428a-a838-6c5de9bb8d96"); }
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
            var YakExe = "C:\\Program Files\\Rhino 7 WIP\\System\\yak.exe";
            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("YAK_EXE")))
            {
                YakExe = Path.Combine(
                    Environment.GetEnvironmentVariable("GITHUB_WORKSPACE"), 
                    Environment.GetEnvironmentVariable("YAK_EXE")
                    );
            }
            if (!File.Exists(YakExe))
            {
                Console.WriteLine("We rely on YAK from Rhino7 for this distribution.");
                return "0.0.0";
            }

            var process = new Process
            {
                StartInfo =
                {
                    CreateNoWindow = true,
                    FileName = YakExe,
                    Arguments = "search proceduralcs",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                }
            };

            process.Start();
            var output = process.StandardOutput.ReadToEnd();
            var err = process.StandardError.ReadToEnd();
            process.WaitForExit();

            if (!string.IsNullOrEmpty(err))
            {
                Console.WriteLine($"Caught the following error while searching for proceduralcs with YAK: {err}");
                return "0.0.0";
            }

            return output.Split('(')[1].Split(')')[0];
        }

        private static string GetNextBuildVersion()
        {
            var currentVersion = GetCurrentVersion().Split('.').Select(x => Convert.ToInt32(x)).ToList();
            var date = DateTime.Now;
            if (date.Year > currentVersion[0] || date.Month > currentVersion[1])
            {
                return "0";
            }

            return (currentVersion[2] + 1).ToString();
        }

        private static string NextVersion()
        {
            var buildNumber = GetNextBuildVersion();
            var date = DateTime.Now.ToString("yyyy.M");
            return $"{date}.{buildNumber}";
        }
    }
}