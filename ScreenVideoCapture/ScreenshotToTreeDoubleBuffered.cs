using Prefab;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScreenVideoCapture
{
    class ScreenshotToTreeDoubleBuffered
    {


        private static int WIDTH = (int) System.Windows.SystemParameters.VirtualScreenWidth;
        private static int HEIGHT = (int) System.Windows.SystemParameters.VirtualScreenHeight;
        private int[] _currentBuffer = new int[WIDTH * HEIGHT];
        private int[] _previousBuffer = new int[WIDTH * HEIGHT];

        private Bitmap _currentFrame;
        private Bitmap _previousFrame;


        public ScreenshotToTreeDoubleBuffered()
        {
          
        }

        public Tree GetScreenshotAndCreateTree(int width, int height, System.Drawing.Bitmap bitmap,  Dictionary<string, object> tags)
        {

            CopyPixelsFromDrawingBitmap(bitmap, _currentBuffer);
            _currentFrame = Bitmap.FromPixels(bitmap.Width, bitmap.Height, _currentBuffer);

            // Perform diff
            //if (_currentFrame.Pixels.Equals(_previousFrame.Pixels))
            if (_currentFrame.Equals(_previousFrame))
            {
                return null;
            }

            IBoundingBox invalidated = GetDirtyRect(_previousFrame, _currentFrame);
            tags.Add("invalidated", invalidated);

            Tree root = Tree.FromPixels(_currentFrame, tags);

            //Swap the buffers we're writting to
            int[] tmp = _currentBuffer;
            _currentBuffer = _previousBuffer;
            _previousBuffer = tmp;


            //Set the previous bitmap
            _previousFrame = Bitmap.FromPixels(_currentFrame.Width, _currentFrame.Height, _previousBuffer);


            return root;
        }


        public static IBoundingBox GetDirtyRect(Bitmap previous, Bitmap current)
        {

            if (previous == null || previous.Width != current.Width || previous.Height != current.Height)
                return new BoundingBox(0, 0, current.Width, current.Height);

            List<RectData> rects = new List<RectData>();

            Parallel.For<RectData>(0, current.Height,
                () => new RectData(int.MaxValue, int.MaxValue, 0, 0),
                (row, loop, bb) =>
                {
                    for (int col = 0; col < current.Width; col++)
                    {
                        if (current[row, col] != previous[row, col])
                        {
                            if (row < bb.Top)
                                bb.Top = row;
                            if (col < bb.Left)
                                bb.Left = col;
                            if (col > bb.Right)
                                bb.Right = col;
                            if (row > bb.Bottom)
                                bb.Bottom = row;
                        }
                    }
                    return bb;
                },

                (bb) =>
                {
                    lock (((ICollection)rects).SyncRoot)
                        rects.Add(bb);
                }
                );

            IBoundingBox dirty = Union(rects);

            return dirty;
        }


        private class RectData
        {
            public int Left, Top, Right, Bottom;

            public RectData(int left, int top, int right, int bottom)
            {
                Left = left;
                Top = top;
                Right = right;
                Bottom = bottom;
            }
        }

        private static IBoundingBox Union(IEnumerable<RectData> rects)
        {
            int left = int.MaxValue;
            int top = int.MaxValue;
            int bottom = 0;
            int right = 0;
            bool assigned = false;

            foreach (RectData rect in rects)
            {
                if (rect.Top != int.MaxValue)
                {
                    left = Math.Min(left, rect.Left);
                    top = Math.Min(top, rect.Top);
                    bottom = Math.Max(bottom, rect.Bottom);
                    right = Math.Max(right, rect.Right);
                    assigned = true;
                }
            }

            if (assigned)
                return new BoundingBox(left, top, right - left + 1, bottom - top + 1);



            return null;
        }

        public static void CopyPixelsFromDrawingBitmap(System.Drawing.Bitmap source, int[] buffer)
        {
            System.Drawing.Imaging.BitmapData bitmapData = source.LockBits(
                new System.Drawing.Rectangle(0, 0, source.Width, source.Height),
                System.Drawing.Imaging.ImageLockMode.ReadOnly,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb
            );
            System.Runtime.InteropServices.Marshal.Copy(bitmapData.Scan0, buffer, 0, source.Width * source.Height);

            source.UnlockBits(bitmapData);
        }


    }
}
