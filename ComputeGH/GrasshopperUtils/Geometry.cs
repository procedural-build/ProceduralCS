using Grasshopper;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.Geometry;
using Rhino.Geometry.Intersect;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;

namespace ComputeGH.Grasshopper.Utils
{
    public class Geometry
    {
        public static void checkNames(List<ObjRef> objList)
        {
            List<string> names = new List<string>();
            string objName = "";

            for (int i = 0; i < objList.Count; i++)
            {
                objName = objList[i].Object().Attributes.GetUserString("ComputeName");

                // First define a new name (if it doesnt already exist)
                if (objName == "")
                {
                    objName = Guid.NewGuid().ToString();
                }

                // Now check for uniqueness
                if (names.Contains(objList[i].Object().Attributes.GetUserString("ComputeName")))
                {
                    int counter = 1;
                    while (names.Contains(objList[i].Object().Attributes.GetUserString("ComputeName")))
                    {
                        counter++;
                    }

                    ;
                    objName = objName + "." + counter.ToString("D3");
                }

                // Set the name of the object
                objList[i].Object().Attributes.SetUserString("ComputeName", objName);
            }
        }

        public static List<double[]> pntsToArrays(List<Point3d> points)
        {
            List<double[]> pntList = new List<double[]>(points.Count);
            foreach (Point3d p in points)
            {
                pntList.Add(new double[3] { p.X, p.Y, p.Z });
            }

            return pntList;
        }

        public static bool checkName(string name)
        {
            return (name == fixName(name));
        }

        public static string fixName(string name)
        {
            if ((name == "") | name == null)
            {
                name = Guid.NewGuid().ToString();
            }

            name = name.Replace(" ", "_");
            if (Char.IsDigit(name[0]))
            {
                name = "_" + name;
            }

            return name;
        }

        public static List<ObjRef> getVisibleObjects()
        {
            List<ObjRef> objRefList = new List<ObjRef>();
            RhinoDoc doc = RhinoDoc.ActiveDoc;
            ObjectEnumeratorSettings settings = new ObjectEnumeratorSettings();
            settings.VisibleFilter = true;
            settings.HiddenObjects = false;

            foreach (RhinoObject rhObj in doc.Objects.GetObjectList(settings))
            {
                objRefList.Add(new ObjRef(rhObj));
            }

            return objRefList;
        }

        // getObjRef Overloaded Methods (GH_Brep and GH_Mesh)
        public static ObjRef GetObjRef<T>(T ghObj) where T : IGH_GeometricGoo
        {
            return new ObjRef(ghObj.ReferenceID);
        }

        public static List<ObjRef> GetObjRef<T>(List<T> ghObjList) where T : IGH_GeometricGoo
        {
            return ghObjList.Select(GetObjRef).ToList();
        }

        public static List<string> GetObjRefStrings<T>(List<T> ghObjList) where T : IGH_GeometricGoo
        {
            return ghObjList.Select(refId => GetObjRef(refId).ObjectId.ToString()).ToList();
        }

        // Set Overloaded Methods
        public static void setDocObjectUserString(RhinoObject docObj, string key, string value)
        {
            docObj.Attributes.SetUserString(key, value);
        }

        public static void setDocObjectUserString(ObjRef docObjRef, string key, string value)
        {
            setDocObjectUserString(docObjRef.Object(), key, value);
        }

        public static void setDocObjectUserString<T>(T ghObj, string key, string value) where T : IGH_GeometricGoo
        {
            setDocObjectUserString(GetObjRef(ghObj), key, value);
        }

        // Get Overloaded Methods
        public static string getDocObjectUserString(RhinoObject docObj, string key)
        {
            return docObj.Attributes.GetUserString(key);
        }

        public static string getDocObjectUserString(ObjRef docObjRef, string key)
        {
            return getDocObjectUserString(docObjRef.Object(), key);
        }

        public static string getDocObjectUserString<T>(T ghObj, string key) where T : IGH_GeometricGoo
        {
            return getDocObjectUserString(GetObjRef(ghObj), key);
        }

        // Get or Set Overloaded Methods
        public static string getOrSetDocObjectUserString(RhinoObject docObj, string key, string value)
        {
            string v = getDocObjectUserString(docObj, key);
            if (((v == null) | (v == "")) & !(value == null))
            {
                setDocObjectUserString(docObj, key, value);
                v = value;
            }

            return v;
        }

        public static string getOrSetDocObjectUserString(ObjRef docObjRef, string key, string value)
        {
            return getOrSetDocObjectUserString(docObjRef.Object(), key, value);
        }

        public static string getOrSetDocObjectUserString<T>(T ghObj, string key, string value)
            where T : IGH_GeometricGoo
        {
            return getOrSetDocObjectUserString(GetObjRef(ghObj), key, value);
        }

