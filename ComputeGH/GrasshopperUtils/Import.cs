using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using NLog;
using Rhino.Geometry;

namespace ComputeGH.Grasshopper.Utils
{
    public static class Import
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static Dictionary<string, Mesh> LoadMeshFromPath(
            string meshPath,
            List<string> exclude,
            List<string> include,
            string fileType = "obj")
        {
            var data = new Dictionary<string, Mesh>();
            if (File.Exists(meshPath) && meshPath.EndsWith(fileType))
            {
                Logger.Info($"{meshPath} is a file");
                return LoadMeshFromFile(meshPath, data);
            }

            return LoadMeshFromFolder(meshPath, fileType, data, exclude, include);
        }

        private static Dictionary<string, Mesh> LoadMeshFromFile(
            string filePath, 
            Dictionary<string, Mesh> data
            )
        {
            Logger.Debug($"Loading mesh from from {filePath}");
            var name = Path.GetFileNameWithoutExtension(filePath);

            if (!data.ContainsKey(name))
            {
                data.Add(name, LoadMesh(filePath));
            }

            return data;
        }

        private static Dictionary<string, Mesh> LoadMeshFromFolder(
            string folderPath, 
            string fileType,
            Dictionary<string, Mesh> data,
            List<string> exclude,
            List<string> include
            )
        {
            var files = Directory.GetFiles(folderPath).Where(file => file.EndsWith(fileType));
            if (exclude != null && exclude.Count > 0)
            {
                files = files.Where(file => !exclude.Any(file.Contains)).ToArray();
            }
            if (include != null && include.Count > 0)
            {
                files = files.Where(file => include.Any(file.Contains)).ToArray();
            }
            
            return files.Aggregate(data, (current, file) => LoadMeshFromFile(file, current));
        }

        private static Mesh LoadMesh(string file)
        {
            var fileData = File.ReadAllLines(file);
            var mesh = new Mesh();
            
            foreach (var line in fileData)
            {
                var parts = line.Split(new char[] {' '}, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length > 0)
                {
                    switch (parts[0])
                    {
                        case "v":
                            double x, y, z;
                            double.TryParse(parts[1], out x);
                            double.TryParse(parts[2], out y);
                            double.TryParse(parts[3], out z);
                            mesh.Vertices.Add(x, y, z);
                            break;
                        case "f":
                            var faceIndices = new List<int>();
                            for (var partIndex = 1; partIndex < parts.Length; partIndex++)
                            {
                                var faceIndex = int.Parse(parts[partIndex].Split(new char[] {'/'},
                                    StringSplitOptions.RemoveEmptyEntries)[0]) - 1; 
                                faceIndices.Add(faceIndex);
                            }

                            if (parts.Length > 4)
                            {
                                mesh.Faces.AddFace(faceIndices[0], faceIndices[1], faceIndices[2], faceIndices[3]);
                            }
                            else
                            {
                                mesh.Faces.AddFace(faceIndices[0], faceIndices[1], faceIndices[2]);
                            }
                            break;
                    }
                }
            }
            
            mesh.Normals.ComputeNormals();
            mesh.UnifyNormals();
            mesh.Compact();
            
            return mesh;
        }
        
        public static Tuple<List<Mesh>, List<Mesh>, string> LoadIDFFromPath(string filePath)
        {
            Logger.Debug($"Loading IDF from from {filePath}");
            var geometry = new List<Mesh>();
            var zoneDict = new Dictionary<string, List<Mesh>>();
            var propertiesDict = new Dictionary<string, Dictionary<string, string>>();
            
            var fileData = File.ReadAllLines(filePath);
            var foundSurface = false;
            var name = "";
            var vertices = new List<Point3d>();
            var face = new List<int>();
            var faceIndex = 0;
            var zone = "";
            var xyz = new string[]{};
            foreach (var line in fileData)
            {
                switch (foundSurface)
                {
                    case true when string.IsNullOrWhiteSpace(line) || string.IsNullOrEmpty(line):
                        // Add surface data
                        var surface = new Mesh();
                        surface.Vertices.AddVertices(vertices);
                        if (face.Count == 4)
                        {
                            surface.Faces.AddFace(face[0], face[1], face[2], face[3]);    
                        }
                        else
                        {
                            surface.Faces.AddFace(face[0], face[1], face[2]);   
                        }
                        geometry.Add(surface);
                        zoneDict[zone].Add(surface);
                        
                        // Reset variables
                        foundSurface = false;
                        face = new List<int>();
                        name = "";
                        faceIndex = 0;
                        zone = "";
                        vertices = new List<Point3d>();
                        break;
                    case true:
                        var parts = line.Split(new []{'!', '-'}, StringSplitOptions.RemoveEmptyEntries);
                        var key = parts[1].Trim();
                        var value = parts[0].Replace(",", "").Replace(";", "").Trim();
                        if (key == "Name")
                        {
                            name = value;
                            propertiesDict.Add(value, new Dictionary<string, string>());
                        }
                        else if (key.StartsWith("X,Y,Z"))
                        {
                            xyz = value.Split(',');
                            vertices.Add(new Point3d{X=double.Parse(xyz[0]), Y=double.Parse(xyz[1]), Z=double.Parse(xyz[2])});
                            face.Add(faceIndex);
                            faceIndex++;
                        }
                        else if (key.StartsWith("Vertex"))
                        {
                            xyz = xyz.Append(value).ToArray();
                            if (xyz.Length == 3)
                            {
                                vertices.Add(new Point3d{X=double.Parse(xyz[0]), Y=double.Parse(xyz[1]), Z=double.Parse(xyz[2])});
                                face.Add(faceIndex);
                                faceIndex++;
                                xyz = new string[] { };
                            }
                            
                        }
                        else if (key == "Zone Name")
                        {
                            Logger.Debug($"Adding property: {key} with {value}");
                            propertiesDict[name].Add(key, value);
                            zone = value;
                            if (!zoneDict.ContainsKey(zone))
                            {
                                zoneDict.Add(zone, new List<Mesh>());
                            }
                        }
                        else
                        {
                            propertiesDict[name].Add(key, value);
                        }
                        break;
                    case false when line.ToLower().Contains("buildingsurface:detailed"):
                        foundSurface = true;
                        break;
                }
            }

            Logger.Debug("Done with file");
            var settings = new JsonSerializerSettings {Formatting = Formatting.Indented};
            var properties = JsonConvert.SerializeObject(propertiesDict, settings);
            var zones = new List<Mesh>();
            foreach (var meshes in zoneDict.Values)
            {
                var mesh = new Mesh();
                mesh.Append(meshes);
                zones.Add(mesh);
            }
            return new Tuple<List<Mesh>, List<Mesh>, string>(geometry, zones, properties);
        }
    }
}