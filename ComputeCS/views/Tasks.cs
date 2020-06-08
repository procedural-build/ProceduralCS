using System;
using System.Collections.Generic;
using System.Linq;
using ComputeCS;
using ComputeCS.types;
using Newtonsoft.Json;


namespace ComputeCS
{
    public class Tasks
    {
        public ComputeClient client = null;

        public Project project = null;
        
        public Tasks(ComputeClient _client, Project _project) {
            client = _client;
            project = _project;
            client.http.endPoint = $"/api/project/{project.UID}/task/";
        }

        public Task GetOrCreate(string name, bool create = false)
        {
            /* Try to get a project from its Name/Number. If it does not exist then 
            (optionally) create it.
            */

            // Check that at least a name or number is provided
            if (name == null)
            {
                throw new ArgumentException("Please provide a task name at minimum");
            }

            // Do the Get or Create
            try {
                return GetByName(name);
            } catch (ArgumentNullException) {
                if (create) {
                    return Create(name);
                }
            }

            // Return null possible if no Project found and not created.
            return null;
        }

        public Task GetByName(string name = null) 
        {
            var tasks = List(name);
            if (tasks.Count > 1)
            {
                throw new ArgumentException(
                    @"Found more than one task. Please provide a unique 
                    task name to identify a particular task."
                );
            } else if (tasks.Count == 0) {
                throw new ArgumentNullException(
                    "No task found."
                );
            }
            return tasks.First();
        }

        public List<Task> List(string name = null) 
        {
            /* 
            Get a list of all Projects that this user can access 
            Optional query parameters may be provided to filter against name or number
            */
            return client.Request<List<Task>>(
                $"/api/project/{project.UID}/task/",
                new Dictionary<string, object>() 
                {
                    {"name", name}
                }
            );
        }

        public Task Create(string name = null) {
            /* Create a new project with provided name and number 
            */
            return client.Request<Task>(
                $"/api/project/{project.UID}/task/", null,
                httpVerb.POST,
                new Dictionary<string, object>()
                {
                    {"name", name},
                    {"config", new Dictionary<string, string>()
                    {
                        {"case_dir", "foam"},
                        {"task_type", "parent"}
                    }}
                }
            );
        }
    }
}