        // Get Methods for Grasshopper Objects (GH_Brep & GH_Mesh)
        public static string getGHObjectUserString<T>(T ghObj, string key) where T : IGH_GeometricGoo
        {
            if (ghObj.TypeName == "Brep")
            {
                Brep b;
                bool success = ghObj.CastTo(out b);
                return b.GetUserString(key);
            }
            else if (ghObj.TypeName == "Mesh")
            {
                Mesh m;
                bool success = ghObj.CastTo(out m);
                return m.GetUserString(key);
            }
            else if (ghObj.TypeName == "Curve")
            {
                Curve c;
                bool success = ghObj.CastTo(out c);
                return c.GetUserString(key);
            }
            else
            {
                throw new Exception("Expected Brep, Mesh or Curve object");
            }
        }

        // Set Methods for Grasshopper Objects (GH_Brep & GH_Mesh)
        public static void setGHObjectUserString<T>(T ghObj, string key, string value) where T : IGH_GeometricGoo
        {
            bool success;
            if (ghObj.TypeName == "Brep")
            {
                Brep b;
                success = ghObj.CastTo(out b);
                b.SetUserString(key, value);
            }
            else if (ghObj.TypeName == "Mesh")
            {
                Mesh m;
                success = ghObj.CastTo(out m);
                m.SetUserString(key, value);
            }
            else
            {
                throw new Exception("Expected GH_Brep or GH_Mesh object");
            }
        }

        // Get or Set Methods for Grasshopper Objects (GH_Brep & GH_Mesh)
        public static string getOrSetGHObjectUserString<T>(T ghObj, string key, string value) where T : IGH_GeometricGoo
        {
            string v = getGHObjectUserString(ghObj, key);
            if (((v == null) | (v == "")) & !(value == null))
            {
                setGHObjectUserString(ghObj, key, value);
                v = value;
            }

            return v;
        }

        // Generic Set User String Methods 
        // GH_Brep objects get and set the user string from its DocObject 
        // GH_Mesh objects get and set the user string from the Value.Attributes
        public static void setUserString<T>(T ghObj, string key, string value) where T : IGH_GeometricGoo
        {
            if (ghObj.IsReferencedGeometry)
            {
                setDocObjectUserString(ghObj, key, value);
            }
            else
            {
                setGHObjectUserString(ghObj, key, value);
            }
        }

        // Generic Get User String Methods 
        // GH_Brep objects get and set the user string from its DocObject 
        // GH_Mesh objects get and set the user string from the Value.Attributes
        public static string getUserString<T>(T ghObj, string key) where T : IGH_GeometricGoo
        {
            if (ghObj.IsReferencedGeometry)
            {
                return getDocObjectUserString(ghObj, key);
            }
            else
            {
                return getGHObjectUserString(ghObj, key);
            }
        }

        // Generic Get or Set User String Methods 
        // GH_Brep objects get and set the user string from its DocObject 
        // GH_Mesh objects get and set the user string from the Value.Attributes
        public static string getOrSetUserString<T>(T ghObj, string key, string value) where T : IGH_GeometricGoo
        {
            if (ghObj.IsReferencedGeometry)
            {
                return getOrSetDocObjectUserString(ghObj, key, value);
            }
            else
            {
                return getOrSetGHObjectUserString(ghObj, key, value);
            }
        }

        public static Dictionary<string, DataTree<object>> CreateAnalysisMesh(
            List<Surface> baseSurfaces,
            double gridSize,
            List<Brep> excludeGeometry,
            double offset,
            string offsetDirection)
        {
            var analysisMesh = new DataTree<object>();
            var faceCenters = new DataTree<object>();
            var faceNormals = new DataTree<object>();
            var index = 0;
            var doneEvents = new ManualResetEvent[baseSurfaces.Count];
            var callBacks = new List<ThreadedCreateAnalysisMesh>();
            foreach (var surface in baseSurfaces)
            {
                doneEvents[index] = new ManualResetEvent(false);
                var callBack = new ThreadedCreateAnalysisMesh
                {
                    gridSize = gridSize,
                    excludeGeometry = excludeGeometry,
                    offset = offset,
                    offsetDirection = offsetDirection,
                    doneEvent = doneEvents[index]
                };
                ThreadPool.QueueUserWorkItem(callBack.ThreadPoolCallback);
                callBacks.Add(callBack);
                index++;
            }

            WaitHandle.WaitAll(doneEvents);

            index = 0;
            foreach (var callBack in callBacks)
            {
                var mesh = callBack.analysisMesh;
                var path = new GH_Path(index);
                analysisMesh.Add(mesh, path);

                foreach (var normal in mesh.FaceNormals)
                {
                    faceNormals.Add(normal, path);
                }

                for (var i = 0; i < mesh.Faces.Count(); i++)
                {
                    faceCenters.Add(mesh.Faces.GetFaceCenter(i), path);
                }

                index++;
            }
            return new Dictionary<string, DataTree<object>>
            {
                {"analysisMesh", analysisMesh},
                {"faceCenters", faceCenters},
                {"faceNormals", faceNormals},
            };
        }

