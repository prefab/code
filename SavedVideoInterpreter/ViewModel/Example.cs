using Prefab;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SavedVideoInterpreter
{
    public class Example : DependencyObject
    {

        public Example(Image image, string annotationLib, ImageAnnotation annotation)
        {
            Img = image.Source;
            Annotation = annotation;
            AnnotationLibrary = annotationLib;
        }
        public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register("IsSelected", typeof(bool), typeof(Example));
        public static readonly DependencyProperty SrcProperty = DependencyProperty.Register("Img", typeof(ImageSource), typeof(Example));



        public ImageAnnotation Annotation
        {
            get;
            private set;
        }

        public string AnnotationLibrary
        {
            get;
            private set;
        }


        public ImageSource Img
        {
            get
            {
                return (ImageSource)GetValue(SrcProperty);
            }
            set
            {
                SetValue(SrcProperty, value);
            }
        }

        public bool IsSelected
        {
            get
            {
                return (bool)GetValue(IsSelectedProperty);
            }

            set
            {
                SetValue(IsSelectedProperty, value);
            }
        }
    }
}
