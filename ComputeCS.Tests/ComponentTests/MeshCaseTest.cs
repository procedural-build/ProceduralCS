using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using ComputeCS.types;
using ComputeCS;

namespace ComputeCS.Tests.ComponentTests
{
    [TestFixture]
    public class TestMesh
    {

        /* Component core dictionary input - this is always a JSON string
        containing core parameters from upstream components that may be 
        needed in downstream components.  
        Downstream components will get or default or error against these 
        parameters.
        */

        /* Component inputs*/

        // Inputs for BlockMesh
        private string type;

        private double cellSize;

        private List<List<int>> boundingBox;

        private Dictionary<string, string> params_;

        // Inputs for SnappyHexMesh
        private Dictionary<string, object> defaultSurfaces;
        private List<Dictionary<string, object>> surfaces;


        [SetUp]
        public void SetUp()
        {

            // Input parameters (these will be input into the component)
            type = "simpleBox";
            cellSize = 0.25;
            boundingBox = new List<List<int>>
            {
                new List<int>{-1, -1, 0}, new List<int>{1, 1, 2}   
            };
            params_ = new Dictionary<string, string>
            {
                {"square", "true"},
                {"z0", "true"}
            };
            defaultSurfaces = new Dictionary<string, object>
            {
                {"name", "Plane"},
                {"level", new List<int>{3, 3}}
            };
            surfaces = new List<Dictionary<string, object>>
            {
                new Dictionary<string, object>
                {
                    {"name", "Plane"},
                    {"level", new List<int>{3, 3}},
                },
                new Dictionary<string, object>
                {
                    {"name", "Cube.005"},
                    {"level", new List<int>{4, 4}},
                    {"layers", 3}
                },
            };
        }

        [Test]
        public void TestMeshCase()
        {
            var outputs = ComputeCS.Components.Mesh.Setup(
                "",
                type,
                cellSize,
                boundingBox,
                params_,
                defaultSurfaces,
                surfaces,
                null
            );
            
            Console.WriteLine($"Got Output: {outputs["out"]}");
            // Components should always output json string "out" - this is the core dictoinary
            Assert.IsTrue(outputs.ContainsKey("out"));
        }
    }
}