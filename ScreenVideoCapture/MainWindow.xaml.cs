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
using System.ComponentModel;
using System.Threading;
using System.Windows.Interop;
using Microsoft.Win32;
using Prefab;
using PrefabUtils;

namespace ScreenVideoCapture
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            _state = State.None;
        }

        CaptureThread _capture;
        LowLevelMouseHook _mousehook;
        Tree _selectedWindow;
        WindowHighlight _windowHiglight;
        State _state;
        BackgroundWorker _windowGetter;
        IEnumerable<Tree> _windows;
        WindowStreamCapture _windowEnumerator;
     

        enum UpdateType
        {
            EnableRecord,
            DisableRecord,
            HighlightWindow,
            DisableSelecting,
            EnableSelecting,
            ChangeRecordToStopRecording,
            ChangeStopRecordingToRecord
        }


        enum State
        {
            Recording,
            Selecting,
            SelectedButNotRecording,
            None

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _windowGetter = new BackgroundWorker();
            _windowGetter.DoWork += new DoWorkEventHandler(_windowGetter_DoWork);
            _windowGetter.RunWorkerAsync();

            _windowHiglight = new WindowHighlight();
            _windowHiglight.Owner = this;


            _updateUI = new UpdateDel(UpdateUI);
            _mousehook = new LowLevelMouseHook("low level mouse hook");
            _mousehook.OnMouseMove += new EventHandler<System.Windows.Forms.MouseEventArgs>(_mousehook_OnMouseMove);
            _mousehook.OnMouseDown += new EventHandler<System.Windows.Forms.MouseEventArgs>(_mousehook_OnMouseDown);
            _mousehook.Install();

        }

        void _windowGetter_DoWork(object sender, DoWorkEventArgs e)
        {
            _windowEnumerator = new WindowStreamCapture();
            while (true)
            {
                _windows = _windowEnumerator.GetAllWindowsWithoutPixels().Where(ShouldConsiderWindow);
                Thread.Sleep(100);
            }
        }


        private delegate void UpdateDel(UpdateType updateType);
        private UpdateDel _updateUI;

        private void UpdateUI(UpdateType updatetype)
        {
            switch (updatetype)
            {
                case UpdateType.EnableRecord:
                    RecordButton.IsEnabled = true;
                    break;
                case UpdateType.DisableRecord:
                    RecordButton.IsEnabled = false;
                    break;

                case UpdateType.ChangeRecordToStopRecording:
                    RecordButton.Content = "Stop Recording";
                    break;

                case UpdateType.ChangeStopRecordingToRecord:
                    RecordButton.Content = "Record";
                    break;
                
                case UpdateType.HighlightWindow:
                    _windowHiglight.Left = _selectedWindow.Left;
                    _windowHiglight.Top = _selectedWindow.Top;
                    _windowHiglight.Width = _selectedWindow.Width;
                    _windowHiglight.Height = _selectedWindow.Height;
                    _windowHiglight.Show();
                    break;

                case UpdateType.DisableSelecting:
                    SelectAWindowButton.IsEnabled = false;
                    break;

                case UpdateType.EnableSelecting:
                    SelectAWindowButton.IsEnabled = true;
                    break;

                default:
                    break;
            }
        }

        void _mousehook_OnMouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            switch (_state)
            {
                case State.Selecting:
                    _state = State.SelectedButNotRecording;
                    Dispatcher.BeginInvoke(_updateUI, UpdateType.EnableRecord);
                    Dispatcher.BeginInvoke(_updateUI, UpdateType.EnableSelecting);
                    break;

                default: //We don't care about global mouse events unless we're selecting a window to record. So do nothing here.
                    break;
            }
        }

        void _mousehook_OnMouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            switch(_state){
                case State.Selecting:
                    IEnumerable<Tree> windows = _windows;

                     Tree underCursor = windows.FirstOrDefault((wo) => BoundingBox.Contains(wo, e.X, e.Y));
                    //Win32.POINT pt = new Win32.POINT();
                    //pt.x = e.X;
                    //pt.y = e.Y;

                   

                    //WindowOccurrence underCursor = _windowEnumerator.CreateOccurrence(Win32.WindowFromPoint(pt));
                     if (underCursor != null)
                     {
                         _selectedWindow = underCursor;
                         Dispatcher.BeginInvoke(_updateUI, UpdateType.HighlightWindow);
                     }
                break;
            }
        }

        private bool ShouldConsiderWindow(Tree window)
        {
            IntPtr highlightHandle = IntPtr.Zero;
            if (_windowHiglight != null)
                highlightHandle = _windowHiglight.Handle;

            return window != null &&
                (((IntPtr)window["handle"]) != highlightHandle) &&
                ((Win32.WindowStyles)window["style"] & Win32.WindowStyles.WS_ICONIC) != Win32.WindowStyles.WS_ICONIC && // not minimized
                ((Win32.WindowStyles)window["style"] & Win32.WindowStyles.WS_CHILD) != Win32.WindowStyles.WS_CHILD && //not a child window
                 ((Win32.WindowStyles)window["style"] & Win32.WindowStyles.WS_VISIBLE) == Win32.WindowStyles.WS_VISIBLE &&
                ((( (Win32.WindowStyles)window["style"] & Win32.WindowStyles.WS_POPUP) != Win32.WindowStyles.WS_POPUP ||
                ((Win32.WindowStyles)window["exstyle"] & Win32.WindowStyles.WS_EX_TOOLWINDOW) != Win32.WindowStyles.WS_EX_TOOLWINDOW) || window.Height > 29) && //is not a tooltip (not a popup or not a tool window or bigger than 28 high). (28 px is a hack to distinguish tooltips from other windows).
                (window.Width < System.Windows.SystemParameters.VirtualScreenWidth ||
                 window.Height < System.Windows.SystemParameters.VirtualScreenHeight);
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
           if(RecordButton.IsChecked.Value == true)
                 e.Cancel = true;
        }

        private void ToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            if (_selectedWindow != null)
            {
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.AddExtension = true;
                sfd.DefaultExt = ".avi";
                if (sfd.ShowDialog().Value)
                {
                    _state = State.Recording;
                    _capture = new CaptureThread(sfd.FileName);
                    _capture.UsePrintWindow = UsePrintWindow.IsChecked.Value;
                    _capture.Start((IntPtr)_selectedWindow["handle"]);

                    Dispatcher.BeginInvoke(_updateUI, UpdateType.ChangeRecordToStopRecording);
                    Dispatcher.BeginInvoke(_updateUI, UpdateType.DisableSelecting);
                }
                else
                {
                    RecordButton.IsChecked = false;
                }
            }
            else
            {
                RecordButton.IsChecked = false;
            }

        }

        private void ToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            if (_state == State.Recording)
            {
                _state = State.SelectedButNotRecording;
                _capture.Stop();
                Dispatcher.BeginInvoke(_updateUI, UpdateType.ChangeStopRecordingToRecord);
                Dispatcher.BeginInvoke(_updateUI, UpdateType.EnableSelecting);
            }
        }

        private void SelectAWindowButton_Click(object sender, RoutedEventArgs e)
        {
            switch (_state)
            {
                case State.None:
                case State.SelectedButNotRecording:
                    _state = State.Selecting;
                    Dispatcher.BeginInvoke(_updateUI, UpdateType.DisableSelecting);
                    break;
            }
        }

        private void UsePrintWindow_Checked_1(object sender, RoutedEventArgs e)
        {
            if(_capture != null)
                _capture.UsePrintWindow = true;
        }

        private void UsePrintWindow_Unchecked_1(object sender, RoutedEventArgs e)
        {
            if(_capture != null)
                _capture.UsePrintWindow = false;
        }
    }
}
