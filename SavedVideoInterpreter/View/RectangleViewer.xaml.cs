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
using System.Collections.ObjectModel;


namespace SavedVideoInterpreter
{
    /// <summary>
    /// Interaction logic for RectangleViewer.xaml
    /// </summary>
    public partial class RectangleViewer : UserControl
    {



        private event EventHandler<RectMouseEventArgs> _lookBiggerClick;
        private event EventHandler<RectMouseEventArgs> _lookSmallerClick;

        public event EventHandler<RectMouseEventArgs> RectangleLookBiggerClicked
        {
            add
            {
                _lookBiggerClick += value;
            }
            remove
            {
                _lookBiggerClick -= value;
            }
        }

        public event EventHandler<RectMouseEventArgs> RectangleLookSmallerClicked
        {
            add
            {
                _lookSmallerClick += value;
            }
            remove
            {
                _lookSmallerClick -= value;
            }
        }
       

        private event RoutedEventHandler _rectClosed;
        public event RoutedEventHandler RectangleClosed
        {
            add
            {
                _rectClosed += value;
            }

            remove
            {
                _rectClosed -= value;
            }
        }

        private event EventHandler<RectMouseEventArgs> _rectDoubleClick;
        public event EventHandler<RectMouseEventArgs> RectangleDoubleClicked
        {
            add
            {
                _rectDoubleClick += value;
            }

            remove
            {
                _rectDoubleClick -= value;
            }
        }

        public event EventHandler<RectMouseEventArgs> _rectMouseUp;
        public event EventHandler<RectMouseEventArgs> RectangleMouseUp
        {
            add
            {
                _rectMouseUp += value;
            }

            remove
            {
                _rectMouseUp -= value;
            }
        }





        public static readonly DependencyProperty RectanglesProperty = DependencyProperty.Register("Rectangles", typeof(BindingList<SelectableBoundingBox>), typeof(RectangleViewer));

        private event EventHandler _rectangleSelectionChanged;
        public event EventHandler RectangleSelectionChanged
        {
            add
            {
                _rectangleSelectionChanged += value;
            }

            remove
            {
                _rectangleSelectionChanged -= value;
            }
        }


        public BindingList<SelectableBoundingBox> Rectangles
        {
            get
            {
                return (BindingList<SelectableBoundingBox>)GetValue(RectanglesProperty);
            }

            set
            {
                SetValue(RectanglesProperty, value);

            }
        }


        private enum EditingState
        {
            None,
            Left,
            Top,
            Width,
            Height,
            LeftAndHeight,
            TopAndWidth,
            WidthAndHeight,
            TopAndLeft
        }

        private EditingState _editingState;

        public enum StyleType
        {
            UserDrawn,
            PrefabRecognized,
            FalsePositives
        }

        public StyleType StyleTemplate
        {
            get
            {
                return StyleType.PrefabRecognized;
            }

            set
            {

                switch (value)
                {
                    case StyleType.PrefabRecognized:
                        RectangleListBox.ItemTemplate = (DataTemplate)FindResource("ItemTemplate");
                        break;

                    case StyleType.UserDrawn:
                        RectangleListBox.ItemTemplate = (DataTemplate)FindResource("DrawnItemTemplate");
                        break;

                    case StyleType.FalsePositives:
                        RectangleListBox.ItemTemplate = (DataTemplate)FindResource("FalsePositiveItemTemplate");
                        break;
                }
            }
        }

        public class RectMouseEventArgs : EventArgs
        {

            public RectMouseEventArgs(SelectableBoundingBox rect)
            {
                Rect = rect;
            }

            public SelectableBoundingBox Rect
            {
                get;
                private set;
            }

        }


        private const int c_rectCornerSize = 2;

        public RectangleViewer()
        {
            DataContext = this;
            InitializeComponent();
            Rectangles = new BindingList<SelectableBoundingBox>();
            StyleTemplate = StyleType.PrefabRecognized;
        }

        private void CloseRectButton_Click(object sender, RoutedEventArgs e)
        {
            SelectableBoundingBox sb = (SelectableBoundingBox)((Button)sender).DataContext;
            Rectangles.Remove(sb);
        }


        private void CloseFalsePositive_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void CloseTruePositive_Click(object sender, RoutedEventArgs e)
        {
            if (_rectClosed != null)
            {
                SelectableBoundingBox sb = (SelectableBoundingBox)((Button)sender).DataContext;
                _rectClosed(sb, e);
            }
        }

