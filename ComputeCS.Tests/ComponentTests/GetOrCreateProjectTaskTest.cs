using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using ComputeCS.types;
using ComputeCS.Tests;

namespace ComputeCS.Tests.ComponentTests
{
    [TestFixture]
    public class TestProjectTask
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
        private string project_name;
        private int project_number;
        private string task_name;
        private bool create;
        
        
        [SetUp]
        public void SetUp()
        {
            client = new ComputeClient(user.host);
        
            // Get the JWT access tokens as usual
            var tokens = client.Auth(user.username, user.password);
            Console.WriteLine($"Got access token: {tokens.Access}");

            // Core input string (from previous/upstream component(s))
            core_input = new Inputs {
                Auth = tokens,
                Url = user.host
            }.ToJson();

            // Input parameters (these will be input into the component)
            project_name = "Project Test";
            project_number = 1;
            task_name = "Test Task";
            create = false;

        }

        [Test]
        public void TestGetOrCreateProjectAndTask()
        {
            // Here is the component/function - this will be wrapped in Grasshopper/Dynamo boilerplate
            var outputs = ComputeCS.Components.ProjectAndTask.GetOrCreate(
                core_input,
                project_name,
                project_number,
                task_name,
                create
            );
            
            Console.WriteLine($"Got Output: {outputs}");
            // Components should always output json string "out" - this is the core dictoinary
            Assert.IsTrue(outputs != null);

            // We can deserialise the output here and do more assertions
        }
    }
}