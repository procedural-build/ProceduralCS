using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace ComputeGH.Grasshopper.Utils
{
    public static class Export
    {
        public static byte[] STLObject
            (
            List<GH_Mesh> surfaceMeshes,
            bool WriteRefinementRegions = false
            )
        {
            var uniEncoding = new UnicodeEncoding();
            var stream = new MemoryStream();
            using (var memStream = new StreamWriter(stream))
            {
                foreach (var ghMesh in surfaceMeshes)
                {
                    var mesh = ghMesh.Value;
                    var name = Geometry.getUserString(ghMesh, "ComputeName");
                    var refinementDetails = Geometry.getUserString(ghMesh, "ComputeRefinementRegion");
                    if (refinementDetails != null && WriteRefinementRegions == false)
                    {
                        continue;
                    }

                    mesh.Faces.ConvertQuadsToTriangles();
                    mesh.FaceNormals.ComputeFaceNormals();
                    mesh.FaceNormals.UnitizeFaceNormals();

                    MeshFace face;
                    var verts = new int[3] { 0, 0, 0 };
                    memStream.Write($"solid {name}\n");

                    for (int f = 0; f < mesh.Faces.Count; f++)
                    {
                        face = mesh.Faces[f];
                        verts[0] = face.A;
                        verts[1] = face.B;
                        verts[2] = face.C;

                        var iv = CultureInfo.InvariantCulture;
                        memStream.Write(
                            $"facet normal {mesh.FaceNormals[f].X.ToString(iv)} {mesh.FaceNormals[f].Y.ToString(iv)} {mesh.FaceNormals[f].Z.ToString(iv)}\n");

                        memStream.Write(" outer loop\n");
                        foreach (int v in verts)
                        {
                            memStream.Write(
                                $"  vertex {mesh.Vertices[v].X.ToString(iv)} {mesh.Vertices[v].Y.ToString(iv)} {mesh.Vertices[v].Z.ToString(iv)}\n");
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
                    new Dictionary<string, byte[]> { { regionName, STLObject(new List<GH_Mesh>() { mesh }, true) } }
                );
            }

            return stls;
        }

        public static Dictionary<string, byte[]> MeshToObj(GH_Structure<GH_Mesh> meshes, List<string> names)
        {
            var objs = new Dictionary<string, byte[]>();
            var i = 0;
            foreach (var _meshes in meshes.Branches)
            {
                objs.Add(names[i], ObjObject(_meshes));
                i++;
            }

            return objs;
        }

        public static byte[] ObjObject(List<GH_Mesh> meshes)
        {
            var uniEncoding = new UnicodeEncoding();
            var stream = new MemoryStream();
            using (var memStream = new StreamWriter(stream))
            {
                foreach (var mesh in meshes)
                {
                    foreach (var vertex in mesh.Value.Vertices)
                    {
                        memStream.WriteLine($"v {vertex.X} {vertex.Y} {vertex.Z}");
                    }
                    memStream.WriteLine("");

                    foreach (var normal in mesh.Value.Normals)
                    {
                        memStream.WriteLine($"vn {normal.X} {normal.Y} {normal.Z}");
                    }

                    memStream.WriteLine("");

                    foreach (var face in mesh.Value.Faces)
                    {
                        var vertA = face.A + 1;
                        var vertB = face.B + 1;
                        var vertC = face.C + 1;
                        if (face.IsQuad)
                        {
                            var vertD = face.D + 1;
                            memStream.WriteLine($"f {vertA}/{vertA}/{vertA} {vertB}/{vertB}/{vertB} {vertC}/{vertC}/{vertC} {vertD}/{vertD}/{vertD}");
                        }
                        else
                        {
                            memStream.WriteLine($"f {vertA}/{vertA}/{vertA} {vertB}/{vertB}/{vertB} {vertC}/{vertC}/{vertC}");
                        }
                    }
                }
            }

            return stream.ToArray();
        }

        public static void MeshToObjFile(List<GH_Mesh> meshes, string filePath)
        {
            var objBytes = ObjObject(meshes);
            File.WriteAllBytes(filePath, objBytes);
        }
    }
}