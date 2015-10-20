using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AviFile;
using System.Threading;
using System.ComponentModel;
using System.Collections;
using Prefab;
using EdgeDetection;
using System.Windows.Media.Imaging;
using System.Windows;
using System.Threading.Tasks;
using PrefabSingle;

namespace SavedVideoInterpreter
{
    class VideoInterpreter
    {
        private VideoFrames _frames;
        private Thread _interpretFrame;
        private const int WIDTH = 2000;
        private const int HEIGHT = 2000;
        private int[] _currentBuffer = new int[WIDTH * HEIGHT];
        private int[] _previousBuffer = new int[WIDTH * HEIGHT];

        private Bitmap _previousFrame;
        private Bitmap _currentFrame;
        private Tree _previousTree;
        private VirtualizingCollection<BitmapSource> _framesCollection;

        public event EventHandler<InterpretedFrame> FrameInterpreted;
        public event EventHandler<RectsSnappedArgs> RectsSnapped;
        public event EventHandler<PrototypesEventArgs> PrototypesBuilt;
        public event EventHandler<PrototypesEventArgs> PrototypesRemoved;


        private Queue<int> _interpretQueue;
        private AutoResetEvent _elementAdded;

        private bool _running;

        private PrefabInterpretationLogic _interpretationLogic;


        public class InterpretedFrame : EventArgs
        {
            public readonly Tree Tree;

            public InterpretedFrame(Tree tree)
            {
                this.Tree = tree;
            }
        }

        public int FrameCount
        {
            get
            {
                return _frames.GetFrameCount();
            }
        }

        public void Stop()
        {
            lock (((ICollection)_interpretQueue).SyncRoot)
            {
                _running = false;
                _elementAdded.Set();
            }
        }

        public static VideoInterpreter FromVideoOrImage(string aviFileLocation, PrefabInterpretationLogic logic)
        {
            VideoInterpreter i = new VideoInterpreter(logic);

            i._frames = new VideoFrames(aviFileLocation);

            i._interpretFrame = new Thread(i._interpretFrame_DoWork);
            i._interpretFrame.Priority = ThreadPriority.Highest;
            i._interpretFrame.Start();

            i._framesCollection = new VirtualizingCollection<BitmapSource>(i.FrameCount, i.GetFramesInRange);

            return i;
        }

        public static VideoInterpreter FromMultipleScreenshots(IEnumerable<string> files, PrefabInterpretationLogic logic)
        {
            VideoInterpreter i = new VideoInterpreter(logic);
            List<System.Drawing.Bitmap> screenshots = new List<System.Drawing.Bitmap>();
            foreach(string file in files)
            {
                System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(file);
                screenshots.Add(bmp);
            }


            i._frames = VideoFrames.FromMultipleScreenshots(screenshots);
            i._interpretFrame = new Thread(i._interpretFrame_DoWork);
            i._interpretFrame.Priority = ThreadPriority.Highest;
            i._interpretFrame.Start();
            i._framesCollection = new VirtualizingCollection<BitmapSource>(i.FrameCount, i.GetFramesInRange);

            return i;
        }

        private VideoInterpreter(PrefabInterpretationLogic logic) 
        {
            _interpretationLogic = logic;
            _running = true;
            _interpretQueue = new Queue<int>();
            _elementAdded = new AutoResetEvent(false);
        }
        public static VideoInterpreter FromAnnotations(LayerInterpretationLogic logic)
        {
            VideoInterpreter i = new VideoInterpreter(logic);


            i._frames = VideoFrames.FromAnnotations(logic);

            i._interpretFrame = new Thread(i._interpretFrame_DoWork);
            i._interpretFrame.Priority = ThreadPriority.Highest;
            i._interpretFrame.Start();

            i._framesCollection = new VirtualizingCollection<BitmapSource>(i.FrameCount, i.GetFramesInRange);

            return i;
        }


        public void InterpretFrame(int frameIndex)
        {
            lock (((ICollection)_interpretQueue).SyncRoot)
            {
                _interpretQueue.Enqueue(frameIndex);
                _elementAdded.Set();
            }
        }


        private void _interpretFrame_DoWork()
        {
            while (_running)
            {
                if (_elementAdded.WaitOne())
                {
                    int index = -1;
                    lock (((ICollection)_interpretQueue).SyncRoot)
                    {
                        while(_interpretQueue.Count > 0)
                            index = _interpretQueue.Dequeue();
                    }
                    if(index >= 0)
                        InterpretHelper(index);
                }
            }
        }

