using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace SavedVideoInterpreter
{
    public class ParameterKeyValuePair : DependencyObject
    {

        public static readonly DependencyProperty KeyProperty = DependencyProperty.Register("Key", typeof(DependencyString), typeof(ParameterKeyValuePair));
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(DependencyString), typeof(ParameterKeyValuePair));

        public DependencyString Key
        {
            get { return (DependencyString)GetValue(KeyProperty); }
            set { SetValue(KeyProperty, value); }
        }

        public DependencyString Value
        {
            get { return (DependencyString)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public ParameterKeyValuePair(string key, string value)
        {
            Key = new DependencyString(key);
            Value = new DependencyString(value);
        }
    }
}
