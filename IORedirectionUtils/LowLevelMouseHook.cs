using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using System.Windows.Forms;
namespace PrefabUtils
{
    /// <summary>
    /// 
    /// </summary>
    public class LowLevelMouseHook
    {
        /// <summary>
        /// 
        /// </summary>
        private struct DoubleClickState
        {
            public static readonly DoubleClickState Empty = new DoubleClickState();
            public MouseButtons LastDownButton;
            public long LastDownTime;
            public Point LastDownPt;
            public bool IssueClickOnMouseUp;
            public bool IssueDoubleClickOnMouseUp;
        }

        /// <summary>
        /// 
        /// </summary>
        public event EventHandler<MouseEventArgs> OnMouseMove;
        public event EventHandler<MouseEventArgs> OnMouseDown;
        public event EventHandler<MouseEventArgs> OnMouseUp;
        public event EventHandler<MouseEventArgs> OnMouseClick;
        public event EventHandler<MouseEventArgs> OnMouseDoubleClick;
        public event EventHandler<MouseEventArgs> OnMouseWheel;

        // double-click members
        private DoubleClickState _state;

        // hook members
        private string _name;
        private IntPtr _hookID;
        private Win32.WindowsHookProc _hookProc;
        private System.Drawing.Point _physicalCursorLocation;
        private System.Drawing.Point _prev;
        private System.Drawing.Point _curr;
        private MoveStrategy _strategy;

        public delegate void PreviewMouseDownDelegate(MouseEventArgs e, SuppressArgs suppressOrNot);
        public PreviewMouseDownDelegate OnPreviewMouseDown;
        public delegate IntPtr WindowsHookProc(int nCode, int wParam, IntPtr lParam);
        /// <summary>
        /// This is passed to preview events. By setting Handled to true, the subsequent event will
        /// not be fired. This lets you intercept events and block them.
        /// </summary>
        public class SuppressArgs
        {
            public bool SuppressMouseEvent;
        }

        private enum MoveStrategy
        {
            Standard,
            Clipped,
            Relative
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        public LowLevelMouseHook(string name)
        {
            _name = name;
            _hookID = IntPtr.Zero;
            _hookProc = HookCallback;
            _state = DoubleClickState.Empty;
            _strategy = MoveStrategy.Standard;
        }

        /// <summary>
        /// 
        /// </summary>
        ~LowLevelMouseHook()
        {
            Uninstall();
        }

