//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading;
//using ComputeCS.Components;
//using ComputeCS.types;
//using ComputeCS.utils.Cache;
//using ComputeCS.utils.Queue;
//using ComputeGH.Grasshopper.Utils;
//using ComputeGH.Properties;
//using Grasshopper;
//using Grasshopper.Kernel;
//using Grasshopper.Kernel.Data;
//using Grasshopper.Kernel.Parameters;
//using Grasshopper.Kernel.Types;
//using Newtonsoft.Json;
//using Rhino;
//using Rhino.Geometry;
//using Mesh = Rhino.Geometry.Mesh;

//namespace ComputeCS.Grasshopper
//{
//    public class GHProbeResults : PB_Component
//    {
//        /// <summary>
//        /// Initializes a new instance of the GHProbeResults class.
//        /// </summary>
//        public GHProbeResults()
//            : base("Load Probe Results", "Load Probe Results",
//                "Loads the probe results from a file",
//                "Compute", "CFD")
//        {
//        }

//        /// <summary>
//        /// Registers all the input parameters for this component.
//        /// </summary>
//        protected override void RegisterInputParams(GH_InputParamManager pManager)
//        {
//            pManager.AddTextParameter("Folder", "Folder", "Folder path to where to results are", GH_ParamAccess.item);
//            pManager.AddTextParameter("Mesh Path", "MeshPath", "The path to the analysis mesh from where the probe points are generated",
//                GH_ParamAccess.item);
//            pManager.AddTextParameter("Overrides", "Overrides",
//                "Optional overrides to apply to loading the results.\n" +
//                "The overrides lets you exclude or include files from the folder, that you want to download.\n" +
//                "If you want to exclude all files that ends with '.txt', then you can do that with: {\"exclude\": [\".txt\"]}\n" +
//                "Distance from mesh face center to result point. Used for reconstructing the mesh.\n" +
//                "Outputs let you specify consistent outputs from this components, as the outputs are dynamic they created at load time. You can avoid that with this override.\n" +
//                "The overrides takes a JSON formatted string as follows:\n" +
//                "{\n" +
//                "  \"exclude\": List[string],\n" +
//                "  \"include\": List[string],\n" +
//                "  \"distance\": number\n," +
//                "  \"outputs\": List[string]\n," +
//                "\n}",
//                GH_ParamAccess.item, "");
//            pManager.AddBooleanParameter("Rerun", "Rerun", "Rerun this component.", GH_ParamAccess.item);

//            pManager[0].Optional = true;
//            pManager[1].Optional = true;
//            pManager[2].Optional = true;
//            pManager[3].Optional = true;
//        }

//        /// <summary>
//        /// Registers all the output parameters for this component.
//        /// </summary>
//        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
//        {
//            pManager.AddTextParameter("Info", "Info", "Description of the outputs", GH_ParamAccess.tree);
//            pManager.AddPointParameter("Points", "Points", "Points that were extracted from probe files",
//                GH_ParamAccess.tree);
//            pManager.AddMeshParameter("Mesh", "Mesh",
//                "Correct mesh that matches the extracted points from probe files", GH_ParamAccess.tree);
//        }

//        /// <summary>
//        /// This is the method that actually does the work.
//        /// </summary>
//        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
//        protected override void SolveInstance(IGH_DataAccess DA)
//        {
//            string folder = null;
//            string meshPath = null;
//            var refresh = false;
//            var _overrides = "";

//            DA.GetData(0, ref folder);
//            DA.GetData(1, ref meshPath);
//            DA.GetData(2, ref _overrides);
//            DA.GetData(3, ref refresh);

//            var overrides = new ProbeResultOverrides().FromJson(_overrides) ?? new ProbeResultOverrides{Exclude = null, Include = null, Distance = 0.1, Outputs = null};

//            AddOverrideOutputs(overrides);

//            if (string.IsNullOrEmpty(folder))
//            {
//                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Please provide a valid string to the Folder input.");
//            }

