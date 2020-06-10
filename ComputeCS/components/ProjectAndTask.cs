using System.Collections.Generic;
using ComputeCS.types;

namespace ComputeCS.Components
{
    public static class ProjectAndTask
    {
        public static Dictionary<string, object> GetOrCreate(
            string inputJson,
            string projectName,
            int projectNumber,
            string taskName,
            bool create
        )
        {
            var inputData = new Inputs().FromJson(inputJson);
            // Unpack to an AuthToken instances
            var tokens = inputData.Auth;

            // Get a list of Projects for this user
            var project = new GenericViewSet<Project>(
                tokens,
                $"{inputData.Url}/api/project/"
            ).GetOrCreate(
                new Dictionary<string, object>
                {
                    {"name", projectName},
                    {"number", projectNumber}
                },
                null,
                create
            );

            // We won't get here unless the last getting of Project succeeded
            // Create the task if the Project succeeded
            var task = new GenericViewSet<Task>(
                tokens,
                $"{inputData.Url}/api/project/{project.UID}/task/"
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

            // We could have a function here that makes life easier to
            // merge the outputs with the provided inputs
            inputData.Task = task;
            inputData.Project = project;
            var output = inputData.ToJson();

            return new Dictionary<string, object>
            {
                {"out", output}
            };
        }
    }
}