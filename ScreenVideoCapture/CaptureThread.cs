using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using AviFile;
using Prefab;
using System.Drawing.Imaging;
using PrefabUtils;
namespace ScreenVideoCapture
{
    class CaptureThread
    {

        private AviManager _aviManager;
        private VideoStream _videoStream;
        private AutoResetEvent _exitEvent;
        public bool UsePrintWindow;
        private bool _running;
        private string _saveLoc;
        //private HashSet<string> _expensiveStopList;
        private WindowStreamCapture _windowCapture;
        private BitmapPool _pool;

        private int frame_num;


         public CaptureThread(string saveLoc)
        {
            _saveLoc = saveLoc;
            _aviManager = new AviManager(saveLoc, false);

            _windowCapture = new WindowStreamCapture();

            _exitEvent = new AutoResetEvent(false);
            _pool = new BitmapPool();
            _running = false;
            UsePrintWindow = true;

             frame_num = 0;
        }

         public void Start(IntPtr window)
         {
             ThreadPool.QueueUserWorkItem(new WaitCallback(ThreadRun), window);
         }

        public void Stop()
        {
            if (_running)
            {
                _exitEvent.Set();
            }
        }
        

        private void ThreadRun(object arg)
        {
            if (!_running)
            {
                _running = true;

                while (!_exitEvent.WaitOne(0, false))
                {

                    IntPtr windowHandle = (IntPtr)arg;
                    //Get the active window so we can translate it.
                    Tree window = _windowCapture.CaptureWindowWithPixels(windowHandle, UsePrintWindow, false);

                    if (window != null)
                    {
                        Prefab.Bitmap capture = window["capturedpixels"] as Prefab.Bitmap ;
                        System.Drawing.Bitmap bmp = _pool.GetInstance(capture.Width, capture.Height);
                        Bitmap.ToSystemDrawingBitmap(capture, bmp);

                        // Get Window features
                        Win32.WINDOWINFO windowinfo = new Win32.WINDOWINFO(true);
                        Win32.GetWindowInfo(windowHandle, ref windowinfo);

                        // Save as png image
                        String filename = string.Format("{0}_f{1:D4}.png", _saveLoc.Substring(0, _saveLoc.Length - 4), frame_num);
                        bmp.Save(@filename, ImageFormat.Png);
                        frame_num++;

                        
                        if (_videoStream == null)
                        {
                            _videoStream = _aviManager.AddVideoStream(false, 20, bmp);

                        }
                        else
                            _videoStream.AddFrame(bmp);

                        _pool.ReturnInstance(bmp);
                    }

                    //Let's not melt our processor ;)
                    Thread.Sleep(50);
                }
                _running = false;
                _aviManager.Close();
                
            }
        }




        
    }
}
