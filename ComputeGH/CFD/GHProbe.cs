//using System;
//using System.Collections.Generic;
//using System.Drawing;
//using System.Linq;
//using System.Threading;
//using ComputeCS.Components;
//using ComputeCS.Exceptions;
//using ComputeCS.utils.Cache;
//using ComputeCS.utils.Queue;
//using ComputeGH.Grasshopper.Utils;
//using ComputeGH.Properties;
//using Grasshopper.Kernel;
//using Grasshopper.Kernel.Data;
//using Grasshopper.Kernel.Types;

//namespace ComputeCS.Grasshopper
//{
//    public class GHProbe : PB_Component
//    {
//        /// <summary>
//        /// Initializes a new instance of the GHProbe class.
//        /// </summary>
//        public GHProbe()
//            : base("Probe Points", "Probe Points",
//                "Probe the CFD case to get the results in the desired points",
//                "Compute", "CFD")
//        {
//        }

//        /// <summary>
//        /// Registers all the input parameters for this component.
//        /// </summary>
//        protected override void RegisterInputParams(GH_InputParamManager pManager)
//        {
//            pManager.AddTextParameter("Input", "Input", "Input from previous Compute Component", GH_ParamAccess.item);
//            pManager.AddPointParameter("Points", "Points", "Points to create the sample sets from.",
//                GH_ParamAccess.tree);
//            pManager.AddTextParameter("Names", "Names",
//                "Give names to each branch of in the points tree. The names can later be used to identify the points.",
//                GH_ParamAccess.list);
//            pManager.AddTextParameter("Fields", "Fields", "Choose which fields to probe. Default is U",
//                GH_ParamAccess.list);
//            pManager.AddMeshParameter("Mesh", "Mesh",
//                "Input your analysis mesh here, if you wish to visualize your results in the browser",
//                GH_ParamAccess.tree);
//            pManager.AddIntegerParameter("CPUs", "CPUs",
//                "CPUs to use. Valid choices are:\n1, 2, 4, 8, 16, 18, 24, 36, 48, 64, 72, 96. \nIn most cases it is not advised to use more CPUs than 1, as the time it takes to decompose and reconstruct the case will exceed the speed-up gained by multiprocessing the probing.",
//                GH_ParamAccess.item, 1);
//            pManager.AddTextParameter("DependentOn", "DependentOn",
//                "By default the probe task is dependent on a wind tunnel task or a task running simpleFoam. If you want it to be dependent on another task. Please supply the name of that task here.",
//                GH_ParamAccess.item, "VirtualWindTunnel");
//            pManager.AddTextParameter("Case Directory", "Case Dir",
//                "Folder to probe on the Compute server. Default is VWT", GH_ParamAccess.item);
//            pManager.AddTextParameter("Overrides", "Overrides",
//                "Takes overrides in JSON format: \n" +
//                "{\n\t\"FIELD\": \"VALUE\", ...\n}\n" +
//                "Fields you can overwrite:\n" +
//                "libs\n" +
//                "executeControl\n" +
//                "writeControl\n" +
//                "interpolationScheme\n" +
//                "setFormat",
//                GH_ParamAccess.item);
//            pManager.AddBooleanParameter("Create", "Create",
//                "Whether to create a new probe task, if one doesn't exist. If the Probe task already exists, then this component will create a new task config, that will run after the previous config is finished.",
//                GH_ParamAccess.item, false);

//            pManager[2].Optional = true;
//            pManager[3].Optional = true;
//            pManager[4].Optional = true;
//            pManager[5].Optional = true;
//            pManager[6].Optional = true;
//            pManager[7].Optional = true;
//            pManager[8].Optional = true;
//            pManager[9].Optional = true;
//        }

//        /// <summary>
//        /// Registers all the output parameters for this component.
//        /// </summary>
//        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
//        {
//            pManager.AddTextParameter("Output", "Output", "Output", GH_ParamAccess.item);
//        }

//        /// <summary>
//        /// This is the method that actually does the work.
//        /// </summary>
//        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
//        protected override void SolveInstance(IGH_DataAccess DA)
//        {
//            string inputJson = null;
//            var points = new GH_Structure<GH_Point>();
//            var names = new List<string>();
//            var fields = new List<string>();
//            var mesh = new GH_Structure<GH_Mesh>();
//            var cpus = 1;
//            var dependentOn = "VirtualWindTunnel";
//            var caseDir = "VWT";
//            var overrides = "";
//            var create = false;

