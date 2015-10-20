using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace SavedVideoInterpreter
{
    /// <summary>
    /// Interaction logic for AnnotationGridView.xaml
    /// </summary>
    public partial class AnnotationGridView : UserControl
    {

        public static readonly DependencyProperty ScreenshotsProperty = DependencyProperty.Register("Screenshots", typeof(VirtualizingCollection<BitmapSource>), typeof(AnnotationGridView));
        public static readonly DependencyProperty SelectedIndexProperty = DependencyProperty.Register("SelectedIndex", typeof(int), typeof(AnnotationGridView));
      
        public AnnotationGridView()
        {
            InitializeComponent();
            DataContext = this;
            
        }

        public event SelectionChangedEventHandler SelectionChanged
        {
            add
            {
                _selectionChanged += value;
            }

            remove
            {
                _selectionChanged -= value;
            }
        }

        


        private event SelectionChangedEventHandler _selectionChanged;
        
        public VirtualizingCollection<BitmapSource> Screenshots
        {

            get
            {
                return (VirtualizingCollection<BitmapSource>)GetValue(ScreenshotsProperty);
            }

            set
            {
                SetValue(ScreenshotsProperty, value);
            }

        }

        public int SelectedIndex
        {
            get
            {
                return (int)GetValue(SelectedIndexProperty);
            }
            set
            {

                SetValue(SelectedIndexProperty, value);
                ListBoxView.ScrollIntoView(Screenshots[value]);
            }
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_selectionChanged != null)
                _selectionChanged(sender, e);
        }
    }
}
