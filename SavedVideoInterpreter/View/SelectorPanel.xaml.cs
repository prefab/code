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

namespace SavedVideoInterpreter
{
    /// <summary>
    /// Interaction logic for SelectorPanel.xaml
    /// </summary>
    public partial class SelectorPanel : UserControl
    {

        public static readonly DependencyProperty SelectorsProperty = DependencyProperty.Register("Selectors", typeof(BindingList<Selector>), typeof(SelectorPanel));
        
        public BindingList<Selector> Selectors
        {
            get
            {
                return (BindingList<Selector>)GetValue(SelectorsProperty);
            }

            set
            {
                SetValue(SelectorsProperty, value);

            }
        }

        public event EventHandler _selectorChecked;
        public event EventHandler SelectorChecked
        {
            add
            {
                _selectorChecked += value;
            }

            remove
            {
                _selectorChecked -= value;
            }
        }

        public event EventHandler _selectorUnChecked;
        public event EventHandler SelectorUnChecked
        {
            add
            {
                _selectorUnChecked += value;
            }

            remove
            {
                _selectorUnChecked -= value;
            }
        }
        
        

        private event EventHandler _selectorChanged;
        public event EventHandler SelectorChanged
        {
            add
            {
                _selectorChanged += value;
            }
            remove
            {
                _selectorChanged -= value;
            }
        }



        public SelectorPanel()
        {
            DataContext = this;
            InitializeComponent();
            Selectors = new BindingList<Selector>();
            
        }

        private void CustomCP_SelectedColorChanged(object sender, DropDownCustomColorPicker.ColorChangedEventArgs e)
        {
            if (_selectorChanged != null)
                _selectorChanged(sender, e);
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_selectorChanged != null)
                _selectorChanged(sender, e);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Selector s = new Selector();
            Selectors.Add(s);
        }

        private void ShowBox_Checked(object sender, RoutedEventArgs e)
        {
            if (_selectorChecked != null)
                _selectorChecked(sender, e);
        }

        private void ShowBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if(_selectorUnChecked != null){
                _selectorUnChecked(sender, e) ;
            }
        }





    }
}
