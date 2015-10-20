using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;

namespace SavedVideoInterpreter
{
    public class LayerChainItem : DependencyObject
    {
        public static readonly DependencyProperty NameProperty = 
            DependencyProperty.Register("Name", typeof(string), typeof(LayerChainItem));

        public static readonly DependencyProperty FullPathProperty =
          DependencyProperty.Register("FullPath", typeof(string), typeof(LayerChainItem));

        public static readonly DependencyProperty RelativePathProperty =
  DependencyProperty.Register("RelativePath", typeof(string), typeof(LayerChainItem));


        public static readonly DependencyProperty KeysProperty =
            DependencyProperty.Register("ParameterKeys", typeof(ObservableCollection<string>), typeof(LayerChainItem));

        public static readonly DependencyProperty ValuesProperty =
            DependencyProperty.Register("ParameterValues", typeof(ObservableCollection<object>), typeof(LayerChainItem));

        public override string ToString()
        {
            return Name;
        }
        public string Name
        {
            get { return (string)GetValue(NameProperty); }
            set { SetValue(NameProperty, value); }
        }

        public string FullPath
        {
            get { return (string)GetValue(FullPathProperty); }
            set { SetValue(FullPathProperty, value); }
        }

        public string RelativePath
        {
            get { return (string)GetValue(RelativePathProperty); }
            set { SetValue(RelativePathProperty, value); }
        }

        public ObservableCollection<string> ParameterKeys
        {
            get { return (ObservableCollection<string>)GetValue(KeysProperty); }
            set { SetValue(KeysProperty, value); }
        }

        public ObservableCollection<object> ParameterValues
        {
            get { return (ObservableCollection<object>)GetValue(ValuesProperty); }
            set { SetValue(ValuesProperty, value); }
        }

        public Visibility DeleteButtonVisibility
        {
            get;
            set;
        }

        public LayerChainItem() 
        {
            ParameterKeys = new ObservableCollection<string>();
            ParameterValues = new ObservableCollection<object>();
            DeleteButtonVisibility = Visibility.Visible;
        }
    }
}
