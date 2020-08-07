using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ComputeCS.Components
{
    public static class ProbeResult
    {
        public static Dictionary<string, Dictionary<string, object>> ReadProbeResults(string folder)
        {

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
            return Path.GetFileName(filePath).Split('_').Last().Split('.')[0];
        }

        public static List<object> ReadProbeData(string file)
        {
            var data = new List<object>();
            var lines = File.ReadAllLines(file);
            foreach (var line in lines)
            {
                var data_ = line.Split('\t').Skip(3).Select(x => double.Parse(x)).ToList();
                if (data_.Count == 1)
                {
                    data.Add(data_[0]);
                }
                else
                {
                    data.Add(data_);
                }
            }

            return data;
        }

        public static Dictionary<string, Dictionary<string, object>> GetDataFromFolder(string folder,
            Dictionary<string, Dictionary<string, object>> data)
        {
            var dataPath = FolderToDataPath(folder);

            foreach (string file in Directory.GetFiles(folder))
            {
                var fieldName = FileNameToFieldName(file);
                var values = ReadProbeData(file);
                if (!data.ContainsKey(fieldName))
                {
                    data.Add(fieldName, new Dictionary<string, object>());
                }

                data[fieldName].Add(dataPath, values);
            }

            return data;
        }

        public static string FolderToDataPath(string folder)
        {
            return folder.Split(Path.DirectorySeparatorChar).Last();
        }
    }
}