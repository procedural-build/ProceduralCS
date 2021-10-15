using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ComputeCS.types;

namespace ComputeCS.Components
{
    public class EnergyPlusCompute
    {
         public static Task CreateEnergyPlusActionTask(
            AuthTokens tokens,
            string url,
            Task parentTask,
            Project project,
            types.EnergySolution solution,
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

            var actionTaskConfig = new Dictionary<string, object>
            {
                {"task_type", "magpy"},
                {"cmd", "energyplus.io.tasks.write_idf"},
                {"config", solution},
                {"case_dir", caseDir},
            };
            
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
        public static Task CreateEnergyPlusTask(
            AuthTokens tokens,
            string url,
            Task parentTask,
            Task dependentOnTask,
            Project project,
            types.EnergySolution solution,
            string caseDir,
            bool create
        )
        {
            var taskConfig = new Dictionary<string, object>
            {
                {"task_type", "energyplus"},
                {"cmd", "energyplus"},
                {"case_dir", caseDir},
            };
            if (solution.Overrides != null)
            {
                taskConfig.Add("overrides", solution.Overrides);
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
            var energyPlusTask = TaskViews.GetCreateOrUpdateTask(
                tokens,
                url,
                $"/api/task/",
                new Dictionary<string, object>
                {
                    {"name", "EnergyPlus"},
                    {"parent", parentTask.UID},
                    {"dependent_on", dependentOnTask.UID},
                    {"project", project.UID}
                },
                createParams,
                create
            );
            if (energyPlusTask.ErrorMessages != null && energyPlusTask.ErrorMessages.Count > 0)
            {
                throw new Exception(energyPlusTask.ErrorMessages.First());
            }

            return energyPlusTask;
        }
    }
}