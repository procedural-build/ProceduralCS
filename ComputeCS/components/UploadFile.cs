using System.Collections.Generic;
using ComputeCS.types;

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

            var response = new GenericViewSet<Dictionary<string, object>>(
                tokens,
                inputData.Url,
                $"/api/task/{parentTask.UID}/file/"
            ).GetOrCreate(
                new Dictionary<string, object>
                {
                    {"file", path},
                },
                new Dictionary<string, object>
                {
                    {"text", text}
                },
                upload
            );

            if (response.ContainsKey("file"))
            {
                return inputData.Url + $"/project/${project.UID}/task/${parentTask.UID}/files/${path}/";
            }

            return "";
        }
    }
}