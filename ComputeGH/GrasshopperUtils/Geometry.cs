using System;
using System.Collections.Generic;

using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using Rhino.DocObjects;

namespace ComputeCS.Grasshopper.Utils
{
    public class Geometry
    {
        public static void checkNames(List<ObjRef> objList)
        {
            List<string> names = new List<string>();
            string objName = "";

            for (int i = 0; i < objList.Count; i++)
            {
                objName = objList[i].Object().Attributes.GetUserString("ghODSName");

                // First define a new name (if it doesnt already exist)
                if (objName == "")
                {
                    objName = Guid.NewGuid().ToString();
                }

                // Now check for uniqueness
                if (names.Contains(objList[i].Object().Attributes.GetUserString("ghODSName")))
                {
                    int counter = 1;
                    while (names.Contains(objList[i].Object().Attributes.GetUserString("ghODSName"))) { counter++; };
                    objName = objName + "." + counter.ToString("D3");
                }

                // Set the name of the object
                objList[i].Object().Attributes.SetUserString("ghODSName", objName);
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
            if ((name == "") | name == null) { name = Guid.NewGuid().ToString(); }
            name = name.Replace(" ", "_");
            if (Char.IsDigit(name[0])) { name = "_" + name; }
            return name;
        }

        public static List<ObjRef> getVisibleObjects()
        {
            List<ObjRef> objRefList = new List<ObjRef>();
            Rhino.RhinoDoc doc = Rhino.RhinoDoc.ActiveDoc;
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
        public static ObjRef getObjRef<T>(T ghObj) where T : IGH_GeometricGoo
        {
            return new ObjRef(ghObj.ReferenceID);
        }

        public static List<ObjRef> getObjRef<T>(List<T> ghObjList) where T : IGH_GeometricGoo
        {
            List<ObjRef> objRefList = new List<ObjRef>();
            for (int i = 0; i < ghObjList.Count; i++) { objRefList.Add(getObjRef(ghObjList[i])); }
            return objRefList;
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
            setDocObjectUserString(getObjRef(ghObj), key, value);
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
            return getDocObjectUserString(getObjRef(ghObj), key);
        }

        // Get or Set Overloaded Methods
        public static string getOrSetDocObjectUserString(RhinoObject docObj, string key, string value)
        {
            string v = getDocObjectUserString(docObj, key);
            if (((v == null) | (v == "")) & !(value == null)) { setDocObjectUserString(docObj, key, value); v = value; }
            return v;
        }

        public static string getOrSetDocObjectUserString(ObjRef docObjRef, string key, string value)
        {
            return getOrSetDocObjectUserString(docObjRef.Object(), key, value);
        }

        public static string getOrSetDocObjectUserString<T>(T ghObj, string key, string value) where T : IGH_GeometricGoo
        {
            return getOrSetDocObjectUserString(getObjRef(ghObj), key, value);
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
            if (((v == null) | (v == "")) & !(value == null)) { setGHObjectUserString(ghObj, key, value); v = value; }
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
    }
}
