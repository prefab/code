using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Prefab;
using System.Windows;
namespace EdgeDetection
{
    public class RectangleFinder
    {
        private Bitmap[] Bitmaps;
        
        public Bitmap _Bitmap;
        
        private List<Prefab.Point> Points;
        
        private Prefab.Point Start;
        
        private List<Int32Rect> FoundRects;
        
        private int Thresh;


        /// <summary>
        /// The click point that is farthest to the left.
        /// </summary>
        private int FarthestLeftClick;

        /// <summary>
        /// The farthest right click.
        /// </summary>
        private int FarthestRightClick;

        /// <summary>
        /// Minimum Y-Value of click points
        /// </summary>
        private int FarthestUpClick;

        /// <summary>
        /// Max Y-Value of click points
        /// </summary>
        private int FarthestDownClick;

        private int MinX;
        private int MinY;
        private int MaxX;
        private int MaxY;

        /// <summary>
        /// Which portion of the rectangle we are currently trying
        /// to define.
        /// </summary>
        private enum Mode { Up, Right, Down, Left };

        /// <summary>
        /// The possible return values when branching during our search.
        /// </summary>
        private enum ReturnValue { Closed, FailedNormal, FailedDontContinue };



        public static Int32Rect SnapRect(Bitmap bitmap)
        {
            Bitmap grayscale = GrayscaleEdgeDetector.ConvertToGrayscale(bitmap, false);
            Bitmap up = GrayscaleEdgeDetector.GradientUp(grayscale);
            Bitmap right = GrayscaleEdgeDetector.GradientRight(grayscale);
            Bitmap down = GrayscaleEdgeDetector.GradientDown(grayscale);
            Bitmap left = GrayscaleEdgeDetector.GradientLeft(grayscale);

            int width = right.Width - 1;
            int height = up.Height - 1;

            Bitmap[] bmps = new Bitmap[] { up, right, down, left };

            List<Int32Rect> rects = FindRectsAllThreshs(bmps, new List<Prefab.Point>() { new Prefab.Point(bitmap.Width / 2, bitmap.Height / 2) },
                GrayscaleEdgeDetector.CountOfEachGradient(up));

            //double closest = double.PositiveInfinity;
            Int32Rect biggest = Int32Rect.Empty;
            foreach (Int32Rect rect in rects)
            {
                Console.WriteLine(rect.X + ", " + rect.Y + ", " + rect.Width + ", " + rect.Height);

                double area = rect.Width * rect.Height;
                if(area > biggest.Width * biggest.Height)
                {
                    biggest = rect;
                    
                }
                //double dist = Math.Sqrt((rect.X * rect.X) + (rect.Y * rect.Y) + 
                //    (rect.Width - bitmap.Width) * (rect.Width - bitmap.Width) +
                //    (rect.Height - bitmap.Height) * (rect.Height - bitmap.Height));
                //if (dist < closest)
                //{
                //    biggest = rect;
                //    closest = dist;
                //}
            }

            return biggest;
        }

        public static List<Int32Rect> FindRects(Bitmap[] bitmaps, List<Prefab.Point> points)
        {
            RectangleFinder r = new RectangleFinder();
            r.FoundRects = new List<Int32Rect>();
            r.Bitmaps = bitmaps;
            r.Points = points;
            
            r.SetupBoundaryPoints();
            r.Thresh = 0;

            int x, y;
            x = r.FarthestLeftClick - 1;
            y = points[0].Y;

            r.Start = new Prefab.Point(x, y);
            return r.FindRects();
        }

        public static List<Int32Rect> FindRectsAllThreshs(Bitmap[] bitmaps, List<Prefab.Point> points, int[] histogram) {
            RectangleFinder r = new RectangleFinder();
            r.FoundRects = new List<Int32Rect>();
            r.Points = points;
            r.SetupBoundaryPoints();
            List<Int32Rect> foundRects = new List<Int32Rect>();
            for (int i = 0; i < 255; i += 1)
            {
                if (histogram[i] > 0)
                {
                    r.Bitmaps = r.CopyBitmaps(bitmaps);
                    r.Thresh = i;

                    int x, y;
                    x = r.FarthestLeftClick - 1;
                    y = points[0].Y;
                    r.Start = new Prefab.Point(x, y);
                    AddUnique(r.FindRects(), foundRects);
                }
            }
            return foundRects;
        }

