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
        private string input_json_string;

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
            input_json_string = new SerializeOutput {
                Auth = tokens,
                Url = "https://compute.procedural.build"
            }.ToJson();

            // Input parameters (these will be input into the component)
            project_name = "Test Project";
            project_number = 1;
            task_name = "Test Task";
            create = false;

        }

        [Test]
        public void GetOrCreateProjectAndTask()
        {
            Dictionary<string, string> input_dict = ComputeCS.Utils.DeserializeJsonString(input_json_string);
            // Unpack to an AuthToken instances
            AuthTokens tokens = input_dict["auth"];

            // Get a list of Projects for this user
            var project = new GenericViewSet<Project>(
                tokens, 
                "/api/project/"
            ).GetOrCreate(
                new Dictionary<string, object> {
                    {"name", project_name},
                    {"number", project_number}
                }, 
                null,
                create
            );

            // We won't get here unless the last getting of Project succeeded
            // Create the task if the Project succeded
            var task = new GenericViewSet<Task>(
                tokens, 
                $"/api/project/{project.UID}/task/"
            ).GetOrCreate(
                new Dictionary<string, object> {
                    {"name", task_name}
                }, 
                new Dictionary<string, object> {
                    {"config", new Dictionary<string, string> {
                        {"case_dir", "foam"},
                        {"task_type", "parent"}         // This is optional - task types of "parent" will not execute jobs
                    }}
                }, 
                create
            );

            // We could have a function here that makes life easier to
            // merge the outputs with the provided inputs
            var ouput_string = new SerializeOutput {
                Auth = tokens,
                Url = "https://compute.procedural.build",
                Task = task,
                Project = project,
            };

            Assert.IsNotNull(project, "Got null project");
            Assert.IsNotNull(task, "Got null task");
        }
    }
}