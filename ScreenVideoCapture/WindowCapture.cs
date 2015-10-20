
using Prefab;
using PrefabUtils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScreenVideoCapture
{
    class WindowStreamCapture
    {
        private List<Tree> _windows;
        private Dictionary<IntPtr, ExpensiveWindowInfo> _windowInfo;



        private BitmapPool _pool;

        private ScreenshotToTreeDoubleBuffered _bitmapToTree;


        

        public WindowStreamCapture()
        {
            _windows = new List<Tree>();
            _windowInfo = new Dictionary<IntPtr, ExpensiveWindowInfo>();

            _pool = new BitmapPool();
            _bitmapToTree = new ScreenshotToTreeDoubleBuffered();
        }

        public IEnumerable<Tree> GetAllWindowsWithoutPixels()
        {
            _windows.Clear();
            Win32.EnumWindows(new Win32.EnumWindowsProc(EnumWindows), IntPtr.Zero);
            return new List<Tree>(_windows);
        }

        /// <summary>
        /// Currently determines the closest window to the cursor.
        /// </summary>
        /// <param name="hwnd"></param>
        /// <param name="lparams"></param>
        /// <returns></returns>
        private bool EnumWindows(IntPtr hwnd, IntPtr lparams)
        {
            Win32.WINDOWINFO windowinfo = new Win32.WINDOWINFO(true);
            Win32.GetWindowInfo(hwnd, ref windowinfo);
            Tree occurrence = CaptureWindowWithoutPixels(hwnd, windowinfo);

            _windows.Add(occurrence);

            return true;
        }
        

        public Tree CaptureWindowWithPixels(IntPtr hwnd, bool usePrintWindow, bool fixedSize = false)
        {
            Win32.WINDOWINFO windowinfo = new Win32.WINDOWINFO(true);
            Win32.GetWindowInfo(hwnd, ref windowinfo);
            return CaptureWindow(hwnd, windowinfo, usePrintWindow, fixedSize);
        }

        public Tree CaptureWindowWithoutPixels(IntPtr hwnd)
        {
            Win32.WINDOWINFO windowinfo = new Win32.WINDOWINFO(true);
            Win32.GetWindowInfo(hwnd, ref windowinfo);
            return CaptureWindowWithoutPixels(hwnd, windowinfo);
        }

        private Dictionary<string, object> GetWindowAttributes(IntPtr hwnd, Win32.WINDOWINFO windowinfo)
        {
           
            Dictionary<string, object> attributes = new Dictionary<string, object>();
            attributes["handle"] = hwnd;


            ExpensiveWindowInfo expensiveInfo;

            if (!_windowInfo.TryGetValue(hwnd, out expensiveInfo))
            {
                expensiveInfo = new ExpensiveWindowInfo(Win32.GetClassName(hwnd), Win32.GetWindowText(hwnd), Win32.GetProcessPathFromWindowHandle(hwnd));
                _windowInfo.Add(hwnd, expensiveInfo);
            }

            attributes["processfilename"] = expensiveInfo.ProcessFilePath;
            attributes["title"] = expensiveInfo.Title;
            attributes["classname"] = expensiveInfo.ClassName;
            attributes["style"] = windowinfo.dwStyle;
            attributes["exstyle"] = windowinfo.dwExStyle;

            return attributes;
        }

        private Tree CaptureWindowWithoutPixels(IntPtr hwnd, Win32.WINDOWINFO windowinfo)
        {
            BoundingBox windowRect = new BoundingBox(windowinfo.rcWindow.left, windowinfo.rcWindow.top,
              windowinfo.rcWindow.right - windowinfo.rcWindow.left + 1, windowinfo.rcWindow.bottom - windowinfo.rcWindow.top + 1);

            Dictionary<string, object> attributes = GetWindowAttributes(hwnd, windowinfo);


            if (windowRect.Width > 0 && windowRect.Height > 0)
            {

                Tree window = Tree.FromBoundingBox(windowRect, attributes);
                return window;
            }

            return null;
        
        }

        private Tree CaptureWindow(IntPtr hwnd, Win32.WINDOWINFO windowinfo, bool usePrintWindow, bool fixedSize = false)
        {
            BoundingBox windowRect = new BoundingBox(windowinfo.rcWindow.left, windowinfo.rcWindow.top,
                            windowinfo.rcWindow.right - windowinfo.rcWindow.left + 1, windowinfo.rcWindow.bottom - windowinfo.rcWindow.top + 1);

            var attributes = GetWindowAttributes(hwnd, windowinfo);

            if (windowRect.Width > 0 && windowRect.Height > 0)
            {
                System.Drawing.Bitmap bmp;
                int width = windowRect.Width;
                int height = windowRect.Height;

                if(fixedSize)
                {
                    width = (int)System.Windows.SystemParameters.VirtualScreenWidth;
                    height = (int)System.Windows.SystemParameters.VirtualScreenHeight;
                }

                if (!usePrintWindow)
                    bmp = GetBitmapFromScreen(hwnd, null, width, height);
                else
                    bmp = GetBitmapFromPrintWindow(hwnd, null, width, height);


                Tree window = _bitmapToTree.GetScreenshotAndCreateTree(windowRect.Width, windowRect.Height, bmp, attributes);
                //_pool.ReturnInstance(bmp);
                return window;
            }

            return null;
        }



        private class ExpensiveWindowInfo
        {
            public ExpensiveWindowInfo(string classname, string title, string filepath)
            {
                ClassName = classname;
                Title = title;
                ProcessFilePath = filepath;
            }
            public string ClassName
            {
                get;
                private set;
            }
            public string Title
            {
                get;
                private set;
            }
            public string ProcessFilePath
            {
                get;
                private set;
            }
        }



        public static System.Drawing.Bitmap GetBitmapFromScreen(IntPtr handle, BitmapPool pool, int width, int height)
        {
            System.Drawing.Bitmap bitmap;
            if(pool != null)
                 bitmap = pool.GetInstance(width, height);
            else
                bitmap = new System.Drawing.Bitmap(width, height);
            Win32.GetThumbnailUsingCopyFromScreen(handle, bitmap);
            return bitmap;
        }

        public static System.Drawing.Bitmap GetBitmapFromPrintWindow(IntPtr handle, BitmapPool pool, int width, int height)
        {
             System.Drawing.Bitmap bitmap;
             if (pool != null)
                 bitmap = pool.GetInstance(width, height);
             else
                 bitmap = new System.Drawing.Bitmap(width, height);
            Win32.GetThumbnailUsingPrintWindow(handle, bitmap);
            return bitmap;
        }

    }
}
