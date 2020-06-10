using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using ComputeCS.types;
using ComputeCS;

namespace ComputeCS.UnitTests.GetOrCreateProjectTask
{
    [TestFixture]
    public class ProjectTask_projectTask
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
            core_input = new SerializeOutput {
                Auth = tokens,
                Url = "https://compute.procedural.build"
            }.ToJson();
            Dictionary<string, string> input_dict = ComputeCS.Utils.DeserializeJsonString(core_input);

            // Input parameters (these will be input into the component)
            project_name = "Test Project";
            project_number = 1;
            task_name = "Test Task";
            create = false;

        }

        [Test]
        public void GetOrCreateProjectAndTask_Test()
        {
            // Here is the componet/function - this will be wrapped in Grasshopper/Dynamo boilerplate
            Dictionary<string, object> outputs = new ComputeCS.Components.GetOrCreateProjectTask().run(
                core_input,
                project_name,
                project_number,
                task_name,
                create
            );
            
            // Components should always output json string "out" - this is the core dictoinary
            Assert.IsTrue(outputs.ContainsKey("out"));

            // We can deserialise the output here and do more assertions
        }
    }
}