        private void Rectangle_MouseDown(object sender, MouseButtonEventArgs e)
        {
           
            Rectangle rect = (Rectangle)sender;
            Point pos = e.GetPosition(rect);
            SelectableBoundingBox sbb = (SelectableBoundingBox)rect.DataContext;


                if (e.ClickCount == 2)
                {
                    if (_rectDoubleClick != null)
                        _rectDoubleClick(this, new RectMouseEventArgs((SelectableBoundingBox)((Rectangle)sender).DataContext));
                }

                _editingState = GetMouseOverState(sbb, pos);
                if (_editingState != EditingState.None)
                {
                    rect.CaptureMouse();
                }
                else
                {
                    sbb.IsSelected = !sbb.IsSelected;

                    if (_rectangleSelectionChanged != null)
                        _rectangleSelectionChanged(sbb, null);
                }
            
        }

        private EditingState GetMouseOverState(SelectableBoundingBox sbb, Point pos)
        {
            if (pos.X <= c_rectCornerSize && pos.Y > c_rectCornerSize && pos.Y < sbb.Height - c_rectCornerSize)
            {
                return EditingState.Left;  
            }

            else if (pos.X >= sbb.Width - c_rectCornerSize && pos.Y > c_rectCornerSize && pos.Y < sbb.Height - c_rectCornerSize)
            {
                return EditingState.Width;
                
            }
            else if (pos.Y <= c_rectCornerSize && pos.X > c_rectCornerSize && pos.X < sbb.Width - c_rectCornerSize)
            {
                return EditingState.Top;
                
            }
            else if (pos.Y >= sbb.Height - c_rectCornerSize && pos.X > c_rectCornerSize && pos.X < sbb.Width - c_rectCornerSize)
            {
                return EditingState.Height;
            }
            else if (pos.X <= c_rectCornerSize && pos.Y >= sbb.Height - c_rectCornerSize)
            {
                return EditingState.LeftAndHeight;
            }
            else if (pos.X >= sbb.Width - c_rectCornerSize && pos.Y <= c_rectCornerSize)
            {
                 return EditingState.TopAndWidth;
            }
            else if (pos.X >= sbb.Width - c_rectCornerSize && pos.Y >= sbb.Height - c_rectCornerSize)
            {
                return EditingState.WidthAndHeight;

            }
            else if (pos.X <= c_rectCornerSize && pos.Y <= c_rectCornerSize)
            {
                return EditingState.TopAndLeft;
            }
            else
            {
                return EditingState.None;
            }
        }

        private void NonEditableRectangle_MouseDown(object sender, MouseButtonEventArgs e)
        {
           
            if (e.ClickCount == 2)
            {
                if (_rectDoubleClick != null)
                    _rectDoubleClick(this, new RectMouseEventArgs((SelectableBoundingBox)((Rectangle)sender).DataContext));
            }

            Rectangle rect = (Rectangle)sender;
            SelectableBoundingBox sbb = (SelectableBoundingBox)rect.DataContext;

            if (e.ChangedButton == MouseButton.Right)
            {
                //ContextMenu menu = new ContextMenu();
                //MenuItem bigger = new MenuItem();
                //bigger.Header = "View Parent";

                //MenuItem smaller = new MenuItem();
                //smaller.Header = "View Children";

                //Separator sep = new Separator();

                //MenuItem delete = new MenuItem();
                //delete.Header = "Delete Prototype";

                //bigger.Click += bigger_Click;
                //smaller.Click += smaller_Click;
                //menu.Items.Add(bigger);
                //menu.Items.Add(smaller);
                //menu.Items.Add(sep);
                //menu.Items.Add(delete);

                //rect.ContextMenu = menu;
                //rect.ContextMenu.Visibility = System.Windows.Visibility.Visible;

            }
            else
            {

                sbb.IsSelected = !sbb.IsSelected;

                if (_rectMouseUp != null)
                    _rectMouseUp(this, new RectMouseEventArgs((SelectableBoundingBox)rect.DataContext));

                if (_rectangleSelectionChanged != null)
                    _rectangleSelectionChanged(sbb, null);
            }
        }

