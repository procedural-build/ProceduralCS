using System;
using System.Collections.Generic;
using System.Linq;
using ComputeCS.Components;
using ComputeGH.Properties;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Parameters;
using Rhino.Geometry;

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
              "Compute", "Utils")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Folder", "Folder", "Folder path to where to results are", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Info", "Info", "Description of the outputs", GH_ParamAccess.item);

        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string folder = null;

            if (!DA.GetData(0, ref folder)) return;

            var results = ProbeResult.ReadProbeResults(folder);
            

            foreach (var key in results.Keys)
            {
                var data = ConvertToDataTree(results[key]);
                AddToOutput(DA, key, data);
            }

            var info = UpdateInfo(results);
            DA.SetData(0, info);
            
            RemoveUnusedOutputs(results);
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
                    var path = new GH_Path(new int[]{patchCounter, angleCounter});
                    var data_ = (List<object>)data[patchKey][fieldKey];
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
                //Params.Output.Add(p);
                //index = Params.Output.Count;
                Params.OnParametersChanged();
                ExpireSolution(true);
            }
            else
            {
                DA.SetDataTree(index, data);
            }


            
        }

        private static string UpdateInfo(Dictionary<string, Dictionary<string, Dictionary<string, object>>> data)
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
            var patchKey = data[fieldKey].Keys.ToList().First();
            info += "\nAngles:\n";
            foreach (var key in data[fieldKey][patchKey].Keys)
            {
                info += $"{{*;{j}}} is {key} degrees\n";
                j++;
            }
            return info;
        }

        private void RemoveUnusedOutputs(Dictionary<string, Dictionary<string, Dictionary<string, object>>> data)
        {
            var keys = data.Keys.ToList();
            keys.Add("Info");
            var changes = false;
            
            foreach (var param in Params.Output)
            {
                if (!keys.Contains(param.Name))
                {
                    Params.UnregisterOutputParameter(param);
                    changes = true;
                }
            }

            if (changes)
            {
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
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return null; // Resources.IconMesh;
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