        private void InterpretHelper(int index)
        {
            System.Drawing.Bitmap bitmap = GetBitmap(index);
            VideoCapture capture = new VideoCapture(index, bitmap);
            CopyPixelsFromDrawingBitmap(bitmap, _currentBuffer);
            _currentFrame = Bitmap.FromPixels(bitmap.Width, bitmap.Height, _currentBuffer);

            IBoundingBox invalidated = GetDirtyRect(_previousFrame, _currentFrame);

            var tags = new Dictionary<string, object>();
            tags.Add("videocapture", capture);
            tags.Add("invalidated", invalidated);
            tags.Add("previous", _previousTree);

            Tree root = Tree.FromPixels(_currentFrame, tags);


            Tree interpretation = _interpretationLogic.Interpret(root); 
            
            if (FrameInterpreted != null)
                FrameInterpreted(this, new InterpretedFrame(interpretation));

            //Swap the buffers we're writting to
            int[] tmp = _currentBuffer;
            _currentBuffer = _previousBuffer;
            _previousBuffer = tmp;


            //Set the previous bitmap and tree
            _previousTree = interpretation;
            _previousFrame = Bitmap.FromPixels(_currentFrame.Width, _currentFrame.Height, _previousBuffer);
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


        private System.Drawing.Bitmap GetBitmap(int index)
        {
            return _frames.GetBitmap(index);
        }

        private BitmapSource GetBitmapSource(int index)
        {
            return ViewablePrototypeItem.ToBitmapSource(Bitmap.FromSystemDrawingBitmap(_frames.GetBitmap(index)));
        }


        public void SnapRectangles(IEnumerable<IBoundingBox> rects, int frameIndex)
        {
            ThreadPool.QueueUserWorkItem(Snap, new object[]{rects, frameIndex});
        }

        private void Snap(object arg)
        {
            object[] argarr = arg as object[];
            IEnumerable<IBoundingBox> tosnap = argarr[0] as IEnumerable<IBoundingBox>;
            int index = (int)argarr[1];

            List<IBoundingBox> allsnapped = new List<IBoundingBox>();
            

            Bitmap tocrop = Bitmap.FromSystemDrawingBitmap(GetBitmap(index));
            
            foreach (IBoundingBox rect in tosnap)
            {
                System.Windows.Int32Rect snapped = RectangleFinder.SnapRect(Bitmap.Crop(tocrop, rect));
                BoundingBox snappedbb = new BoundingBox(snapped.X + rect.Left + 1, snapped.Y + rect.Top + 1, snapped.Width, snapped.Height);
                allsnapped.Add(snappedbb);
            }

            if (RectsSnapped != null)
            {
                RectsSnapped(this, new RectsSnappedArgs(allsnapped));
            }
        }

        public void Close()
        {
            _frames.Close();
        }

        private static void CopyPixelsFromDrawingBitmap(System.Drawing.Bitmap source, int[] buffer)
        {
                System.Drawing.Imaging.BitmapData bitmapData = source.LockBits(
                    new System.Drawing.Rectangle(0, 0, source.Width, source.Height),
                    System.Drawing.Imaging.ImageLockMode.ReadOnly,
                    System.Drawing.Imaging.PixelFormat.Format32bppArgb
                );
                System.Runtime.InteropServices.Marshal.Copy(bitmapData.Scan0, buffer, 0, source.Width * source.Height);

                source.UnlockBits(bitmapData);
        }

        public class VideoCapture
        {

            private System.Drawing.Bitmap _bitmap;

            public int FrameIndex
            {
                get;
                private set;
            }

            public int Width
            {
                get { return _bitmap.Width; }
            }

            public int Height
            {
                get { return _bitmap.Height; }
            }

            public VideoCapture(int frameIndex, System.Drawing.Bitmap pixels)
            {
                _bitmap = pixels;
                FrameIndex = frameIndex;
            }

            public void CopyToBuffer(int[] buffer)
            {
                CopyPixelsFromDrawingBitmap(_bitmap, buffer);
            }

 

            public void DisposePixels()
            {
                _bitmap.Dispose();
            }


            public void CopyToWriteableBitmap(System.Windows.Media.Imaging.WriteableBitmap dest, IBoundingBox regionToUpdate)
            {
                SystemDrawingBitmapToWriteableBitmap(_bitmap, dest, regionToUpdate);
            }

            private void SystemDrawingBitmapToWriteableBitmap(System.Drawing.Bitmap source, System.Windows.Media.Imaging.WriteableBitmap dest, IBoundingBox regionToUpdate)
            {

                if (regionToUpdate == null)
                    regionToUpdate = new BoundingBox(0, 0, source.Width, source.Height);
                System.Drawing.Imaging.BitmapData bitmapData = source.LockBits(new System.Drawing.Rectangle(regionToUpdate.Left, regionToUpdate.Top, regionToUpdate.Width, regionToUpdate.Height),
                                            System.Drawing.Imaging.ImageLockMode.ReadOnly,
                                            System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                dest.WritePixels(new Int32Rect(0, 0, regionToUpdate.Width, regionToUpdate.Height), bitmapData.Scan0, bitmapData.Stride * regionToUpdate.Height, bitmapData.Stride, regionToUpdate.Left, regionToUpdate.Top);

                source.UnlockBits(bitmapData);
            }
        }

        

        //public List<System.Drawing.Bitmap> GetAllFrames()
        //{
        //    return GetFramesInRange(0, FrameCount);
        //}

        public List<BitmapSource> GetFramesInRange(int startInclusive, int count)
        {
            List<BitmapSource> frames = new List<BitmapSource>();
            for (int i = startInclusive; i < startInclusive + count; i++)
            {
                if (i < FrameCount)
                {
                    System.Drawing.Bitmap bmp = GetBitmap(i);
                    BitmapSource src = ViewablePrototypeItem.ToBitmapSource(Bitmap.FromSystemDrawingBitmap(bmp));
                    frames.Add(src);
                }
            }

            return frames;
        }


        
        public VirtualizingCollection<BitmapSource> FrameCollection
        {
            get { return _framesCollection; }
        }

    }
}
