using System;
using System.Collections.Generic;
using System.IO;
using ComputeCS.types;
using System.Linq;

namespace ComputeCS.Components
{
    public static class DownloadContent
    {
        public static bool Download(
            string inputJson,
            string downloadPath, 
            string localPath,
            bool reload
        )
        {
            var inputData = new Inputs().FromJson(inputJson);
            var tokens = inputData.Auth;
            var parentTask = inputData.Task;
        

            if (parentTask == null)
            {
                throw new System.Exception("Cannot download content without a parent task.");
            }

            // If local path exists; return true
            var extractedFolder = Path.Combine(localPath, downloadPath.Split('/').Last());
            if (Directory.Exists(extractedFolder) && !reload)
            {
                return true;
            }

            // Download file from task task
            var downloadedFile = new GenericViewSet<string>(
                tokens,
                inputData.Url,
                $"/api/task/{parentTask.UID}/file/"
            ).Retrieve(downloadPath, localPath, new Dictionary<string, object> { {"download", true } });

            // If the file was downloaded return true
            if (File.Exists(downloadedFile))
            {
                if (downloadedFile.EndsWith(".zip"))
                {
                    if (reload) { Directory.Delete(extractedFolder); }
                    System.IO.Compression.ZipFile.ExtractToDirectory(downloadedFile, localPath);
                }
                return true;
            }

            // Return false if everything else fails.
            return false;
        }
    }
}
