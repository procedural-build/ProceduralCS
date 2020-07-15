using System;
using System.Collections.Generic;
using System.Text;
using ComputeCS.types;

namespace ComputeCS.Components
{
    public static class Probe
    {
        public static Dictionary<string, object> ProbePoints(
            string inputJson,
            string sampleSets,
            string fields,
            string dependentOn = "",
            bool create = false
        )
        {
            var inputData = new Inputs().FromJson(inputJson);
            var tokens = inputData.Auth;
            var parentTask = inputData.Task;
            var subTasks = inputData.SubTasks;
            var simulationTask = GetSimulationTask(subTasks, dependentOn);

            if (parentTask == null) {return null;}
            if (simulationTask == null) {return null;}

            var task = new GenericViewSet<Task>(
                tokens,
                inputData.Url,
                "/api/task/"
            ).GetOrCreate(
                new Dictionary<string, object> {
                    {"name", "PostProcess"},
                    {"parent", parentTask.UID},
                    {"dependent_on", simulationTask.UID}
                },
                new Dictionary<string, object> {
                    {"config", new Dictionary<string, object> {
                        {"task_type", "cfd"},
                        {"cmd", "probe"},
                        {"sampleSets", sampleSets},
                        {"fields", fields},
                    }}
                },
                create
            );

            inputData.SubTasks.Add(task);

            return new Dictionary<string, object>
            {
                {"out", inputData.ToJson()}
            };
        }

        private static Task GetSimulationTask(
            List<Task> subTasks,
            string dependentName = ""
        )
        {

            foreach (Task subTask in subTasks)
            {
                if (dependentName == subTask.Name)
                {
                    return subTask;
                }
                else if ((string)subTask.Config["cmd"] == "wind_tunnel")
                {
                    return subTask;
                }
                else if (((List<string>)subTask.Config["commands"])[0] == "simpleFoam")
                {
                    return subTask;
                }
            }
            return null;
        }
    }
}
