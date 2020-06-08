using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using ComputeCS.types;
using ComputeCS.Tests;
using CFDSolution = ComputeCS.Components.CFDSolution;

namespace ComputeCS.Tests.ComponentTests
{
    [TestFixture]
    public class TestComputeCase
    {
        private UserSettings user = new UserSettings();
        private ComputeClient client;

        /* Component core dictionary input - this is always a JSON string
        containing core parameters from upstream components that may be 
        needed in downstream components.  
        Downstream components will get or default or error against these 
        parameters.
        */
        private string core_input;

        /* Component inputs*/
        private bool compute;
        private Project project;
        private Task task;
        private Mesh mesh;
        private types.CFDSolution solution;
        
        
        [SetUp]
        public void SetUp()
        {
            client = new ComputeClient(user.host);
        
            // Get the JWT access tokens as usual
            var tokens = client.Auth(user.username, user.password);
            Console.WriteLine($"Got access token: {tokens.Access}");

            // Core input string (from previous/upstream component(s))
            core_input = SerializeIO.OutputToJson(new Inputs {
                Auth = tokens,
                Url = user.host,
                Project = project,
                Task = task,
                Mesh = mesh,
                CFDSolution = solution
            });

            // Input parameters (these will be input into the component)
            compute = false;

        }

        [Test]
        public void TestComputeRun()
        {
            // Here is the component/function - this will be wrapped in Grasshopper/Dynamo boilerplate
            Dictionary<string, object> outputs = ComputeCS.Components.Compute.Create(
                core_input
            );
            
            Console.WriteLine($"Got Output: {outputs["out"]}");
            // Components should always output json string "out" - this is the core dictoinary
            Assert.IsTrue(outputs.ContainsKey("out"));

            // We can deserialise the output here and do more assertions
        }
    }
}