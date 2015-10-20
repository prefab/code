using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows;
using Prefab;

namespace SavedVideoInterpreter
{
    public static class BubbleCursorVisualizer
    {

        public static PathGeometry GetBubbleCursorPathFigure(IBoundingBox closestOccurrence, double cursorleft, double cursortop)
        {
            if (closestOccurrence == null)
                return new PathGeometry();

            List<System.Windows.Point> pointlist = new List<System.Windows.Point>();

            System.Windows.Point p1 = new System.Windows.Point(closestOccurrence.Left, closestOccurrence.Top);
            System.Windows.Point p2 = new System.Windows.Point(closestOccurrence.Left + closestOccurrence.Width, closestOccurrence.Top);
            System.Windows.Point p3 = new System.Windows.Point(closestOccurrence.Left + closestOccurrence.Width, closestOccurrence.Top + closestOccurrence.Height);
            System.Windows.Point p4 = new System.Windows.Point(closestOccurrence.Left, closestOccurrence.Top + closestOccurrence.Height);

            pointlist.Add(p1);
            pointlist.Add(p2);
            pointlist.Add(p3);
            pointlist.Add(p4);

            PathGeometry path = new PathGeometry();
            PathSegmentCollection collection = BubbleCursorVisualizer.PointsAroundWidget(pointlist);

            //Box around widget
            PathFigure figure = new PathFigure();
            figure.StartPoint = p1;
            figure.Segments = collection;
            figure.IsClosed = false;
            path.Figures.Add(figure);
            //----------------

            //Tractor beam
            if (!BoundingBox.Contains(closestOccurrence, (int)cursorleft, (int)cursortop))
            {
                PathFigure tractorbeam = new PathFigure();
                tractorbeam.StartPoint = new System.Windows.Point(cursorleft, cursortop);
                tractorbeam.Segments = BubbleCursorVisualizer.PointsForTractorBeam(tractorbeam.StartPoint, pointlist);
                tractorbeam.IsClosed = true;
                path.Figures.Add(tractorbeam);
            }
            //------------------

            path.FillRule = FillRule.EvenOdd;

            return path;
        }

        private static PathSegmentCollection PointsAroundWidget(List<System.Windows.Point> pointlist)
        {
            PathSegmentCollection collection = new PathSegmentCollection();

            int index = 0;
            collection.Add(new ArcSegment(pointlist[index], new System.Windows.Size(0, 0), 0, true, SweepDirection.Clockwise, false));


            index = (index + 1) % pointlist.Count;
            collection.Add(new ArcSegment(pointlist[index], new System.Windows.Size(0, 0), 0, true, SweepDirection.Clockwise, false));


            index = (index + 1) % pointlist.Count;
            collection.Add(new ArcSegment(pointlist[index], new System.Windows.Size(0, 0), 0, true, SweepDirection.Clockwise, false));


            index = (index + 1) % pointlist.Count;
            collection.Add(new ArcSegment(pointlist[index], new System.Windows.Size(0, 0), 0, true, SweepDirection.Clockwise, false));

            return collection;
        }

        private static PathSegmentCollection PointsForTractorBeam(System.Windows.Point cursorLocation, List<System.Windows.Point> pointlist)
        {
            //Get the path that will be used to render the tractor beam.
            PathSegmentCollection collection = new PathSegmentCollection();

            //We will use the points that create the biggest angle for the tractor beam coming from the cursor.
            System.Windows.Point[] startAndEnd = PointsThatMakeBiggestAngle(cursorLocation, pointlist);

            //Make sure they are sorted by x value (the tractor beam is rendered clockwise).
            if (startAndEnd[0].X < startAndEnd[1].X)
            {
                System.Windows.Point tmp = startAndEnd[0];
                startAndEnd[0] = startAndEnd[1];
                startAndEnd[1] = tmp;
            }

            //Get the index of the start System.Windows.Point in the list of points around the widget and add it to the path.
            int index = pointlist.IndexOf(startAndEnd[0]);
            collection.Add(new ArcSegment(pointlist[index], new System.Windows.Size(0, 0), 0, true, SweepDirection.Clockwise, false));


            //Get the index of the end System.Windows.Point in the list of points around the widget.
            int endIndex = pointlist.IndexOf(startAndEnd[1]);
            
            
            //Now add the System.Windows.Point that is closest to the cursor if it's not the starting or ending point.
            //This creates a wedge at the end of the tractor beam.
            System.Windows.Point closest = ClosestToCursor(cursorLocation, pointlist);
            if (closest != startAndEnd[0] && closest != startAndEnd[1])
                collection.Add(new ArcSegment(pointlist[pointlist.IndexOf(closest)], new System.Windows.Size(0, 0), 0, true, SweepDirection.Clockwise, false));
            
            
            //Now add the end System.Windows.Point to the path.
            collection.Add(new ArcSegment(pointlist[endIndex], new System.Windows.Size(0, 0), 0, true, SweepDirection.Clockwise, false));

            
            return collection;

        }

