using Newtonsoft.Json;
using NUnit.Framework;
using System;
using ComputeCS.types;
using ComputeCS;

namespace ComputeCS.Tests
{
    [TestFixture]
    public class TestProjects
    {
        private UserSettings user = new UserSettings();
        private ComputeClient client;
        
        
        [SetUp]
        public void SetUp()
        {
            client = new ComputeClient(user.host);
        }

        [Test]
        public void TestProjectList()
        {
            // Get the JWT access tokens as usual
            var tokens = client.Auth(user.username, user.password);
            Console.WriteLine($"Got access token: {tokens.Access}");

            // Instantiate the Project object
            //var projects = new ComputeCS.Projects(client);

            // Get a list of Projects for this user
            var projects = new GenericViewSet<Project>(tokens, user.host, "/api/project/").List();

            Console.Write($"Got projects: {JsonConvert.SerializeObject(projects)}");

            Assert.IsNotEmpty(projects, "Got empty project list");
        }
    }
}