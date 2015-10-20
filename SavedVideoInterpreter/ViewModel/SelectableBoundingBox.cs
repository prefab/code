using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows;

using System.Windows.Media.Imaging;
using System.Windows.Media;
using Prefab;

namespace SavedVideoInterpreter
{
    public class SelectableBoundingBox : DependencyObject, IBoundingBox
    {

        //private EditorModel _model;
        public SelectableBoundingBox(int left, int top, int width, int height, SolidColorBrush color, SolidColorBrush selectedColor = null)
        {
            
            Left = left;
            Top = top;
            Width = width;
            Height = height;
            Color = color;
            if (selectedColor != null)
                SelectedColor = selectedColor;
            else
                SelectedColor = color;

            //_model = model;
        }

        public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register("IsSelected", typeof(Boolean), typeof(SelectableBoundingBox));
        public static readonly DependencyProperty WidthProperty = DependencyProperty.Register("Width", typeof(int), typeof(SelectableBoundingBox));
        public static readonly DependencyProperty HeightProperty = DependencyProperty.Register("Height", typeof(int), typeof(SelectableBoundingBox));
        public static readonly DependencyProperty TopOffsetProperty = DependencyProperty.Register("TopOffset", typeof(int), typeof(SelectableBoundingBox));
        public static readonly DependencyProperty LeftOffsetProperty =  DependencyProperty.Register("LeftOffset", typeof(int), typeof(SelectableBoundingBox));
        public static readonly DependencyProperty ColorProperty = DependencyProperty.Register("Color", typeof(SolidColorBrush), typeof(SelectableBoundingBox));
        public static readonly DependencyProperty SelectedColorProperty = DependencyProperty.Register("SelectedColor", typeof(SolidColorBrush), typeof(SelectableBoundingBox));

        public SolidColorBrush Color
        {
            get { return (SolidColorBrush)GetValue(ColorProperty); }
            set { SetValue(ColorProperty, value); }
        }

        public SolidColorBrush SelectedColor
        {
            get { return (SolidColorBrush)GetValue(SelectedColorProperty); }
            set { SetValue(SelectedColorProperty, value); }
        }
        
        public bool IsSelected
        {
            get { return (Boolean)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }

        #region IBoundingBox Members

        public int Height
        {
            get { return (int)GetValue(HeightProperty); }
            set { SetValue(HeightProperty, value); }
        }

        public bool IsNull
        {
            get { return false; }
        }

        public int Left
        {
            get { return (int)GetValue(LeftOffsetProperty); }
            set { SetValue(LeftOffsetProperty, value); }
        }

        public int Top
        {
            get { return (int)GetValue(TopOffsetProperty); }
            set { SetValue(TopOffsetProperty, value); }
        }

        public int Width
        {
            get { return (int)GetValue(WidthProperty); }
            set { SetValue(WidthProperty, value); }
        }

        #endregion


        public BitmapSource Image
        {
            get
            {
                return null;
                //return _model.GetCurrentBitmap().Crop(TopOffset, LeftOffset, Height, Width).ToBitmapSource();
            }
        }

        public class WithTreeNode : SelectableBoundingBox
        {

            public WithTreeNode(Tree node, Tree root, int left, int top, int width, int height, SolidColorBrush color) 
                : base(left, top, width, height, color)
            {
                TreeNode = node;
                Root = root;
            }

            public enum Type
            {
                FromAnnotation,
                FromClick,
                FromTreeBrowser,
                FromSelector
            }

            public Type CreationType
            {
                get;
                set;
            }
            
            public Tree TreeNode
            {
                get;
                set;
            }

            public Tree Root
            {
                get;
                private set;
            }

            public IEnumerable<Pair> Attributes
            {
                get
                {

                    List<Pair> pairs = new List<Pair>();
                    foreach (KeyValuePair<string, object> attr in TreeNode.GetTags())
                    {
                        Pair pair = new Pair(attr.Key, attr.Value.ToString());
                        pairs.Add(pair);
                    }


                    return pairs;
                }
            }

            public class Pair
            {
                public Pair(string key, string value)
                {
                    Key = key;
                    Value = value;
                }
                public string Key
                {
                    get;
                    private set;
                }

                public string Value
                {
                    get;
                    set;
                }
            }

            
            
        }
    }
}
