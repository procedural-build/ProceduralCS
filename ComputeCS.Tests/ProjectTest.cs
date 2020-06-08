using Newtonsoft.Json;
using NUnit.Framework;
using System;
using ComputeCS.types;
using ComputeCS;

namespace ComputeCS.UnitTests.Projects
{
    [TestFixture]
    public class Projects_projects
    {
        private UserSettings user = new UserSettings();
        private ComputeClient client;
        
        
        [SetUp]
        public void SetUp()
        {
            client = new ComputeClient(user.host);
        }

        [Test]
        public void Projects_List()
        {
            // Get the JWT access tokens as usual
            var tokens = client.Auth(user.username, user.password);
            Console.WriteLine($"Got access token: {tokens.Access}");

            // Instantiate the Project object
            var projects = new ComputeCS.Projects(client);

            // Get a list of Projects for this user
            var project_list = new GenericViewSet<Project>(tokens, "/api/project/").List();

            Console.Write($"Got projects: {JsonConvert.SerializeObject(project_list)}");

            Assert.IsNotEmpty(project_list, "Got empty project list");
        }
    }
}