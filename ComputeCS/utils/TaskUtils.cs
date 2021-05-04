using System.Collections.Generic;
using ComputeCS.types;
using Newtonsoft.Json.Linq;
using NLog;

namespace ComputeCS
{
    public class TaskUtils
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

            foreach (var subTask in subTasks)
            {
                if (string.IsNullOrEmpty(subTask.UID)) continue;

                if (dependentName != subTask.Name) continue;
                
                Logger.Info($"Found simulation task: {subTask.UID}");
                return subTask;
            }

            Logger.Info($"Did not find a simulation task with name: {dependentName}. Trying to fetch it.");
            
            var queryParams = new Dictionary<string, object>
            {
                {"name", dependentName},
                {"parent", parentTaskId},
            };
            
            var task = new GenericViewSet<Task>(
                tokens,
                url,
                "/api/task/"
            ).GetByQueryParams(queryParams);

            if (task != null) return task;
            
            Logger.Info($"No task on the server with name: {dependentName}");
            return null;

        }
    }
}