        private static void AddUnique(List<Int32Rect> justFound, List<Int32Rect> previouslyFound)
        {
            foreach (Int32Rect r in justFound)
            {
                if (!previouslyFound.Contains(r))
                    previouslyFound.Add(r);
            }
        }

        private Bitmap[] CopyBitmaps(Bitmap[] bitmaps)
        {
            Bitmap[] cpy = new Bitmap[bitmaps.Length];
            for (int i = 0; i < cpy.Length; i++)
                cpy[i] = Prefab.Bitmap.DeepCopy(bitmaps[i]);

            return cpy;
        }

        private List<Int32Rect> FindRects()
        {
            bool closed = false;
            
            int count = 0;
            while (Start.X >= 0)
            {
                ResetPathValues();
                //_Bitmaps = CopyBitmaps(_BitmapsCpy);
                int value = Bitmap(Start, Mode.Left);
                if (value > Thresh)
                    closed = false;

                if (!closed && value <= Thresh)
                {
                    SetPathValue(Start, Mode.Left);
                    ReturnValue path = Run(Start, Mode.Up);
                    if (path == ReturnValue.FailedNormal)
                        closed = false;
                    else if (path == ReturnValue.Closed) {
                        closed = true;
                        Int32Rect rect = Rectangle();
                        ClearRectangle(rect);
                        rect.X += 1;
                       
                        rect.Width -= 1;
                        
                        if(rect.Width > 4 && rect.Height > 4)
                            FoundRects.Add(rect);
                    } else {
                        closed = false;
                        Int32Rect rect = new Int32Rect();
                        ClearRectangle(rect);
                    }
                    
                }
                Start = new Prefab.Point(Start.X - 1, Start.Y);
                count++;
            }

            return FoundRects;
        }

        private void ResetPathValues()
        {
            MinY = int.MaxValue;
            MinX = int.MaxValue;
            MaxX = 0;
            MaxY = 0;
        }

        private Int32Rect Rectangle()
        {
            return new Int32Rect(MinX, MinY, MaxX - MinX + 1, MaxY - MinY + 1);
        }

        private void ClearRectangle(Int32Rect rect)
        {
            for (int row = rect.Y; row < rect.Y + rect.Height; row++)
            {
                SetVisited(row, rect.X, 0);
                SetVisited(row, rect.X + rect.Width - 1, 0);
            }
            for (int col = rect.X; col < rect.X + rect.Width; col++)
            {
                SetVisited(rect.Y, col, 0);
                SetVisited(rect.Y + rect.Height - 1, col, 0);
            }
        }


        private ReturnValue Run(Prefab.Point currNode, Mode mode )
        {
            
            #region escape cases
            //check if an edge or was visited
            if (!BitmapContainsOffset(Bitmaps[0], currNode) || Bitmap(currNode, mode) > Thresh)
                return ReturnValue.FailedNormal;

            //mark visited
            SetVisited(currNode, Thresh + 1);

            //Set a property defining our path
            SetPathValue(currNode, mode);

            //If we're not surrounding the mouse clicks, we fail
            if (!AroundPoints(currNode, mode))
                return ReturnValue.FailedNormal;

            //Check if we closed the rectangle
            if (Done(currNode, mode))
                return ReturnValue.Closed;

            #endregion

            Prefab.Point child = Child(currNode, mode);
            ReturnValue value = Run(child, NextMode(mode));
            if (value == ReturnValue.Closed)
                return value;
            else if (value == ReturnValue.FailedDontContinue)
                return value;
            else if (value == ReturnValue.FailedNormal && mode == Mode.Left && currNode.X == Start.X && currNode.Y > Start.Y)
                return ReturnValue.FailedDontContinue;
            


            return Run(Sibling(currNode, mode), mode);
        }

        private bool BitmapContainsOffset(Bitmap bmp, Prefab.Point location) {
            return location.Y < bmp.Height && location.X < bmp.Width && location.X >= 0 &&
                location.Y >= 0;
        }

        private void SetPathValue(Prefab.Point currNode, Mode mode)
        {
            switch (mode)
            {
                case Mode.Up:
                    if (MinY > currNode.Y)
                        MinY = currNode.Y;
                    break;
                case Mode.Right:
                    if (MaxX < currNode.X)
                        MaxX = currNode.X;
                    break;
                case Mode.Down:
                    if (MaxY < currNode.Y)
                        MaxY = currNode.Y;
                    break;
                case Mode.Left:
                    if (MinX > currNode.X)
                        MinX = currNode.X;
                    break;
            }
        }

