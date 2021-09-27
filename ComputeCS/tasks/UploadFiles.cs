using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ComputeCS.types;
using Newtonsoft.Json;
using NLog;

namespace ComputeCS.Tasks
{
    public static class Upload
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static void UploadBSDFFile(
            AuthTokens tokens,
            string url,
            string taskId,
            List<RadianceMaterial> materials
        )
        {
            foreach (var material in materials)
            {
                if (material.Overrides?.BSDF == null) continue;
                var bsdfIndex = 0;
                foreach (var bsdfPath in material.Overrides.BSDFPath)
                {
                    if (bsdfPath == "clear.xml")
                    {
                        bsdfIndex++;
                        continue;
                    }

                    var bsdfFileContent = File.ReadAllBytes(bsdfPath);

                    var uploadTask = new GenericViewSet<Dictionary<string, object>>(
                        tokens,
                        url,
                        $"/api/task/{taskId}/file/bsdf/{material.Overrides.BSDF[bsdfIndex]}"
                    ).Update(
                        null,
                        new Dictionary<string, object>
                        {
                            {"file", bsdfFileContent}
                        }
                    );
                    if (uploadTask.ContainsKey("error_messages"))
                    {
                        throw new Exception(uploadTask["error_messages"].ToString());
                    }

                    bsdfIndex++;
                }

                if (material.Overrides.ScheduleValues != null)
                {
                    UploadScheduleFiles(tokens, url, taskId, material);
                }
            }
        }

        public static void UploadScheduleFiles(
            AuthTokens tokens,
            string url,
            string taskId,
            RadianceMaterial material
        )
        {
            var scheduleIndex = 0;
            foreach (var schedule in material.Overrides.ScheduleValues)
            {
                var stream = new MemoryStream();
                using (var memStream = new StreamWriter(stream))
                {
                    memStream.Write(
                        $"{JsonConvert.SerializeObject(schedule)}");
                }

                var scheduleContent = stream.ToArray();
                var scheduleName = $"{material.Name.Replace("*", "")}_{scheduleIndex}.json";
                if (material.Overrides.Schedules == null)
                {
                    material.Overrides.Schedules = new List<string>();
                }

                material.Overrides.Schedules.Add(scheduleName);
                var uploadTask = new GenericViewSet<Dictionary<string, object>>(
                    tokens,
                    url,
                    $"/api/task/{taskId}/file/schedules/{scheduleName}"
                ).Update(
                    null,
                    new Dictionary<string, object>
                    {
                        {"file", scheduleContent}
                    }
                );
                if (uploadTask.ContainsKey("error_messages"))
                {
                    throw new Exception(uploadTask["error_messages"].ToString());
                }

                scheduleIndex++;
            }

            material.Overrides.ScheduleValues = null;
        }

        public static void UploadEPWFile(
            AuthTokens tokens,
            string url,
            string taskId,
            string epwFile
        )
        {
            var epwFileContent = File.ReadAllBytes(epwFile);
            var epwName = Path.GetFileName(epwFile);
            {
                // Upload EPW File to parent task
                var uploadTask = new GenericViewSet<Dictionary<string, object>>(
                    tokens,
                    url,
                    $"/api/task/{taskId}/file/weather/{epwName}"
                ).Update(
                    null,
                    new Dictionary<string, object>
                    {
                        {"file", epwFileContent}
                    }
                );
                if (uploadTask.ContainsKey("error_messages"))
                {
                    throw new Exception(uploadTask["error_messages"].ToString());
                }
            }
        }

        public static void UploadGeometry(
            AuthTokens tokens,
            string url,
            string taskId,
            byte[] geometryFile,
            string caseFolder,
            string fileName,
            List<Dictionary<string, byte[]>> refinementRegions = null
        )
        {
            // Upload File to parent task
            var geometryUpload = new GenericViewSet<Dictionary<string, object>>(
                tokens,
                url,
                $"/api/task/{taskId}/file/{caseFolder}/{fileName}.stl"
            ).Update(
                null,
                new Dictionary<string, object>
                {
                    {"file", geometryFile}
                }
            );
            if (geometryUpload.ContainsKey("error_messages"))
            {
                Console.Write(geometryUpload["error_messages"]);
            }

            if (refinementRegions != null)
            {
                foreach (var refinementRegion in refinementRegions)
                {
                    var refinementFileName = refinementRegion.Keys.First();
                    var file = refinementRegion[refinementFileName];
                    var refinementUpload = new GenericViewSet<Dictionary<string, object>>(
                        tokens,
                        url,
                        $"/api/task/{taskId}/file/foam/constant/triSurface/{refinementFileName}.stl"
                    ).Update(
                        null,
                        new Dictionary<string, object>
                        {
                            {"file", file}
                        }
                    );
                    if (refinementUpload.ContainsKey("error_messages"))
                    {
                        Console.Write(refinementUpload["error_messages"]);
                    }
                }
            }
        }

        public static void UploadPointsFiles(
            AuthTokens tokens,
            string url,
            string taskId,
            List<string> names,
            List<List<List<double>>> points,
            string caseDir,
            string fileExtension = "pts"
        )
        {
            var index = 0;
            foreach (var name in names)
            {
                Logger.Info($"Uploading probes from set {name} to the server");
                new GenericViewSet<Dictionary<string, object>>(
                    tokens,
                    url,
                    $"/api/task/{taskId}/file/{caseDir}/{name}.{fileExtension}"
                ).Update(
                    null,
                    new Dictionary<string, object>
                    {
                        {"file", points[index]}
                    }
                );
                index++;
            }
        }
        
        public static void UploadMeshFile(
            AuthTokens tokens,
            string url,
            string taskId,
            Dictionary<string, byte[]> meshFiles,
            string caseDir = "geometry"
        )
        {
            foreach (var name in meshFiles.Keys)
            {
                Logger.Info($"Uploading mesh: {name} to the server");
                var upload = new GenericViewSet<Dictionary<string, object>>(
                    tokens,
                    url,
                    $"/api/task/{taskId}/file/{caseDir}/{name}.obj"
                ).Update(
                    null,
                    new Dictionary<string, object>
                    {
                        {"file", meshFiles[name]}
                    }
                );
                if (upload.ContainsKey("errors"))
                {
                    Logger.Error(upload["error"]);
                    throw new Exception($"Got error while uploading mesh to server: {upload["error"]}");
                }

                Logger.Debug($"Uploaded {caseDir}/{name}.obj to server");
            }
        }
    }
}