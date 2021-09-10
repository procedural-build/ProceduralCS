using System.Collections.Generic;
using ComputeCS.types;

namespace ComputeCS.Tasks
{
    public static class CFD
    {
        public static Task CreateSimpleCase(
            AuthTokens tokens,
            string url,
            string path,
            string parentTaskId,
            string meshTaskId,
            CFDSolution solution,
            bool create)
        {
            var config = new Dictionary<string, object>
            {
                {"task_type", "cfd"},
                {"cmd", "pipeline"},
                {
                    "commands", new List<string>
                    {
                        solution.Solver,
                        "reconstructPar -noZero"
                    }
                },
                {"cpus", solution.CPUs},
                {"iterations", solution.Iterations}
            };
            
            if (solution.Overrides != null && solution.Overrides.ContainsKey("webhook"))
            {
                config.Add("webhook", solution.Overrides["webhook"]);
            }
            
            var createParams = new Dictionary<string, object>
            {
                {
                    "config", config
                }
            };

            return createParams.ContainsKey("config")
                ? SendCFDTask(tokens, url, path, solution, parentTaskId, meshTaskId, createParams, create): null;
        }

        public static Task CreateVWTCase(
            AuthTokens tokens,
            string url,
            string path,
            string parentTaskId,
            string meshTaskId,
            CFDSolution solution,
            bool create)
        {
            var config = new Dictionary<string, object>
            {
                {"task_type", "cfd"},
                {"cmd", "wind_tunnel"},
                {"commands", solution.Angles},
                {"cpus", solution.CPUs},
                {"iterations", solution.Iterations},
                {"solver", solution.Solver}
            };

            if (solution.Overrides != null)
            {
                if (solution.Overrides.ContainsKey("copy_folder"))
                {
                    config.Add("overrides",
                        new Dictionary<string, object> {{"copy_folder", solution.Overrides["copy_folder"]}});
                }

                if (solution.Overrides.ContainsKey("webhook"))
                {
                    config.Add("webhook", solution.Overrides["webhook"]);
                }
            }

            var createParams = new Dictionary<string, object>
            {
                {
                    "config", config
                }
            };

            return SendCFDTask(tokens, url, path, solution, parentTaskId, meshTaskId, createParams, create);
        }

        private static Task SendCFDTask(
            AuthTokens tokens,
            string url,
            string path,
            CFDSolution solution,
            string parentTaskId,
            string dependentOnId,
            Dictionary<string, object> createParams,
            bool create
        )
        {
            var queryParams = new Dictionary<string, object>
            {
                {"name", solution.CaseType},
                {"parent", parentTaskId}
            };
            if (!string.IsNullOrEmpty(dependentOnId))
            {
                createParams.Add("dependent_on", dependentOnId);
            }
            var cfdTask = new GenericViewSet<Task>(
                tokens,
                url,
                path
            ).GetOrCreate(
                queryParams,
                createParams,
                create
            );
            return cfdTask;
        }
    }
}