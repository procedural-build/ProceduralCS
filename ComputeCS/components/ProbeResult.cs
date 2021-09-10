using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NLog;

namespace ComputeCS.Components
{
    public static class ProbeResult
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        
        public static Dictionary<string, Dictionary<string, Dictionary<string, object>>> ReadProbeResults(
            string folder,
            List<string> exclude,
            List<string> include
        )
        {
            var data = new Dictionary<string, Dictionary<string, Dictionary<string, object>>>();
            if (File.Exists(folder))
            {
                Logger.Info($"{folder} is a file");
                var dataPath = FolderToDataPath(Directory.GetParent(folder)?.FullName);
                return GetDataFromFile(folder, data, dataPath);
            }
            var subFolders = Directory.GetDirectories(folder)
                .OrderBy(_folder => float.Parse(FolderToDataPath(_folder)));
            data = subFolders.Aggregate(data,
                (current, subFolder) => GetDataFromFolder(subFolder, current, exclude, include));

            data = GetDataFromFolder(folder, data, exclude, include);
            return data;
        }


        public static Dictionary<string, string> FileNameToNames(string filePath)
        {
            string fieldName;
            string patchName;
            
            if (filePath.EndsWith((".xy")))
            {
                var names = Path.GetFileName(filePath).Replace(".xy", "").Split('_');
                fieldName = names[names.Length - 1];
                patchName = names[0];
                if (names.Length > 2)
                {
                    patchName = string.Join("_", names.Take(names.Length - 1));
                }
            }
            else
            {
                fieldName = Path.GetFileName(filePath);
                patchName = Directory.GetParent(filePath)?.Name;
            }

            return new Dictionary<string, string>
            {
                {"field", fieldName},
                {"patch", patchName}
            };
        }

        public static Dictionary<string, object> ReadProbeData(string file)
        {
            
            var lines = File.ReadAllLines(file);
            if (file.EndsWith(".xy"))
            {
                return new Dictionary<string, object> {{"xy", ReadXYProbeData(lines)}};
            }
            
            if (lines[0].StartsWith("# Probe"))
            {
                return ReadFunctionProbeData(lines);
            }

            return new Dictionary<string, object>();

        }

        public static Dictionary<string, object> ReadFunctionProbeData(string[] lines)
        {
            var data = new Dictionary<string, object>();
            foreach (var line in lines)
            {
                if (line.StartsWith("#")) continue;

                var matches = Regex.Matches(line, @"\(?\((.*?)\)");
                var time = Regex.Match(line, @"\d+");
                var row = new List<object>();
                foreach (var match in matches)
                {
                    row.Add(match.ToString().Trim(new char[]{'(', ')'}).Split(' ').Select(value => double.Parse(value)).ToList());
                    
                }

                data.Add(time.ToString(), row);
            }
            return data;
        }
        
        public static List<object> ReadXYProbeData(string[] lines)
        {
            var data = new List<object>();
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
            Dictionary<string, Dictionary<string, Dictionary<string, object>>> data,
            List<string> excludes,
            List<string> includes
        )
        {
            var dataPath = FolderToDataPath(folder);

            var files = Directory.GetFiles(folder).Where(file => file.EndsWith(".xy"));
            if (!files.Any())
            {
                files = Directory.GetFiles(folder).Where(file => Path.GetExtension(file) == "");
            }

            if (excludes != null && excludes.Count > 0)
            {
                files = files.Where(file => !excludes.Any(file.Contains)).ToArray();
            }
            if (includes != null && includes.Count > 0)
            {
                files = files.Where(file => includes.Any(file.Contains)).ToArray();
            }

            return files.Aggregate(data, (current, file) => GetDataFromFile(file, current, dataPath));
            
        }

        public static Dictionary<string, Dictionary<string, Dictionary<string, object>>> GetDataFromFile(
            string file,
            Dictionary<string, Dictionary<string, Dictionary<string, object>>> data,
            string dataPath
            )
        {
            Logger.Debug($"Getting probe results from {file}");
            
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

            if (values.ContainsKey("xy"))
            {
                data[fieldName][patchName].Add(dataPath, values["xy"]);   
            }
            else
            {
                foreach (var timeKey in values.Keys)
                {
                    data[fieldName][patchName].Add(timeKey, values[timeKey]);     
                }
                  
            }
            

            return data;
        }

        private static string FolderToDataPath(string folder)
        {
            return folder.Split(Path.DirectorySeparatorChar).Last();
        }

        public static Dictionary<string, List<List<double>>> ReadPointsFromResults(
            string folder,
            List<string> exclude,
            List<string> include
        )
        {
            var data = new Dictionary<string, List<List<double>>>();
            if (File.Exists(folder))
            {
                Logger.Info($"{folder} is a file");
                return GetPointsFromFile(folder, data);
            }
            var subFolders = Directory.GetDirectories(folder).OrderBy(f => float.Parse(FolderToDataPath(f)));
            data = subFolders.Aggregate(data, (current, subFolder) => GetPointsFromFolder(subFolder, current, exclude, include));

            data = GetPointsFromFolder(folder, data, exclude, include);
            return data;
        }

        public static Dictionary<string, List<List<double>>> GetPointsFromFolder(
            string folder,
            Dictionary<string, List<List<double>>> data,
            List<string> excludes,
            List<string> includes
            )
        {
            var files = Directory.GetFiles(folder).Where(file => file.EndsWith(".xy"));
            if (!files.Any())
            {
                files = Directory.GetFiles(folder).Where(file => Path.GetExtension(file) == "");
            }
            
            if (excludes != null && excludes.Count > 0)
            {
                files = files.Where(file => !excludes.Any(file.Contains)).ToArray();
            }
            if (includes != null && includes.Count > 0)
            {
                files = files.Where(file => includes.Any(file.Contains)).ToArray();
            }

            return files.Aggregate(data, (current, file) => GetPointsFromFile(file, current));
        }

        public static Dictionary<string, List<List<double>>> GetPointsFromFile(
            string file,
            Dictionary<string, List<List<double>>> data
        )
        {
            Logger.Debug($"Getting probe points from {file}");
            var names = FileNameToNames(file);
            var patchName = names["patch"];

            if (!data.ContainsKey(patchName))
            {
                data.Add(patchName, ReadPoints(file));
            }

            return data;
        }

        public static List<List<double>> ReadPoints(string file)
        {
            var lines = File.ReadAllLines(file);
            if (file.EndsWith(".xy"))
            {
                return ReadXYPoints(lines);
            }
            
            if (lines[0].StartsWith("# Probe"))
            {
                return ReadFunctionPoints(lines);
            }

            return  new List<List<double>>();

        }

        public static List<List<double>> ReadXYPoints(string[] lines)
        {
            var data = new List<List<double>>();
            foreach (var line in lines)
            {
                data.Add(line.Split('\t').Take(3).Select(x => double.Parse(x)).ToList());
            }

            return data;
        }
        
        public static List<List<double>> ReadFunctionPoints(string[] lines)
        {
            var data = new List<List<double>>();
            foreach (var line in lines)
            {
                if (!line.StartsWith("# Probe")) continue;

                var _data = Regex.Replace(line, @"# Probe \d \(", "");

                data.Add(_data.Trim(')').Split(' ').Select(value => double.Parse(value)).ToList());
            }
            return data;
        }
        
    }
}