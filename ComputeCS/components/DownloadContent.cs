using ComputeCS.types;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ComputeCS.Components
{
    public class DownloadContentResponse
    {
        public bool FilesFoundForTask;
        public List<DownloadFile> Files;
    }

    public static class DownloadContent
    {
        public static DownloadContentResponse Download(
            string inputJson,
            string downloadPath,
            string overrides
        )
        {
            var inputData = new Inputs().FromJson(inputJson);
            var tokens = inputData.Auth;
            var parentTask = inputData.Task;
            var overrideDict = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(overrides);

            if (parentTask?.UID == null)
            {
                throw new Exception("Cannot download content without a parent task.");
            }

            var queryParams = new Dictionary<string, object> { { "filepath", downloadPath }, { "hash", true } };
            if (overrideDict != null && overrideDict.ContainsKey("exclude"))
            {
                queryParams.Add("exclude", string.Join(",", overrideDict["exclude"]));
            }
            if (overrideDict != null && overrideDict.ContainsKey("include"))
            {
                queryParams.Add("pattern", string.Join(",", overrideDict["include"]));
            }
            var serverFiles = new GenericViewSet<TaskFile>(
                tokens,
                inputData.Url,
                $"/api/task/{parentTask.UID}/file/"
            ).List(queryParams);

            return new DownloadContentResponse
            {
                FilesFoundForTask = serverFiles.Count > 0,
                Files = serverFiles
                    .Where(f => f.File.StartsWith(downloadPath))
                    .Select(f => DownloadTaskFile(f, tokens, inputData.Url, parentTask.UID))
                    .ToList()
            };
        }

        private static DownloadFile DownloadTaskFile(TaskFile serverFile, AuthTokens tokens, string url, string parentUID)
        {
            var filePathUnix = serverFile.File;

            var response = new GenericViewSet<string>(
                tokens,
                url,
                $"/api/task/{parentUID}/file/"
            ).RetrieveObjectAsBytes(filePathUnix, new Dictionary<string, object> { { "download", true } });

            return new DownloadFile
            {
                FilePathUnix = filePathUnix,
                Content = response,
            };
        }
    }
}