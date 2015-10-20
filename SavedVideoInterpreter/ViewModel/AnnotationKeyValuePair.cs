using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace SavedVideoInterpreter
{
    public class AnnotationKeyValuePair : DependencyObject
    {
        public static readonly DependencyProperty KeyProperty = DependencyProperty.Register("Key", typeof(string), typeof(AnnotationKeyValuePair));
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(string), typeof(AnnotationKeyValuePair), new PropertyMetadata(""));
        public static readonly DependencyProperty IsEditingProperty = DependencyProperty.Register("IsEditing", typeof(bool), typeof(AnnotationKeyValuePair),
            new PropertyMetadata(new PropertyChangedCallback(IsEditingChangedCallback)));

        public event DependencyPropertyChangedEventHandler IsEditingChanged;

        public string Key
        {
            get
            {
                return (string)GetValue(KeyProperty);
            }

            set
            {
                SetValue(KeyProperty, value);
            }
        }

        public string OldValue
        {
            get;
            private set;
        }

        public string Value
        {
            get
            {
                return (string)GetValue(ValueProperty);
            }

            set
            {
                SetValue(ValueProperty, value);
            }
        }

        public bool IsEditing
        {
            get
            {
                return (bool)GetValue(IsEditingProperty);
            }

            set
            {
                SetValue(IsEditingProperty, value);
            }
        }

        private static void IsEditingChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            AnnotationKeyValuePair kvp = sender as AnnotationKeyValuePair;
            if (kvp.IsEditingChanged != null)
            {
                kvp.IsEditingChanged(kvp, args);
            }

            if ((bool)args.NewValue == false)
                kvp.OldValue = kvp.Value;
        }

        public AnnotationKeyValuePair(string key, string value)
        {
            
            Key = key;
            Value = value;
            OldValue = value;
        }
    }
}
