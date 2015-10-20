using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;
using DropDownCustomColorPicker;
using Prefab;
namespace SavedVideoInterpreter
{
    public class Selector : DependencyObject
    {
        public static readonly DependencyProperty ColorProperty = CustomColorPicker.ColorProperty.AddOwner( typeof(Selector)
            , new FrameworkPropertyMetadata(new PropertyChangedCallback(ColorChanged)));

        //public static readonly DependencyProperty SelectedColorProperty = DependencyProperty.Register("SelectedColor",
        //    typeof(SolidColorBrush), typeof(Selector));

        public static readonly DependencyProperty SelectorTextProperty = DependencyProperty.Register("SelectorText", 
            typeof(string), typeof(Selector)
            , new FrameworkPropertyMetadata(new PropertyChangedCallback(TextChanged)));

        public static readonly DependencyProperty ShowProperty = DependencyProperty.Register("Show", 
            typeof(bool), typeof(Selector));

        

        public bool Show
        {
            get { return (bool)GetValue(ShowProperty); }
            set { SetValue(ShowProperty, value); }
        }
        public SolidColorBrush Color
        {
            get { return (SolidColorBrush)GetValue(ColorProperty); }
            set { SetValue(ColorProperty, value); }
        }

        public SolidColorBrush SelectedColor
        {
            //get { return (SolidColorBrush)GetValue(SelectedColorProperty); }
            //set { SetValue(SelectedColorProperty, value); }

            get;
            set;
        }

        public string SelectorText
        {
            get { return (string)GetValue(SelectorTextProperty); }
            set { SetValue(SelectorTextProperty, value); }
        }

        public Func<Tree, bool> SelectorCode
        {
            get;
            private set;
        }

        public Selector()
        {
            Color color =  System.Windows.Media.Color.FromArgb(80, 254, 162, 254);
            Color = new SolidColorBrush(color);
            SelectorText = "";
        }

        public Selector(Color color, string text)
        {
            Color = new SolidColorBrush(color);
            SelectorText = text;
        }

        private static void ColorChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            Selector selector = obj as Selector;
            
        }

        private static void SelectedColorChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {

        }

        private static void TextChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            Selector selector = obj as Selector;
            selector.SelectorCode = ParseString(args.NewValue as string);
        }


        private static bool NodeContainsAttributeIgnoreCase(Tree node, string attribute)
        {
            var attrs = node.GetTags();
            foreach (var attr in attrs)
            {
                if (attr.Key.ToLower().Contains(attribute.ToLower()))
                    return true;
            }

            return false;
        }

        

        private static Func<Tree, bool> ParseString(string selector)
        {
            try
            {
                //This is a really cheap way of parsing this stuff now.
                //TODO: make a better parser that supports more general operations.
                selector = selector.ToLower();
                string[] splitWhite = selector.Split(new char[]{' '}, StringSplitOptions.RemoveEmptyEntries);
                List<string> split = new List<string>(splitWhite);

                Regex regex = new Regex("((child )|(parent ))*(has (.+))|((.+) != (.+))|((.+) = (.+))");
                MatchCollection matches = regex.Matches(selector);

                if (matches.Count > 0)
                    Console.WriteLine("success");

                //int comparitorIndex = 0;


                if (split.Count() == 2 && split.ElementAt(0).Equals("has"))
                    return node => NodeContainsAttributeIgnoreCase(node, split.ElementAt(1));

                if (split.Count() != 3)
                    throw new Exception();

                switch (split.ElementAt(1))
                {
                    case "=":
                        string attribute = split.ElementAt(0);
                        string value = split.ElementAt(2);
                        if (attribute.Equals("is_leaf"))
                            return (node) => TestLeaf(node, value);

                         return (node) => AttributeEquals(node, split.ElementAt(0), split.ElementAt(2));
                    
                    default:
                        return new Func<Tree, bool>( (node) => false );   
                }
            }

            catch
            {
                return new Func<Tree, bool>( (node) => false );
            }
        }

       

        private static bool TestLeaf(Tree node, string value)
        {
            bool boolValue = false;
            bool couldParse = bool.TryParse(value, out boolValue);

            if (!couldParse)
                return false;

            bool isLeaf = ( !(node.HasTag("type") && node["type"].Equals("frame")) && node.GetChildren().Count() == 0 );

            return boolValue == isLeaf;
        }

        private static bool AttributeEquals(Tree node, string attr, string value)
        {
            return node.HasTag(attr) && node[attr].ToString().ToLower().Equals(value);

        }
    }
}
