using System;
using System.IO;
using System.IO.Compression;
using ComputeCS.Components;
using Microsoft.VisualBasic;
using NUnit.Framework;

namespace ComputeCS.Tests.ComponentTests
{
    [TestFixture]
    public class ProbeResultTest
    {
        private string folder = "";

        [SetUp]
        public void SetUp()
        {
            var tmpDir = Path.GetTempPath();
            folder = Path.Combine(tmpDir, "probeResult");
            if (Directory.Exists(folder))
            {
                Directory.Delete(folder, true);
            }
            else
            {
                FileSystem.MkDir(folder);
            }

            ZipFile.ExtractToDirectory(
                Path.Combine(Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.ToString(),
                    "TestData/internalCloud.zip"), folder);
        }

        [Test]
        public void TestProbeResult()
        {
            var outputs = ProbeResult.ReadProbeResults(Path.Combine(folder, "internalCloud"));
            Assert.IsTrue(outputs.ContainsKey("p"));
            Assert.IsTrue(outputs.ContainsKey("U"));
        }
    }
}