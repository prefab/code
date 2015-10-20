using Prefab;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace SavedVideoInterpreter
{

    public class TextBlockRect
    {

        public Tree TreeNode
        {
            get;
            set;
        }
        public string Text
        {
            get;
            set;
        }

        public TextAlignment Alignment
        {
            get;
            set;
        }

        public double FontSize
        {
            get;
            set;
        }

        public FontFamily FontFamily
        {
            get;
            set;
        }

        public FontWeight FontWeight
        {
            get;
            set;
        }

        public SolidColorBrush Color
        {
            get;
            set;
        }

        public int Top
        {
            get;
            set;
        }

        public int Left
        {
            get;
            set;
        }

        public int Width
        {
            get;
            set;
        }

        public int Height
        {
            get;
            set;
        }

    }

}
