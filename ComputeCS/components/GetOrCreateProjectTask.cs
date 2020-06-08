using System;
using System.Collections.Generic;
using System.Linq;
using ComputeCS.types;
using Newtonsoft.Json;

namespace ComputeCS
{
    public class Components
    {
        public static Dictionary<string, object> GetOrCreateProjectTask(
            string input_json_string,
            string project_name,
            int project_number,
            string task_name,
            bool create
        ) {
            Dictionary<string, string> input_dict = ComputeCS.Utils.DeserializeJsonString(input_json_string);
            // Unpack to an AuthToken instances
            AuthTokens tokens = new AuthTokens(); // Actually should be input_dict["auth"];

            // Get a list of Projects for this user
            var project = new GenericViewSet<Project>(
                tokens, 
                "/api/project/"
            ).GetOrCreate(
                new Dictionary<string, object> {
                    {"name", project_name},
                    {"number", project_number}
                }, 
                null,
                create
            );

            // We won't get here unless the last getting of Project succeeded
            // Create the task if the Project succeded
            var task = new GenericViewSet<Task>(
                tokens, 
                $"/api/project/{project.UID}/task/"
            ).GetOrCreate(
                new Dictionary<string, object> {
                    {"name", task_name}
                }, 
                new Dictionary<string, object> {
                    {"config", new Dictionary<string, string> {
                        {"case_dir", "foam"},
                        {"task_type", "parent"}         // This is optional - task types of "parent" will not execute jobs
                    }}
                }, 
                create
            );

            // We could have a function here that makes life easier to
            // merge the outputs with the provided inputs
            var output_string = new SerializeOutput {
                Auth = tokens,
                Url = "https://compute.procedural.build",
                Task = task,
                Project = project,
            }.ToJson();

            return new Dictionary<string, object> {
                {"out", output_string}
            };
        }
    }
}