        private Mode NextMode(Mode mode)
        {
            switch (mode)
            {
                case Mode.Up:
                    return Mode.Right;
                case Mode.Right:
                    return Mode.Down;
                case Mode.Down:
                    return Mode.Left;
                default:
                    return Mode.Up;
            }
            
        }

        private bool AroundPoints(Prefab.Point currNode, Mode mode)
        {
            switch (mode)
            {
                case Mode.Up:
                    return currNode.X <= Start.X;
                case Mode.Right:
                    return currNode.Y < FarthestUpClick;
                case Mode.Down:
                    return currNode.X > FarthestRightClick;
                case Mode.Left:
                    return currNode.Y > FarthestDownClick;
            }
            return false;
        }

        private List<Prefab.Point> NexTurn(Prefab.Point turn, List<Prefab.Point> turns)
        {
            List<Prefab.Point> cpy = new List<Prefab.Point>();
            cpy.AddRange(turns);
            cpy.Add(turn);
            return cpy;
        }

        private int Bitmap(Prefab.Point currNode, Mode mode)
        {
            return Bitmap(currNode.Y, currNode.X, mode);
        }

        private int Bitmap(int row, int col, Mode mode)
        {
            Bitmap bw = null;
            switch (mode) {
                case Mode.Up:
                    bw = Bitmaps[2];
                    break;
                case Mode.Down:
                    bw = Bitmaps[0];
                    break;
                case Mode.Right:
                    bw = Bitmaps[3];
                    break;
                case Mode.Left:
                    bw = Bitmaps[1];
                    break;
            }
            return bw[ row, col];
        }

        private Prefab.Point Child(Prefab.Point currNode, Mode currMode)
        {
            switch (currMode)
            {
                case Mode.Up:
                    return new Prefab.Point(currNode.X + 1, currNode.Y);
                case Mode.Right:
                    return new Prefab.Point(currNode.X, currNode.Y + 1);
                case Mode.Down:
                    return new Prefab.Point(currNode.X - 1, currNode.Y);
                case Mode.Left:
                    return new Prefab.Point(currNode.X, currNode.Y - 1);

            }
            throw new Exception("Invalid number of turns");
        }

        private Prefab.Point Sibling(Prefab.Point currNode, Mode currMode)
        {
            switch (currMode)
            {
                case Mode.Up:
                    return new Prefab.Point(currNode.X, currNode.Y - 1);
                    
                case Mode.Right:
                    return new Prefab.Point(currNode.X + 1, currNode.Y);
                    
                case Mode.Down:
                    return new Prefab.Point(currNode.X, currNode.Y + 1);
                    
                case Mode.Left:
                    return new Prefab.Point(currNode.X - 1, currNode.Y);
                    
            }
            return currNode;
            throw new Exception("Invalid mode");
        }

        private bool Done(Prefab.Point currNode, Mode mode)
        {
            if (currNode.X == Start.X
                && currNode.Y - 1 == Start.Y)
                return true;

            return false;
        }



        private void SetVisited(int row, int col, int value)
        {
            for (int i = 0; i < Bitmaps.Length; i++)
                Bitmaps[i][ row, col] = value;
        }

        private void SetVisited(Prefab.Point offset, int value)
        {
            SetVisited(offset.Y, offset.X, value);
        }

        /// <summary>
        /// Records the minimum and maximum
        /// x and y values of the click points.
        /// </summary>
        private void SetupBoundaryPoints()
        {
            List<int> xs = new List<int>();
            List<int> ys = new List<int>();
            foreach (Prefab.Point p in Points)
            {
                xs.Add(p.X);
                ys.Add(p.Y);
            }

            int[] xarr = xs.ToArray();
            int[] yarr = ys.ToArray();

            Array.Sort(xarr);
            Array.Sort(yarr);

            FarthestLeftClick = xarr[0];
            FarthestRightClick = xarr[xarr.Length - 1];
            FarthestUpClick = yarr[0];
            FarthestDownClick = yarr[yarr.Length - 1];
        }

       
    }
}