//            // Get Cache to see if we already did this
//            var cacheKey = folder + meshPath + _overrides;
//            var cachedValues = StringCache.getCache(cacheKey);
//            DA.DisableGapLogic();

//            if (!string.IsNullOrEmpty(folder) && (cachedValues == null || refresh))
//            {
//                const string queueName = "probeResults";
//                StringCache.setCache(InstanceGuid.ToString(), "");

//                // Get queue lock
//                var queueLock = StringCache.getCache(queueName);
//                if (queueLock != "true")
//                {
//                    StringCache.setCache(queueName, "true");
//                    StringCache.setCache(cacheKey + "progress", "Loading...");

//                    QueueManager.addToQueue(queueName, () =>
//                    {
//                        try
//                        {
//                            var results = ProbeResult.ReadProbeResults(
//                                folder,
//                                overrides.Exclude,
//                                overrides.Include
//                            );
//                            cachedValues = JsonConvert.SerializeObject(results);
//                            StringCache.setCache(cacheKey, cachedValues);

//                            var points = ProbeResult.ReadPointsFromResults(
//                                folder,
//                                overrides.Exclude,
//                                overrides.Include
//                                );
//                            probePoints = ConvertPointsToDataTree(points);

//                            if (!string.IsNullOrEmpty(meshPath))
//                            {
//                                loadedMeshes = Import.LoadMeshFromPath(meshPath, overrides.Exclude, overrides.Include);    
//                            }


//                            if (loadedMeshes != null && loadedMeshes.Any())
//                            {
//                                if (points != null && points.Any())
//                                {
//                                    try
//                                    {
//                                        correctedMesh = CorrectMesh(loadedMeshes, points, overrides.Distance ?? 0.1);
//                                    }
//                                    catch (InvalidOperationException error)
//                                    {
//                                        AddRuntimeMessage(GH_RuntimeMessageLevel.Warning,
//                                            $"Could not construct new mesh. Got error: {error.Message}");
//                                    }
//                                }
//                                else
//                                {
//                                    correctedMesh = CorrectMesh(loadedMeshes);
//                                }

//                            }

//                            probeResults = new Dictionary<string, DataTree<object>>();
//                            foreach (var key in results.Keys)
//                            {
//                                probeResults.Add(key, ConvertToDataTree(results[key]));
//                            }

//                            info = UpdateInfo(results);
//                            resultKeys = results.Keys.ToList();
//                            StringCache.setCache(cacheKey + "progress", "Done");
//                        }
//                        catch (Exception e)
//                        {
//                            StringCache.setCache(InstanceGuid.ToString(), e.Message);
//                            StringCache.setCache(cacheKey, "error");
//                            StringCache.setCache(cacheKey + "progress", "");
//                        }

//                        ExpireSolutionThreadSafe(true);
//                        Thread.Sleep(2000);
//                        StringCache.setCache(queueName, "");
//                    });
//                    ExpireSolutionThreadSafe(true);
//                }
//            }


//            HandleErrors();

//            if (probePoints != null)
//            {
//                DA.SetDataTree(1, probePoints);
//            }

//            if (correctedMesh != null)
//            {
//                DA.SetDataTree(2, correctedMesh);
//            }

//            if (info != null)
//            {
//                DA.SetDataTree(0, info);
//            }

//            if (probeResults != null)
//            {
//                foreach (var key in probeResults.Keys)
//                {
//                    AddToOutput(DA, key, probeResults[key], overrides.Outputs);
//                }
//            }

//            if (resultKeys != null && overrides.Outputs == null)
//            {
//                resultKeys.Add("Info");
//                resultKeys.Add("Points");
//                resultKeys.Add("Mesh");
//                RemoveUnusedOutputs(resultKeys);
//            }

//            Message = StringCache.getCache(cacheKey + "progress");
//        }

//        private DataTree<object> CorrectMesh(
//            Dictionary<string, Mesh> meshes
//        )
//        {

