using System.Drawing;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Drawing.Imaging;

namespace ScreenCapture
{

        /// <summary>
        /// A class that maintains a pool of System.Drawing.Bitmaps.
        /// The pool leverages the heuristic that most of the bitmaps
        /// we ask for are the same size, so it keeps bitmaps of the same
        /// size in a queue. If you ask for a size that is not at the head
        /// of the queue, it creates a new one and destroys the bitmap at the
        /// head of the queue. Returned bitmaps are enqueued. A pool is thread
        /// safe.
        /// </summary>
        public sealed class BitmapPool
        {

            private const int MAX_WIDTH = 2650;
            private const int MAX_HEIGHT = 1600;
            private const int MAX_POOL_SIZE = 500;
            private static int _numCreated;
            private LinkedList<Bitmap> _pool;
            private HashSet<Bitmap> _allItemsThatExist;
            private AutoResetEvent _itemAvailable;
            private WaitHandle[] _eventArray;


            public BitmapPool()
            {
                _pool = new LinkedList<Bitmap>();
                _allItemsThatExist = new HashSet<Bitmap>();
                _numCreated = 0;
                _itemAvailable = new AutoResetEvent(true);
                _eventArray = new WaitHandle[] { _itemAvailable };
            }

            public Bitmap GetInstance(int width, int height)
            {
                Bitmap bmp = null;
                WaitHandle.WaitAny(_eventArray);
                lock (((ICollection)_pool).SyncRoot)
                {
                    if (_pool.Count == 0)
                    {
                        bmp = Create(width, height);
                    }
                    else
                    {
                        bmp = TryGet(width, height);
                        if (bmp == null)
                        {
                            if (_numCreated < MAX_POOL_SIZE)
                            {
                                bmp = Create(width, height);

                            }
                            else
                            {
                                Bitmap toDestroy = _pool.First.Value;
                                _pool.RemoveFirst();
                                _allItemsThatExist.Remove(toDestroy);
                                toDestroy.Dispose();

                                bmp = Create(width, height);
                            }
                        }
                    }

                    if (_numCreated < MAX_POOL_SIZE)
                        _itemAvailable.Set();
                }

                return bmp;
            }

            private Bitmap Create(int width, int height)
            {
                _numCreated++;
                Bitmap bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);
                _allItemsThatExist.Add(bmp);

                return bmp;
            }

            private Bitmap TryGet(int width, int height)
            {
                LinkedListNode<Bitmap> curr = _pool.First;
                while (curr != null)
                {
                    if (curr.Value.Width == width && curr.Value.Height == height)
                    {
                        _pool.Remove(curr);
                        return curr.Value;
                    }
                    curr = curr.Next;
                }

                return null;
            }

            public void ReturnInstance(Bitmap bmp)
            {
                lock (((ICollection)_pool).SyncRoot)
                {
                    if (_allItemsThatExist.Contains(bmp))
                    {
                        _pool.AddFirst(bmp);
                        _itemAvailable.Set();
                    }
                }
            }

        }
    }

