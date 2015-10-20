using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.ComponentModel;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using Prefab;
using System.Windows.Data;
using PrefabIdentificationLayers.Prototypes;

namespace SavedVideoInterpreter
{
    public class ViewablePrototypeItem : DependencyObject
    {

        public ViewablePrototypeItem(Ptype ptype, string library, IEnumerable<ImageAnnotation> positives, IEnumerable<ImageAnnotation> negatives, Bitmap bitmap = null)
        {


            if (bitmap != null)
            {
                IsContent = true;
                Image img = ToImage(bitmap);
                CapturedImage = img;
            }
            else
            {
                Guid = ptype.Id;

                PrototypeVisual = ptype;

            }

            Dictionary<string, Bitmap> screenshots = new Dictionary<string, Bitmap>();
            if (positives != null)
            {
                PositiveExamples = new ObservableCollection<Example>();
                foreach (ImageAnnotation img in positives)
                {
                    
                    if (!screenshots.ContainsKey(img.ImageId))
                        screenshots[img.ImageId] = AnnotationLibrary.GetImage(library, img.ImageId);
                     
                    Bitmap screenshot = screenshots[img.ImageId];


                    Image exampleImage = GetImageFromExample(screenshot, img.Region);
                    Example example = new Example(exampleImage, library, img);
                    PositiveExamples.Add(example);

                }
            }

            if (negatives != null)
            {

                NegativeExamples = new ObservableCollection<Example>();
                foreach (ImageAnnotation img in negatives)
                {
                    if (!screenshots.ContainsKey(img.ImageId))
                        screenshots[img.ImageId] = AnnotationLibrary.GetImage(library, img.ImageId);

                    Bitmap screenshot = screenshots[img.ImageId];


                    Image exampleImage = GetImageFromExample(screenshot, img.Region);
                    Example example = new Example(exampleImage, library, img);
                    NegativeExamples.Add(example);
                }
            }

            
        }

        public static Image ToImage(Bitmap bitmap)
        {
            Image img = new Image();
            img.Source = ToBitmapSource(bitmap);
            img.SnapsToDevicePixels = true;
            img.Stretch = System.Windows.Media.Stretch.Uniform;
            RenderOptions.SetBitmapScalingMode(img, BitmapScalingMode.NearestNeighbor);

            return img;
        }

        private static Image GetImageFromExample(Bitmap screenshot, IBoundingBox region)
        {
            Bitmap example = Bitmap.Crop(screenshot, region);
            Image exampleImage = new Image();
            exampleImage.Source = ToBitmapSource(example);
            exampleImage.Stretch = Stretch.Uniform;
            RenderOptions.SetBitmapScalingMode(exampleImage, BitmapScalingMode.NearestNeighbor);

            return exampleImage;
        }


        public static BitmapSource ToBitmapSource(Bitmap bitmap)
        {
            List<System.Windows.Media.Color> colors = new List<System.Windows.Media.Color>();
            colors.Add(System.Windows.Media.Colors.Red);
            colors.Add(System.Windows.Media.Colors.Blue);
            colors.Add(System.Windows.Media.Colors.Green);
            BitmapPalette pallete = new BitmapPalette(colors);

            BitmapSource source = BitmapSource.Create(bitmap.Width, bitmap.Height, 96, 96, System.Windows.Media.PixelFormats.Bgra32,
                pallete, bitmap.Pixels, 4 * bitmap.Width);

            return source;
        }


        public static readonly DependencyProperty PositiveExamplesProperty =
    DependencyProperty.Register("PositiveExamples", typeof(ObservableCollection<Example>), typeof(ViewablePrototypeItem));

        public static readonly DependencyProperty NegativeExamplesProperty =
  DependencyProperty.Register("NegativeExamples", typeof(ObservableCollection<Example>), typeof(ViewablePrototypeItem));


        public static readonly DependencyProperty PrototypeVisualProperty =
            DependencyProperty.Register("PrototypeVisual", typeof(Ptype), typeof(ViewablePrototypeItem));


        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register("IsSelected", typeof(bool), typeof(ViewablePrototypeItem));



        public static readonly DependencyProperty IsContentProperty =
            DependencyProperty.Register("IsContent", typeof(bool), typeof(ViewablePrototypeItem));




        public bool IsContent
        {
            get
            {
                return (bool)GetValue(IsContentProperty);
            }

            set
            {
                SetValue(IsContentProperty, value);
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

        public ObservableCollection<Example> PositiveExamples
        {
            get
            {
                return (ObservableCollection<Example>)GetValue(PositiveExamplesProperty);
            }
            set
            {
                SetValue(PositiveExamplesProperty, value);
            }
        }

        public ObservableCollection<Example> NegativeExamples
        {
            get
            {
                return (ObservableCollection<Example>)GetValue(NegativeExamplesProperty);
            }
            set
            {
                SetValue(NegativeExamplesProperty, value);
            }
        }

        public Ptype PrototypeVisual
        {
            get
            {
                return (Ptype)GetValue(PrototypeVisualProperty);
            }

            set
            {
                SetValue(PrototypeVisualProperty, value);
            }
        }

        public string Guid
        {
            get;
            private set;
        }

        public Image CapturedImage
        {
            get;
            private set;
        }

    }
}
