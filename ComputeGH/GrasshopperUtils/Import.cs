//using System;
//using System.Collections.Generic;
//using System.Globalization;
//using System.IO;
//using System.Linq;
//using NLog;
//using Rhino.Geometry;

//namespace ComputeGH.Grasshopper.Utils
//{
//    public static class Import
//    {
//        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

//        public static Dictionary<string, Mesh> LoadMeshFromPath(
//            string meshPath,
//            List<string> exclude,
//            List<string> include,
//            string fileType = "obj"
//            )
//        {
//            var data = new Dictionary<string, Mesh>();
//            if (!File.Exists(meshPath)) return LoadMeshFromFolder(meshPath, fileType, data, exclude, include);

//            Logger.Info($"{meshPath} is a file");
//            return LoadMeshFromFile(meshPath, data);

//        }

//        private static Dictionary<string, Mesh> LoadMeshFromFile(
//            string filePath,
//            Dictionary<string, Mesh> data
//        )
//        {
//            Logger.Debug($"Loading mesh from from {filePath}");
//            var name = Path.GetFileNameWithoutExtension(filePath);

//            if (!data.ContainsKey(name))
//            {
//                data.Add(name, LoadMesh(filePath));
//            }

//            return data;
//        }

//        private static Dictionary<string, Mesh> LoadMeshFromFolder(
//            string folderPath,
//            string fileType,
//            Dictionary<string, Mesh> data,
//            List<string> exclude,
//            List<string> include
//        )
//        {
//            var files = Directory.GetFiles(folderPath).Where(file => file.EndsWith(fileType));
//            if (exclude != null && exclude.Count > 0)
//            {
//                files = files.Where(file => !exclude.Any(file.Contains)).ToArray();
//            }

//            if (include != null && include.Count > 0)
//            {
//                files = files.Where(file => include.Any(file.Contains)).ToArray();
//            }

//            return files.Aggregate(data, (current, file) => LoadMeshFromFile(file, current));
//        }

//        private static Mesh LoadMesh(string file)
//        {
//            var extension = Path.GetExtension(file);

//            switch (extension)
//            {
//                case ".obj":
//                    return LoadObjMesh(file);
//                case ".vtk":
//                    return LoadVtkMesh(file);
//                default:
//                    throw new FileLoadException($"Could not load {file}. The only supported formats are .vtk and .obj");
//            }
//        }

//        private static Mesh LoadObjMesh(string file)
//        {
//            var fileData = File.ReadAllLines(file);
//            var mesh = new Mesh();

//            foreach (var line in fileData)
//            {
//                var parts = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
//                if (parts.Length > 0)
//                {
//                    switch (parts[0])
//                    {
//                        case "v":
//                            double x, y, z;
//                            double.TryParse(parts[1], out x);
//                            double.TryParse(parts[2], out y);
//                            double.TryParse(parts[3], out z);
//                            mesh.Vertices.Add(x, y, z);
//                            break;
//                        case "f":
//                            var faceIndices = new List<int>();
//                            for (var partIndex = 1; partIndex < parts.Length; partIndex++)
//                            {
//                                var faceIndex = int.Parse(parts[partIndex].Split(new char[] { '/' },
//                                    StringSplitOptions.RemoveEmptyEntries)[0]) - 1;
//                                faceIndices.Add(faceIndex);
//                            }

//                            if (parts.Length > 4)
//                            {
//                                mesh.Faces.AddFace(faceIndices[0], faceIndices[1], faceIndices[2], faceIndices[3]);
//                            }
//                            else
//                            {
//                                mesh.Faces.AddFace(faceIndices[0], faceIndices[1], faceIndices[2]);
//                            }

//                            break;
//                    }
//                }
//            }

//            mesh.Normals.ComputeNormals();
//            mesh.UnifyNormals();
//            mesh.Compact();

//            return mesh;
//        }

//        private static Mesh LoadVtkMesh(string file)
//        {
//            var lines = File.ReadAllLines(file);
//            var mesh = new Mesh();

//            var foundPoints = false;
//            var foundPolygons = false;
//            foreach (var line in lines)
//            {
//                if (string.IsNullOrEmpty(line) || string.IsNullOrWhiteSpace(line))
//                {
//                    continue;
//                }
//                else if (line.StartsWith("POINTS"))
//                {
//                    foundPoints = true;
//                }
//                else if (line.StartsWith("POLYGONS"))
//                {
//                    foundPoints = false;
//                    foundPolygons = true;
//                }
//                else if (foundPoints)
//                {
//                    var data = line.Split(' ').Select(double.Parse).ToArray();
//                    mesh.Vertices.Add(data[0], data[1], data[2]);
//                }
//                else if (foundPolygons)
//                {
//                    var data = line.Split(' ').Select(int.Parse).ToArray();
//                    if (data[0] == 4)
//                    {
//                        mesh.Faces.AddFace(data[1], data[2], data[3], data[4]);
//                    }
//                    else
//                    {
//                        mesh.Faces.AddFace(data[1], data[2], data[3]);
//                    }
//                }
//            }

//            mesh.Normals.ComputeNormals();
//            mesh.UnifyNormals();
//            mesh.Compact();

//            return mesh;
//        }
//    }
//}