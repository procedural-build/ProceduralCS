//using System;
//using System.Collections.Generic;
//using System.Drawing;
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
//using Rhino;

//namespace ComputeCS.Grasshopper
//{
//    public class GHThresholdResults : PB_Component
//    {
//        /// <summary>
//        /// Initializes a new instance of the GHProbeResults class.
//        /// </summary>
//        public GHThresholdResults()
//            : base("Wind Threshold Results", "Wind Threshold Results",
//                "Loads wind threshold results from a file(s).",
//                "Compute", "CFD")
//        {
//        }

//        /// <summary>
//        /// Registers all the input parameters for this component.
//        /// </summary>
//        protected override void RegisterInputParams(GH_InputParamManager pManager)
//        {
//            pManager.AddTextParameter("Folder", "Folder", "Folder path to where to results are", GH_ParamAccess.item);
//            pManager.AddIntegerParameter("ThresholdType", "ThresholdType", "0: Wind Thresholds\n1: Lawsons Criteria",
//                GH_ParamAccess.item, 0);
//            pManager.AddTextParameter("ThresholdFrequency", "ThresholdFrequency",
//                "Thresholds frequencies for different wind comfort categories. This only applies if you have chosen 1 in the ThresholdType." +
//                "\nInput should be a list JSON formatted strings, " +
//                "with the fields: \"field\" and \"value\", respectively describing the category name and threshold frequency in % that the wind velocity should be less than." +
//                "\nThe category names should match the names from the Wind Threshold component. Only matching category names will be shown." +
//                "\nThe default values corresponds to the Lawson 2001 Criteria",
//                GH_ParamAccess.list,
//                new List<string>
//                {
//                    "{\"field\": \"sitting\", \"value\": 5}",
//                    "{\"field\": \"standing\", \"value\": 5}",
//                    "{\"field\": \"strolling\", \"value\": 5}",
//                    "{\"field\": \"business_walking\", \"value\": 5}",
//                    "{\"field\": \"uncomfortable\", \"value\": 95}",
//                    "{\"field\": \"unsafe_frail\", \"value\": 99.977}",
//                    "{\"field\": \"unsafe_all\", \"value\": 99.977}"
//                });
//            pManager.AddBooleanParameter("Rerun", "Rerun", "Rerun this component.", GH_ParamAccess.item);

//            pManager[1].Optional = true;
//            pManager[2].Optional = true;
//            pManager[3].Optional = true;
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
//            var criteria = 0;
//            var thresholdsFrequencies = new List<string>();
//            var refresh = false;

//            if (!DA.GetData(0, ref folder)) return;
//            DA.GetData(1, ref criteria);
//            DA.GetDataList(2, thresholdsFrequencies);
//            DA.GetData(3, ref refresh);

//            var cacheKey = folder + criteria + string.Join("", thresholdsFrequencies);
//            var cachedValues = StringCache.getCache(cacheKey);
//            DA.DisableGapLogic();

//            if (cachedValues == null || refresh)
//            {
//                const string queueName = "thresholdResults";
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
//                            var results = WindThreshold.ReadThresholdResults(folder);
//                            if (criteria == 1)
//                            {
//                                _thresholdFrequencies = thresholdsFrequencies
//                                    .Select(frequency => new Thresholds.WindThreshold().FromJson(frequency)).ToList();
//                                lawsonResults = WindThreshold.LawsonsCriteria(results, _thresholdFrequencies);
//                            }
//                            else
//                            {
//                                (thresholdOutput, thresholdLegend) = TransposeThresholdMatrix(results);
//                            }
//                            StringCache.setCache(cacheKey + "progress", "Done");
//                            StringCache.setCache(cacheKey, "results");
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

//            if (lawsonResults != null && criteria == 1)
//            {
//                foreach (var season in lawsonResults.Keys)
//                {
//                    var data = ConvertLawsonToDataTree(lawsonResults[season]);
//                    AddToOutput(DA, season, data);
//                }

//                info = UpdateInfo(lawsonResults.First().Value.Keys.ToList(), _thresholdFrequencies, criteria);
//                RemoveUnusedOutputs(lawsonResults.Keys.ToList());
//            }

