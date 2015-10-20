using Prefab;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SavedVideoInterpreter
{
    /// <summary>
    /// Interaction logic for BubbleCursorOverlay.xaml
    /// </summary>
    public partial class BubbleCursorOverlay : UserControl
    {

        public static DependencyProperty MouseLeftProperty = DependencyProperty.Register("TargetLeft", typeof(int), typeof(BubbleCursorOverlay));
        public static DependencyProperty MouseTopProperty = DependencyProperty.Register("TargetTop", typeof(int), typeof(BubbleCursorOverlay));
        public static DependencyProperty TargetWidthProperty = DependencyProperty.Register("TargetWidth", typeof(int), typeof(BubbleCursorOverlay));
        public static DependencyProperty TargetHeightProperty = DependencyProperty.Register("TargetHeight", typeof(int), typeof(BubbleCursorOverlay));
        public static DependencyProperty GeometryProperty = DependencyProperty.Register("BubbleCursorGeometry", typeof(PathGeometry), typeof(BubbleCursorOverlay));


        public PathGeometry BubbleCursorGeometry
        {
            get
            {
                return (PathGeometry)GetValue(GeometryProperty);
            }

            set
            {
                SetValue(GeometryProperty, value);
            }
        }

        public int TargetLeft
        {
            get { return (int)GetValue(MouseLeftProperty); }
            set { SetValue(MouseLeftProperty, value); }
        }

        public int TargetTop
        {
            get { return (int)GetValue(MouseTopProperty); }
            set { SetValue(MouseTopProperty, value); }
        }

        public int TargetWidth
        {
            get { return (int)GetValue(TargetWidthProperty); }
            set { SetValue(TargetWidthProperty, value); }
        }

        public int TargetHeight
        {
            get { return (int)GetValue(TargetHeightProperty); }
            set { SetValue(TargetHeightProperty, value); }
        }


        public BubbleCursorOverlay()
        {
            InitializeComponent();
            DataContext = this;
        }
        
        public static Tree GetClosestTarget(int left, int top, Tree tree)
        {
            double dist = double.MaxValue;
            return GetClosestHelper(left, top, tree, out dist);
        }

        
        
        private static Tree GetClosestHelper(int left, int top, Tree node, out double closestDistance)
        {
            double currBestDist = double.MaxValue;
            Tree closestTarget = null;
            if(node.HasTag("is_target") && node["is_target"].ToString().ToLower().Equals("true") 
                && (!node.HasTag("is_minor") || node["is_minor"].ToString().ToLower().Equals("false") || Keyboard.IsKeyDown(Key.LeftCtrl) ) )
            {
                currBestDist = DistanceBetweenPointAndRectangle(left, top, node);
                closestTarget = node;
            }

            

            double bestChildDist = double.MaxValue;
            Tree closestChild = null;

            foreach (Tree child in node.GetChildren())
            {
                double childDist = double.MaxValue;
                Tree childCandidate = GetClosestHelper(left, top, child, out childDist);
                if (childCandidate != null && childDist < bestChildDist)
                {
                    bestChildDist = childDist;
                    closestChild = childCandidate;
                }
            }

            if (bestChildDist < currBestDist)
            {
                closestTarget = closestChild;
                currBestDist = bestChildDist;
            }

            closestDistance = currBestDist;

            return closestTarget;
        }




        /// <summary>
        /// Returns the distance from a point to a rectangle.
        /// </summary>
        /// <param name="p"></param>
        /// <param name="bb"></param>
        /// <returns></returns>
        public static double DistanceBetweenPointAndRectangle(int x, int y, IBoundingBox bb)
        {
            Prefab.Point closest = ClosestPointOnRectangleToPoint(x, y, bb);
           
            return DistanceBetweenTwoPoints(x, y, closest.X, closest.Y);
        }

        /// <summary>
        /// Returns closest point within a rectangle to the given point.
        /// </summary>
        /// <param name="p"></param>
        /// <param name="bb"></param>
        /// <returns></returns>
        public static Prefab.Point ClosestPointOnRectangleToPoint(int px, int py, IBoundingBox r)
        {
            int x, y;
            if (px < r.Left)
                x = r.Left;
            else if (px > r.Left + r.Width)
                x = r.Left + r.Width;
            else
                x = px;

            if (py < r.Top)
                y = r.Top;
            else if (py > r.Top + r.Height)
                y = r.Top + r.Height;
            else
                y = py;

            return new Prefab.Point(x, y);
        }

        public static double DistanceBetweenTwoPoints(double x1, double y1, double x2, double y2)
        {
            double deltx = x2 - x1;
            double delty = y2 - y1;
            return (double)Math.Sqrt(deltx * deltx + delty * delty);
        }
    }
}
