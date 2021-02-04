using System;
using System.Collections.Generic;
using System.Linq;
using ComputeCS;
using ComputeCS.types;
using Newtonsoft.Json;


namespace ComputeCS
{
    public static class Tasks
    {
        public static Task GetCreateOrUpdateTask(
            AuthTokens tokens,
            string url,
            string path,
            Dictionary<string, object> queryParams,
            Dictionary<string, object> createParams,
            bool create
        )
        {
            try
            {
                var task = new GenericViewSet<Task>(
                    tokens,
                    url,
                    path
                ).GetByQueryParams(queryParams);
                
                if (task != null && create)
                {
                    if (new List<string>{"failed", "finished", "stopped"}.IndexOf(task.Status) != -1)
                    {
                        createParams.Add("status", "pending");
                    }

                    task = new GenericViewSet<Task>(
                        tokens,
                        url,
                        path
                    ).PartialUpdate(
                        task.UID,
                        createParams,
                        new Dictionary<string, object> {{"runAs", "last"}}
                    );
                }

                return task;
            }
            catch (ArgumentException err)
            {
                if (create)
                {
                    // Merge the query_params with create_params
                    if (createParams == null)
                    {
                        createParams = queryParams;
                    }
                    else
                    {
                        createParams = queryParams
                            .Union(createParams)
                            .ToDictionary(s => s.Key, s => s.Value);
                    }

                    // Create the object
                    return new GenericViewSet<Task>(
                        tokens,
                        url,
                        path
                    ).Create(createParams);
                }

                return new Task {ErrorMessages = new List<string> {err.Message}};
            }
        }
    }
}