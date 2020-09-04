using System;
using System.Collections.Generic;
using ComputeGH.Properties;
using Rhino.Geometry;
using Grasshopper3D = Grasshopper;
using Grasshopper.Kernel;
using System.Drawing;
using System.Linq;
using Grasshopper.Kernel.Types;

namespace ComputeCS.Grasshopper
{
    public class GHRecolorMesh : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the GHRecolorMesh class.
        /// </summary>
        public GHRecolorMesh()
          : base("Recolor Mesh", "Recolor Mesh",
              "Recoloring mesh with parallel threading",
              "Compute", "Utils")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "Mesh", "Mesh to recolor", GH_ParamAccess.item);
            pManager.AddNumberParameter("Result", "Result", "Result with the same number of faces as mesh", GH_ParamAccess.list);
            pManager.AddColourParameter("List Of Colors", "Colors", "List of colours", GH_ParamAccess.list);
            pManager.AddNumberParameter("Lower Boundary", "LowBound", "Optional lower bound for the coloring. Default is 0.0", GH_ParamAccess.item);
            pManager.AddNumberParameter("Upper Boundary", "UpperBound", "Optional upper bound for the coloring. Default is the max value of Result.", GH_ParamAccess.item);
            
            pManager[3].Optional = true;
            pManager[4].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddMeshParameter("Colored Mesh", "Colored Mesh", "Recoloured mesh based on result and colors", GH_ParamAccess.item);
            pManager.AddColourParameter("Colors", "Colors", "Colors use in the mesh. Can be used to create a legend with.", GH_ParamAccess.list);
            pManager.AddNumberParameter("Values", "Values", "Values that be be use the create a legend with", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //1.0 Collecting data

            Mesh mesh = new Mesh();
            List<double> result = new List<double>();
            List<Color> coloraslist = new List<Color>();

            double ming = 0.0;
            double maxg = 1.0;
            var legendText = string.Empty;

            //1.1 Return conditions
            if ((!DA.GetData(0, ref mesh)))
                return;

            if ((!DA.GetDataList(1, result)))
                return;

            if (!DA.GetDataList(2, coloraslist))
                return;

            DA.GetData(3, ref ming);

            if (!DA.GetData(4, ref maxg)) { maxg = result.Max(); }
               
            //2.0 Setting up the run;

            if (mesh.Faces.Count() != result.Count()) { AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "unequal faces and result"); return; }

            List<Mesh> meshall = new List<Mesh>();
            Mesh ms = new Mesh();


            var numbers = Enumerable.Range(0, mesh.Faces.Count());
            var _result = numbers.AsParallel().AsOrdered();

            int fA = 0;
            var gradients = Gradients(coloraslist.ToArray(), ming, maxg);
            foreach (var i in _result)
            {
                var cf = gradients.ColourAt(result[i]);
                MeshFace face = mesh.Faces[i];

                ms.Vertices.Add(mesh.Vertices[face.A]);
                ms.Vertices.Add(mesh.Vertices[face.B]);
                ms.Vertices.Add(mesh.Vertices[face.C]);

                ms.VertexColors.Add(cf);
                ms.VertexColors.Add(cf);
                ms.VertexColors.Add(cf);

                if (face.IsQuad)
                {
                    ms.Vertices.Add(mesh.Vertices[face.D]);
                    ms.VertexColors.Add(cf);
                    ms.Faces.AddFace(fA, fA + 1, fA + 2, fA + +3);
                    fA = fA + 4;

                }
                else
                {
                    ms.Faces.AddFace(fA, fA + 1, fA + 2);
                    fA = fA + 3;
                }
            }
            DA.SetData(0, ms);
            GenerateLegendValues(DA, gradients, maxg, ming);
            
        }

        // X. Extra additional useful functions
        // X.01 Gradient maker 
        private Grasshopper3D.GUI.Gradient.GH_Gradient Gradients(Color[] colorarray, double t0, double t1)
        {
            Grasshopper3D.GUI.Gradient.GH_Gradient gradient2 = new Grasshopper3D.GUI.Gradient.GH_Gradient();
            for (int i = 0; i < colorarray.Count(); i++)
            {
                double grip = t0 + (double)i * (t1 - t0) / (colorarray.Count() - 1); //fix 10.10.17 add t0, to rescale the gradient grip
                gradient2.AddGrip(grip, colorarray[i]);
            }
            return gradient2;
        }


        private void GenerateLegendValues(
            IGH_DataAccess DA,
            Grasshopper3D.GUI.Gradient.GH_Gradient gradient,
            double max, 
            double min
            )
        {
            if (max <= min)
            {
                return;
            } 
            var colors = new List<Color>();
            var values = new List<double>();
            var stepSize = (max - min) / 10;

            for (var i = min; i < max; i += stepSize)
            {
                colors.Add(gradient.ColourAt(i));
                values.Add(i);
            }

            colors.Reverse();
            values.Reverse();
            
            DA.SetDataList(1, colors);
            DA.SetDataList(2, values);
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
            get { return new Guid("77e98319-4f23-4035-ba2c-abc5ff7c9524"); }
        }
    }
}