//            if (thresholdOutput != null && criteria == 0)
//            {
//                foreach (var key in thresholdOutput.Keys)
//                {
//                    AddToOutput(DA, key, thresholdOutput[key]);
//                }

//                info = UpdateInfo(
//                    thresholdLegend["patch"],
//                    thresholdLegend["threshold"].Select(threshold => new Thresholds.WindThreshold {Field = threshold})
//                        .ToList(),
//                    criteria
//                );
//                RemoveUnusedOutputs(thresholdOutput.Keys.ToList());
//            }

//            if (info != null)
//            {
//                DA.SetDataTree(0, info);
//            }
//            Message = StringCache.getCache(cacheKey + "progress");
//        }

//        private static DataTree<object> ConvertLawsonToDataTree(Dictionary<string, List<int>> data)
//        {
//            var patchCounter = 0;

//            var output = new DataTree<object>();
//            foreach (var patchKey in data.Keys)
//            {
//                var points = data[patchKey];
//                var path = new GH_Path(patchCounter);
//                foreach (var value in points)
//                {
//                    output.Add(value, path);
//                }

//                patchCounter++;
//            }

//            return output;
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

//            // results[threshold][patch][pointIndex][season]
//            // results[season][patch][threshold][pointIndex]

//            foreach (var thresholdKey in resultsToTranspose.Keys)
//            {
//                foreach (var patchKey in resultsToTranspose[thresholdKey].Keys)
//                {
//                    //var pointIndex = 0;
//                    var pointValues = (List<List<double>>) resultsToTranspose[thresholdKey][patchKey];
//                    foreach (var pointValue in pointValues)
//                    {
//                        var seasonCounter = 0;
//                        foreach (var seasonValue in pointValue)
//                        {
//                            var seasonKey = seasons[seasonCounter];

//                            if (!legend["season"].Contains(seasonKey))
//                            {
//                                legend["season"].Add(seasonKey);
//                            }

//                            if (!legend["patch"].Contains(patchKey))
//                            {
//                                legend["patch"].Add(patchKey);
//                            }

//                            if (!legend["threshold"].Contains(thresholdKey))
//                            {
//                                legend["threshold"].Add(thresholdKey);
//                            }

//                            if (!output.ContainsKey(seasonKey))
//                            {
//                                output.Add(seasonKey, new DataTree<object>());
//                            }

//                            var patchCounter = legend["patch"].IndexOf(patchKey);
//                            var thresholdCounter = legend["threshold"].IndexOf(thresholdKey);

//                            var path = new GH_Path(patchCounter, thresholdCounter);
//                            output[seasonKey].Add(seasonValue, path);
//                            seasonCounter++;
//                        }

//                        //pointIndex++;
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

//        private static DataTree<object> UpdateInfo(List<string> patchKeys, List<Thresholds.WindThreshold> thresholds,
//            int criteria)
//        {
//            var info = "Patch Names:\n";
//            var output = new DataTree<object>();

//            if (criteria == 1)
//            {
//                var i = 0;
//                foreach (var key in patchKeys)
//                {
//                    info += $"{{{i}}} is {key}\n";
//                    i++;
//                }

//                info += "\nComfort Categories\n";
//                var j = 0;
//                foreach (var threshold in thresholds)
//                {
//                    info += $"{threshold.Field} is {j}\n";
//                    output.Add(threshold.Field, new GH_Path(1));
//                    j++;
//                }
//            }
//            else
//            {
//                var i = 0;
//                foreach (var key in patchKeys)
//                {
//                    info += $"{{{i};*}} is {key}\n";
//                    i++;
//                }

//                info += "\nThreshold Categories\n";
//                var j = 0;
//                foreach (var threshold in thresholds)
//                {
//                    info += $"{{*;{j}}} is {threshold.Field}\n";
//                    output.Add(threshold.Field, new GH_Path(1));
//                    j++;
//                }
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
//        public override Guid ComponentGuid => new Guid("d2422b3c-e13a-46a4-9700-ec3d53544013");

//        private DataTree<object> info;
//        private Dictionary<string, Dictionary<string, List<int>>> lawsonResults;
//        private List<Thresholds.WindThreshold> _thresholdFrequencies;
//        private Dictionary<string, DataTree<object>> thresholdOutput;
//        private Dictionary<string, List<string>> thresholdLegend;
//    }
//}