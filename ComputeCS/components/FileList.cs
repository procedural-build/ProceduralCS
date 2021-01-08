using System.Collections.Generic;
using System.Linq;
using ComputeCS.types;

namespace ComputeCS.Components
{
    public static class FileList
    {
        public static string GetFileList(
            string inputJson,
            string exclude,
            string include
        )
        {
            var inputData = new Inputs().FromJson(inputJson);
            // Unpack to an AuthToken instances
            var tokens = inputData.Auth;
            var taskId = inputData.Task.UID;
            
            var queryParams = new Dictionary<string, object>();
            if (!string.IsNullOrEmpty(exclude))
            {
                queryParams.Add("exclude", exclude);
            }
            if (!string.IsNullOrEmpty(include))
            {
                queryParams.Add("pattern", include);
            }

            var files = new GenericViewSet<TaskFile>(
                tokens,
                inputData.Url,
                $"/api/task/{taskId}/file/"
            ).List(queryParams);

            return string.Join(",", files.Select(file => file.File));
        }
        
    }
}