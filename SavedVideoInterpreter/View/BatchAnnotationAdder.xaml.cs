using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SavedVideoInterpreter
{
    /// <summary>
    /// Interaction logic for BatchAnnotationAdder.xaml
    /// </summary>
    public partial class BatchAnnotationAdder : Window
    {
        public static readonly DependencyProperty LibrariesProperty = DependencyProperty.Register("Libraries", typeof(BindingList<string>), typeof(BatchAnnotationAdder));

        private event EventHandler<PropertiesControl.AnnotationAddedArgs> _added;


        public event EventHandler<PropertiesControl.AnnotationAddedArgs> AnnotationAdded
        {
            add
            {
                _added += value;
            }

            remove
            {
                _added -= value;
            }
        }


        public BindingList<string> Libraries
        {
            get
            {
                return (BindingList<string>)GetValue(LibrariesProperty);
            }
            set
            {
                SetValue(LibrariesProperty, value);
            }
        }

        public BatchAnnotationAdder()
        {
            InitializeComponent();
            Libraries = new BindingList<string>();
        }

        private void Submit_Click(object sender, RoutedEventArgs e)
        {
            if (_added != null)
            {
                _added(this, new PropertiesControl.AnnotationAddedArgs(LibraryBox.SelectedItem.ToString(), ValueBox.Text));
            }
        }

        internal void SetLibraries(IEnumerable<string> libs)
        {
            Libraries.Clear();
            foreach (string lib in libs)
                Libraries.Add(lib);

            Libraries.Add("+ Add a New Library");
            
            LibraryBox.SelectedIndex = 0;
        }

        private void LibraryBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LibraryBox.SelectedIndex == Libraries.Count - 1)
            {
                LibraryBox.SelectedValue = "Type a name here.";
            }
        }

        private void LibraryBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!Libraries.Contains(LibraryBox.Text))
            {
                Libraries.Add(LibraryBox.Text);       
            }
        }

        private void ValueBox_Loaded(object sender, RoutedEventArgs e)
        {
            ValueBox.Text = "{ \"create_a_tag_name_like_this\" = \"create_a_tag_value_like_this\" }";
        }
    }
}
