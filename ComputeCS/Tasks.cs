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
        public ComputeClient Client = null;
        public Project Project = null;
        
        public Task GetOrCreate(string taskName, bool create = false)
        {
            var token = this.Client.GetAccessToken();
            var httpClient = new RESTClient
            {
                endPoint = this.Client.url + $"/api/project/{Project.UID}/task/" + QueryParams(taskName),
                httpMethod = httpVerb.GET,
                token = token
            };
            var response = httpClient.makeRequest();
            var tasks = JsonConvert.DeserializeObject<List<Task>>(response);
            if (tasks.Count > 1)
            {
                throw new ArgumentException(
                    "Found more than one task. Please check your inputs to identify a unique task ");
            }

            if (tasks.Count == 0)
            {
                if (!create)
                {
                    throw new ArgumentException(
                        "Did not find any task with the provided name. Please set create to true to create a new task");
                }
                else
                {
                    httpClient.endPoint = this.Client.url + $"/api/project/{Project.UID}/task/";
                    httpClient.httpMethod = httpVerb.POST;
                    httpClient.payload = new Dictionary<string, object>()
                    {
                        {"name", taskName},
                        {"config", new Dictionary<string, string>()
                        {
                            {"case_dir", "foam"},
                            {"task_type", "parent"}
                        }}
                    };
                    var createResponse = httpClient.makeRequest();
                    return JsonConvert.DeserializeObject<Task>(response);
                }
            }
            else
            {
                return tasks.First();
            }
        }

        private string QueryParams(string name)
        {
            if (name == null)
            {
                throw new ArgumentException("Please provide a project name or number");
            }
            else
            {
                return $"?name={name}&parent=null";
            }
        }
    }
}