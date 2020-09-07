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
    public class GHThresholdResults : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the GHProbeResults class.
        /// </summary>
        public GHThresholdResults()
            : base("Wind Threshold Results", "Wind Threshold Results",
                @"Loads wind threshold results from a file(s)." +
                "\nLAWSON CRITERIA" +
                "\n0: Comfortable for dining" +
                "\n1: Comfortable for sitting" +
                "\n2: Comfortable for walking" +
                "\n3: Exceeds all criteria",
                "Compute", "Utils")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Folder", "Folder", "Folder path to where to results are", GH_ParamAccess.item);
            pManager.AddIntegerParameter("ThresholdType", "ThresholdType", "0: Wind Thresholds\n1: Lawsons Criteria",
                GH_ParamAccess.item, 0);

            pManager[1].Optional = true;
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
            var criteria = 0;

            if (!DA.GetData(0, ref folder)) return;
            DA.GetData(1, ref criteria);

            var results = WindThreshold.ReadThresholdResults(folder);
            var info = string.Empty;
            if (criteria == 1)
            {
                var lawsons = WindThreshold.LawsonsCriteria(results);
                foreach (var season in lawsons.Keys)
                {
                    var data = ConvertLawsonToDataTree(lawsons[season]);
                    AddToOutput(DA, season, data);
                }

                info = UpdateInfo(lawsons.First().Value.Keys.ToList());
                RemoveUnusedOutputs(lawsons.Keys.ToList());
            }
            else
            {
                foreach (var key in results.Keys)
                {
                    var data = ConvertToDataTree(results[key]);
                    AddToOutput(DA, key, data);
                }

                info = UpdateInfo(results.First().Value.Keys.ToList());
                RemoveUnusedOutputs(results.Keys.ToList());
            }


            DA.SetData(0, info);
        }

        private static DataTree<object> ConvertLawsonToDataTree(Dictionary<string, List<int>> data)
        {
            var patchCounter = 0;

            var output = new DataTree<object>();
            foreach (var patchKey in data.Keys)
            {
                var points = data[patchKey];
                var path = new GH_Path(patchCounter);
                foreach (var value in points)
                {
                    output.Add(value, path);
                }

                patchCounter++;
            }

            return output;
        }


        private static DataTree<object> ConvertToDataTree(Dictionary<string, object> data)
        {
            var patchCounter = 0;

            var output = new DataTree<object>();
            foreach (var patchKey in data.Keys)
            {
                var seasonValues = (List<List<double>>) data[patchKey];
                var seasonCounter = 0;

                foreach (var value in seasonValues)
                {
                    var path = new GH_Path(new[] {patchCounter, seasonCounter});
                    foreach (var x in value)
                    {
                        output.Add(x, path);
                    }

                    seasonCounter++;
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

        private static string UpdateInfo(List<string> patchKeys)
        {
            var info = "Patch Names:\n";
            var i = 0;
            foreach (var key in patchKeys)
            {
                info += $"{{{i}}} is {key}\n";
                i++;
            }

            return info;
        }


        private void RemoveUnusedOutputs(List<string> keys)
        {
            keys.Add("Info");
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
            get { return Resources.IconMesh; }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("d2422b3c-e13a-46a4-9700-ec3d53544013"); }
        }
    }
}