//            var newMeshes = new DataTree<object>();
//            var j = 0;
//            foreach (var key in meshes.Keys)
//            {
//                var newMesh = new Mesh();

//                // Check mesh normal. If the normal direction is fx Z, check that the points and mesh have the same value. If not throw an error. 
//                GH_Convert.ToMesh(meshes[key], ref newMesh, GH_Conversion.Primary);

//                var path = new GH_Path(j);
//                newMeshes.Add(newMesh, path);
//                j++;
//            }

//            return newMeshes;
//        }

//        private DataTree<object> CorrectMesh(
//            Dictionary<string, Mesh> meshes,
//            Dictionary<string, List<List<double>>> points,
//            double distance
//        )
//        {

//            var newMeshes = new DataTree<object>();
//            var j = 0;
//            foreach (var key in meshes.Keys)
//            {
//                if (!points.ContainsKey(key)) continue;

//                var patchPoints = points[key];
//                var ghPoints = patchPoints.Select(point => new Point3d(point[0], point[1], point[2])).ToList();
//                var mesh = new Mesh();

//                // Check mesh normal. If the normal direction is fx Z, check that the points and mesh have the same value. If not throw an error. 
//                GH_Convert.ToMesh(meshes[key], ref mesh, GH_Conversion.Primary);
//                var faceCenters = Enumerable.Range(0, mesh.Faces.Count())
//                    .Select(index => mesh.Faces.GetFaceCenter(index)).ToList();
//                var faceIndices = RTree.Point3dClosestPoints(faceCenters, ghPoints, distance);

//                var newMesh = new Mesh();
//                newMesh.Vertices.AddVertices(mesh.Vertices);
//                foreach (var face in faceIndices)
//                {
//                    if (face.Length > 0)
//                    {
//                        newMesh.Faces.AddFace(mesh.Faces[face[0]]);
//                    }
//                }

//                newMesh.Normals.ComputeNormals();
//                newMesh.UnifyNormals();
//                newMesh.Compact();

//                var path = new GH_Path(j);
//                newMeshes.Add(newMesh, path);
//                j++;
//            }

//            return newMeshes;
//        }

//        private void AddOverrideOutputs(ProbeResultOverrides overrides)
//        {
//            if (overrides.Outputs == null || !overrides.Outputs.Any()) return;

//            foreach (var output in overrides.Outputs)
//            {
//                AddOutput(output);
//            }

//            var outputs = new List<string> {"Info", "Points", "Mesh"};
//            outputs.AddRange(overrides.Outputs);
//            RemoveUnusedOutputs(outputs);
//        }
//        private static DataTree<object> ConvertToDataTree(Dictionary<string, Dictionary<string, object>> data)
//        {
//            var patchCounter = 0;

//            var output = new DataTree<object>();
//            foreach (var patchKey in data.Keys)
//            {
//                var angleCounter = 0;
//                foreach (var fieldKey in data[patchKey].Keys)
//                {
//                    var path = new GH_Path(new int[] {patchCounter, angleCounter});
//                    var data_ = new List<object>();
//                    try
//                    {
//                        data_ = (List<object>) data[patchKey][fieldKey];
//                    }
//                    catch (Exception)
//                    {
//                        data_ = ((List<double>)data[patchKey][fieldKey]).Select(elem => (object)elem).ToList();
//                    }

//                    if (data_.Count < 1)
//                    {
//                        output.Add(null, path);
//                        continue;
//                    }
//                    var dataType = data_.First().GetType();
//                    if (dataType == typeof(double))
//                    {
//                        foreach (double element in data_)
//                        {
//                            output.Add(element, path);
//                        }
//                    }
//                    else if (dataType == typeof(List<double>))
//                    {
//                        foreach (List<double> row in data_)
//                        {
//                            output.Add(new Point3d(row[0], row[1], row[2]), path);
//                        }
//                    }

//                    angleCounter++;
//                }

//                patchCounter++;
//            }

//            return output;
//        }

