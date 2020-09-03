using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ComputeCS.Components
{
    public static class ProbeResult
    {
        public static Dictionary<string, Dictionary<string, Dictionary<string, object>>> ReadProbeResults(string folder)
        {
            var data = new Dictionary<string, Dictionary<string, Dictionary<string, object>>>();
            var subFolders = Directory.GetDirectories(folder).OrderBy(f => float.Parse(FolderToDataPath(f)));
            foreach (var subFolder in subFolders)
            {
                data = GetDataFromFolder(subFolder, data);
            }

            data = GetDataFromFolder(folder, data);
            return data;
        }

        public static Dictionary<string, string> FileNameToNames(string filePath)
        {
            var names = Path.GetFileName(filePath).Split('.')[0].Split('_');
            return new Dictionary<string, string>
            {
                {"field", names[1]},
                {"patch", names[0]}
            };
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

        public static List<object> ReadPoints(string file)
        {
            var data = new List<object>();
            var lines = File.ReadAllLines(file);
            foreach (var line in lines)
            {
                data.Add(line.Split('\t').Take(3).Select(x => double.Parse(x)).ToList());
            }

            return data;
        }

        public static Dictionary<string, Dictionary<string, Dictionary<string, object>>> GetDataFromFolder(
            string folder,
            Dictionary<string, Dictionary<string, Dictionary<string, object>>> data)
        {
            var dataPath = FolderToDataPath(folder);

            foreach (var file in Directory.GetFiles(folder))
            {
                if (!file.EndsWith(".xy"))
                {
                    continue;
                }

                var names = FileNameToNames(file);
                var fieldName = names["field"];
                var patchName = names["patch"];
                var values = ReadProbeData(file);

                if (!data.ContainsKey(fieldName))
                {
                    data.Add(fieldName, new Dictionary<string, Dictionary<string, object>>());
                }

                if (!data[fieldName].ContainsKey(patchName))
                {
                    data[fieldName].Add(patchName, new Dictionary<string, object>());
                }

                data[fieldName][patchName].Add(dataPath, values);
                //data = GetPointData(file, data, patchName);
            }

            return data;
        }

        public static string FolderToDataPath(string folder)
        {
            return folder.Split(Path.DirectorySeparatorChar).Last();
        }
    }
}