using System;
using System.Collections.Generic;
using System.Linq;
using ComputeCS.Components;
using ComputeGH.Properties;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using Mesh = Rhino.Geometry.Mesh;

namespace ComputeCS.Grasshopper
{
    public class GHProbeResults : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the GHProbeResults class.
        /// </summary>
        public GHProbeResults()
            : base("Probe Results", "Probe Results",
                "Loads the probe results from a file",
                "Compute", "CFD")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Folder", "Folder", "Folder path to where to results are", GH_ParamAccess.item);
            pManager.AddMeshParameter("Mesh", "Mesh", "Original mesh from where the probe points is generated",
                GH_ParamAccess.tree);

            pManager[1].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Info", "Info", "Description of the outputs", GH_ParamAccess.tree);
            pManager.AddPointParameter("Points", "Points", "Points that were extracted from probe files",
                GH_ParamAccess.tree);
            pManager.AddMeshParameter("Mesh", "Mesh",
                "Correct mesh that matches the extracted points from probe files", GH_ParamAccess.tree);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string folder = null;
            var meshes = new GH_Structure<GH_Mesh>();

            if (!DA.GetData(0, ref folder)) return;
            DA.GetDataTree(1, out meshes);

            var results = ProbeResult.ReadProbeResults(folder);

            foreach (var key in results.Keys)
            {
                var data = ConvertToDataTree(results[key]);
                AddToOutput(DA, key, data);
            }

            var points = ProbeResult.ReadPointsFromResults(folder);
            var GHPoints = ConvertPointsToDataTree(points);
            DA.SetDataTree(1, GHPoints);

            if (meshes.Any())
            {
                var newMesh = CorrectMesh(meshes, points);    
                DA.SetDataTree(2, newMesh);
            }

            var info = UpdateInfo(results);
            DA.SetDataTree(0, info);

            if (results != null)
            {
                var keys = results.Keys.ToList();
                keys.Add("Info");
                keys.Add("Points");
                keys.Add("Mesh");
                RemoveUnusedOutputs(keys);
            }

        }

        private static DataTree<object> CorrectMesh(
            GH_Structure<GH_Mesh> meshes,
            Dictionary<string, List<List<double>>> points
            )
        {

            var newMeshes = new DataTree<object>();
            var patches = points.Keys.ToList();
            var j = 0;
            foreach (var branch in meshes.Branches)
            {
                var patchPoints = points[patches[j]];
                var GHPoints = patchPoints.Select(point => new Point3d(point[0], point[1], point[2])).ToList();
                var mesh = new Mesh();
                
                // Check mesh normal. If the normal direction is fx Z, check that the points and mesh have the same value. If not throw an error. 
                GH_Convert.ToMesh(branch.First(), ref mesh, GH_Conversion.Primary);
                var faceCenters = Enumerable.Range(0, mesh.Faces.Count()).Select(index => mesh.Faces.GetFaceCenter(index)).ToList();
                var faceIndices = RTree.Point3dClosestPoints(faceCenters, GHPoints, 0.1);

                var newMesh = new Mesh();
                newMesh.Vertices.AddVertices(mesh.Vertices);
                foreach (var face in faceIndices)
                {
                    if (face.Length > 0) {
                        newMesh.Faces.AddFace(mesh.Faces[face[0]]);
                    }
                    
                }

                newMesh.Normals.ComputeNormals();
                newMesh.UnifyNormals();
                newMesh.Compact();
                
                var path = new GH_Path(j);
                newMeshes.Add(newMesh, path);
                j++;
            }

            return newMeshes;
        }

        private static DataTree<object> ConvertToDataTree(Dictionary<string, Dictionary<string, object>> data)
        {
            var patchCounter = 0;

            var output = new DataTree<object>();
            foreach (var patchKey in data.Keys)
            {
                var angleCounter = 0;
                foreach (var fieldKey in data[patchKey].Keys)
                {
                    var path = new GH_Path(new int[] {patchCounter, angleCounter});
                    var data_ = (List<object>) data[patchKey][fieldKey];
                    var dataType = data_.First().GetType();
                    if (dataType == typeof(double))
                    {
                        foreach (double element in data_)
                        {
                            output.Add(element, path);
                        }
                    }
                    else if (dataType == typeof(List<double>))
                    {
                        foreach (List<double> row in data_)
                        {
                            output.Add(new Point3d(row[0], row[1], row[2]), path);
                        }
                    }

                    angleCounter++;
                }

                patchCounter++;
            }

            return output;
        }

        private static DataTree<object> ConvertPointsToDataTree(Dictionary<string, List<List<double>>> data)
        {
            var patchCounter = 0;

            var output = new DataTree<object>();
            foreach (var patchKey in data.Keys)
            {
                var path = new GH_Path(patchCounter);
                foreach (var point in data[patchKey])
                {
                    output.Add(new Point3d(point[0], point[1], point[2]), path);
                }

                patchCounter++;
            }

            return output;
        }

        private void AddToOutput(IGH_DataAccess DA, string name, DataTree<object> data)
        {
            var index = 0;
            var found = false;

            foreach (var param in Params.Output)
            {
                if (param.Name == name)
                {
                    found = true;
                    break;
                }

                index++;
            }

            if (!found)
            {
                var p = new Param_GenericObject
                {
                    Name = name,
                    NickName = name,
                    Access = GH_ParamAccess.tree
                };
                Params.RegisterOutputParam(p);
                Params.OnParametersChanged();
                ExpireSolution(true);
            }
            else
            {
                DA.SetDataTree(index, data);
            }
        }

        private static DataTree<object> UpdateInfo(
            Dictionary<string, Dictionary<string, Dictionary<string, object>>> data)
        {
            var fieldKey = data.Keys.ToList().First();
            var info = "Patch Names:\n";
            var i = 0;
            foreach (var key in data[fieldKey].Keys)
            {
                info += $"{{{i};*}} is {key}\n";
                i++;
            }

            var j = 0;
            var angles = new List<string>();
            var patchKey = data[fieldKey].Keys.ToList().First();
            info += "\nAngles:\n";
            foreach (var key in data[fieldKey][patchKey].Keys)
            {
                info += $"{{*;{j}}} is {key} degrees\n";
                angles.Add(key);
                j++;
            }

            var output = new DataTree<object>();
            output.Add(info, new GH_Path(0));

            foreach (var angle in angles)
            {
                output.Add(angle, new GH_Path(1));
            }

            return output;
        }

        private void RemoveUnusedOutputs(List<string> keys)
        {
            var parametersToDelete = new List<IGH_Param>();

            foreach (var param in Params.Output)
            {
                if (!keys.Contains(param.Name))
                {
                    parametersToDelete.Add(param);
                }
            }

            if (parametersToDelete.Count() > 0)
            {
                foreach (var param in parametersToDelete)
                {
                    Params.UnregisterOutputParameter(param);
                    Params.Output.Remove(param);
                }

                Params.OnParametersChanged();
                ExpireSolution(true);
            }
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                if (System.Environment.GetEnvironmentVariable("RIDER") == "true")
                {
                    return null;
                }

                return Resources.IconMesh;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("74163c8b-25fd-466f-a56a-d2beeebcaccb"); }
        }
    }
}