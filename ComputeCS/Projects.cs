using System;
using System.Collections.Generic;
using System.Linq;
using ComputeCS.types;
using Newtonsoft.Json;

namespace ComputeCS
{
    public class Projects
    {
        public ComputeClient client = null;
        
        public Projects(ComputeClient _client) {
            client = _client;
            client.http.endPoint = "/api/project/";
            client.http.httpMethod = httpVerb.GET;
        }

        public Project GetOrCreate(string name, int? number, bool create = false)
        {
            /* Try to get a project from its Name/Number. If it does not exist then 
            (optionally) create it.
            */

            // Check that at least a name or number is provided
            if (name == null && number == null)
            {
                throw new ArgumentException("Please provide a project name or number at minimum");
            }

            // Do the Get or Create
            try {
                return GetByNameNumber(name, number);
            } catch (ArgumentNullException) {
                if (create) {
                    return Create(name, number);
                }
            }

            // Return null possible if no Project found and not created.
            return null;
        }

        public Project GetByNameNumber(string name = null, int? number = null) 
        {
            var projects = List(name, number);
            if (projects.Count > 1)
            {
                throw new ArgumentException(
                    @"Found more than one project. Please provide both a project 
                    number and a name to identify an unique project"
                );
            } else if (projects.Count == 0) {
                throw new ArgumentNullException(
                    "No project found."
                );
            }
            return projects.First();
        }

        public List<Project> List(string name = null, int? number = null) 
        {
            /* Get a list of all Projects that this user can access 
            Optional query parameters may be provided to filter against name or number
            */
            return client.Request<List<Project>>(
                "/api/project/",
                new Dictionary<string, object>() 
                {
                    {"name", name},
                    {"number", number}
                }
            );
        }

        public Project Create(string name = null, int? number = null) {
            /* Create a new project with provided name and number 
            */
            return client.Request<Project>(
                "/api/project/", null,
                httpVerb.POST,
                new Dictionary<string, object>()
                {
                    {"name", name},
                    {"number", number}
                }
            );
        }
    }
}