        public static Dictionary<string, DataTree<object>> CreateAnalysisMesh(
            List<Brep> baseSurfaces,
            double gridSize,
            List<Brep> excludeGeometry,
            double offset,
            string offsetDirection)
        {
            var analysisMesh = new DataTree<object>();
            var faceCenters = new DataTree<object>();
            var faceNormals = new DataTree<object>();
            var index = 0;
            var doneEvents = new ManualResetEvent[baseSurfaces.Count];
            var callBacks = new List<ThreadedCreateAnalysisMesh>();
            foreach (var surface in baseSurfaces)
            {
                doneEvents[index] = new ManualResetEvent(false);
                var callBack = new ThreadedCreateAnalysisMesh
                {
                    surface = surface,
                    gridSize = gridSize,
                    excludeGeometry = excludeGeometry,
                    offset = offset,
                    offsetDirection = offsetDirection,
                    doneEvent = doneEvents[index]
                };
                ThreadPool.QueueUserWorkItem(callBack.ThreadPoolCallback);
                callBacks.Add(callBack);
                index++;
            }

            WaitHandle.WaitAll(doneEvents);

            index = 0;
            foreach (var callBack in callBacks)
            {
                var mesh = callBack.analysisMesh;
                var path = new GH_Path(index);
                analysisMesh.Add(mesh, path);

                foreach (var normal in mesh.FaceNormals)
                {
                    faceNormals.Add(normal, path);
                }

                for (var i = 0; i < mesh.Faces.Count(); i++)
                {
                    faceCenters.Add(mesh.Faces.GetFaceCenter(i), path);
                }

                index++;
            }

            return new Dictionary<string, DataTree<object>>
            {
                {"analysisMesh", analysisMesh},
                {"faceCenters", faceCenters},
                {"faceNormals", faceNormals},
            };
        }

        private static Mesh CreateMeshFromSurface(Brep surface, double gridSize)
        {
            var meshParams = MeshingParameters.DefaultAnalysisMesh;
            meshParams.MaximumEdgeLength = gridSize * 1.2;
            meshParams.MinimumEdgeLength = gridSize * 0.8;

            try
            {
                return Mesh.CreateFromBrep(surface, meshParams).First();
            }
            catch
            {
                throw new Exception("Error in converting Brep to Mesh");
            }
        }

        private static Brep SubtractBrep(Surface surface, List<Brep> excludeGeometry)
        {
            var brepSurface = Brep.CreateFromSurface(surface);
            return _SubtractBrep(brepSurface, excludeGeometry);
        }

        private static Brep SubtractBrep(Brep brepSurface, List<Brep> excludeGeometry)
        {
            return _SubtractBrep(brepSurface, excludeGeometry);
        }

        private static Brep _SubtractBrep(Brep brepSurface, List<Brep> excludeGeometry)
        {
            const double tolerance = 0.1;
            foreach (var brep in excludeGeometry)
            {
                var intersectionCurves = new Curve[] { };
                var intersectionPoints = new Point3d[] { };
                var isIntersecting = Intersection.BrepBrep(brep, brepSurface, tolerance, out intersectionCurves,
                    out intersectionPoints);
                if (isIntersecting && intersectionCurves.Length > 0)
                {
                    var splitFaces = brepSurface.Split(intersectionCurves, tolerance);

                    try
                    {
                        brepSurface = splitFaces.Where(face => face != null).First(face =>
                            !brep.IsPointInside(AreaMassProperties.Compute(face).Centroid, tolerance, false));
                    }
                    catch (InvalidOperationException)
                    {
                        foreach (var face in splitFaces)
                        {
                            if (face.Vertices.Select(v => v.Location)
                                .Any(vertex => !brep.IsPointInside(vertex, tolerance, false)))
                            {
                                brepSurface = face;
                            }
                        }
                    }
                }
            }

            return brepSurface;
        }

        private static Brep MoveSurface(Brep surface, double offset, string offsetDirection)
        {
            return _MoveSurface(surface, offset, offsetDirection);
        }

        private static Brep MoveSurface(Surface surface, double offset, string offsetDirection)
        {
            var _surface = Brep.CreateFromSurface(surface);
            return _MoveSurface(_surface, offset, offsetDirection);
        }

