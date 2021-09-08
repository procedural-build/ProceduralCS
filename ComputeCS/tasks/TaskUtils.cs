using System;
using System.Collections.Generic;
using System.Linq;
using ComputeCS.Exceptions;
using ComputeCS.types;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;

namespace ComputeCS.Tasks
{
    public class Utils
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static Task GetDependentTask(
            List<Task> subTasks,
            string dependentName,
            string url,
            AuthTokens tokens,
            string parentTaskId
        )
        {
            if (subTasks == null)
            {
                return null;
            }

            foreach (var subTask in subTasks.Where(subTask => !string.IsNullOrEmpty(subTask.UID))
                .Where(subTask => dependentName == subTask.Name && subTask.Parent.UID == parentTaskId))
            {
                Logger.Info($"Found simulation task: {subTask.UID}");
                return subTask;
            }

            Logger.Info($"Did not find a simulation task with name: {dependentName}. Trying to fetch it.");

            var queryParams = new Dictionary<string, object>
            {
                {"name", dependentName},
                {"parent", parentTaskId},
            };

            try
            {
                var task = new GenericViewSet<Task>(
                    tokens,
                    url,
                    "/api/task/"
                ).GetByQueryParams(queryParams);
                return task;
            }
            catch (ArgumentException err)
            {
                if (err.Message != "No object found.")
                {
                    Logger.Error($"Could not get Task. Got error: {err.Message}");
                    throw new ArgumentException(err.Message);
                }

                Logger.Info($"No task on the server with name: {dependentName}");
                return null;
            }
        }

        public static List<Task> GetDependentTask(
            List<Task> subTasks,
            List<string> dependentNames,
            string url,
            AuthTokens tokens,
            string parentTaskId
        )
        {
            return dependentNames.Select(name => GetDependentTask(subTasks, name, url, tokens, parentTaskId)).ToList();
        }

        public static Task CreateParent(
            AuthTokens tokens,
            string url,
            string path,
            string taskName,
            Dictionary<string, object> overrides,
            bool create
        )
        {
            var taskQueryParams = new Dictionary<string, object>
            {
                {"name", taskName}
            };
            if (overrides.ContainsKey("parent"))
            {
                taskQueryParams.Add("parent", overrides["parent"]);
            }

            var config = new Dictionary<string, object>
            {
                {
                    "task_type", "parent"
                },
                {"case_dir", "foam"}
            };

            if (overrides.ContainsKey("case_dir"))
            {
                config["case_dir"] = overrides["case_dir"];
                config["show_meta_data"] = overrides["show_meta_data"];
            }


            var taskCreateParams = new Dictionary<string, object>
            {
                {
                    "config", config
                }
            };

            if (overrides.ContainsKey("copy_from"))
            {
                taskCreateParams.Add("copy_from", overrides["copy_from"]);
            }

            if (overrides.ContainsKey("comment"))
            {
                taskCreateParams.Add("comment", overrides["comment"]);
            }

            var task = new GenericViewSet<Task>(
                tokens,
                url,
                path
            ).GetOrCreate(
                taskQueryParams,
                taskCreateParams,
                create
            );

            if (task.ErrorMessages != null)
            {
                if (task.ErrorMessages.First() == "No object found.")
                {
                    throw new NoObjectFoundException($"No task with name: {taskName} found");
                }

                throw new Exception(task.ErrorMessages.First());
            }

            return task;
        }
    }
}