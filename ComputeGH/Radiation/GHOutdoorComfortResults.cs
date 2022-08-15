//using System;
//using System.Collections.Generic;
//using System.Drawing;
//using System.Linq;
//using System.Threading;
//using ComputeCS.Components;
//using ComputeCS.types;
//using ComputeCS.utils.Cache;
//using ComputeCS.utils.Queue;
//using ComputeGH.Properties;
//using Grasshopper;
//using Grasshopper.Kernel;
//using Grasshopper.Kernel.Data;
//using Grasshopper.Kernel.Parameters;
//using ComputeGH.Grasshopper.Utils;


//namespace ComputeGH.Radiation
//{
//    public class GHOutdoorComfortResults : PB_Component
//    {
//        /// <summary>
//        /// Initializes a new instance of the GHProbeResults class.
//        /// </summary>
//        public GHOutdoorComfortResults()
//            : base("Outdoor Comfort Results", "Comfort Results",
//                "Loads the outdoor comfort results from a file(s).",
//                "Compute", "Radiation")
//        {
//        }

//        /// <summary>
//        /// Registers all the input parameters for this component.
//        /// </summary>
//        protected override void RegisterInputParams(GH_InputParamManager pManager)
//        {
//            pManager.AddTextParameter("Folder", "Folder", "Folder path to where to results are", GH_ParamAccess.item);
//            pManager.AddBooleanParameter("Rerun", "Rerun", "Rerun this component.", GH_ParamAccess.item);

//            pManager[1].Optional = true;
//        }

//        /// <summary>
//        /// Registers all the output parameters for this component.
//        /// </summary>
//        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
//        {
//            pManager.AddTextParameter("Info", "Info", "Description of the outputs", GH_ParamAccess.item);
//            pManager.AddNumberParameter(
//                "winter_morning", "winter_morning",
//                "Included period: 12/01-03/01 07:00-10:00", GH_ParamAccess.tree);
//            pManager.AddNumberParameter(
//                "winter_noon", "winter_noon",
//                "Included period: 12/01-03/01 11:00-14:00", GH_ParamAccess.tree);
//            pManager.AddNumberParameter(
//                "winter_afternoon", "winter_afternoon",
//                "Included period: 12/01-03/01 15:00-18:00", GH_ParamAccess.tree);
//            pManager.AddNumberParameter(
//                "winter_evenings", "winter_evenings",
//                "Included period: 12/01-03/01 19:00-22:00", GH_ParamAccess.tree);
//            pManager.AddNumberParameter(
//                "spring_morning", "spring_morning",
//                "Included period: 03/01-06/01 07:00-10:00", GH_ParamAccess.tree);
//            pManager.AddNumberParameter(
//                "spring_noon", "spring_noon",
//                "Included period: 03/01-06/01 11:00-14:00", GH_ParamAccess.tree);
//            pManager.AddNumberParameter(
//                "spring_afternoon", "spring_afternoon",
//                "Included period: 03/01-06/01 15:00-18:00", GH_ParamAccess.tree);
//            pManager.AddNumberParameter(
//                "spring_evenings", "spring_evenings",
//                "Included period: 03/01-06/01 19:00-22:00", GH_ParamAccess.tree);
//            pManager.AddNumberParameter(
//                "summer_morning", "summer_morning",
//                "Included period: 06/01-09/01 07:00-10:00", GH_ParamAccess.tree);
//            pManager.AddNumberParameter(
//                "summer_noon", "summer_noon",
//                "Included period: 06/01-09/01 11:00-14:00", GH_ParamAccess.tree);
//            pManager.AddNumberParameter(
//                "summer_afternoon", "summer_afternoon",
//                "Included period: 06/01-09/01 15:00-18:00", GH_ParamAccess.tree);
//            pManager.AddNumberParameter(
//                "summer_evenings", "summer_evenings",
//                "Included period: 06/01-09/01 19:00-22:00", GH_ParamAccess.tree);
//            pManager.AddNumberParameter(
//                "fall_morning", "fall_morning",
//                "Included period: 09/01-12/01 07:00-10:00", GH_ParamAccess.tree);
//            pManager.AddNumberParameter(
//                "fall_noon", "fall_noon",
//                "Included period: 09/01-12/01 11:00-14:00", GH_ParamAccess.tree);
//            pManager.AddNumberParameter(
//                "fall_afternoon", "fall_afternoon",
//                "Included period: 09/01-12/01 15:00-18:00", GH_ParamAccess.tree);
//            pManager.AddNumberParameter(
//                "fall_evenings", "fall_evenings",
//                "Included period: 09/01-12/01 19:00-22:00", GH_ParamAccess.tree);
//            pManager.AddNumberParameter(
//                "yearly", "yearly",
//                "Included period: 1/1-12/31 00:00-24:00", GH_ParamAccess.tree);
//        }

//        /// <summary>
//        /// This is the method that actually does the work.
//        /// </summary>
//        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
//        protected override void SolveInstance(IGH_DataAccess DA)
//        {
//            string folder = null;
//            var refresh = false;

//            if (!DA.GetData(0, ref folder)) return;
//            DA.GetData(1, ref refresh);

