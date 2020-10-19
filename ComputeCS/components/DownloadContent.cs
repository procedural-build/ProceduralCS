using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.IO;
using ComputeCS.types;
using System.Linq;
using ComputeCS.utils.Cache;

namespace ComputeCS.Components
{
    public static class DownloadContent
    {
        public static bool Download(
            string inputJson,
            string downloadPath,
            string localPath,
            List<string> exclude
        )
        {
            var inputData = new Inputs().FromJson(inputJson);
            var tokens = inputData.Auth;
            var parentTask = inputData.Task;


            if (parentTask == null || parentTask.UID == null)
            {
                throw new Exception("Cannot download content without a parent task.");
            }

            if (!Directory.Exists(localPath))
            {
                Directory.CreateDirectory(localPath);
            }

            var localFiles = Directory.EnumerateFiles(localPath, "*", SearchOption.AllDirectories)
                .Select(file => file.Remove(0, localPath.Length + 1)).ToList();
            foreach (var fileWin in localFiles)
            {
                var fileUnix = fileWin.Replace('\\', '/');
                if (StringCache.getCache(fileUnix) == null)
                {
                    StringCache.setCache(fileUnix, GetMD5(Path.Combine(localPath, fileWin)));
                }
            }

            var queryParams = new Dictionary<string, object> {{"filepath", downloadPath}, {"hash", true}};
            if (exclude.Count > 0)
            {
                queryParams.Add("exclude", exclude);
            }
            var serverFiles = new GenericViewSet<TaskFile>(
                tokens,
                inputData.Url,
                $"/api/task/{parentTask.UID}/file/"
            ).List(queryParams);

            if (serverFiles.Count == 0)
            {
                return false;
            }
            
            foreach (var serverFile in serverFiles)
            {
                // fileName is Unix path
                var filePathUnix = serverFile.File;
                var fileHash = serverFile.Hash;

                if (!filePathUnix.StartsWith(downloadPath))
                {
                    continue;
                }
                var filePathWin = filePathUnix.Remove(0, downloadPath.Length + 1).Replace('/', '\\');
                var localFilePath = Path.Combine(localPath, filePathWin);
                var localFileDirectory =
                    string.Join("\\", localFilePath.Split('\\').Take(localFilePath.Split('\\').Length - 1));
                if (!Directory.Exists(localFileDirectory))
                {
                    Directory.CreateDirectory(localFileDirectory); 
                }
                if (!localFiles.Contains(filePathWin))
                {
                    var response = new GenericViewSet<string>(
                        tokens,
                        inputData.Url,
                        $"/api/task/{parentTask.UID}/file/"
                    ).Retrieve(filePathUnix, localFileDirectory, new Dictionary<string, object> {{"download", true}});
                    StringCache.setCache(filePathUnix, GetMD5(localFilePath));
                }
                else if (fileHash != StringCache.getCache(filePathUnix.Remove(0, downloadPath.Length + 1)))
                {
                    var response = new GenericViewSet<string>(
                        tokens,
                        inputData.Url,
                        $"/api/task/{parentTask.UID}/file/"
                    ).Retrieve(filePathUnix, localFileDirectory, new Dictionary<string, object> {{"download", true}});
                    StringCache.setCache(filePathUnix, GetMD5(localFilePath));
                }
            }
            return true;
        }

        public static string GetMD5(string filename)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(filename))
                {
                    return BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", string.Empty).ToLower();
                }
            }
        }
    }
}