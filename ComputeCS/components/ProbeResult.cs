using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ComputeCS.Components
{
    public static class ProbeResult
    {
        public static Dictionary<string, Dictionary<string, object>> ReadProbeResults(string folder)
        {

            // outputs = {"U", "p"}
            // for subfolder in folder
            //      dataPath = subFolder
            //
            //      for file in subfolder
            //          fieldName = FileNameToFieldName(file)
            //          values = ReadProbeData(file)
            //          outputs[fieldName].Add(values, dataPath)
            // for file in folder
            //      fieldName = FileNameToFieldName(file)
            //      values = ReadProbeData(file)
            //      outputs[fieldName].Add(values, dataPath)

            var data = new Dictionary<string, Dictionary<string, object>>();
            foreach (string subFolder in Directory.GetDirectories(folder))
            {
                data = GetDataFromFolder(subFolder, data);
            }
            data = GetDataFromFolder(folder, data);
            return data;
        }

        public static string FileNameToFieldName(string filePath)
        {
            return "";
        }

        public static List<object> ReadProbeData(string file)
        {
            return new List<object>();
        }

        public static Dictionary<string, Dictionary<string, object>> GetDataFromFolder(string folder, Dictionary<string, Dictionary<string, object>> data)
        {
            var dataPath = FolderToDataPath(folder);

            foreach (string file in Directory.GetFiles(folder))
            {
                var fieldName = FileNameToFieldName(file);
                var values = ReadProbeData(file);
                data[fieldName].Add(dataPath, values);
            }
            return data;
        }

        public static string FolderToDataPath(string folder)
        {
            return folder;
        }
    }
}
