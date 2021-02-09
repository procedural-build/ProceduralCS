using System;
using System.Collections.Generic;
using System.Linq;
using ComputeCS.types;

namespace ComputeCS.Components
{
    public static class ProjectAndTask
    {
        public static string GetOrCreate(
            string inputJson,
            string projectName,
            int? projectNumber,
            string taskName,
            bool create
        )
        {
            var inputData = new Inputs().FromJson(inputJson);
            // Unpack to an AuthToken instances
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
                create
            );
            // We won't get here unless the last getting of Project succeeded
            // Create the task if the Project succeeded
            if (project.ErrorMessages != null)
            {
                throw new Exception(project.ErrorMessages.First());
            }

            try
            {
                var task = new GenericViewSet<Task>(
                    tokens,
                    inputData.Url,
                    $"/api/project/{project.UID}/task/"
                ).GetOrCreate(
                    new Dictionary<string, object>
                    {
                        {"name", taskName}
                    },
                    new Dictionary<string, object>
                    {
                        {
                            "config", new Dictionary<string, string>
                            {
                                {"case_dir", "foam"},
                                {"task_type", "parent"} // This is optional - task types of "parent" will not execute jobs
                            }
                        }
                    },
                    create
                );
                inputData.Task = task;
            }
            catch (Exception) {}

            


            // We could have a function here that makes life easier to
            // merge the outputs with the provided inputs
            
            inputData.Project = project;

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

            return string.Join(";",tasks.Select(task => task.ToJson()));
        }
    }
}