//        private static DataTree<object> ConvertPointsToDataTree(Dictionary<string, List<List<double>>> data)
//        {
//            var patchCounter = 0;

//            var output = new DataTree<object>();
//            foreach (var patchKey in data.Keys)
//            {
//                var path = new GH_Path(patchCounter);
//                foreach (var point in data[patchKey])
//                {
//                    output.Add(new Point3d(point[0], point[1], point[2]), path);
//                }

//                patchCounter++;
//            }

//            return output;
//        }

//        private void AddToOutput(IGH_DataAccess DA, string name, DataTree<object> data, List<string> outputs)
//        {
//            var index = 0;
//            var found = false;

//            foreach (var param in Params.Output)
//            {
//                if (param.Name == name)
//                {
//                    found = true;
//                    break;
//                }

//                index++;
//            }

//            if (!found && (outputs == null || !outputs.Any()))
//            {
//                AddOutput(name);
//            }
//            else if (!found && outputs.Contains(name))
//            {
//                AddOutput(name);
//            }
//            else if (!found)
//            {
//                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, $"We found {name} in your results. It is not outputted because {string.Join(",", outputs)} does not contain {name}. Try to remove outputs or add {name} to the outputs");
//            }
//            else
//            {
//                DA.SetDataTree(index, data);
//            }
//        }

//        private void AddOutput(string name)
//        {
//            if (Params.Output.Any(param => param.Name == name)) return;

//            var p = new Param_GenericObject
//            {
//                Name = name,
//                NickName = name,
//                Access = GH_ParamAccess.tree
//            };
//            Params.RegisterOutputParam(p);
//            Params.OnParametersChanged();
//            ExpireSolution(true);
//        }

//        private static DataTree<object> UpdateInfo(
//            Dictionary<string, Dictionary<string, Dictionary<string, object>>> data)
//        {
//            var fieldKey = data.Keys.ToList().First();
//            var info = "Patch Names:\n";
//            var i = 0;
//            var patches = data[fieldKey].Keys.ToList();
//            foreach (var key in patches)
//            {
//                info += $"{{{i};*}} is {key}\n";
//                i++;
//            }

//            var j = 0;
//            var angles = new List<string>();
//            var patchKey = patches.First();
//            info += "\nAngles:\n";
//            foreach (var key in data[fieldKey][patchKey].Keys)
//            {
//                info += $"{{*;{j}}} is {key} degrees\n";
//                angles.Add(key);
//                j++;
//            }

//            var output = new DataTree<object>();
//            output.Add(info, new GH_Path(0));

//            foreach (var patch in patches)
//            {
//                output.Add(patch, new GH_Path(1));
//            }

//            foreach (var angle in angles)
//            {
//                output.Add(angle, new GH_Path(2));
//            }

//            return output;
//        }

//        private void RemoveUnusedOutputs(List<string> keys)
//        {
//            var parametersToDelete = Params.Output.Where(param => !keys.Contains(param.Name)).ToList();

//            if (parametersToDelete.Any())
//            {
//                foreach (var param in parametersToDelete)
//                {
//                    Params.UnregisterOutputParameter(param);
//                    Params.Output.Remove(param);
//                }

//                Params.OnParametersChanged();
//                ExpireSolution(true);
//            }
//        }

//        /// <summary>
//        /// Provides an Icon for the component.
//        /// </summary>
//        protected override System.Drawing.Bitmap Icon => Resources.IconMesh;

//        /// <summary>
//        /// Gets the unique ID for this component. Do not change this ID after release.
//        /// </summary>
//        public override Guid ComponentGuid => new Guid("74163c8b-25fd-466f-a56a-d2beeebcaccb");

//        private DataTree<object> probePoints;
//        private Dictionary<string, DataTree<object>> probeResults;
//        private DataTree<object> correctedMesh;
//        private DataTree<object> info;
//        private List<string> resultKeys;
//        private Dictionary<string, Mesh> loadedMeshes;
//    }
//}