using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.IO;
using ComputeCS.types;
using System.Linq;
using System.Threading;
using ComputeCS.utils.Cache;
using Newtonsoft.Json;

namespace ComputeCS.Components
{
    public static class DownloadContent
    {
        public static bool Download(
            string inputJson,
            string downloadPath,
            string localPath,
            string overrides
        )
        {
            var inputData = new Inputs().FromJson(inputJson);
            var tokens = inputData.Auth;
            var parentTask = inputData.Task;
            var overrideDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(overrides);


            if (parentTask?.UID == null)
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
            if (overrideDict != null && overrideDict.ContainsKey("exclude"))
            {
                queryParams.Add("exclude", overrideDict["exclude"]);
            }
            if (overrideDict != null && overrideDict.ContainsKey("include"))
            {
                queryParams.Add("pattern", overrideDict["include"]);
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

                var filePathWin = filePathUnix == downloadPath ? filePathUnix.Split('/').Last() : filePathUnix.Remove(0, downloadPath.Length + 1).Replace('/', '\\');
                
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
                    Thread.Sleep(100);
                }
                else if (fileHash != StringCache.getCache(filePathUnix.Remove(0, downloadPath.Length + 1)))
                {
                    var response = new GenericViewSet<string>(
                        tokens,
                        inputData.Url,
                        $"/api/task/{parentTask.UID}/file/"
                    ).Retrieve(filePathUnix, localFileDirectory, new Dictionary<string, object> {{"download", true}});
                    StringCache.setCache(filePathUnix, GetMD5(localFilePath));
                    Thread.Sleep(100);
                }
            }
            return true;
        }

        public static string GetMD5(string filename)
        {
            if (!File.Exists(filename))
            {
                return null;
            }
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