//            if (!DA.GetData(0, ref inputJson)) return;
//            if (!DA.GetDataTree(1, out points)) return;
//            if (!DA.GetDataList(2, names))
//            {
//                for (var i = 0; i < points.Branches.Count; i++)
//                {
//                    names.Add($"set{i.ToString()}");
//                }
//            }
//            else
//            {
//                foreach (var name in names)
//                {
//                    ValidateName(name);
//                }
//            }

//            if (!DA.GetDataList(3, fields))
//            {
//                fields.Add("U");
//            }

//            DA.GetDataTree(4, out mesh);
//            DA.GetData(5, ref cpus);
//            DA.GetData(6, ref dependentOn);
//            DA.GetData(7, ref caseDir);
//            DA.GetData(8, ref overrides);
//            DA.GetData(9, ref create);

//            var convertedPoints = Geometry.ConvertPointsToList(points);
//            caseDir = caseDir.TrimEnd('/');

//            // Get Cache to see if we already did this
//            var cacheKey = inputJson + string.Join("", points) + string.Join("", fields) + string.Join("", names);
//            var cachedValues = StringCache.getCache(cacheKey);
//            DA.DisableGapLogic();

//            if (cachedValues == null || create)
//            {
//                var queueName = "probe";

//                // Get queue lock
//                var queueLock = StringCache.getCache(queueName);
//                if (queueLock != "true")
//                {
//                    StringCache.setCache(queueName, "true");
//                    StringCache.setCache(cacheKey, null);
//                    QueueManager.addToQueue(queueName, () =>
//                    {
//                        try
//                        {
//                            var meshFile = Export.MeshToObj(mesh, names);
//                            var results = Probe.ProbePoints(
//                                inputJson,
//                                convertedPoints,
//                                fields,
//                                names,
//                                ComponentUtils.ValidateCPUs(cpus),
//                                meshFile,
//                                dependentOn,
//                                caseDir,
//                                overrides,
//                                create
//                            );
//                            cachedValues = results;
//                            StringCache.setCache(cacheKey, cachedValues);
//                            if (create)
//                            {
//                                StringCache.setCache(cacheKey + "create", "true");
//                            }
//                        }
//                        catch (NoObjectFoundException)
//                        {
//                            StringCache.setCache(cacheKey + "create", "");
//                        }
//                        catch (Exception e)
//                        {
//                            StringCache.setCache(InstanceGuid.ToString(), e.Message);
//                            StringCache.setCache(cacheKey, "error");
//                            StringCache.setCache(cacheKey + "create", "");
//                        }


//                        ExpireSolutionThreadSafe(true);
//                        Thread.Sleep(2000);
//                        StringCache.setCache(queueName, "");
//                    });
//                }
//            }

//            HandleErrors();
//            Message = "";

//            if (cachedValues != null)
//            {
//                DA.SetData(0, cachedValues);

//                if (StringCache.getCache(cacheKey + "create") == "true")
//                {
//                    Message = "Task Created";
//                }
//            }
//        }

//        private void ValidateName(string name)
//        {
//            var illegalCharacters = new List<string> {"?", "&", "/", "%", "#", "!", "+", " "};
//            if (illegalCharacters.Any(name.Contains))
//            {
//                AddRuntimeMessage(GH_RuntimeMessageLevel.Error,
//                    $"{name} contains illegal characters. " +
//                    $"A name cannot include any on the following characters: {string.Join(", ", illegalCharacters)}"
//                );
//            }

//            if (int.TryParse(name, out var _))
//            {
//                AddRuntimeMessage(GH_RuntimeMessageLevel.Error,
//                    $"{name} contains illegal characters. A name cannot be a number"
//                );
//            }
//        }

//        /// <summary>
//        /// Provides an Icon for the component.
//        /// </summary>
//        protected override Bitmap Icon => Resources.IconMesh;

//        /// <summary>
//        /// Gets the unique ID for this component. Do not change this ID after release.
//        /// </summary>
//        public override Guid ComponentGuid => new Guid("2f0bdda2-f7eb-4fc7-8bf0-a2fd5a787493");
//    }
//}