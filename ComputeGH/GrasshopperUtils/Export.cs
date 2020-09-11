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
        public static byte[] STLObject(List<GH_Mesh> surfaceMeshes)
        {
            UnicodeEncoding uniEncoding = new UnicodeEncoding();
            MemoryStream stream = new MemoryStream();
            using (var memStream = new StreamWriter(stream))
            {
                foreach (GH_Mesh ghMesh in surfaceMeshes)
                {
                    Mesh mesh = ghMesh.Value;
                    string name = Geometry.getUserString(ghMesh, "ComputeName");

                    mesh.Faces.ConvertQuadsToTriangles();
                    mesh.FaceNormals.ComputeFaceNormals();
                    mesh.FaceNormals.UnitizeFaceNormals();

                    MeshFace face;
                    int[] verts = new int[3] {0, 0, 0};
                    memStream.Write($"solid {name}\n");

                    for (int f = 0; f < mesh.Faces.Count; f++)
                    {
                        face = mesh.Faces[f];
                        verts[0] = face.A;
                        verts[1] = face.B;
                        verts[2] = face.C;

                        memStream.Write(
                            $"facet normal {mesh.FaceNormals[f].X} {mesh.FaceNormals[f].Y} {mesh.FaceNormals[f].Z}\n");

                        memStream.Write(" outer loop\n");
                        foreach (int v in verts)
                        {
                            memStream.Write(
                                $"  vertex {mesh.Vertices[v].X} {mesh.Vertices[v].Y} {mesh.Vertices[v].Z}\n");
                        }

                        memStream.Write(" endloop\n");

                        memStream.Write("endfacet\n");
                    }

                    memStream.Write("endsolid\n");
                }
            }

            return stream.ToArray();
        }

        public static List<Dictionary<string, byte[]>> RefinementRegionsToSTL(List<GH_Mesh> surfaceMeshes)
        {
            var stls = new List<Dictionary<string, byte[]>>();

            foreach (var mesh in surfaceMeshes)
            {
                var regionName = Geometry.getUserString(mesh, "ComputeName");
                var refinementDetails = Geometry.getUserString(mesh, "ComputeRefinementRegion");
                if (regionName == null || refinementDetails == null)
                {
                    continue;
                }

                stls.Add(
                    new Dictionary<string, byte[]> {{regionName, STLObject(new List<GH_Mesh>() {mesh})}}
                );
            }

            return stls;
        }
    }
}