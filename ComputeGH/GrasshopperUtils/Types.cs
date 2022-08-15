//using System;
//using Grasshopper.Kernel;
//using Grasshopper.Kernel.Types;
//using Rhino;
//using Rhino.Display;
//using Rhino.DocObjects;
//using Rhino.Geometry;

//namespace ComputeGH.Grasshopper.Utils
//{
//    public class Types
//    {
//        public sealed class TextGoo : GH_GeometricGoo<Text3d>, IGH_PreviewData, IGH_BakeAwareData
//        {
//            #region constructors

//            public TextGoo() : this(new Text3d("Existence is pain", Plane.WorldXY, 10))
//            {
//            }

//            public TextGoo(Text3d text)
//            {
//                m_value = text;
//            }

//            private static Text3d DuplicateText3d(Text3d original)
//            {
//                if (original == null) return null;
//                var text = new Text3d(original.Text, original.TextPlane, original.Height)
//                {
//                    Bold = original.Bold,
//                    Italic = original.Italic,
//                    FontFace = original.FontFace
//                };
//                return text;
//            }

//            public override IGH_GeometricGoo DuplicateGeometry()
//            {
//                return new TextGoo(DuplicateText3d(m_value));
//            }

//            #endregion

//            #region properties

//            public override string TypeName => "3D Text";

//            public override string TypeDescription => "3D Text";

//            public override string ToString() => m_value == null ? "<null>" : m_value.Text;


//            public override BoundingBox Boundingbox => m_value?.BoundingBox ?? BoundingBox.Empty;

//            public override BoundingBox GetBoundingBox(Transform xform)
//            {
//                if (m_value == null)
//                    return BoundingBox.Empty;

//                var box = m_value.BoundingBox;
//                var corners = xform.TransformList(box.GetCorners());
//                return new BoundingBox(corners);
//            }

//            #endregion

//            #region methods

//            public override IGH_GeometricGoo Transform(Transform xform)
//            {
//                var text = DuplicateText3d(m_value);
//                if (text == null)
//                    return new TextGoo(null);

//                var plane = text.TextPlane;
//                var point = plane.PointAt(1, 1);

//                plane.Transform(xform);
//                point.Transform(xform);
//                var dd = point.DistanceTo(plane.Origin);

//                text.TextPlane = plane;
//                text.Height *= dd / Math.Sqrt(2);
//                return new TextGoo(text);
//            }

//            public override IGH_GeometricGoo Morph(SpaceMorph xmorph) => DuplicateGeometry();
//            #endregion

//            #region preview

//            BoundingBox IGH_PreviewData.ClippingBox => Boundingbox;

//            void IGH_PreviewData.DrawViewportWires(GH_PreviewWireArgs args)
//            {
//                if (m_value == null)
//                    return;
//                args.Pipeline.Draw3dText(m_value, args.Color);
//            }

//            void IGH_PreviewData.DrawViewportMeshes(GH_PreviewMeshArgs args)
//            {
//                // Do not draw in meshing layer.
//            }

//            #endregion

//            #region baking

//            bool IGH_BakeAwareData.BakeGeometry(RhinoDoc doc, ObjectAttributes att, out Guid id)
//            {
//                id = Guid.Empty;
//                if (m_value == null)
//                    return false;

//                if (att == null)
//                    att = doc.CreateDefaultAttributes();

//                id = doc.Objects.AddText(m_value, att);
//                return true;
//            }

//            #endregion
//        }
//    }
//}