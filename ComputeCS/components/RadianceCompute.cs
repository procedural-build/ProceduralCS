using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ComputeCS.types;

namespace ComputeCS.Components
{
    public static class RadianceCompute
    {
        public static Task CreateRadianceActionTask(
            AuthTokens tokens,
            string url,
            Task parentTask,
            Task probeTask,
            Project project,
            types.RadiationSolution solution,
            string caseDir,
            bool create
        )
        {
            var actionTaskQueryParams = new Dictionary<string, object>
            {
                {"name", "Actions"},
                {"parent", parentTask.UID},
                {"project", project.UID}
            };
            if (probeTask != null)
            {
                actionTaskQueryParams.Add("dependent_on", probeTask.UID);
            }

            var actionTaskConfig = new Dictionary<string, object>
            {
                {"task_type", "magpy"},
                {"cmd", "radiance.io.tasks.write_rad"},
                {"materials", solution.Materials.Select(material => material.ToDict()).ToList()},
                {"case_dir", caseDir},
                {"method", solution.Method},
            };
            if (solution.Overrides != null && solution.Overrides.ReinhartDivisions > 1)
            {
                actionTaskConfig.Add("reinhart_divisions", solution.Overrides.ReinhartDivisions);
            }
            
            // Tasks to Handle MagPy Celery Actions
            // First Action to create Mesh Files
            var actionTask = TaskViews.GetCreateOrUpdateTask(
                tokens,
                url,
                $"/api/task/",
                actionTaskQueryParams,
                new Dictionary<string, object>
                {
                    {
                        "config", actionTaskConfig
                    }
                },
                create
            );
            
            if (actionTask.ErrorMessages == null || actionTask.ErrorMessages.Count <= 0) return actionTask;
            
            if (actionTask.ErrorMessages.First() == "No object found.")
            {
                return null;
            }
            throw new Exception(actionTask.ErrorMessages.First());

        }


        public static Task CreateRadianceTask(
            AuthTokens tokens,
            string url,
            Task parentTask,
            Task dependentOnTask,
            Project project,
            types.RadiationSolution solution,
            string caseDir,
            bool create
        )
        {
            var taskConfig = new Dictionary<string, object>
            {
                {"task_type", "radiance"},
                {"cmd", solution.Method},
                {"case_type", solution.CaseType},
                {"cpus", solution.CPUs},
                {"case_dir", caseDir},
                {"probes", solution.Probes},
            };
            if (solution.Overrides != null)
            {
                taskConfig.Add("overrides", solution.Overrides.ToDict());
            }

            if (!string.IsNullOrEmpty(solution.EPWFile))
            {
                taskConfig.Add("epw_file", Path.GetFileName(solution.EPWFile));
            }
            var createParams = new Dictionary<string, object>
            {
                {
                    "config", taskConfig
                }
            };
            var radianceTask = TaskViews.GetCreateOrUpdateTask(
                tokens,
                url,
                $"/api/task/",
                new Dictionary<string, object>
                {
                    {"name", Utils.SnakeCaseToHumanCase(solution.Method)},
                    {"parent", parentTask.UID},
                    {"dependent_on", dependentOnTask.UID},
                    {"project", project.UID}
                },
                createParams,
                create
            );
            if (radianceTask.ErrorMessages != null && radianceTask.ErrorMessages.Count > 0)
            {
                throw new Exception(radianceTask.ErrorMessages.First());
            }

            return radianceTask;
        }
    }
}