using System;
using System.Collections.Generic;
using System.Linq;
using ComputeCS.types;
using Newtonsoft.Json;

namespace ComputeCS
{
    public class Projects
    {
        public ComputeClient Client = null;
        
        public Project GetOrCreate(string projectName, int? projectNumber, bool create = false)
        {
            // Check to see if tokens is still valid. If not get new access token
            var token = this.Client.GetAccessToken();
            var httpClient = new RESTClient
            {
                endPoint = this.Client.url + "/api/project/" + QueryParams(projectName, projectNumber),
                httpMethod = httpVerb.GET,
                token = token
            };
            var response = httpClient.makeRequest();
            var projects = JsonConvert.DeserializeObject<List<Project>>(response);
            if (projects.Count > 1)
            {
                throw new ArgumentException(
                    "Found more than one project. Please provide both a project number and a name to identify an unique project");
            }

            if (projects.Count == 0)
            {
                if (!create)
                {
                    throw new ArgumentException(
                        "Did not find any project with the provided name and/or number. If you want to create a project project set create to true");
                }
                else
                {
                    httpClient.endPoint = this.Client.url + "/api/project/";
                    httpClient.httpMethod = httpVerb.POST;
                    httpClient.payload = new Dictionary<string, object>()
                    {
                        {"name", projectName},
                        {"number", Convert.ToString(projectNumber)}
                    };
                    var createResponse = httpClient.makeRequest();
                    return JsonConvert.DeserializeObject<Project>(response);
                }
            }
            else
            {
                return projects.First();
            }
        }

        private string QueryParams(string name, int? number)
        {
            if (name == null && number == null)
            {
                throw new ArgumentException("Please provide a project name or number");
            }

            if (name != null && number != null)
            {
                return $"?name={name}&number={Convert.ToString(number)}";
            }

            if (number != null)
            {
                return $"?number={Convert.ToString(number)}";
            }
            else
            {
                return $"?name={name}";
            }
        }
    }
}