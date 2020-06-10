using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using ComputeCS.types;
using ComputeCS;

namespace ComputeCS.Tests.ComponentTests
{
    [TestFixture]
    public class TestCFDSolution
    {
        /* Component core dictionary input - this is always a JSON string
        containing core parameters from upstream components that may be 
        needed in downstream components.  
        Downstream components will get or default or error against these 
        parameters.
        */
        /* Component inputs*/

        private List<int> cpus;

        private string solver;
        
        private List<Dictionary<string, object>> boundaryConditions;

        private Dictionary<string, int> iterations;

        private int numberOfAngles;
        
        private Dictionary<string, object> overrides;


        [SetUp]
        public void SetUp()
        {


            // Input parameters (these will be input into the component)
            cpus = new List<int>{2, 2, 1};
            solver = "simpleFoam";
            boundaryConditions = new List<Dictionary<string, object>>
            {
                new Dictionary<string, object>
                {
                    {"names", new List<string>{"Cube"}},
                    {"preset", "fixedVelocity"},
                    {"overrides", new Dictionary<string, object>
                    {
                        {"U", new Dictionary<string, string>
                        {
                            {"value", "uniform (0 0 5)"}
                        }}
                    }}
                }
            };
            iterations = new Dictionary<string, int>
            {
                {"init", 1000},
                {"run", 800}
            };
            numberOfAngles = 16;
            overrides = new Dictionary<string, object>
                {
                    {"presets", null},
                    {"fields", null},
                    {"caseFiles", null}
                };
            
        }

        [Test]
        public void Testsolution()
        {
            var outputs = ComputeCS.Components.CFDSolution.Setup(
                "",
                cpus,
                solver,
                boundaryConditions,
                iterations,
                numberOfAngles,
                overrides
            );

            Console.WriteLine($"Got Output: {outputs["out"]}");
            // Components should always output json string "out" - this is the core dictoinary
            Assert.IsTrue(outputs.ContainsKey("out"));
        }
    }
}