using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using ComputeCS.Grasshopper.Utils;

namespace ComputeCS.Grasshopper.Utils
{
    public static class Export
    {
        public static MemoryStream STLObject(List<GH_Mesh> surfaceMeshes)
        {
            UnicodeEncoding uniEncoding = new UnicodeEncoding();
            MemoryStream stream = new MemoryStream();
            using (MemoryStream memStream = stream)
            {
                foreach (GH_Mesh ghMesh in surfaceMeshes)
                {
                    Mesh mesh = ghMesh.Value;
                    string name = Geometry.getUserString(ghMesh, "ComputeName");

                    mesh.Faces.ConvertQuadsToTriangles();
                    mesh.FaceNormals.ComputeFaceNormals();
                    mesh.FaceNormals.UnitizeFaceNormals();

                    MeshFace face;
                    int[] verts = new int[3] { 0, 0, 0 };
                    byte[] name_ = uniEncoding.GetBytes($"solid {name}\n");
                    memStream.Write(name_, (int)memStream.Length, name_.Length);

                    for (int f = 0; f < mesh.Faces.Count; f++)
                    {
                        face = mesh.Faces[f];
                        verts[0] = face.A;
                        verts[1] = face.B;
                        verts[2] = face.C;

                        byte[] face_ = uniEncoding.GetBytes($"facet normal {mesh.FaceNormals[f].X} {mesh.FaceNormals[f].Y} {mesh.FaceNormals[f].Z}\n");
                        memStream.Write(face_, (int)memStream.Length, face_.Length);

                        byte[] outLoop = uniEncoding.GetBytes(" outer loop\n");
                        memStream.Write(outLoop, (int)memStream.Length, outLoop.Length);
                        foreach (int v in verts)
                        {
                            byte[] vertex_ = uniEncoding.GetBytes($"  vertex {mesh.Vertices[v].X} {mesh.Vertices[v].Y} {mesh.Vertices[v].Z}\n");
                            memStream.Write(vertex_, (int)memStream.Length, vertex_.Length);
                        }
                        byte[] endLoop = uniEncoding.GetBytes(" endloop\n");
                        memStream.Write(endLoop, (int)memStream.Length, endLoop.Length);

                        byte[] endFace = uniEncoding.GetBytes("endfacet\n");
                        memStream.Write(endFace, (int)memStream.Length, endFace.Length);
                    }

                    byte[] endSolid = uniEncoding.GetBytes("endsolid\n");
                    memStream.Write(endSolid, (int)memStream.Length, endSolid.Length);
                }
            }

            return stream;
        }
    }
}