        /// <summary>
        /// 
        /// </summary>
        public string Name
        {
            get { return _name; }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Install()
        {
            Uninstall(); // stop the hook if it is already active first

            // then install a new hook
            Process procCurrent = Process.GetCurrentProcess();
            ProcessModule procMod = procCurrent.MainModule;
            _hookID = Win32.SetWindowsHookEx(
                        (int)Win32.WH.MOUSE_LL,
                        _hookProc,
                        Win32.GetModuleHandle(procMod.ModuleName),
                        0u
                        );
            procMod.Dispose();
            procCurrent.Dispose();

            // set this special registry key so the hook won't be uninstalled during periods of high activity
            RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop\", true);
            if (key == null)
            {
                key = Registry.CurrentUser.CreateSubKey(@"Control Panel\Desktop\");
            }
            key.SetValue("LowLevelHooksTimeout", 10000, RegistryValueKind.DWord);
            key.Close();
        }

        /// <summary>
        /// 
        /// </summary>
        public void Uninstall()
        {
            if (_hookID != IntPtr.Zero)
            {
                Win32.UnhookWindowsHookEx(_hookID);
            }
            _hookID = IntPtr.Zero;
        }

        /// <summary>
        /// 
        /// </summary>
        public bool IsActive
        {
            get { return _hookID != IntPtr.Zero; }
        }

        public void SetClip(int x, int y)
        {
            _strategy = MoveStrategy.Clipped;
            _physicalCursorLocation = new Point(x, y);
            _prev = _physicalCursorLocation;
            Win32.SetCursorPos(x, y);
        }

        public void ClearClip()
        {
            _strategy = MoveStrategy.Standard;
            _physicalCursorLocation = _curr;
            _prev = new Point(_curr.X, _curr.Y);
            Win32.SetCursorPos(_curr.X, _curr.Y);
        }

        public void ResumeClipping()
        {
            _strategy = MoveStrategy.Clipped;
        }

        public void UseRelativeMovement()
        {
            _strategy = MoveStrategy.Relative;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="nCode"></param>
        /// <param name="wParam"></param>
        /// <param name="lParam"></param>
        /// <returns></returns>
        private IntPtr HookCallback(int nCode, int wParam, IntPtr lParam)
        {
            long now = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            if (nCode >= 0)
            {
                Win32.MSLLHOOKSTRUCT info = (Win32.MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(Win32.MSLLHOOKSTRUCT));


                bool isDown = false;
                int nClicks = 1;
                MouseButtons button = MouseButtons.None;

                #region setting mouse data
                Win32.INPUT input = new Win32.INPUT();
                input.type = (IntPtr)Win32.INPUTF.MOUSE;
                input.mi.dwExtraInfo = (IntPtr)0;
                input.mi.mouseData = 0;
                input.mi.time = (IntPtr)0;
                input.mi.dx = (IntPtr)0;
                input.mi.dy = (IntPtr)0;
                switch (wParam) // figure out which button was pressed, if any
                {
                    ////////// down events
                    case (int)Win32.WM.LBUTTONDOWN:
                        input.mi.dwFlags = (IntPtr)Win32.MOUSEEVENTF.LEFTDOWN;
                        button = MouseButtons.Left;
                        isDown = true;
                        nClicks = HandleDoubleClickLogic(button, info.pt.x, info.pt.y, now);
                        break;
                    case (int)Win32.WM.RBUTTONDOWN:
                        input.mi.dwFlags = (IntPtr)Win32.MOUSEEVENTF.RIGHTDOWN;
                        button = MouseButtons.Right;
                        isDown = true;
                        nClicks = HandleDoubleClickLogic(button, info.pt.x, info.pt.y, now);
                        break;
                    case (int)Win32.WM.MBUTTONDOWN:
                        input.mi.dwFlags = (IntPtr)Win32.MOUSEEVENTF.MIDDLEDOWN;
                        button = MouseButtons.Middle;
                        isDown = true;
                        nClicks = HandleDoubleClickLogic(button, info.pt.x, info.pt.y, now);
                        break;
                    case (int)Win32.WM.XBUTTONDOWN:

                        if ((wParam >> 16) == 1)
                        { // high-order word specifies which button was pressed
                            button = MouseButtons.XButton1;
                            input.mi.dwFlags = (IntPtr)Win32.MOUSEEVENTF.XBUTTON1;
                        }
                        else if ((wParam >> 16) == 2)
                        {
                            button = MouseButtons.XButton2;
                            input.mi.dwFlags = (IntPtr)Win32.MOUSEEVENTF.XBUTTON2;
                        }

                        else Debug.Fail("XButton message received without a button identifier.");
                        isDown = true;
                        nClicks = HandleDoubleClickLogic(button, info.pt.x, info.pt.y, now);
                        break;

                    ////////// up events
                    case (int)Win32.WM.LBUTTONUP:
                        input.mi.dwFlags = (IntPtr)Win32.MOUSEEVENTF.LEFTUP;
                        button = MouseButtons.Left;
                        nClicks = _state.IssueDoubleClickOnMouseUp ? 2 : 1;
                        break;
                    case (int)Win32.WM.RBUTTONUP:
                        input.mi.dwFlags = (IntPtr)Win32.MOUSEEVENTF.RIGHTUP;
                        button = MouseButtons.Right;
                        nClicks = _state.IssueDoubleClickOnMouseUp ? 2 : 1;
                        break;
                    case (int)Win32.WM.MBUTTONUP:
                        input.mi.dwFlags = (IntPtr)Win32.MOUSEEVENTF.MIDDLEUP;
                        button = MouseButtons.Middle;
                        nClicks = _state.IssueDoubleClickOnMouseUp ? 2 : 1;
                        break;
                    case (int)Win32.WM.XBUTTONUP:
                        if ((wParam >> 16) == 1)
                        { // high-order word specifies which button was pressed
                            button = MouseButtons.XButton1;
                            input.mi.dwFlags = (IntPtr)Win32.MOUSEEVENTF.XUP;
                        }
                        else if ((wParam >> 16) == 2)
                        {
                            button = MouseButtons.XButton2;
                            input.mi.dwFlags = (IntPtr)Win32.MOUSEEVENTF.XUP;
                        }
                        else Debug.Fail("XButton message received without a button identifier.");
                        nClicks = _state.IssueDoubleClickOnMouseUp ? 2 : 1;
                        break;
                }

                short delta = -1; // wheel
                if (wParam == (int)Win32.WM.MOUSEWHEEL)
                {
                    input.mi.dwFlags = (IntPtr)Win32.MOUSEEVENTF.WHEEL;
                    delta = (short)((info.mouseData >> 16) & 0xffff);
                    nClicks = 0;
                }

                #endregion

                MouseEventArgs e = null;


                switch (_strategy)
                {
                    case MoveStrategy.Standard:
                        _curr = new Point(info.pt.x, info.pt.y);
                        _prev = _curr;
                        Win32.SetCursorPos(_curr.X, _curr.Y);
                        break;


                    case MoveStrategy.Clipped:

                        // set up the event argument
                        int deltx = info.pt.x - _prev.X;
                        int delty = info.pt.y - _prev.Y;

                        if (deltx < 100 && delty < 100)
                        {
                            int newx = _curr.X + deltx;
                            int newy = _curr.Y + delty;
                            _curr.X += deltx;
                            _curr.Y += delty;

                            #region clip to the screen bounds
                            if (_curr.X < 0)
                                _curr.X = 0;
                            else if (_curr.X >= System.Windows.SystemParameters.VirtualScreenWidth)
                                _curr.X = (int)System.Windows.SystemParameters.VirtualScreenWidth - 1;

                            if (_curr.Y < 0)
                                _curr.Y = 0;
                            else if (_curr.Y >= System.Windows.SystemParameters.VirtualScreenHeight)
                                _curr.Y = (int)System.Windows.SystemParameters.VirtualScreenHeight - 1;
                            #endregion

                        }
                        break;


                    case MoveStrategy.Relative:
                        deltx = info.pt.x - _prev.X;
                        delty = info.pt.y - _prev.Y;
                        if (deltx < 100 && delty < 100)
                        {
                            int newx = _curr.X + deltx;
                            int newy = _curr.Y + delty;
                            _curr.X += deltx;
                            _curr.Y += delty;

                            _physicalCursorLocation.X += deltx;
                            _physicalCursorLocation.Y += delty;

                            _prev = _physicalCursorLocation;

                            #region clip to the screen bounds
                            if (_curr.X < 0)
                                _curr.X = 0;
                            else if (_curr.X >= System.Windows.SystemParameters.VirtualScreenWidth)
                                _curr.X = (int)System.Windows.SystemParameters.VirtualScreenWidth - 1;

                            if (_curr.Y < 0)
                                _curr.Y = 0;
                            else if (_curr.Y >= System.Windows.SystemParameters.VirtualScreenHeight)
                                _curr.Y = (int)System.Windows.SystemParameters.VirtualScreenHeight - 1;
                            #endregion
                            Win32.SetCursorPos(_physicalCursorLocation.X, _physicalCursorLocation.Y);
                        }
                        break;
                }

                e = new MouseEventArgs(button, nClicks, _curr.X, _curr.Y, delta);


                // fire the appropriate event
                if (button != MouseButtons.None)
                {

                    if (isDown)
                    {
                        if (OnPreviewMouseDown != null)
                        {
                            SuppressArgs hp = new SuppressArgs();
                            OnPreviewMouseDown(e, hp);
                            if (!hp.SuppressMouseEvent)
                            {
                                Win32.SendMouseDown(e);
                                if (OnMouseDown != null)
                                    OnMouseDown(this, e);
                            }
                        }
                        else
                        {
                            Win32.SendMouseDown(e);
                            if (OnMouseDown != null)
                                OnMouseDown(this, e);

                        }
                    }
                    else // button came up
                    {
                        if (Control.MouseButtons != MouseButtons.None)
                            Win32.SendMouseUp(e);

                        if (_state.IssueDoubleClickOnMouseUp)
                        {
                            if (OnMouseDoubleClick != null)
                                OnMouseDoubleClick(this, e);
                            _state = DoubleClickState.Empty; // unset
                        }
                        else if (_state.IssueClickOnMouseUp)
                        {
                            if (OnMouseClick != null)
                                OnMouseClick(this, e);
                            _state.IssueClickOnMouseUp = false; // unset
                        }
                        if (OnMouseUp != null)
                        {
                            OnMouseUp(this, e);
                        }
                    }
                }
                else // no buttons -- wheel or mouse movement
                {

                    if (delta != -1)
                    {
                        Win32.SendMouseWheel(e);
                        //Win32.SendInput(1, ref input, Marshal.SizeOf(new Win32.INPUT()));
                        if (OnMouseWheel != null)
                        {
                            OnMouseWheel(this, e);
                        }
                    }
                    else // mouse movement
                    {
                        if (OnMouseMove != null)
                        {
                            OnMouseMove(this, e);
                        }
                    }
                }
            }

            Win32.CallNextHookEx(_hookID, nCode, wParam, lParam);
            return (IntPtr)0;
        }




        /// <summary>
        /// Handler for double-click logic that should be called on any button-down event.
        /// </summary>
        /// <param name="button">The button that was pressed down.</param>
        /// <param name="x">The x-coordinate of the button press.</param>
        /// <param name="y">The y-coordinate of the button press.</param>
        /// <param name="time">The time, in milliseconds, of the button press.</param>
        /// <returns>The number of clicks, 1 or 2, that have occurred.</returns>
        private int HandleDoubleClickLogic(MouseButtons button, int x, int y, long time)
        {
            int nClicks;
            if (_state.LastDownButton == button // has to be same button
                && _state.LastDownPt.X == x // has to be same X-coord
                && _state.LastDownPt.Y == y // has to be same Y-coord
                && time - _state.LastDownTime <= Win32.GetDoubleClickTime()) // has to happen fast enough
            {
                _state.IssueDoubleClickOnMouseUp = true; // dlbclk determination is made on the second down
                _state.IssueClickOnMouseUp = false; // no single click is now loaded
                nClicks = 2;
            }
            else // no double click here, so set this state
            {
                _state.LastDownButton = button;
                _state.LastDownTime = time;
                _state.LastDownPt = new Point(x, y);
                _state.IssueDoubleClickOnMouseUp = false; // no double click was determined
                _state.IssueClickOnMouseUp = true; // a single click is "loaded," and the first mouse-up of any button unloads it
                nClicks = 1;
            }
            return nClicks;
        }
    }
}
