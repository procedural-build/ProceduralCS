using System;
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
            var names = Path.GetFileName(filePath).Replace(".xy", "").Split('_');
            var fieldName = names[names.Length-1];
            var patchName = names[0];
            if (names.Length > 2)
            {
                patchName = string.Join("_", names.Take(names.Length - 1));
            }
            return new Dictionary<string, string>
            {
                {"field", fieldName},
                {"patch", patchName}
            };
        }

        public static List<object> ReadProbeData(string file)
        {
            var data = new List<object>();
            var lines = File.ReadAllLines(file);
            foreach (var line in lines)
            {

                var test = line.Split(' ').ToList();
                if (test.Count != 6 || test.Count != 4)
                {
                    Console.WriteLine(test);
                }
                
                var data_ = line.Split(' ').Skip(3).Select(x => double.Parse(x)).ToList();
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
            }

            return data;
        }

        public static string FolderToDataPath(string folder)
        {
            return folder.Split(Path.DirectorySeparatorChar).Last();
        }

        public static Dictionary<string, List<List<double>>> ReadPointsFromResults(string folder)
        {
            var data = new Dictionary<string, List<List<double>>>();
            var subFolders = Directory.GetDirectories(folder).OrderBy(f => float.Parse(FolderToDataPath(f)));
            foreach (var subFolder in subFolders)
            {
                data = GetPointsFromFolder(subFolder, data);
            }

            data = GetPointsFromFolder(folder, data);
            return data;
        }
        
        public static Dictionary<string, List<List<double>>> GetPointsFromFolder(
            string folder,
            Dictionary<string, List<List<double>>> data)
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

                if (!data.ContainsKey(patchName))
                {
                    data.Add(patchName, ReadPoints(file));
                }

            }

            return data;
        }
        
        public static List<List<double>> ReadPoints(string file)
        {
            var data = new List<List<double>>();
            var lines = File.ReadAllLines(file);
            foreach (var line in lines)
            {
                data.Add(line.Split('\t').Take(3).Select(x => double.Parse(x)).ToList());
            }

            return data;
        }
        
        public static Dictionary<string, Dictionary<string, object>> GetPointData(
            string file,
            Dictionary<string, Dictionary<string, object>> data,
            string patchName
        )
        {
            if (!data.ContainsKey("Points"))
            {
                data.Add("Points", new Dictionary<string, object>());
            }
            if (!data["Points"].ContainsKey(patchName))
            {
                data["Points"].Add(patchName, ReadPoints(file));
            }

            return data;
        }
    }
}