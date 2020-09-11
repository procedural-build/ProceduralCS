using System;
using System.Collections.Generic;
using Rhino.Geometry;

namespace ComputeCS.Grasshopper.Utils
{
    public static class Domain
    {
        public static BoundingBox getMultiBoundingBox(List<BoundingBox> x)
        {
            Point3d pmin = x[0].Min;
            Point3d pmax = x[0].Max;
            foreach (BoundingBox bb in x)
            {
                if (bb.Min.X < pmin.X)
                {
                    pmin.X = bb.Min.X;
                }
                if (bb.Min.Y < pmin.Y)
                {
                    pmin.Y = bb.Min.Y;
                }
                if (bb.Min.Z < pmin.Z)
                {
                    pmin.Z = bb.Min.Z;
                }
                if (bb.Max.X > pmax.X)
                {
                    pmax.X = bb.Max.X;
                }
                if (bb.Max.Y > pmax.Y)
                {
                    pmax.Y = bb.Max.Y;
                }
                if (bb.Max.Z > pmax.Z)
                {
                    pmax.Z = bb.Max.Z;
                }
            }
            BoundingBox A = new BoundingBox(pmin, pmax);
            return A;
        }

        public static BoundingBox getMultiBoundingBox(List<BoundingBox> x, bool z0)
        {
            BoundingBox A = getMultiBoundingBox(x);
            Point3d pmin = A.Min;
            if (z0 == true)
            {
                pmin.Z = 0;
            }
            A = new BoundingBox(pmin, A.Max);
            return A;
        }

        public static BoundingBox getMultiBoundingBox(List<BoundingBox> x, bool z0, bool centerXY)
        {
            BoundingBox A = getMultiBoundingBox(x, z0);
            if (centerXY)
            {
                double dx = Math.Max(Math.Abs(A.Max.X), Math.Abs(A.Min.X));
                double dy = Math.Max(Math.Abs(A.Max.Y), Math.Abs(A.Min.Y));
                Point3d pmin = new Point3d(-1 * dx, -1 * dy, A.Min.Z);
                Point3d pmax = new Point3d(dx, dy, A.Max.Z);
                A = new BoundingBox(pmin, pmax);
            }
            return A;
        }

        public static Box getMultiBoundingBox(List<BoundingBox> x, double cellSize, bool z0, bool centerXY, double xyScale, double xyOffset, double zScale, bool square)
        {
            Point3d pmin;
            Point3d pmax;
            BoundingBox A = getMultiBoundingBox(x, z0, centerXY);

            if (xyScale > 0)
            {
                A.Transform(Transform.Scale(new Plane(A.Center, Vector3d.ZAxis), xyScale, xyScale, 1.0));
            }

            if (xyOffset > 0)
            {
                pmin = A.Min;
                pmax = A.Max;
                pmin.X = pmin.X - xyOffset;
                pmin.Y = pmin.Y - xyOffset;
                pmax.X = pmax.X + xyOffset;
                pmax.Y = pmax.Y + xyOffset;
                A = new BoundingBox(pmin, pmax);
            }

            if (square)
            {
                double maxl = Math.Max(Math.Abs(A.Max.X - A.Min.X), Math.Abs(A.Max.Y - A.Min.Y));
                pmin = new Point3d(A.Center.X - (maxl / 2), A.Center.Y - (maxl / 2), A.Min.Z);
                pmax = new Point3d(A.Center.X + (maxl / 2), A.Center.Y + (maxl / 2), A.Max.Z);
                A = new BoundingBox(pmin, pmax);
            }

            if (zScale > 0)
            {

                Point3d aflat = new Point3d(A.Center.X, A.Center.Y, z0 ? A.Min.Z : A.Center.Z);
                A.Transform(Transform.Scale(new Plane(aflat, Vector3d.ZAxis), 1, 1, zScale));
            }

            // Do the rounding to the nearest cellSize
            double dx = Math.Abs(A.Max.X - A.Min.X);
            double dy = Math.Abs(A.Max.Y - A.Min.Y);
            double dz = Math.Abs(A.Max.Z - A.Min.Z);
            int nx = (int)(dx / cellSize) + 1;
            int ny = (int)(dy / cellSize) + 1;
            int nz = (int)(dz / cellSize) + 1;
            dx = nx * cellSize;
            dy = ny * cellSize;
            dz = nz * cellSize;
            if (z0)
            {
                pmin = new Point3d(A.Center.X - (dx / 2), A.Center.Y - (dy / 2), A.Min.Z);
                pmax = new Point3d(A.Center.X + (dx / 2), A.Center.Y + (dy / 2), A.Min.Z + dz);
            }
            else
            {
                pmin = new Point3d(A.Center.X - (dx / 2), A.Center.Y - (dy / 2), A.Center.Z - (dz / 2));
                pmax = new Point3d(A.Center.X + (dx / 2), A.Center.Y + (dy / 2), A.Center.Z + (dz / 2));
            }
            A = new BoundingBox(pmin, pmax);

            return new Box(A);
        }

        public static Point3d getLocationInMesh(Box boundingBox)
        {
            double r = (4 + (new Random().NextDouble() - 1)) / 16.0;
            Point3d kPoint = boundingBox.Center;
            Vector3d kPointVect = (new Vector3d(boundingBox.X.Length, boundingBox.Y.Length, boundingBox.Z.Length)) * r;

            Transform t = Transform.ChangeBasis(boundingBox.Plane, Plane.WorldXY);
            kPointVect.Transform(t);
            kPoint += kPointVect;

            return kPoint;
        }

        public static Box Create2DDomain(Box boundingBox, Plane cutPlane, double cellSize)
        {
            // Adjust for the cutPlane
            Point3d[] intersectionPoints;
            Curve[] intersectionCurves;
            var isIntersect = Rhino.Geometry.Intersect.Intersection.BrepPlane(
                boundingBox.ToBrep(),
                cutPlane,
                0,
                out intersectionCurves,
                out intersectionPoints
            );

            if (!isIntersect)
            {
                throw new Exception("Plane must intersect geometry bounding box");
            }

            if (intersectionCurves.Length == 0)
            {
                throw new Exception("No intersection curves found");
            }

            // Get all of the points intersected by the box.
            var points = new List<Point3d>();
            foreach (var curve in intersectionCurves)
            {
                Polyline edges;
                curve.TryGetPolyline(out edges);
                points.AddRange(edges.ToArray());
            }

            return new Box(cutPlane, points)
            {
                Z = new Interval(cutPlane.OriginZ - cellSize / 2, cutPlane.OriginZ + cellSize / 2)
            };

        }
    }
}
