using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace SavedVideoInterpreter
{
    public class DependencyString : DependencyObject
    {
        public string Value
        {
            get { return (string)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public bool IsEditing
        {
            get { return (bool)GetValue(IsEditingProperty); }
            set { SetValue(IsEditingProperty, value); }
        }

        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }

        public string OldValue
        {
            get;
            set;
        }
        public event DependencyPropertyChangedEventHandler IsEditingChanged;

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(string), typeof(DependencyString), new UIPropertyMetadata(""));
        public static readonly DependencyProperty IsEditingProperty =
            DependencyProperty.Register("IsEditing", typeof(bool), typeof(DependencyString), new UIPropertyMetadata(false, new PropertyChangedCallback(IsEditingChangedCallback)));

        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register("IsSelected", typeof(bool), typeof(DependencyString));
        private static void IsEditingChangedCallback(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            DependencyString str = obj as DependencyString;

            if (str.IsEditingChanged != null)
                str.IsEditingChanged(str, args);

            if ((bool)args.NewValue == false)
                str.OldValue = str.Value;
        }

        public override string ToString()
        {
            return Value;
        }

        public DependencyString(string s)
        {
            Value = s;
            OldValue = s;
        }


    }

}
