using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace SavedVideoInterpreter
{
    public class GridItem : DependencyObject
    {
        //public DependencyProperty VisProperty = DependencyProperty.Register("Visibility", typeof(Visibility), typeof(GridItem));
        public string DocumentName { get; set; }

        public string Data { get; set; }

        public ICommand DeleteCommand { get; set; }

        public Visibility Visibility { get; set; }
    }
}
