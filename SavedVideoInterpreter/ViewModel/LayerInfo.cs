using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace SavedVideoInterpreter
{
    public class LayerInfo : DependencyObject
    {
        public static readonly DependencyProperty NameProperty = DependencyProperty.Register("Name", typeof(DependencyString), typeof(LayerInfo));
        public static readonly DependencyProperty FileProperty = DependencyProperty.Register("File", typeof(DependencyString), typeof(LayerInfo));


        public string Code
        {
            get;
            set;
        }

        public Dictionary<string, string> Parameters
        {
            get;
            set;
        }


        public DependencyString Name
        {
            get { return (DependencyString)GetValue(NameProperty); }
            set { SetValue(NameProperty, value); }
        }

        public DependencyString File
        {
            get { return (DependencyString)GetValue(FileProperty); }
            set { SetValue(FileProperty, value); }
        }

        public LayerInfo(string name, string file)
        {
            Name = new DependencyString(name);
            if(file != null && System.IO.File.Exists(file))
                Code = System.IO.File.ReadAllText(file);

            Parameters = new Dictionary<string, string>();
            AllowSave = true;
            AllowExecute = true;
            File = new DependencyString(file);
        }

        public bool AllowSave
        {
            get;
            set;
        }

        public bool AllowExecute
        {
            get;
            set;
        }
    }
}
