using Microsoft.Win32.SafeHandles;
using Prefab;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PrefabUtils
{
    public class Win32
    {

        // [DllImport("user32.dll")]
        // private static extern IntPtr CopyImage(IntPtr hImage,int uType,int cxDesired,int cyDesired,int fuFlags);

        //[DllImport("user32.dll")]
        //private static extern IntPtr GetCursor();

        //public static void HideSystemCursor()
        //{
        //    _oldcursor = GetCursor();
        //    _oldcursor = CopyImage(_oldcursor, IMAGE_CURSOR, 0, 0, LR_COPYDELETEORG);

        //    IntPtr hcursor = LoadCursor(IntPtr.Zero, (int)IDC_STANDARD_CURSORS.IDC_ARROW);
        //    SetSystemCursor(hcursor, (int)OCR_SYSTEM_CURSORS.OCR_NORMAL);    
        //}

        //public static void ShowSystemCursor()
        //{
        //    SetSystemCursor(_oldcursor, (int)OCR_SYSTEM_CURSORS.OCR_NORMAL);
        //}

        //private const int IMAGE_CURSOR = 2;
        //private const int LR_COPYDELETEORG = 0x008;

        //private static IntPtr _oldcursor;

        //[DllImport("user32.dll")]
        //static extern IntPtr LoadCursor(IntPtr hInstance, int lpCursorName);
        //[DllImport("user32.dll")]
        //private static extern bool SetSystemCursor(IntPtr hcur, uint id);

        private enum OCR_SYSTEM_CURSORS : uint
        {

            /// <summary>
            /// Standard arrow and small hourglass
            /// </summary>
            OCR_APPSTARTING = 32650,
            /// <summary>
            /// Standard arrow
            /// </summary>
            OCR_NORMAL = 32512,
            /// <summary>
            /// Crosshair
            /// </summary>
            OCR_CROSS = 32515,
            /// <summary>
            /// Windows 2000/XP: Hand
            /// </summary>
            OCR_HAND = 32649,
            /// <summary>
            /// Arrow and question mark
            /// </summary>
            OCR_HELP = 32651,
            /// <summary>
            /// I-beam
            /// </summary>
            OCR_IBEAM = 32513,
            /// <summary>
            /// Slashed circle
            /// </summary>
            OCR_NO = 32648,
            /// <summary>
            /// Four-pointed arrow pointing north, south, east, and west
            /// </summary>
            OCR_SIZEALL = 32646,
            /// <summary>
            /// Double-pointed arrow pointing northeast and southwest
            /// </summary>
            OCR_SIZENESW = 32643,
            /// <summary>
            /// Double-pointed arrow pointing north and south
            /// </summary>
            OCR_SIZENS = 32645,
            /// <summary>
            /// Double-pointed arrow pointing northwest and southeast
            /// </summary>
            OCR_SIZENWSE = 32642,
            /// <summary>
            /// Double-pointed arrow pointing west and east
            /// </summary>
            OCR_SIZEWE = 32644,
            /// <summary>
            /// Vertical arrow
            /// </summary>
            OCR_UP = 32516,
            /// <summary>
            /// Hourglass
            /// </summary>
            OCR_WAIT = 32514
        }


        private enum IDC_STANDARD_CURSORS : int
        {

            IDC_ARROW = 32512,
            IDC_IBEAM = 32513,
            IDC_WAIT = 32514,
            IDC_CROSS = 32515,
            IDC_UPARROW = 32516,
            IDC_SIZE = 32640,
            IDC_ICON = 32641,
            IDC_SIZENWSE = 32642,
            IDC_SIZENESW = 32643,
            IDC_SIZEWE = 32644,
            IDC_SIZENS = 32645,
            IDC_SIZEALL = 32646,
            IDC_NO = 32648,
            IDC_APPSTARTING = 32650,
            IDC_HELP = 32651
        }

        public static void SendMouseDown(MouseEventArgs e)
        {
            MOUSEINPUT m = new MOUSEINPUT();
            m.dx = (IntPtr)0;
            m.dy = (IntPtr)0;
            m.mouseData = 0;
            m.time = (IntPtr)0;

            MOUSEEVENTF button = MOUSEEVENTF.LEFTDOWN;
            if (e.Button == MouseButtons.Right)
                button = MOUSEEVENTF.RIGHTDOWN;

            m.dwFlags = (IntPtr)(MOUSEEVENTF.ABSOLUTE | button);

            INPUT i = new INPUT();
            i.type = (IntPtr)INPUTF.MOUSE;
            i.mi = m;

            INPUT[] inputs = new INPUT[] { i };
            int isize = Marshal.SizeOf(i);
            SendInput(1, inputs, isize);
        }

        public static void SendMouseUp(System.Windows.Forms.MouseEventArgs e)
        {
            MOUSEINPUT m = new MOUSEINPUT();
            m.dx = (IntPtr)0;
            m.dy = (IntPtr)0;
            m.mouseData = 0;
            m.time = (IntPtr)0;

            MOUSEEVENTF button = MOUSEEVENTF.LEFTUP;
            if (e.Button == MouseButtons.Right)
                button = MOUSEEVENTF.RIGHTUP;

            m.dwFlags = (IntPtr)(MOUSEEVENTF.ABSOLUTE | button);

            INPUT i = new INPUT();
            i.type = (IntPtr)INPUTF.MOUSE;
            i.mi = m;

            INPUT[] inputs = new INPUT[] { i };
            int isize = Marshal.SizeOf(i);
            SendInput(1, inputs, isize);
        }

        public static void SendMouseWheel(System.Windows.Forms.MouseEventArgs e)
        {
            MOUSEINPUT m = new MOUSEINPUT();
            m.dx = (IntPtr)0;
            m.dy = (IntPtr)0;
            m.mouseData = e.Delta;
            m.time = (IntPtr)0;

            MOUSEEVENTF button = MOUSEEVENTF.WHEEL;
            m.dwFlags = (IntPtr)(button);

            INPUT i = new INPUT();
            i.type = (IntPtr)INPUTF.MOUSE;
            i.mi = m;

            INPUT[] inputs = new INPUT[] { i };
            int isize = Marshal.SizeOf(i);
            SendInput(1, inputs, isize);
        }

        //
        // Windows messages
        //
        public static readonly IntPtr HWND_BROADCAST = new IntPtr(0xFFFF);

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, uint Msg, uint wParam, int lParam);

        [DllImport("user32.dll")]
        public static extern bool PostMessage(IntPtr hWnd, uint Msg, uint wParam, int lParam);

        public enum WM : uint // windows messages
        {
            SETFOCUS = 0x0007,
            KILLFOCUS = 0x0008,
            CLOSE = 0x0010,
            ERASEBKGND = 0x0014,
            CANCELMODE = 0x001F,                // cancel modal loop
            NCHITTEST = 0x0084,      // nonclient hit test
            NCMOUSEMOVE = 0x00A0,                // nonclient mouse move
            NCLBUTTONDOWN = 0x00A1,
            NCLBUTTONUP = 0x00A2,
            NCLBUTTONDBLCLK = 0x00A3,
            NCRBUTTONDOWN = 0x00A4,
            NCRBUTTONUP = 0x00A5,
            NCRBUTTONDBLCLK = 0x00A6,
            NCMBUTTONDOWN = 0x00A7,
            NCMBUTTONUP = 0x00A8,
            NCMBUTTONDBLCLK = 0x00A9,
            KEYDOWN = 0x0100,
            KEYUP = 0x0101,
            SYSKEYDOWN = 0x0104,
            SYSKEYUP = 0x0105,
            INITDIALOG = 0x0110,
            COMMAND = 0x0111,
            SYSCOMMAND = 0x0112,
            TIMER = 0x0113,
            HSCROLL = 0x0114,
            VSCROLL = 0x0115,
            INITMENU = 0x0116,
            INITMENUPOPUP = 0x0117,
            MENUSELECT = 0x011F,
            MENUCHAR = 0x0120,
            ENTERIDLE = 0x0121,
            MENURBUTTONUP = 0x0122,
            MENUDRAG = 0x0123,
            MENUGETOBJECT = 0x0124,
            UNINITMENUPOPUP = 0x0125,
            MENUCOMMAND = 0x0126,
            QUERYUISTATE = 0x0129,
            MOUSEMOVE = 0x0200,
            LBUTTONDOWN = 0x0201,
            LBUTTONUP = 0x0202,
            LBUTTONDBLCLK = 0x0203,
            RBUTTONDOWN = 0x0204,
            RBUTTONUP = 0x0205,
            RBUTTONDBLCLK = 0x0206,
            MBUTTONDOWN = 0x0207,
            MBUTTONUP = 0x0208,
            MBUTTONDBLCLK = 0x0209,
            MOUSEWHEEL = 0x020A,
            XBUTTONDOWN = 0x020B,
            XBUTTONUP = 0x020C,
            XBUTTONDBLCLK = 0x020D,
            NCXBUTTONDOWN = 0x00AB,
            NCXBUTTONUP = 0x00AC,
            NCXBUTTONDBLCLK = 0x00AD,
            ENTERMENULOOP = 0x0211,
            EXITMENULOOP = 0x0212,
            USER = 0x0400
        }

        public enum HT : int
        {
            ERROR = -2,
            TRANSPARENT = -1,
            NOWHERE = 0,
            CLIENT = 1,
            CAPTION = 2,
            SYSMENU = 3,
            GROWBOX = 4,
            SIZE = 4,
            MENU = 5,
            HSCROLL = 6,
            VSCROLL = 7,
            MINBUTTON = 8,
            MAXBUTTON = 9,
            LEFT = 10,
            RIGHT = 11,
            TOP = 12,
            TOPLEFT = 13,
            TOPRIGHT = 14,
            BOTTOM = 15,
            BOTTOMLEFT = 16,
            BOTTOMRIGHT = 17,
            BORDER = 18
        }

        public enum SC : uint
        {
            SIZE = 0xF000,
            SEPARATOR = 0xF00F,
            MOVE = 0xF010,
            MINIMIZE = 0xF020,
            MAXIMIZE = 0xF030,
            NEXTWINDOW = 0xF040,
            PREVWINDOW = 0xF050,
            CLOSE = 0xF060,
            VSCROLL = 0xF070,
            HSCROLL = 0xF080,
            MOUSEMENU = 0xF090,
            KEYMENU = 0xF100,
            ARRANGE = 0xF110,
            RESTORE = 0xF120,
            TASKLIST = 0xF130,
            SCREENSAVE = 0xF140,
            HOTKEY = 0xF150,
            DEFAULT = 0xF160,
            MONITORPOWER = 0xF170,
            CONTEXTHELP = 0xF180
        }

        //
        // SendInput function
        //
        [Flags]
        public enum MOUSEEVENTF : uint
        {
            MOVE = 0x01,
            LEFTDOWN = 0x02,
            LEFTUP = 0x04,
            RIGHTDOWN = 0x08,
            RIGHTUP = 0x10,
            MIDDLEDOWN = 0x20,
            MIDDLEUP = 0x40,
            XDOWN = 0x80,
            XUP = 0x100,
            WHEEL = 0x800,
            VIRTUALDESK = 0x4000,
            ABSOLUTE = 0x8000,
            XBUTTON1 = 0x01,
            XBUTTON2 = 0x02
        }

        [StructLayoutAttribute(LayoutKind.Explicit)]
        public struct MOUSEINPUT
        {
            [FieldOffset(0)]
            public IntPtr dx;
            [FieldOffset(4)]
            public IntPtr dy;
            [FieldOffset(8)]
            public int mouseData;
            [FieldOffset(12)]
            public IntPtr dwFlags;
            [FieldOffset(16)]
            public IntPtr time;
            [FieldOffset(20)]
            public IntPtr dwExtraInfo;
        }

        // virtual key codes
        public enum VK : byte // http://msdn.microsoft.com/en-us/library/dd375731.aspx
        {
            LBUTTON = 0x01,
            RBUTTON = 0x02,
            CANCEL = 0x03,
            MBUTTON = 0x04,
            BACK = 0x08,
            TAB = 0x09,
            CLEAR = 0x0C,
            RETURN = 0x0D,
            SHIFT = 0x10,
            CONTROL = 0x11,
            MENU = 0x12,
            PAUSE = 0x13,
            CAPITAL = 0x14,
            ESCAPE = 0x1B,
            SPACE = 0x20,
            PRIOR = 0x21,
            NEXT = 0x22,
            END = 0x23,
            HOME = 0x24,
            LEFT = 0x25,
            UP = 0x26,
            RIGHT = 0x27,
            DOWN = 0x28,
            SELECT = 0x29,
            PRINT = 0x2A,
            EXECUTE = 0x2B,
            SNAPSHOT = 0x2C,
            INSERT = 0x2D,
            DELETE = 0x2E,
            HELP = 0x2F,
            D0 = 0x30,
            D1 = 0x31,
            D2 = 0x32,
            D3 = 0x33,
            D4 = 0x34,
            D5 = 0x35,
            D6 = 0x36,
            D7 = 0x37,
            D8 = 0x38,
            D9 = 0x39,
            A = 0x41,
            B = 0x42,
            C = 0x43,
            D = 0x44,
            E = 0x45,
            F = 0x46,
            G = 0x47,
            H = 0x48,
            I = 0x49,
            J = 0x4A,
            K = 0x4B,
            L = 0x4C,
            M = 0x4D,
            N = 0x4E,
            O = 0x4F,
            P = 0x50,
            Q = 0x51,
            R = 0x52,
            S = 0x53,
            T = 0x54,
            U = 0x55,
            V = 0x56,
            W = 0x57,
            X = 0x58,
            Y = 0x59,
            Z = 0x5A,
            NUMPAD0 = 0x60,
            NUMPAD1 = 0x61,
            NUMPAD2 = 0x62,
            NUMPAD3 = 0x63,
            NUMPAD4 = 0x64,
            NUMPAD5 = 0x65,
            NUMPAD6 = 0x66,
            NUMPAD7 = 0x67,
            NUMPAD8 = 0x68,
            NUMPAD9 = 0x69,
            SEPARATOR = 0x6C,
            SUBTRACT = 0x6D,
            DECIMAL = 0x6E,
            DIVIDE = 0x6F,
            F1 = 0x70,
            F2 = 0x71,
            F3 = 0x72,
            F4 = 0x73,
            F5 = 0x74,
            F6 = 0x75,
            F7 = 0x76,
            F8 = 0x77,
            F9 = 0x78,
            F10 = 0x79,
            F11 = 0x7A,
            F12 = 0x7B,
            F13 = 0x7C,
            F14 = 0x7D,
            F15 = 0x7E,
            F16 = 0x7F,
            F17 = 0x80,
            F18 = 0x81,
            F19 = 0x82,
            F20 = 0x83,
            F21 = 0x84,
            F22 = 0x85,
            F23 = 0x86,
            F24 = 0x87,
            NUMLOCK = 0x90,
            SCROLL = 0x91,
            LSHIFT = 0xA0,
            RSHIFT = 0xA1,
            LCONTROL = 0xA2,
            RCONTROL = 0xA3,
            LMENU = 0xA4,
            RMENU = 0xA5,
            PLAY = 0xFA,
            ZOOM = 0xFB
        }

        [Flags]
        public enum KEYEVENTF : uint
        {
            EXTENDEDKEY = 0x01,
            KEYUP = 0x02,
            UNICODE = 0x04,
            SCANCODE = 0x08
        }

        public struct KEYBDINPUT
        {
            public static readonly KEYBDINPUT Empty;
            public ushort wVk;
            public ushort wScan;
            public KEYEVENTF dwFlags; // uint
            public uint time;
            public UIntPtr dwExtraInfo;
        }

        public struct HARDWAREINPUT
        {
            public static readonly HARDWAREINPUT Empty;
            public uint uMsg;
            public ushort wParamL;
            public ushort wParamH;
        }

        [Flags]
        public enum INPUTF : uint
        {
            MOUSE = 0x00,
            KEYBOARD = 0x01,
            HARDWARE = 0x02
        }


        [StructLayoutAttribute(LayoutKind.Explicit)]
        public struct INPUT
        {
            [FieldOffset(0)]
            public IntPtr type;
#if x86
            //32bit
            [FieldOffset(4)]
#else
            //64bit
            [FieldOffset(8)]
#endif
            public MOUSEINPUT mi;
            //public INPUT_UNION Union;
        }

        // for the cbSize parameter, use System.Runtime.InteropServices.Marshal.SizeOf(i)
        [DllImport("user32.dll", SetLastError = true)]
        static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

        [DllImport("user32.dll")]
        public static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo); // old Win16 call

        //
        // Windows Hooks
        //
        public enum WH : int // http://www.cs.unc.edu/Research/assist/doc/pyhook/public/pyHook.HookManager.HookConstants-class.html
        {
            CALLWNDPROC = 4,
            CALLWNDPROCRET = 12,
            CBT = 5,
            DEBUG = 9,
            FOREGROUNDIDLE = 11,
            GETMESSAGE = 3,
            HARDWARE = 8,
            JOURNALPLAYBACK = 1,
            JOURNALRECORD = 0,
            KEYBOARD = 2,
            KEYBOARD_LL = 13,
            MAX = 15,
            MIN = -1,
            MOUSE = 7,
            MOUSE_LL = 14,
            MSGFILTER = -1,
            SHELL = 10,
            SYSMSGFILTER = 6
        }

        [Flags]
        public enum MK : uint // key state masks for mouse messages
        {
            LBUTTON = 0x01,
            RBUTTON = 0x02,
            SHIFT = 0x04,
            CONTROL = 0x08,
            MBUTTON = 0x10,
            XBUTTON1 = 0x20,
            XBUTTON2 = 0x40
        }

        [Flags]
        public enum KF : uint // keyboard flags
        {
            EXTENDED = 0x100,
            ALTDOWN = 0x2000,
            DLGMODE = 0x00, // TODO
            MENUMODE = 0x00, // TODO
            REPEAT = 0x00, // TODO
            UP = 0x8000
        }

        [Flags]
        public enum LLMHF : uint // low level mouse hook flags
        {
            INJECTED = 0x0001
        }

        [Flags]
        public enum LLKHF : uint // low level keyboard hook flags
        {
            EXTENDED = (KF.EXTENDED >> 8), // 0x0001,
            INJECTED = 0x0010,                // 0x0010,
            ALTDOWN = (KF.ALTDOWN >> 8),   // 0x0020,
            UP = (KF.UP >> 8)              // 0x0080,
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public static readonly POINT Empty;
            public int x;
            public int y;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MSLLHOOKSTRUCT // http://msdn.microsoft.com/en-us/library/ms644970%28VS.85%29.aspx
        {
            public POINT pt;
            public uint mouseData;
            public uint flags; // LLMHF
            public uint time;
            public UIntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class KBDLLHOOKSTRUCT // http://msdn.microsoft.com/en-us/library/ms644967%28VS.85%29.aspx
        {
            public uint vkCode;
            public uint scanCode;
            public uint flags; // LLKHF
            public uint time;
            public UIntPtr dwExtraInfo;
        }

        public delegate IntPtr WindowsHookProc(int nCode, int wParam, IntPtr lParam);
        public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool EnumChildWindows(IntPtr hwndParent, EnumWindowsProc lpEnumFunc, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr SetWindowsHookEx(int idHook, WindowsHookProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, int wParam, IntPtr lParam);

        [DllImport("user32")]
        public static extern int GetDoubleClickTime();

        [DllImport("user32")]
        public static extern int ToAscii(uint uVirtKey, uint uScanCode, byte[] lpKeyState, out short lpChar, uint uFlags);

        [DllImport("user32")]
        public static extern bool GetKeyboardState(byte[] lpKeyState);

        [DllImport("user32.dll")]
        public static extern short GetKeyState(int nVirtKey);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, SWP uFlags);

        public static IntPtr HWND_TOP = new IntPtr(0);

        [DllImport("user32.dll")]
        public static extern bool SetCursorPos(int X, int Y);

        [Flags()]
        public enum SWP : uint
        {
            /// <summary>If the calling thread and the thread that owns the window are attached to different input queues, 
            /// the system posts the request to the thread that owns the window. This prevents the calling thread from 
            /// blocking its execution while other threads process the request.</summary>
            /// <remarks>SWP_ASYNCWINDOWPOS</remarks>
            ASYNCWINDOWPOS = 0x4000,
            /// <summary>Prevents generation of the WM_SYNCPAINT message.</summary>
            /// <remarks>SWP_DEFERERASE</remarks>
            DEFERERASE = 0x2000,
            /// <summary>Draws a frame (defined in the window's class description) around the window.</summary>
            /// <remarks>SWP_DRAWFRAME</remarks>
            DRAWFRAME = 0x0020,
            /// <summary>Applies new frame styles set using the SetWindowLong function. Sends a WM_NCCALCSIZE message to 
            /// the window, eveyn if the window's size is not being changed. If this flag is not specified, WM_NCCALCSIZE 
            /// is sent only when the window's size is being changed.</summary>
            /// <remarks>SWP_FRAMECHANGED</remarks>
            FRAMECHANGED = 0x0020,
            /// <summary>Hides the window.</summary>
            /// <remarks>SWP_HIDEWINDOW</remarks>
            HIDEWINDOW = 0x0080,
            /// <summary>Does not activate the window. If this flag is not set, the window is activated and moved to the 
            /// top of either the topmost or non-topmost group (depending on the setting of the hWndInsertAfter 
            /// parameter).</summary>
            /// <remarks>SWP_NOACTIVATE</remarks>
            NOACTIVATE = 0x0010,
            /// <summary>Discards the entire contents of the client area. If this flag is not specified, the valid 
            /// contents of the client area are saved and copied back into the client area after the window is sized or 
            /// repositioned.</summary>
            /// <remarks>SWP_NOCOPYBITS</remarks>
            NOCOPYBITS = 0x0100,
            /// <summary>Retains the current position (ignores X and Y parameters).</summary>
            /// <remarks>SWP_NOMOVE</remarks>
            NOMOVE = 0x0002,
            /// <summary>Does not change the owner window's position in the Z order.</summary>
            /// <remarks>SWP_NOOWNERZORDER</remarks>
            NOOWNERZORDER = 0x0200,
            /// <summary>Does not redraw changes. If this flag is set, no repainting of any kind occurs. This applies to 
            /// the client area, the nonclient area (including the title bar and scroll bars), and any part of the parent 
            /// window uncovered as a result of the window being moved. When this flag is set, the application must 
            /// explicitly invalidate or redraw any parts of the window and parent window that need redrawing.</summary>
            /// <remarks>SWP_NOREDRAW</remarks>
            NOREDRAW = 0x0008,
            /// <summary>Same as the SWP_NOOWNERZORDER flag.</summary>
            /// <remarks>SWP_NOREPOSITION</remarks>
            NOREPOSITION = 0x0200,
            /// <summary>Prevents the window from receiving the WM_WINDOWPOSCHANGING message.</summary>
            /// <remarks>SWP_NOSENDCHANGING</remarks>
            NOSENDCHANGING = 0x0400,
            /// <summary>Retains the current size (ignores the cx and cy parameters).</summary>
            /// <remarks>SWP_NOSIZE</remarks>
            NOSIZE = 0x0001,
            /// <summary>Retains the current Z order (ignores the hWndInsertAfter parameter).</summary>
            /// <remarks>SWP_NOZORDER</remarks>
            NOZORDER = 0x0004,
            /// <summary>Displays the window.</summary>
            /// <remarks>SWP_SHOWWINDOW</remarks>
            SHOWWINDOW = 0x0040,
        }

        //
        // Animation
        //
        public enum IDANI
        {
            OPEN = 0x01,
            CLOSE = 0x02,
            CAPTION = 0x03
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public static readonly RECT Empty;
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        [DllImport("user32.dll")]
        public static extern bool IsRectEmpty(ref RECT lprc);

        [DllImport("user32.dll")]
        public static extern bool DrawAnimatedRects(IntPtr hWnd, int idAni, ref RECT lprcFrom, ref RECT lprcTo);

        [Flags]
        public enum AW : uint
        {
            HOR_POSITIVE = 0x01,
            HOR_NEGATIVE = 0x02,
            VER_POSITIVE = 0x04,
            VER_NEGATIVE = 0x08,
            CENTER = 0x10,
            HIDE = 0x10000,
            ACTIVATE = 0x20000,
            SLIDE = 0x40000,
            BLEND = 0x80000
        }

        [DllImport("user32.dll")]
        public static extern bool AnimateWindow(IntPtr hWnd, uint dwTime, uint dwFlags);

        [DllImport("user32.dll")]
        public static extern int ShowCursor(bool bShow);

        //
        // Toolbars
        //
        public enum TB : uint
        {
            BUTTONCOUNT = WM.USER + 24,
            GETITEMRECT = WM.USER + 29,
            GETBUTTONTEXT = WM.USER + 45,
            GETRECT = WM.USER + 51
        }

        //
        // Windows
        //
        public enum GW : uint
        {
            HWNDFIRST = 0,
            HWNDLAST = 1,
            HWNDNEXT = 2,
            HWNDPREV = 3,
            OWNER = 4,
            CHILD = 5,
            ENABLEDPOPUP = 6,
            MAX = 6
        }



        [Flags]
        public enum WindowStyles : uint
        {
            WS_OVERLAPPED = 0x00000000,
            WS_POPUP = 0x80000000,
            WS_CHILD = 0x40000000,
            WS_MINIMIZE = 0x20000000,
            WS_VISIBLE = 0x10000000,
            WS_DISABLED = 0x08000000,
            WS_CLIPSIBLINGS = 0x04000000,
            WS_CLIPCHILDREN = 0x02000000,
            WS_MAXIMIZE = 0x01000000,
            WS_BORDER = 0x00800000,
            WS_DLGFRAME = 0x00400000,
            WS_VSCROLL = 0x00200000,
            WS_HSCROLL = 0x00100000,
            WS_SYSMENU = 0x00080000,
            WS_THICKFRAME = 0x00040000,
            WS_GROUP = 0x00020000,
            WS_TABSTOP = 0x00010000,

            WS_MINIMIZEBOX = 0x00020000,
            WS_MAXIMIZEBOX = 0x00010000,

            WS_CAPTION = WS_BORDER | WS_DLGFRAME,
            WS_TILED = WS_OVERLAPPED,
            WS_ICONIC = WS_MINIMIZE,
            WS_SIZEBOX = WS_THICKFRAME,
            WS_TILEDWINDOW = WS_OVERLAPPEDWINDOW,

            WS_OVERLAPPEDWINDOW = WS_OVERLAPPED | WS_CAPTION | WS_SYSMENU | WS_THICKFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX,
            WS_POPUPWINDOW = WS_POPUP | WS_BORDER | WS_SYSMENU,
            WS_CHILDWINDOW = WS_CHILD,

            //Extended Window Styles

            WS_EX_DLGMODALFRAME = 0x00000001,
            WS_EX_NOPARENTNOTIFY = 0x00000004,
            WS_EX_TOPMOST = 0x00000008,
            WS_EX_ACCEPTFILES = 0x00000010,
            WS_EX_TRANSPARENT = 0x00000020,

            //#if(WINVER >= 0x0400)

            WS_EX_MDICHILD = 0x00000040,
            WS_EX_TOOLWINDOW = 0x00000080,
            WS_EX_WINDOWEDGE = 0x00000100,
            WS_EX_CLIENTEDGE = 0x00000200,
            WS_EX_CONTEXTHELP = 0x00000400,

            WS_EX_RIGHT = 0x00001000,
            WS_EX_LEFT = 0x00000000,
            WS_EX_RTLREADING = 0x00002000,
            WS_EX_LTRREADING = 0x00000000,
            WS_EX_LEFTSCROLLBAR = 0x00004000,
            WS_EX_RIGHTSCROLLBAR = 0x00000000,

            WS_EX_CONTROLPARENT = 0x00010000,
            WS_EX_STATICEDGE = 0x00020000,
            WS_EX_APPWINDOW = 0x00040000,

            WS_EX_OVERLAPPEDWINDOW = (WS_EX_WINDOWEDGE | WS_EX_CLIENTEDGE),
            WS_EX_PALETTEWINDOW = (WS_EX_WINDOWEDGE | WS_EX_TOOLWINDOW | WS_EX_TOPMOST),
            //#endif /* WINVER >= 0x0400 */

            //#if(WIN32WINNT >= 0x0500)

            WS_EX_LAYERED = 0x00080000,
            //#endif /* WIN32WINNT >= 0x0500 */

            //#if(WINVER >= 0x0500)

            WS_EX_NOINHERITLAYOUT = 0x00100000, // Disable inheritence of mirroring by children
            WS_EX_LAYOUTRTL = 0x00400000, // Right to left mirroring
            //#endif /* WINVER >= 0x0500 */

            //#if(WIN32WINNT >= 0x0500)

            WS_EX_COMPOSITED = 0x02000000,
            WS_EX_NOACTIVATE = 0x08000000
            //#endif /* WIN32WINNT >= 0x0500 */

        }


        [StructLayout(LayoutKind.Sequential)]
        public struct WINDOWINFO
        {
            public uint cbSize;
            public RECT rcWindow;
            public RECT rcClient;
            public WindowStyles dwStyle;
            public WindowStyles dwExStyle;
            public uint dwWindowStatus;
            public uint cxWindowBorders;
            public uint cyWindowBorders;
            public ushort atomWindowType;
            public ushort wCreatorVersion;

            public WINDOWINFO(Boolean? filler)
                : this()   // Allows automatic initialization of "cbSize" with "new WINDOWINFO(null/true/false)".
            {
                cbSize = (UInt32)(Marshal.SizeOf(typeof(WINDOWINFO)));
            }

            public override string ToString()
            {
                string result = "";
                result += "cbSize = " + cbSize + "\n";
                result += "dwStyle = " + dwStyle + "\n";
                result += "dwExStyle = " + dwExStyle + "\n";
                result += "dwWindowStatus = " + dwWindowStatus + "\n";
                result += "cxWindowBorders = " + cxWindowBorders + "\n";
                result += "cyWindowBorders = " + cyWindowBorders + "\n";
                result += "atomWindowType = " + atomWindowType + "\n";
                result += "wCreatorVersion = " + wCreatorVersion + "\n";

                return result;
            }
        }

        [DllImport("user32.dll")]
        public static extern IntPtr GetWindow(IntPtr hWnd, uint uCmd);

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool GetWindowInfo(IntPtr hwnd, ref WINDOWINFO pwi);

        [DllImport("user32.dll")]
        public static extern IntPtr GetNextWindow(IntPtr hWnd, uint wCmd);

        [DllImport("user32.dll")]
        public static extern bool IsWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsWindowVisible(IntPtr hWnd);

        public enum SW : int
        {
            HIDE = 0,
            SHOWNORMAL = 1,
            NORMAL = 1,
            SHOWMINIMIZED = 2,
            SHOWMAXIMIZED = 3,
            MAXIMIZE = 3,
            SHOWNOACTIVATE = 4,
            SHOW = 5,
            MINIMIZE = 6,
            SHOWMINNOACTIVE = 7,
            SHOWNA = 8,
            RESTORE = 9,
            SHOWDEFAULT = 10,
            FORCEMINIMIZE = 11,
            MAX = 11
        }

        [DllImport("C:\\Users\\mdixon\\Desktop\\TestProjects\\TestingDllInjectionx86\\Release\\TestingDllInjection.dll")]
        public static extern int TestX86();

        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("User32.dll")]
        public static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        public static extern IntPtr WindowFromPoint(POINT Point);

        [DllImport("user32.dll")]
        public static extern IntPtr FindWindowEx(IntPtr hWndParent, IntPtr hWndChildAfter, string lpszClass, string lpszWindow);

        [DllImport("user32.dll")]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern int GetWindowTextLength(IntPtr hWnd);

        public static string GetWindowText(IntPtr hWnd)
        {
            // Allocate correct string length first
            int length = GetWindowTextLength(hWnd);
            StringBuilder sb = new StringBuilder(length + 1);
            GetWindowText(hWnd, sb, sb.Capacity);
            return sb.ToString();
        }

        /// <summary>
        /// Returns the base file name of the process (notepad.exe) for example.
        /// </summary>
        /// <param name="hWnd">The handle to the window</param>
        /// <param name="hModule">The</param>
        /// <param name="lpFileName"></param>
        /// <param name="nSize"></param>
        /// <returns></returns>
        [DllImport("psapi.dll")]
        private static extern uint GetModuleBaseName(IntPtr hWnd, IntPtr hModule, StringBuilder lpFileName, int nSize);

        /// <summary>
        /// Returns the file name associated with the process. It includes the full path of the file.
        /// </summary>
        /// <param name="hWnd"></param>
        /// <param name="hModule"></param>
        /// <param name="lpFileName"></param>
        /// <param name="nSize"></param>
        /// <returns></returns>
        [DllImport("coredll.dll", SetLastError = true)]
        private static extern uint GetModuleFileNameEx(IntPtr hWnd, IntPtr hModule, StringBuilder lpFileName, int nSize);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern SafeFileHandle CreateNamedPipe(
           String pipeName,
           uint dwOpenMode,
           uint dwPipeMode,
           uint nMaxInstances,
           uint nOutBufferSize,
           uint nInBufferSize,
           uint nDefaultTimeOut,
           IntPtr lpSecurityAttributes);

        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowThreadProcessId(IntPtr hWnd, IntPtr ProcessId);

        public static string GetProcessPathFromWindowHandle(IntPtr hwnd)
        {
            try
            {
                uint pid = 0;
                StringBuilder sb = new StringBuilder(2000);
                Win32.GetWindowThreadProcessId(hwnd, ref pid);
                Process p = Process.GetProcessById((int)pid);
                string filename = p.MainModule.FileName;
                p.Dispose();

                return filename;
            }
            catch (Exception e)
            {
                return "exception: " + e.Message;
            }
        }

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        public static string GetClassName(IntPtr hWnd)
        {
            int length = 500;
            StringBuilder sb = new StringBuilder(length);
            GetClassName(hWnd, sb, sb.Capacity);
            return sb.ToString();
        }

        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(IntPtr hWnd, ref RECT lpRect);

        /// <summary>
        /// Gets the rectangle bounding a window
        /// </summary>
        /// <param name="hWnd"></param>
        /// <returns></returns>
        public static IBoundingBox GetWindowRect(IntPtr hWnd)
        {
            RECT rc = new RECT();
            GetWindowRect(hWnd, ref rc);
            return RECTToBoundingBox(rc);
        }

        public static IBoundingBox RECTToBoundingBox(RECT rect)
        {
            return new BoundingBox(rect.left, rect.top, rect.right - rect.left + 1, rect.bottom - rect.top + 1);
        }



        // Declare a delegate
        // The delegate will handle each windowPointer from the enumeration process
        // Read: http://msdn.microsoft.com/en-us/library/d186xcf0(VS.71).aspx

        [Flags]
        public enum WS : int // http://support.microsoft.com/kb/111011
        {
            EXSTYLE = -20,
            EX_TRANSPARENT = 0x00000020,
            CLIPSIBLINGS = 0x04000000,
            CLIPCHILDREN = 0x02000000,
            VISIBLE = 0x10000000,
            DISABLED = 0x08000000,
            MINIMIZE = 0x20000000,
            MAXIMIZE = 0x01000000,
            CAPTION = 0x00C00000,
            BORDER = 0x00800000,
            DLGFRAME = 0x00400000,
            VSCROLL = 0x00200000,
            HSCROLL = 0x00100000,
            SYSMENU = 0x00080000,
            THICKFRAME = 0x00040000,
            MINIMIZEBOX = 0x00020000,
            MAXIMIZEBOX = 0x00010000,
        }

        public enum GWL : int
        {
            EXSTYLE = -20,

        }

        [DllImport("user32.dll")]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll")]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        //
        // Foreground window
        //
        [DllImport("user32.dll")]
        public static extern IntPtr GetDesktopWindow();

        [DllImport("user32.dll")]
        public static extern IntPtr GetTopWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern bool BringWindowToTop(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        public static extern IntPtr SetCapture(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsIconic(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern IntPtr GetParent(IntPtr hWnd);

        //
        // Active window
        //
        [DllImport("user32.dll")]
        public static extern IntPtr SetActiveWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern IntPtr GetActiveWindow();

        //
        // Focused window
        //
        [DllImport("user32.dll")]
        public static extern IntPtr SetFocus(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern IntPtr GetFocus();

        //
        // Sound
        //
        [DllImport("kernel32.dll")]
        public static extern bool Beep(uint dwFreq, uint dwDuration);

        [Flags]
        public enum SND : uint
        {
            SYNC = 0x0,                // can be used with sndPlaySound
            ASYNC = 0x1,                // can be used with sndPlaySound
            NODEFAULT = 0x2,                // can be used with sndPlaySound
            MEMORY = 0x4,                // can be used with sndPlaySound
            LOOP = 0x8,                // can be used with sndPlaySound
            NOSTOP = 0x10,                // can be used with sndPlaySound
            PURGE = 0x40,
            APPLICATION = 0x80,
            NOWAIT = 0x2000,
            RESOURCE = 0x40004,
            ALIAS = 0x10000,
            FILENAME = 0x20000,
            ALIAS_ID = 0x110000
        }

        [DllImport("winmm.dll")]
        public static extern bool PlaySound(byte[] lpszSound, IntPtr hmod, uint fdwSound);

        [DllImport("winmm.dll")]
        public static extern bool sndPlaySound(byte[] lpszSound, uint fuSound);

        //
        // Menus
        //
        [Flags]
        public enum MF : uint
        {
            STRING = 0x00,
            GRAYED = 0x01,
            BITMAP = 0x04,
            BYCOMMAND = 0x00,
            BYPOSITION = 0x400
        }

        [DllImport("user32.dll")]
        public static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

        [DllImport("user32.dll")]
        public static extern bool AppendMenu(IntPtr hMenu, uint uFlags, uint uIDNewItem, string lpNewItem);

        [DllImport("user32.dll")]
        public static extern bool InsertMenu(IntPtr hMenu, uint uPosition, uint uFlags, uint uIDNewItem, string lpNewItem);

        [DllImport("user32.dll")]
        public static extern bool RemoveMenu(IntPtr hMenu, uint uPosition, uint uFlags);

        [DllImport("user32.dll")]
        public static extern bool DeleteMenu(IntPtr hMenu, uint uPosition, uint uFlags);

        [DllImport("user32.dll")]
        public static extern bool EndMenu();

        [DllImport("user32.dll")]
        public static extern bool IsMenu(IntPtr hMenu);

        [DllImport("user32.dll")]
        public static extern IntPtr GetMenu(IntPtr hWnd);

        [Flags]
        public enum TPM : uint
        {
            LEFTBUTTON = 0x0000,
            RECURSE = 0x0001,
            RIGHTBUTTON = 0x0002,
            LEFTALIGN = 0x0000,
            CENTERALIGN = 0x0004,
            RIGHTALIGN = 0x0008,
            TOPALIGN = 0x0000,
            VCENTERALIGN = 0x0010,
            BOTTOMALIGN = 0x0020,
            HORIZONTAL = 0x0000,
            VERTICAL = 0x0040,
            NONOTIFY = 0x0080,
            RETURNCMD = 0x0100,
            HORPOSANIMATION = 0x0400,
            HORNEGANIMATION = 0x0800,
            VERPOSANIMATION = 0x1000,
            VERNEGANIMATION = 0x2000,
            NOANIMATION = 0x4000,
            LAYOUTRTL = 0x8000
        }

        [DllImport("user32.dll")]
        public static extern uint TrackPopupMenu(IntPtr hMenu, uint uFlags, int x, int y, int nReserved, IntPtr hWnd, IntPtr prcRect);

        [DllImport("user32.dll")]
        public static extern uint TrackPopupMenuEx(IntPtr hmenu, uint fuFlags, int x, int y, IntPtr hWnd, IntPtr lptpm);

        [DllImport("kernel32.dll")]
        public static extern bool CloseHandle(IntPtr hObject);

        //
        // Message Box
        //
        [DllImport("user32.dll")]
        public static extern int MessageBox(IntPtr hWnd, string lpText, string lpCaption, uint uType);

        [Flags]
        public enum MB : uint
        {
            ABORTRETRYIGNORE = 0x0002,
            CANCELTRYCONTINUE = 0x0006,
            HELP = 0x4000,
            OK = 0x0000,
            OKCANCEL = 0x0001,
            RETRYCANCEL = 0x0005,
            YESNO = 0x0004,
            YESNOCANCEL = 0x0003,
            ICONEXCLAMATION = 0x0030,
            ICONWARNING = 0x0030,
            ICONINFORMATION = 0x0040,
            ICONASTERISK = 0x0040,
            ICONQUESTION = 0x0020,
            ICONSTOP = 0x0010,
            ICONERROR = 0x0010,
            ICONHAND = 0x0010,
            DEFBUTTON1 = 0x0000,
            DEFBUTTON2 = 0x0100,
            DEFBUTTON3 = 0x0200,
            DEFBUTTON4 = 0x0300,
            APPLMODAL = 0x0000,
            SYSTEMMODAL = 0x1000,
            TASKMODAL = 0x2000,
            DEFAULT_DESKTOP_ONLY = 0x20000,
            RIGHT = 0x80000,
            RTLREADING = 0x100000,
            SETFOREGROUND = 0x10000,
            TOPMOST = 0x40000,
            SERVICE_NOTIFICATION = 0x200000,
            SERVICE_NOTIFICATION_NT3X = 0x40000
        }

        // MessageBox result constants
        public enum ID : int
        {
            OK = 1,
            CANCEL = 2,
            ABORT = 3,
            RETRY = 4,
            IGNORE = 5,
            YES = 6,
            NO = 7,
            CLOSE = 8,
            HELP = 9,
            TRYAGAIN = 10,
            CONTINUE = 11,
            TIMEOUT = 32000
        }

        //
        // Threads and Processes
        //
        [DllImport("user32.dll")]
        public static extern uint WaitForInputIdle(IntPtr hProcess, uint dwMilliseconds);

        [DllImport("user32.dll")]
        public static extern bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach);

        [DllImport("kernel32.dll")]
        public static extern uint GetCurrentThreadId();

        [DllImport("user32.dll")]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, ref uint lpdwProcessId);

        //
        // Control Panel Settings
        //
        public enum SPI : uint
        {
            GETMOUSE = 0x0003,
            SETMOUSE = 0x0004,
            GETFILTERKEYS = 0x0032,
            SETFILTERKEYS = 0x0033,
            GETMOUSESPEED = 0x0070,
            SETMOUSESPEED = 0x0071
        }

        [Flags]
        public enum SPIF : uint
        {
            UPDATEINIFILE = 0x01,
            SENDCHANGE = 0x02,
            SENDWININICHANGE = 0x02
        }

        [Flags]
        public enum FKF : uint
        {
            FILTERKEYSON = 0x0001,
            AVAILABLE = 0x0002,
            HOTKEYACTIVE = 0x0004,
            CONFIRMHOTKEY = 0x0008,
            HOTKEYSOUND = 0x0010,
            INDICATOR = 0x0020,
            CLICKON = 0x0040
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct FILTERKEYS
        {
            public static readonly FILTERKEYS Empty;
            public uint cbSize;
            public uint dwFlags;
            public uint iWaitMSec;
            public uint iDelayMSec;
            public uint iRepeatMSec;
            public uint iBounceMSec;
        };

        [DllImport("user32.dll")] // for getting and setting FILTERKEYS
        public static extern bool SystemParametersInfo(uint uiAction, uint uiParam, ref FILTERKEYS pvParam, uint fWinIni);

        [DllImport("user32.dll")] // for getting values and setting with reference types
        public static extern bool SystemParametersInfo(uint uiAction, uint uiParam, ref uint pvParam, uint fWinIni);

        [DllImport("user32.dll")] // for setting values
        public static extern bool SystemParametersInfo(uint uiAction, uint uiParam, uint pvParam, uint fWinIni);

        //
        // Timers
        //
        [DllImport("user32.dll")]
        public static extern uint SetTimer(IntPtr hWnd, uint nIDEvent, uint uElapse, IntPtr lpTimerFunc);

        [DllImport("user32.dll")]
        public static extern bool KillTimer(IntPtr hWnd, uint uIDEvent);

        [DllImport("User32.dll", SetLastError = true)]
        public static extern bool PrintWindow(IntPtr hwnd, IntPtr hDC, uint nFlags);


        /// <summary>
        ///    Performs a bit-block transfer of the color data corresponding to a
        ///    rectangle of pixels from the specified source device context into
        ///    a destination device context.
        /// </summary>
        /// <param name="hdc">Handle to the destination device context.</param>
        /// <param name="nXDest">The leftmost x-coordinate of the destination rectangle (in pixels).</param>
        /// <param name="nYDest">The topmost y-coordinate of the destination rectangle (in pixels).</param>
        /// <param name="nWidth">The width of the source and destination rectangles (in pixels).</param>
        /// <param name="nHeight">The height of the source and the destination rectangles (in pixels).</param>
        /// <param name="hdcSrc">Handle to the source device context.</param>
        /// <param name="nXSrc">The leftmost x-coordinate of the source rectangle (in pixels).</param>
        /// <param name="nYSrc">The topmost y-coordinate of the source rectangle (in pixels).</param>
        /// <param name="dwRop">A raster-operation code.</param>
        /// <returns>
        ///    <c>true</c> if the operation succeeded, <c>false</c> otherwise.
        /// </returns>
        [DllImport("gdi32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool BitBlt(IntPtr hdc, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hdcSrc, int nXSrc, int nYSrc, TernaryRasterOperations dwRop);

        [DllImport("user32.dll")]
        static extern bool UpdateWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        static extern bool RedrawWindow(IntPtr hWnd, IntPtr lprcUpdate, IntPtr hrgnUpdate, RedrawWindowFlags flags);


        [Flags()]
        private enum RedrawWindowFlags : uint
        {
            /// <summary>
            /// Invalidates the rectangle or region that you specify in lprcUpdate or hrgnUpdate.
            /// You can set only one of these parameters to a non-NULL value. If both are NULL, RDW_INVALIDATE invalidates the entire window.
            /// </summary>
            Invalidate = 0x1,

            /// <summary>Causes the OS to post a WM_PAINT message to the window regardless of whether a portion of the window is invalid.</summary>
            InternalPaint = 0x2,

            /// <summary>
            /// Causes the window to receive a WM_ERASEBKGND message when the window is repainted.
            /// Specify this value in combination with the RDW_INVALIDATE value; otherwise, RDW_ERASE has no effect.
            /// </summary>
            Erase = 0x4,

            /// <summary>
            /// Validates the rectangle or region that you specify in lprcUpdate or hrgnUpdate.
            /// You can set only one of these parameters to a non-NULL value. If both are NULL, RDW_VALIDATE validates the entire window.
            /// This value does not affect internal WM_PAINT messages.
            /// </summary>
            Validate = 0x8,

            NoInternalPaint = 0x10,

            /// <summary>Suppresses any pending WM_ERASEBKGND messages.</summary>
            NoErase = 0x20,

            /// <summary>Excludes child windows, if any, from the repainting operation.</summary>
            NoChildren = 0x40,

            /// <summary>Includes child windows, if any, in the repainting operation.</summary>
            AllChildren = 0x80,

            /// <summary>Causes the affected windows, which you specify by setting the RDW_ALLCHILDREN and RDW_NOCHILDREN values, to receive WM_ERASEBKGND and WM_PAINT messages before the RedrawWindow returns, if necessary.</summary>
            UpdateNow = 0x100,

            /// <summary>
            /// Causes the affected windows, which you specify by setting the RDW_ALLCHILDREN and RDW_NOCHILDREN values, to receive WM_ERASEBKGND messages before RedrawWindow returns, if necessary.
            /// The affected windows receive WM_PAINT messages at the ordinary time.
            /// </summary>
            EraseNow = 0x200,

            Frame = 0x400,

            NoFrame = 0x800
        }

        [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
        public static extern IntPtr CreateCompatibleDC(IntPtr hdc);

        [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
        public static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);

        [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
        public static extern bool DeleteObject(IntPtr hObject);

        [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
        public static extern bool DeleteDC(IntPtr hdc);

        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowDC(IntPtr hWnd);

        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int nWidth, int nHeight);

        [DllImport("user32.dll")]
        public static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern bool ReleaseDC(IntPtr hWnd, IntPtr hDC);

        /// <summary>
        ///     Specifies a raster-operation code. These codes define how the color data for the
        ///     source rectangle is to be combined with the color data for the destination
        ///     rectangle to achieve the final color.
        /// </summary>
        public enum TernaryRasterOperations : uint
        {
            /// <summary>dest = source</summary>
            SRCCOPY = 0x00CC0020,
            /// <summary>dest = source OR dest</summary>
            SRCPAINT = 0x00EE0086,
            /// <summary>dest = source AND dest</summary>
            SRCAND = 0x008800C6,
            /// <summary>dest = source XOR dest</summary>
            SRCINVERT = 0x00660046,
            /// <summary>dest = source AND (NOT dest)</summary>
            SRCERASE = 0x00440328,
            /// <summary>dest = (NOT source)</summary>
            NOTSRCCOPY = 0x00330008,
            /// <summary>dest = (NOT src) AND (NOT dest)</summary>
            NOTSRCERASE = 0x001100A6,
            /// <summary>dest = (source AND pattern)</summary>
            MERGECOPY = 0x00C000CA,
            /// <summary>dest = (NOT source) OR dest</summary>
            MERGEPAINT = 0x00BB0226,
            /// <summary>dest = pattern</summary>
            PATCOPY = 0x00F00021,
            /// <summary>dest = DPSnoo</summary>
            PATPAINT = 0x00FB0A09,
            /// <summary>dest = pattern XOR dest</summary>
            PATINVERT = 0x005A0049,
            /// <summary>dest = (NOT dest)</summary>
            DSTINVERT = 0x00550009,
            /// <summary>dest = BLACK</summary>
            BLACKNESS = 0x00000042,
            /// <summary>dest = WHITE</summary>
            WHITENESS = 0x00FF0062,
            /// <summary>
            /// Capture window as seen on screen.  This includes layered windows 
            /// such as WPF windows with AllowsTransparency="true"
            /// </summary>
            CAPTUREBLT = 0x40000000
        }


        [DllImport("user32.dll")]
        static extern bool InvalidateRect(IntPtr hWnd, IntPtr lpRect, bool bErase);

        public static void GetThumbnailUsingPrintWindow(IntPtr hWnd, System.Drawing.Bitmap dest)
        {
            Graphics g = Graphics.FromImage(dest);
            IntPtr dc = g.GetHdc();

            bool worked = PrintWindow(hWnd, dc, 0);

            g.ReleaseHdc();
            g.Dispose();

            //InvalidateRect(hWnd, IntPtr.Zero, false);

            //RedrawWindow(hWnd, IntPtr.Zero, IntPtr.Zero, (RedrawWindowFlags)(0x0400/*RDW_FRAME*/ | 0x0100/*RDW_UPDATENOW*/ | 0x0001/*RDW_INVALIDATE*/ | 0x80 /*All children*/));

            //UpdateWindow(hWnd);
        }

        public static void GetThumbnailUsingCopyFromScreen(IntPtr hWnd, System.Drawing.Bitmap dest)
        {

            Graphics g = Graphics.FromImage(dest);
            IBoundingBox rect = GetWindowRect(hWnd);
            g.CopyFromScreen(rect.Left, rect.Top, 0, 0, new System.Drawing.Size(rect.Width, rect.Height), CopyPixelOperation.SourceCopy);
            g.Dispose();
        }

        public enum HRESULT : long
        {
            S_FALSE = 0x0001,
            S_OK = 0x0000,
            E_INVALIDARG = 0x80070057,
            E_OUTOFMEMORY = 0x8007000E
        }

        public enum CompositionAction : uint
        {
            DWM_EC_ENABLECOMPOSITION = 0x0001,
            DWM_EC_DISABLECOMPOSITION = 0x0000
        }

        [DllImport("Dwmapi.dll", SetLastError = true)]
        public static extern HRESULT DwmEnableComposition(CompositionAction action);

        public static System.Drawing.Bitmap GetThumbnailUsingBitBlt(IntPtr hWnd)
        {
            int width, height;
            RECT wRect = new RECT();
            System.Drawing.Bitmap image = null;
            IntPtr hDC = IntPtr.Zero, hcDC = IntPtr.Zero, hbmp = IntPtr.Zero;

            try
            {
                //...find the window width and height
                GetWindowRect(hWnd, ref wRect);
                width = wRect.right - wRect.left;
                height = wRect.bottom - wRect.top;

                //...get a DC and compatible DC for the desktop
                hDC = GetDC(hWnd);
                hcDC = CreateCompatibleDC(hDC);

                //...create a bitmap and copy the window from the desktop coordinates to it
                hbmp = CreateCompatibleBitmap(hDC, width, height);
                SelectObject(hcDC, hbmp);

                BitBlt(hcDC, 0, 0, width, height, hDC, 0, 0, TernaryRasterOperations.CAPTUREBLT | TernaryRasterOperations.SRCCOPY);
                //...create a .NET image from the bitmap

                image = System.Drawing.Bitmap.FromHbitmap(hbmp);
            }
            finally
            {
                //...free DCs and bitmap
                if (hDC != IntPtr.Zero) ReleaseDC(hWnd, hDC);
                if (hcDC != IntPtr.Zero) DeleteDC(hcDC);
                if (hbmp != IntPtr.Zero) DeleteObject(hbmp);
            }

            return image;
        }
    }
}
