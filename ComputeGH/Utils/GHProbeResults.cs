using System;
using System.Collections.Generic;
using System.Linq;
using ComputeCS.Components;
using ComputeGH.Properties;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
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
            pManager.AddPointParameter("U", "U", "Velocity vectors", GH_ParamAccess.tree);
            pManager.AddNumberParameter("p", "p", "Pressure", GH_ParamAccess.tree);

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
            

            foreach (string key in results.Keys)
            {
                var data = ConvertToDataTree(results[key]);
                int index = 0;
                if (key == "U") { index = 1; }
                else if (key == "p") { index = 2; }
                DA.SetDataTree(index, data);
                //AddToOutput(key, data);
            }

            var info = UpdateInfo(results);
            DA.SetData(0, info);

            // for key in results.keys()
            //      var data = ConvertToDataTree(results[key])
            //      AddToOutput(key, data)

            //var UVectors = ConvertToGHVectors(results["U"]);
            //var pValues = ConverToGHValues(results["p"]);

            //DA.SetDataTree(0, UVectors);
            //DA.SetDataTree(1, pValues);
        }

        private DataTree<object> ConvertToDataTree(Dictionary<string, object> data)
        {
            var counter = 0;
            var output = new DataTree<object>();
            foreach (var key in data.Keys)
            {
                var path = new GH_Path(counter);
                var data_ = (List<object>)data[key];
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

                counter++;
            }

            return output;
        }

        private void AddToOutput(string name, DataTree<object> data)
        {
            
            /*IGH_Param p = new Grasshopper.Kernel.Parameters.Param_GenericObject
            {
                Name = name,
                NickName = name
            };
            this.Params.Output.Add(p);
            */
        }

        private string UpdateInfo(Dictionary<string, Dictionary<string, object>> data)
        {
            var MasterKey = data.Keys.ToList().First();
            var info = "";
            var i = 0;
            foreach (string key in data[MasterKey].Keys)
            {
                info += $"{i} is {key}\n";
                i++;
            }
            return info;
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