        private static System.Windows.Point[] PointsThatMakeBiggestAngle(System.Windows.Point mousepos, List<System.Windows.Point> pointlist)
        {
            List<Vector> vectors = Vectors(mousepos, pointlist);

            Vector[] biggest = Angle(new Comparison<double>(BiggerAngle), vectors);

            return PointsFromVector(biggest, pointlist);
        }

        private static System.Windows.Point ClosestToCursor(System.Windows.Point cursorLocation, List<System.Windows.Point> pointlist)
        {
            double mindist = double.MaxValue;
            System.Windows.Point closest = pointlist[0];

            foreach (System.Windows.Point p in pointlist)
            {
                double dist = BoundingBox.DistanceBetweenTwoPoints(cursorLocation.X, cursorLocation.Y, p.X, p.Y);
                if (dist < mindist)
                {
                    mindist = dist;
                    closest = p;
                }
            }

            return closest;
        }

        private static int SmallerComparison(double d1, double d2)
        {
            return (int)(d1 - d2);
        }

        private static System.Windows.Point[] PointsFromVector(Vector[] vectors, List<System.Windows.Point> pointlist)
        {
            List<System.Windows.Point> points = new List<System.Windows.Point>();
            foreach (System.Windows.Point p in pointlist)
            {
                if (points.Count == 0 && (p == vectors[0].P2 || p == vectors[1].P2))
                    points.Add(p);
                else if (points.Count == 1 && (p == vectors[0].P2 || p == vectors[1].P2))
                {
                    points.Add(p);
                    break;
                }
            }

            return points.ToArray();
        }

        private static Vector[] Angle(Comparison<double> angleComparer, List<Vector> vectors)
        {
            double bestAngle = 0;
            Vector[] arr = null;
            for (int i = 0; i < vectors.Count; i++)
            {
                for (int j = i + 1; j < vectors.Count; j++)
                {

                    if (arr == null)
                    {
                        arr = new Vector[2];
                        arr[0] = vectors[i];
                        arr[1] = vectors[j];
                        bestAngle = Vector.AngleBetween(arr[0], arr[1]);
                    }
                    else
                    {
                        double angle = Vector.AngleBetween(vectors[i], vectors[j]);
                        if (angleComparer(angle, bestAngle) > 0)
                        {
                            arr[0] = vectors[i];
                            arr[1] = vectors[j];
                            bestAngle = angle;
                        }
                    }
                }
            }
            return arr;
        }

        private static int BiggerAngle(double angle1, double angle2)
        {
            if (double.IsNaN(angle1) && double.IsNaN(angle2))
                return 0;
            else if (double.IsNaN(angle1))
                return -1;
            else if (double.IsNaN(angle2))
                return 1;


            return (int)Math.Sign(angle1 - angle2);
        }

        private static int SmallerAngle(double angle1, double angle2)
        {
            return BiggerAngle(angle2, angle1);
        }

        private static List<Vector> Vectors(System.Windows.Point mousePos, List<System.Windows.Point> points)
        {
            List<Vector> vectors = new List<Vector>();
            for (int i = 0; i < points.Count; i++)
            {
                Vector v = new Vector(mousePos, points[i]);
                vectors.Add(v);
            }

            return vectors;
        }

        private struct Vector
        {
            public double X;
            public double Y;
            public System.Windows.Point P1;
            public System.Windows.Point P2;

            public Vector(System.Windows.Point p1, System.Windows.Point p2)
            {
                X = p1.X - p2.X;
                Y = p1.Y - p2.Y;

                P1 = p1;
                P2 = p2;
            }

            public Vector(double x, double y)
            {
                X = x;
                Y = y;
                P1 = new System.Windows.Point(0, 0);
                P2 = new System.Windows.Point(X, Y);
            }

            public static Vector Normalized(Vector v)
            {
                double magnitude = Math.Sqrt(v.X * v.X + v.Y * v.Y);
                return new Vector(v.X / magnitude, v.Y / magnitude);
            }


            public static double Dot(Vector v1, Vector v2)
            {
                return v1.X * v2.X + v1.Y * v2.Y;
            }

            public static double AngleBetween(Vector v1, Vector v2)
            {
                Vector normalizedV1 = Normalized(v1);
                Vector normalizedV2 = Normalized(v2);

                return Math.Acos(Dot(normalizedV1, normalizedV2));
            }
        }
    }
}
