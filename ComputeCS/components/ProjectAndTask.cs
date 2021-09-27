using System;
using System.Collections.Generic;
using System.Linq;
using ComputeCS.types;
using Newtonsoft.Json;

namespace ComputeCS.Components
{
    public static class ProjectAndTask
    {
        public static string GetOrCreate(
            string inputJson,
            string projectName,
            int? projectNumber,
            string taskName,
            string overrides,
            bool create
        )
        {
            var inputData = new Inputs().FromJson(inputJson);
            // Unpack to an AuthToken instances
            var tokens = inputData.Auth;
            var overrideDict = overrides != null
                ? JsonConvert.DeserializeObject<Dictionary<string, object>>(overrides)
                : new Dictionary<string, object>();

            var queryParams = new Dictionary<string, object>
            {
                {"name", projectName},
            };
            if (projectNumber != null)
            {
                queryParams.Add("number", projectNumber);
            }

            if (overrideDict.ContainsKey("company"))
            {
                queryParams.Add("company", overrideDict["company"]);
            }

            // Get a list of Projects for this user
            var project = new GenericViewSet<Project>(
                tokens,
                inputData.Url,
                "/api/project/"
            ).GetOrCreate(
                queryParams,
                null,
                create
            );
            // We won't get here unless the last getting of Project succeeded
            // Create the task if the Project succeeded
            if (project.ErrorMessages != null)
            {
                throw new Exception(project.ErrorMessages.First());
            }

            inputData.Project = project;

            var parentTask = Tasks.Utils.CreateParent(tokens, inputData.Url, $"/api/project/{project.UID}/task/", taskName,
                overrideDict, create);
            inputData.Task = parentTask;

            if (overrideDict.ContainsKey("nest_with"))
            {
                var nestedParent = Tasks.Utils.CreateParent(tokens, inputData.Url, $"/api/project/{project.UID}/task/",
                    (string) overrideDict["nest_with"],
                    new Dictionary<string, object> {{"parent", parentTask.UID}}, create);
                inputData.Task = nestedParent;
            }

            return inputData.ToJson();
        }

        public static string GetTasks(
            string inputJson,
            string projectName,
            int? projectNumber,
            bool excludeChildren = true
        )
        {
            var inputData = new Inputs().FromJson(inputJson);
            var tokens = inputData.Auth;

            var queryParams = new Dictionary<string, object>
            {
                {"name", projectName},
            };
            if (projectNumber != null)
            {
                queryParams.Add("number", projectNumber);
            }

            // Get a list of Projects for this user
            var project = new GenericViewSet<Project>(
                tokens,
                inputData.Url,
                "/api/project/"
            ).GetOrCreate(
                queryParams,
                null,
                false
            );
            // We won't get here unless the last getting of Project succeeded
            // Create the task if the Project succeeded
            if (project.ErrorMessages != null)
            {
                throw new Exception(project.ErrorMessages.First());
            }

            var tasksQueryParams = new Dictionary<string, object>();
            if (excludeChildren)
            {
                tasksQueryParams.Add("parent", "null");
            }

            var tasks = new GenericViewSet<Task>(
                tokens,
                inputData.Url,
                $"/api/project/{project.UID}/task/"
            ).List(tasksQueryParams);

            return string.Join(";", tasks.Select(task => task.ToJson()));
        }
    }
}