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
        
        private List<string> boundaryConditions;

        private string iterations;

        private int numberOfAngles;
        
        private string overrides;
        private string caseType = "virtualwindtunnel";

        [SetUp]
        public void SetUp()
        {


            // Input parameters (these will be input into the component)
            cpus = new List<int>{2, 2, 1};
            solver = "simpleFoam";
            boundaryConditions = new List<string>{@"{
                    'names': ['Cube'],
                    'preset': 'fixedVelocity',
                    'overrides': {
                        'U': {
                            'value': 'uniform (0 0 5)'
                            }
                    }
                }"
            };
            
            iterations = "{{\"init\", 1000},{\"run\", 800}}";
            numberOfAngles = 16;
            overrides = @"{
                    'presets': null,
                    'fields':, null,
                    'caseFiles': null
                }";
        }

        [Test]
        public void Testsolution()
        {
            var outputs = ComputeCS.Components.CFDSolution.Setup(
                "",
                cpus,
                solver,
                caseType,
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