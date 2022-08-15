//using System;
//using System.Collections.Generic;
//using ComputeCS.types;
//using Grasshopper.Kernel.Types;
//using Rhino.Geometry;

//namespace ComputeGH.Grasshopper.Utils
//{
//    public static class Domain
//    {
//        public static BoundingBox GetMultiBoundingBox(List<BoundingBox> boundingBoxes)
//        {
//            var pointMin = boundingBoxes[0].Min;
//            var pointMax = boundingBoxes[0].Max;
//            foreach (var bb in boundingBoxes)
//            {
//                if (bb.Min.X < pointMin.X)
//                {
//                    pointMin.X = bb.Min.X;
//                }

//                if (bb.Min.Y < pointMin.Y)
//                {
//                    pointMin.Y = bb.Min.Y;
//                }

//                if (bb.Min.Z < pointMin.Z)
//                {
//                    pointMin.Z = bb.Min.Z;
//                }

//                if (bb.Max.X > pointMax.X)
//                {
//                    pointMax.X = bb.Max.X;
//                }

//                if (bb.Max.Y > pointMax.Y)
//                {
//                    pointMax.Y = bb.Max.Y;
//                }

//                if (bb.Max.Z > pointMax.Z)
//                {
//                    pointMax.Z = bb.Max.Z;
//                }
//            }

//            return new BoundingBox(pointMin, pointMax);
//        }

//        public static BoundingBox GetMultiBoundingBox(List<BoundingBox> boundingBoxes, bool z0)
//        {
//            var multiBoundingBox = GetMultiBoundingBox(boundingBoxes);
//            var pointMin = multiBoundingBox.Min;
//            if (z0)
//            {
//                pointMin.Z = 0;
//            }

//            multiBoundingBox = new BoundingBox(pointMin, multiBoundingBox.Max);
//            return multiBoundingBox;
//        }

//        public static BoundingBox GetMultiBoundingBox(List<BoundingBox> boundingBoxes, bool z0, bool centerXY)
//        {
//            var multiBoundingBox = GetMultiBoundingBox(boundingBoxes, z0);

//            if (!centerXY) return multiBoundingBox;

//            var domainX = Math.Max(Math.Abs(multiBoundingBox.Max.X), Math.Abs(multiBoundingBox.Min.X));
//            var domainY = Math.Max(Math.Abs(multiBoundingBox.Max.Y), Math.Abs(multiBoundingBox.Min.Y));
//            var pointMin = new Point3d(-1 * domainX, -1 * domainY, multiBoundingBox.Min.Z);
//            var pointMax = new Point3d(domainX, domainY, multiBoundingBox.Max.Z);
//            multiBoundingBox = new BoundingBox(pointMin, pointMax);

//            return multiBoundingBox;
//        }

//        public static BoundingBox GetMultiBoundingBox(List<BoundingBox> x, double cellSize, bool z0, bool centerXY,
//            double xyScale, double xyOffset, double zScale, bool square)
//        {
//            Point3d pmin;
//            Point3d pmax;
//            var multiBoundingBox = GetMultiBoundingBox(x, z0, centerXY);

//            if (xyScale > 0)
//            {
//                multiBoundingBox.Transform(Transform.Scale(new Plane(multiBoundingBox.Center, Vector3d.ZAxis), xyScale,
//                    xyScale, 1.0));
//            }

//            if (xyOffset > 0)
//            {
//                pmin = multiBoundingBox.Min;
//                pmax = multiBoundingBox.Max;
//                pmin.X -= xyOffset;
//                pmin.Y -= xyOffset;
//                pmax.X += xyOffset;
//                pmax.Y += xyOffset;
//                multiBoundingBox = new BoundingBox(pmin, pmax);
//            }

//            if (square)
//            {
//                var maxl = Math.Max(Math.Abs(multiBoundingBox.Max.X - multiBoundingBox.Min.X),
//                    Math.Abs(multiBoundingBox.Max.Y - multiBoundingBox.Min.Y));
//                pmin = new Point3d(multiBoundingBox.Center.X - (maxl / 2), multiBoundingBox.Center.Y - (maxl / 2),
//                    multiBoundingBox.Min.Z);
//                pmax = new Point3d(multiBoundingBox.Center.X + (maxl / 2), multiBoundingBox.Center.Y + (maxl / 2),
//                    multiBoundingBox.Max.Z);
//                multiBoundingBox = new BoundingBox(pmin, pmax);
//            }

//            if (zScale > 0)
//            {
//                var boundingBoxCenter = new Point3d(multiBoundingBox.Center.X, multiBoundingBox.Center.Y,
//                    z0 ? multiBoundingBox.Min.Z : multiBoundingBox.Center.Z);
//                multiBoundingBox.Transform(Transform.Scale(new Plane(boundingBoxCenter, Vector3d.ZAxis), 1, 1, zScale));
//            }

