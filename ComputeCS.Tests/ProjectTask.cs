using System;
using computeCS;
using NUnit.Framework;
using computeCS.types;

namespace ComputeCS.Tests
{
    [TestFixture]
    public class ProjectTaskTest
    {
        
        [TestCase]
        public void TestGetProject()
        {
            // SETUP
            const string username = "admin";
            const string password = "0358d7bb";
            const string url = "http://localhost:8001";
            
            var initClient = new ComputeClient
            {
                url = url
            };

            var tokens = initClient.Auth(username, password);
            
            // TEST
            bool create = false;
            string projectName = "Project Test";
            var projectNumber = 1;
            
            var client = new ComputeClient
            {
                Tokens = tokens,
                url = url
            };
            var projects = new Projects
            {
                Client = client
            };
            var project = projects.GetOrCreate(projectName, projectNumber, create);
            
            // Assert
            Assert.That(project, Is.Not.Null);
        }
        
        [TestCase]
        public void TestExistingProjectAndTask()
        {
            // SETUP
            const string username = "admin";
            const string password = "0358d7bb";
            const string url = "http://localhost:8001";
            
            var initClient = new ComputeClient
            {
                url = url
            };

            var tokens = initClient.Auth(username, password);
            
            // TEST
            bool create = false;
            var projectName = "Project Test";
            var projectNumber = 1;
            var taskName = "TEST";

            
            var client = new ComputeClient
            {
                Tokens = tokens,
                url = url
            };
            var projects = new Projects
            {
                Client = client
            };
            var project = projects.GetOrCreate(projectName, projectNumber, create);

            var tasks = new Tasks
            {
                Client = client,
                Project = project
            };
            var task = tasks.GetOrCreate(taskName, create);
            
            // Assert
            Assert.That(project, Is.Not.Null);
            Assert.That(task, Is.Not.Null);
        }
        
        [TestCase]
        public void TestExistingProjectAndCreateTask()
        {

        }
        
        [TestCase]
        public void TestCreateProjectAndTask()
        {
            /*ComputeClient client = new ComputeClient();
            
            client.tokens(auth);
            Project project = client.projects.GetOrCreate(projectName, projectNumber, create);
            Task task = client.tasks.getOrCreate(taskName, project, create);
            SerializeOutput output = new SerializeOutput
            {
                Auth = auth,
                Task = task,
                Project = project,
                Url = url
            };
            string strOutput = output.ToJson();
            Assert.That(strOutput, Does.Contain("task"));
            Assert.That(strOutput, Does.Contain("project"));
*/
        }
        
        [TestCase]
        public void TestMultipleProjectsReturned()
        {

        }
    }
}