//            var cacheKey = folder;
//            var cachedValues = StringCache.getCache(cacheKey);
//            DA.DisableGapLogic();

//            if (cachedValues == null || refresh)
//            {
//                const string queueName = "comfortResults";
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
//                            var results = OutdoorComfort.ReadComfortResults(folder);
//                            (comfortOutput, comfortLegend) = TransposeThresholdMatrix(results);
//                            StringCache.setCache(cacheKey + "progress", "Done");
//                            StringCache.setCache(cacheKey, "results");
//                            StringCache.setCache(InstanceGuid.ToString(), "");
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

//            // Handle Errors
//            HandleErrors();

//            if (comfortOutput != null)
//            {
//                foreach (var key in comfortOutput.Keys)
//                {
//                    AddToOutput(DA, key, comfortOutput[key]);
//                }

//                info = UpdateInfo(
//                    comfortLegend["patch"],
//                    comfortLegend["threshold"].Select(threshold => new Thresholds.ComfortThreshold {Field = threshold})
//                        .ToList()
//                );
//            }

//            if (info != null)
//            {
//                DA.SetDataTree(0, info);
//            }

//            Message = StringCache.getCache(cacheKey + "progress");
//        }

//        public static (Dictionary<string, DataTree<object>>, Dictionary<string, List<string>>) TransposeThresholdMatrix(
//            Dictionary<string, Dictionary<string, object>> resultsToTranspose)
//        {
//            var output = new Dictionary<string, DataTree<object>>();
//            var legend = new Dictionary<string, List<string>>
//            {
//                {"season", new List<string>()},
//                {"patch", new List<string>()},
//                {"threshold", new List<string>()}
//            };

//            var seasons = WindThreshold.ThresholdSeasons();

//            foreach (var thresholdKey in resultsToTranspose.Keys)
//            {
//                foreach (var patchKey in resultsToTranspose[thresholdKey].Keys)
//                {
//                    //var pointIndex = 0;
//                    var pointValues = (List<List<double>>) resultsToTranspose[thresholdKey][patchKey];
//                    var seasonCounter = 0;
//                    foreach (var seasonValue in pointValues)
//                    {
//                        var seasonKey = seasons[seasonCounter];

//                        if (!legend["season"].Contains(seasonKey))
//                        {
//                            legend["season"].Add(seasonKey);
//                        }

//                        if (!legend["patch"].Contains(patchKey))
//                        {
//                            legend["patch"].Add(patchKey);
//                        }

//                        if (!legend["threshold"].Contains(thresholdKey))
//                        {
//                            legend["threshold"].Add(thresholdKey);
//                        }

//                        if (!output.ContainsKey(seasonKey))
//                        {
//                            output.Add(seasonKey, new DataTree<object>());
//                        }

//                        var patchCounter = legend["patch"].IndexOf(patchKey);
//                        var thresholdCounter = legend["threshold"].IndexOf(thresholdKey);

//                        var path = new GH_Path(patchCounter, thresholdCounter);
//                        output[seasonKey].AddRange(seasonValue.Select(elem => (object)elem), path);
//                        seasonCounter++;
//                    }
//                }
//            }

//            return (output, legend);
//        }

//        private void AddToOutput(IGH_DataAccess DA, string name, DataTree<object> data)
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

//            if (!found)
//            {
//                var p = new Param_GenericObject
//                {
//                    Name = name,
//                    NickName = name,
//                    Access = GH_ParamAccess.tree
//                };
//                Params.RegisterOutputParam(p);
//                Params.OnParametersChanged();
//                ExpireSolution(true);
//            }
//            else
//            {
//                DA.SetDataTree(index, data);
//            }
//        }

//        private static DataTree<object> UpdateInfo(
//            List<string> patchKeys,
//            List<Thresholds.ComfortThreshold> thresholds
//        )
//        {
//            var info = "Patch Names:\n";
//            var output = new DataTree<object>();
//            var i = 0;
//            foreach (var key in patchKeys)
//            {
//                info += $"{{{i};*}} is {key}\n";
//                i++;
//            }

//            info += "\nThreshold Categories\n";
//            var j = 0;
//            foreach (var threshold in thresholds)
//            {
//                info += $"{{*;{j}}} is {threshold.Field}\n";
//                output.Add(threshold.Field, new GH_Path(1));
//                j++;
//            }


//            output.Add(info, new GH_Path(0));
//            return output;
//        }


//        private void RemoveUnusedOutputs(List<string> keys)
//        {
//            keys.Add("Info");
//            var parametersToDelete = new List<IGH_Param>();

//            foreach (var param in Params.Output)
//            {
//                if (!keys.Contains(param.Name))
//                {
//                    parametersToDelete.Add(param);
//                }
//            }

//            if (parametersToDelete.Count() > 0)
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
//        protected override Bitmap Icon => Resources.IconMesh;

//        /// <summary>
//        /// Gets the unique ID for this component. Do not change this ID after release.
//        /// </summary>
//        public override Guid ComponentGuid => new Guid("73cf955e-d12a-4ec2-9012-bbb1eb161c00");

//        private DataTree<object> info;
//        private Dictionary<string, DataTree<object>> comfortOutput;
//        private Dictionary<string, List<string>> comfortLegend;
//    }
//}