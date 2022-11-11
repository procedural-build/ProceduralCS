using ComputeCS.types;
using System.Collections.Generic;
using System.Text;

namespace ComputeCS.Components
{
    public static class UploadFile
    {
        public static string UploadTextFile(string input, string path, string text, bool upload)
        {
            var inputData = new Inputs().FromJson(input);
            var tokens = inputData.Auth;
            var parentTask = inputData.Task;
            var project = inputData.Project;

            var view = new GenericViewSet<Dictionary<string, object>>(
                tokens,
                inputData.Url,
                $"/api/task/{parentTask.UID}/file/"
            );

            if (upload)
            {
                var response = view.Update(path, new Dictionary<string, object>
                {
                    {"file", Encoding.UTF8.GetBytes(text) }
                });

                return response.ContainsKey("file")
                    ? inputData.Url + $"/project/${project.UID}/task/${parentTask.UID}/files/${path}/"
                    : "";
            }
            else
            {
                var existing = view.List(new Dictionary<string, object>
                {
                    { "pattern", path }
                });
                return existing.Count > 0
                    ? inputData.Url + $"/project/${project.UID}/task/${parentTask.UID}/files/${path}/"
                    : "";
            };
        }
    }
}