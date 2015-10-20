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
using Prefab;
using System.Collections.ObjectModel;

namespace SavedVideoInterpreter
{
    /// <summary>
    /// Interaction logic for ZoomWindow.xaml
    /// </summary>
    public partial class ZoomWindow : Window, INotifyPropertyChanged
    {
        public ZoomWindow()
        {
            DataContext = this;
            InitializeComponent();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            e.Cancel = true;
            Visibility = Visibility.Hidden;
        }

        public BitmapSource ZoomedImage
        {
            get { return _zoomedImage; }

            set
            {
                _zoomedImage = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("ZoomedImage"));
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;

        public event MouseButtonEventHandler ImageMouseDown
        {
            add
            {
                Image.MouseDown += value;
            }
            remove
            {
                Image.MouseDown -= value;
            }
        }

        public event MouseButtonEventHandler ImageMouseUp
        {
            add
            {
                Image.MouseUp += value;
            }
            remove
            {
                Image.MouseUp -= value;
            }
        }

        public event MouseEventHandler ImageMouseMove
        {
            add
            {
                Image.MouseMove += value;
            }

            remove
            {
                Image.MouseMove -= value;
            }
        }


        //public EditorWindow Main
        //{
        //    get { return _main; }
        //    set
        //    {
        //        _main = value;
        //        PropertyChanged(this, new PropertyChangedEventArgs("Main"));
        //    }
        //}

        //private EditorWindow _main;

        private BitmapSource _zoomedImage;

        private void Image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //if (Main != null)
            //{
            //    Image.CaptureMouse();
            //    Point pos = e.GetPosition(Image);
            //    Main.StartDragRecting((int)pos.X, (int)pos.Y);
            //    DragRectangleControl.Visibility = Visibility.Visible;
            //}
        }

        private void Image_MouseUp(object sender, MouseButtonEventArgs e)
        {
            //if (Main != null)
            //{
            //    Image.ReleaseMouseCapture();
            //    Main.StopDragRecting();
            //}
        }

        private void Image_MouseMove(object sender, MouseEventArgs e)
        {
            //if (Main != null)
            //{
            //    Point pos = e.GetPosition(Image);
            //    Main.OnMove((int)pos.X, (int)pos.Y);
            //}
        }
        
    }
}