        private static Brep _MoveSurface(Brep surface, double offset, string offsetDirection)
        {
            var vector = new Vector3d();
            if (offsetDirection == "x")
            {
                vector.X = offset;
                vector.Y = 0;
                vector.Z = 0;
            }
            else if (offsetDirection == "y")
            {
                vector.Y = offset;
                vector.X = 0;
                vector.Z = 0;
            }
            else if (offsetDirection == "z")
            {
                vector.Z = offset;
                vector.Y = 0;
                vector.X = 0;
            }
            else if (offsetDirection == "normal")
            {
                vector = surface.Faces.FirstOrDefault().NormalAt(0.5, 0.5);
            }

            surface.Translate(vector);
            return surface;
        }

        public static Dictionary<string, DataTree<object>> CreatedMeshFromSlicedDomain(
            Box domain,
            double location,
            double gridSize,
            List<Brep> excludeGeometry,
            string sliceDirection)
        {
            var normal = new Vector3d();
            var origin = new Point3d();
            var width = new Interval();
            var height = new Interval();
            if (sliceDirection == "x")
            {
                normal.X = 1;
                origin.X = location;
                width = domain.Y;
                height = domain.Z;
            }
            else if (sliceDirection == "y")
            {
                normal.Y = 1;
                origin.Y = location;
                width = domain.Z;
                height = domain.X;
            }
            else
            {
                normal.Z = 1;
                origin.Z = location;
                width = domain.X;
                height = domain.Y;
            }

            var cutPlane = new Plane(origin, normal);
            var rectangle = new Rectangle3d(cutPlane, width, height);
            var surface = Brep.CreatePlanarBreps(rectangle.ToNurbsCurve(), 0.1).ToList();
            return CreateAnalysisMesh(surface, gridSize, excludeGeometry, 0.0, "z");
        }

        public static Tuple<Mesh, List<Text3d>> CreateLegend(Point3d basePoint, List<Color> colors,
            List<string> values, double scale, double textHeight)
        {
            if (values.Count != colors.Count)
            {
                throw new Exception(
                    $"Length of values: {values.Count} has to be the same as length of colors: {colors.Count}");
            }

            var legend = new Mesh();
            const int sideLength = 1;
            var text = new List<Text3d>();

            for (var i = 0; i < values.Count; i++)
            {
                var vertexCount = i * 4;
                var color = colors[i];
                var newBase = new Point3d
                {
                    X = basePoint.X,
                    Y = basePoint.Y + sideLength * i * scale,
                    Z = basePoint.Z,
                };
                legend.Vertices.Add(newBase.X, newBase.Y, newBase.Z);
                legend.Vertices.Add(newBase.X + sideLength * scale, newBase.Y, newBase.Z);
                legend.Vertices.Add(newBase.X + sideLength * scale, newBase.Y + sideLength * scale, newBase.Z);
                legend.Vertices.Add(newBase.X, newBase.Y + sideLength * scale, newBase.Z);

                legend.VertexColors.Add(color);
                legend.VertexColors.Add(color);
                legend.VertexColors.Add(color);
                legend.VertexColors.Add(color);
                legend.Faces.AddFace(vertexCount, vertexCount + 1, vertexCount + 2, vertexCount + 3);
                text.Add(new Text3d
                (
                    values[i],
                    new Plane(
                        new Point3d(newBase.X + sideLength * scale * 1.1, newBase.Y + sideLength * 0.5,
                            newBase.Z), new Vector3d(0.0, 0.0, 1.0)),
                    textHeight
                ));
            }

            return new Tuple<Mesh, List<Text3d>>(legend, text);
        }


        public static List<List<List<double>>> ConvertPointsToList(GH_Structure<GH_Point> pointTree)
        {
            return pointTree.Branches.Select(branch => branch.Select(point => new List<double> { point.Value.X, point.Value.Y, point.Value.Z }).ToList()).ToList();
        }

        public static List<List<List<double>>> ConvertPointsToList(GH_Structure<GH_Vector> pointTree)
        {
            return pointTree.Branches.Select(branch => branch.Select(point => new List<double> { point.Value.X, point.Value.Y, point.Value.Z }).ToList()).ToList();
        }

        public class ThreadedCreateAnalysisMesh
        {
            public Brep surface;
            public double gridSize;
            public List<Brep> excludeGeometry;
            public double offset;
            public string offsetDirection;
            public ManualResetEvent doneEvent;
            public Mesh analysisMesh = new Mesh();
            public void ThreadPoolCallback(Object threadContext)
            {
                var _surface = MoveSurface(surface, offset, offsetDirection);
                _surface = SubtractBrep(_surface, excludeGeometry);
                analysisMesh = CreateMeshFromSurface(_surface, gridSize);
                analysisMesh.RebuildNormals();

                doneEvent.Set();
            }
        }
    }


}