        void smaller_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menuitem = sender as MenuItem;
            SelectableBoundingBox sbb = menuitem.DataContext as SelectableBoundingBox;
            if (_lookSmallerClick != null)
            {
                _lookSmallerClick(sbb, new RectMouseEventArgs(sbb));
            }
        }

        void bigger_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menuitem = sender as MenuItem;
            SelectableBoundingBox sbb = menuitem.DataContext as SelectableBoundingBox;
            if (_lookBiggerClick != null)
            {
                _lookBiggerClick(sbb, new RectMouseEventArgs(sbb));
            }
        }

        private void Rectangle_MouseLeave(object sender, MouseEventArgs e)
        {
            Cursor = Cursors.Arrow;
        }

        private void Rectangle_MouseMove(object sender, MouseEventArgs e)
        {
            if (Visibility == System.Windows.Visibility.Visible)
            {
                Rectangle rect = (Rectangle)sender;
                SelectableBoundingBox sbb = (SelectableBoundingBox)rect.DataContext;
                Point pos = e.GetPosition(rect);
                switch (_editingState)
                {
                    case EditingState.Left:
                        Cursor = Cursors.SizeWE;
                        sbb.Left += (int)pos.X;
                        sbb.Width -= (int)pos.X;
                        break;
                    case EditingState.Top:
                        Cursor = Cursors.SizeNS;
                        sbb.Top += (int)pos.Y;
                        sbb.Height -= (int)pos.Y;
                        break;

                    case EditingState.Width:
                        Cursor = Cursors.SizeWE;
                        sbb.Width = (int)pos.X;
                        break;

                    case EditingState.Height:
                        Cursor = Cursors.SizeNS;
                        sbb.Height = (int)pos.Y;
                        break;

                    case EditingState.LeftAndHeight:
                        Cursor = Cursors.SizeNESW;
                        sbb.Left += (int)pos.X;
                        sbb.Width -= (int)pos.X;
                        sbb.Height = (int)pos.Y;
                        break;

                    case EditingState.TopAndWidth:
                        Cursor = Cursors.SizeNESW;
                        sbb.Top += (int)pos.Y;
                        sbb.Height -= (int)pos.Y;
                        sbb.Width = (int)pos.X;
                        break;

                    case EditingState.TopAndLeft:
                        Cursor = Cursors.SizeNWSE;
                        sbb.Top += (int)pos.Y;
                        sbb.Height -= (int)pos.Y;
                        sbb.Left += (int)pos.X;
                        sbb.Width -= (int)pos.X;
                        break;

                    case EditingState.WidthAndHeight:
                        Cursor = Cursors.SizeNWSE;
                        sbb.Width = (int)pos.X;
                        sbb.Height = (int)pos.Y;
                        break;

                    case EditingState.None:
                        EditingState mouseover = GetMouseOverState(sbb, pos);
                        switch (mouseover)
                        {
                            case EditingState.Left:
                                Cursor = Cursors.SizeWE;
                                break;
                            case EditingState.Top:
                                Cursor = Cursors.SizeNS;
                                break;

                            case EditingState.Width:
                                Cursor = Cursors.SizeWE;
                                break;

                            case EditingState.Height:
                                Cursor = Cursors.SizeNS;
                                break;

                            case EditingState.LeftAndHeight:
                                Cursor = Cursors.SizeNESW;
                                break;

                            case EditingState.TopAndWidth:
                                Cursor = Cursors.SizeNESW;
                                break;

                            case EditingState.WidthAndHeight:
                                Cursor = Cursors.SizeNWSE;
                                break;


                            case EditingState.TopAndLeft:
                                Cursor = Cursors.SizeNWSE;
                                break;

                            case EditingState.None:
                                Cursor = Cursors.Arrow;
                                break;
                        }

                        break;
                }


                if (sbb.Left < 0)
                    sbb.Left = 0;
                if (sbb.Top < 0)
                    sbb.Top = 0;
                if ( sbb.Left + sbb.Width > Width)
                    sbb.Width = (int)Width - sbb.Left;
                if (sbb.Height + sbb.Top > Height)
                    sbb.Height = (int)Height - sbb.Top;
            }
        }

        private void Rectangle_MouseUp(object sender, MouseButtonEventArgs e)
        {

            _editingState = EditingState.None;
            Rectangle rect = (Rectangle)sender;
            rect.ReleaseMouseCapture();
        }

       



    }
}