//            // Do the rounding to the nearest cellSize
//            var domainSizeX = Math.Abs(multiBoundingBox.Max.X - multiBoundingBox.Min.X);
//            var domainSizeY = Math.Abs(multiBoundingBox.Max.Y - multiBoundingBox.Min.Y);
//            var domainSizeZ = Math.Abs(multiBoundingBox.Max.Z - multiBoundingBox.Min.Z);
//            var nx = (int) (domainSizeX / cellSize) + 1;
//            var ny = (int) (domainSizeY / cellSize) + 1;
//            var nz = (int) (domainSizeZ / cellSize) + 1;
//            domainSizeX = nx * cellSize;
//            domainSizeY = ny * cellSize;
//            domainSizeZ = nz * cellSize;
//            if (z0)
//            {
//                pmin = new Point3d(multiBoundingBox.Center.X - (domainSizeX / 2),
//                    multiBoundingBox.Center.Y - (domainSizeY / 2), multiBoundingBox.Min.Z);
//                pmax = new Point3d(multiBoundingBox.Center.X + (domainSizeX / 2),
//                    multiBoundingBox.Center.Y + (domainSizeY / 2), multiBoundingBox.Min.Z + domainSizeZ);
//            }
//            else
//            {
//                pmin = new Point3d(multiBoundingBox.Center.X - (domainSizeX / 2),
//                    multiBoundingBox.Center.Y - (domainSizeY / 2), multiBoundingBox.Center.Z - (domainSizeZ / 2));
//                pmax = new Point3d(multiBoundingBox.Center.X + (domainSizeX / 2),
//                    multiBoundingBox.Center.Y + (domainSizeY / 2), multiBoundingBox.Center.Z + (domainSizeZ / 2));
//            }

//            multiBoundingBox = new BoundingBox(pmin, pmax);

//            return multiBoundingBox;
//        }

//        public static Point3d GetLocationInMesh(Box boundingBox)
//        {
//            double r = (4 + (new Random().NextDouble() - 1)) / 16.0;
//            Point3d kPoint = boundingBox.Center;
//            Vector3d kPointVect = (new Vector3d(boundingBox.X.Length, boundingBox.Y.Length, boundingBox.Z.Length)) * r;

//            Transform t = Transform.ChangeBasis(boundingBox.Plane, Plane.WorldXY);
//            kPointVect.Transform(t);
//            kPoint += kPointVect;

//            return kPoint;
//        }

//        public static BoundingBox Create2DDomain(BoundingBox boundingBox, Plane cutPlane, double cellSize)
//        {
//            // Adjust for the cutPlane
//            Point3d[] intersectionPoints;
//            Curve[] intersectionCurves;
//            var isIntersect = Rhino.Geometry.Intersect.Intersection.BrepPlane(
//                boundingBox.ToBrep(),
//                cutPlane,
//                0,
//                out intersectionCurves,
//                out intersectionPoints
//            );

//            if (!isIntersect)
//            {
//                throw new Exception("Plane must intersect geometry bounding box");
//            }

//            if (intersectionCurves.Length == 0)
//            {
//                throw new Exception("No intersection curves found");
//            }

//            // Get all of the points intersected by the box.
//            var points = new List<Point3d>();
//            foreach (var curve in intersectionCurves)
//            {
//                Polyline edges;
//                curve.TryGetPolyline(out edges);
//                points.AddRange(edges.ToArray());
//            }

//            var box = new Box(cutPlane, points)
//            {
//                Z = new Interval(cutPlane.OriginZ - cellSize / 2, cutPlane.OriginZ + cellSize / 2)
//            };

//            return box.BoundingBox;
//        }

//        /// <summary>
//        /// This estimation is based on an equation provided by Alexander Jacobson
//        /// </summary>
//        public static double EstimateCellCount(List<IGH_GeometricGoo> geometry, double cellSize, double xyScale,
//            double zScale)
//        {
//            var boundingBox = new BoundingBox();
//            var refinementRegionCount = 0.0;
//            var surfaceAreaCount = 0.0;
//            foreach (var geo in geometry)
//            {
//                boundingBox = BoundingBox.Union(boundingBox, geo.Boundingbox);
//                refinementRegionCount += EstimateRefinementCells(geo, cellSize);
//                surfaceAreaCount += EstimateSurfaceAreaCells(geo, cellSize);
//            }

//            var box = new Box(boundingBox);
//            var hexMeshCount = (box.X.Length * box.Y.Length * box.Z.Length * Math.Pow(xyScale, 2) * zScale) /
//                               Math.Pow(cellSize, 3);
//            return hexMeshCount + refinementRegionCount + surfaceAreaCount;
//        }

//        public static double EstimateRefinementCells(IGH_GeometricGoo geo, double baseCellSize)
//        {
//            var regionName = Geometry.getUserString(geo, "ComputeName");
//            var refinementDetails = Geometry.getUserString(geo, "ComputeRefinementRegion");
//            if (string.IsNullOrEmpty(regionName) || string.IsNullOrEmpty(refinementDetails))
//            {
//                return 0.0;
//            }

//            var details = new RefinementDetails().FromJson(refinementDetails);
//            details.CellSize = baseCellSize;
//            var cellSize = baseCellSize / Math.Pow(2, int.Parse(details.Levels.Split(' ')[1]));
//            var mesh = new Mesh();
//            geo.CastTo(out mesh);

//            return mesh.Volume() / Math.Pow(cellSize, 3);
//        }

//        public static double EstimateSurfaceAreaCells(IGH_GeometricGoo geo, double baseCellSize)
//        {
//            var surfaceName = Geometry.getUserString(geo, "ComputeName");
//            var levels = Geometry.getUserString(geo, "ComputeMeshLevels");

//            if (string.IsNullOrEmpty(surfaceName) || string.IsNullOrEmpty(levels))
//            {
//                return 0.0;
//            }

//            var meshLevel = new MeshLevelDetails().FromJson(levels);
//            meshLevel.CellSize = baseCellSize;
//            var cellSize = baseCellSize / Math.Pow(2, meshLevel.Level.Min);
//            var mesh = new Mesh();
//            geo.CastTo(out mesh);
//            var area = AreaMassProperties.Compute(mesh).Area;

//            return (area / Math.Pow(cellSize, 2)) * 4;
//        }
//    }
//}