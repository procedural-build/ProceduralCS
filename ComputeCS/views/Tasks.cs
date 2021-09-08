using System;
using System.Collections.Generic;
using System.Linq;
using ComputeCS.Exceptions;
using ComputeCS.types;
using NLog;

namespace ComputeCS
{
    public static class TaskViews
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

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
                    if (new List<string> {"failed", "finished", "stopped"}.IndexOf(task.Status) != -1)
                    {
                        Logger.Debug($"Setting status to pending for Task: {task.UID}");
                        createParams.Add("status", "pending");
                    }

                    Logger.Info($"Updating Task: {task.UID}");
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

                    Logger.Info($"Creating new task with create params: {createParams}");
                    // Create the object
                    return new GenericViewSet<Task>(
                        tokens,
                        url,
                        path
                    ).Create(createParams);
                }

                Logger.Error($"Got error: {err.Message} while trying to create task");
                if (err.Message == "No object found.")
                {
                    throw new NoObjectFoundException($"No task with name: {queryParams["name"]} found");
                }

                return new Task {ErrorMessages = new List<string> {err.Message}};
            }
        }
    }
}