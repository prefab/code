using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Prefab;
using PrefabIdentificationLayers.Prototypes;
using PrefabIdentificationLayers.Features;
using PrefabIdentificationLayers.Regions;
using System.Windows.Shapes;

namespace SavedVideoInterpreter
{
    public class PtypeToVisualConverter : DependencyObject, IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {

            Ptype ptype = value as Ptype;
            if (ptype != null)
            {
                return _visualizers[ptype.Model.Name].Visualize(ptype, parameter);
            }

           return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private static readonly Dictionary<string, IPtypeVisualizer> _visualizers = new Dictionary<string, IPtypeVisualizer>()
            { 
                {"onepart", OnePartVisualizer.Instance},
                {"ninepart", NinePartVisualizer.Instance},
                {"null", new NullVisualizer()}
            };

        private class NullVisualizer : IPtypeVisualizer
        {

            public Visual Visualize(Ptype ptype, object parameter)
            {
                return new Grid();
            }
        }


        private class OnePartVisualizer : IPtypeVisualizer
        {
            public static readonly OnePartVisualizer Instance = new OnePartVisualizer();
            private OnePartVisualizer() { }
            public Visual Visualize(Ptype ptype, object parameter = null)
            {
                if (!ptype.Model.Name.Equals("onepart"))
                {
                    throw new Exception("One Part Model cannot render prototypes created by other models.");
                }

                Feature part = ptype.Feature("part");

                Image img = new Image();
                img.Source = ViewablePrototypeItem.ToBitmapSource(part.Bitmap);
                img.SnapsToDevicePixels = true;
                img.Stretch = System.Windows.Media.Stretch.Uniform;
                RenderOptions.SetBitmapScalingMode(img, BitmapScalingMode.NearestNeighbor);

                return img;
            }
        }

        private class NinePartVisualizer : IPtypeVisualizer
        {

            public static readonly NinePartVisualizer Instance = new NinePartVisualizer();
            private NinePartVisualizer() { }
            public Visual Visualize(Ptype ptype, object parameter = null)
            {
                if (!ptype.Model.Name.Equals("ninepart"))
                {
                    throw new Exception("Nine Part Model cannot render prototypes created by other models.");
                }

                Feature bottomleftFeature = ptype.Feature("bottomleft");
                Feature toprightFeature = ptype.Feature("topright");
                Feature bottomrightFeature = ptype.Feature("bottomright");
                Feature topleftFeature = ptype.Feature("topleft");

                Region topregion = ptype.Region("top");
                Region leftregion = ptype.Region("left");
                Region bottomregion = ptype.Region("bottom");
                Region rightregion = ptype.Region("right");
                Region interiorRegion = ptype.Region("interior");

                Grid grid = new Grid();

                ////Find the biggest dimensions for each Part object so that
                ////we can figure out how many cells to add into the grid.
                int longestTopOrBottom = (int)Math.Max(topregion.Bitmap.Width, bottomregion.Bitmap.Width);
                int longestLeftOrRight = (int)Math.Max(leftregion.Bitmap.Height, rightregion.Bitmap.Height);
                int widestTLOrBL = (int)Math.Max(topleftFeature.Bitmap.Width, bottomleftFeature.Bitmap.Width);
                int widestTROrBR = (int)Math.Max(toprightFeature.Bitmap.Width, bottomrightFeature.Bitmap.Width);
                int tallestTLOrTR = (int)Math.Max(topleftFeature.Bitmap.Height, toprightFeature.Bitmap.Height);
                int tallestBLOrBR = (int)Math.Max(bottomleftFeature.Bitmap.Height, bottomrightFeature.Bitmap.Height);
                int interiorWidth = 0;
                int interiorHeight = 0;

                if (interiorRegion != null)
                {
                    interiorHeight = interiorRegion.Bitmap.Height;
                    interiorWidth = interiorRegion.Bitmap.Width;
                }

                //Assign the width and height of the grid.
                int width = Math.Max(widestTLOrBL + longestTopOrBottom + widestTROrBR + 2, interiorWidth + leftregion.Bitmap.Width + rightregion.Bitmap.Width + 2);
                int height = Math.Max(tallestTLOrTR + longestLeftOrRight + tallestBLOrBR + 2, interiorHeight + topregion.Bitmap.Height + bottomregion.Bitmap.Height + 2);

                //Set the rows and columns of the grid.
                for (int row = 0; row < height; row++)
                {
                    RowDefinition rowdef = new RowDefinition();
                    grid.RowDefinitions.Add(rowdef);
                }

                for (int col = 0; col < width; col++)
                {
                    ColumnDefinition coldef = new ColumnDefinition();
                    grid.ColumnDefinitions.Add(coldef);
                }

                //Add each Part to the grid (cells = pixels).
                PtypeVisualizerHelper.AddFeatureToGrid(topleftFeature, new Prefab.Point(0, 0), grid);
                PtypeVisualizerHelper.AddFeatureToGrid(toprightFeature, new Prefab.Point(width - toprightFeature.Bitmap.Width, 0), grid);
                PtypeVisualizerHelper.AddFeatureToGrid(bottomleftFeature, new Prefab.Point(0, height - bottomleftFeature.Bitmap.Height), grid);
                PtypeVisualizerHelper.AddFeatureToGrid(bottomrightFeature, new Prefab.Point(width - bottomrightFeature.Bitmap.Width, height - bottomrightFeature.Bitmap.Height), grid);


                PtypeVisualizerHelper.AddHorizontalRegionToGrid(topregion.Bitmap, new Prefab.Point(topleftFeature.Bitmap.Width + 1, 0), grid);
                PtypeVisualizerHelper.AddHorizontalRegionToGrid(bottomregion.Bitmap, new Prefab.Point(bottomleftFeature.Bitmap.Width + 1, height - bottomregion.Bitmap.Height), grid);
                PtypeVisualizerHelper.AddVerticalRegionToGrid(leftregion.Bitmap, new Prefab.Point(0, topleftFeature.Bitmap.Height + 1), grid);
                PtypeVisualizerHelper.AddVerticalRegionToGrid(rightregion.Bitmap, new Prefab.Point(width - rightregion.Bitmap.Width, toprightFeature.Bitmap.Height + 1), grid);


                if (interiorRegion != null)
                {
                    Prefab.Point location;
                    if (interiorRegion.MatchStrategy.Equals("horizontal"))
                        location = new Prefab.Point(topleftFeature.Bitmap.Width + 1, (height - interiorHeight) / 2);
                    else
                        location = new Prefab.Point((width - interiorWidth) / 2, topleftFeature.Bitmap.Height + 1);

                    PtypeVisualizerHelper.AddEachPixelOfBitmapToGrid(interiorRegion.Bitmap, location, grid);
                }


                return grid;



            }
        }

                /// <summary>
    /// Helper class to visualize prototypes. Use any of these static functions to render
    /// a prototype.
    /// </summary>
    public static class PtypeVisualizerHelper {

        /// <summary>
        /// Adds either the top or bottom region to the grid.
        /// </summary>
        /// <param name="region"></param>
        /// <param name="upperleft"></param>
        /// <param name="grid"></param>
        public static void AddHorizontalRegionToGrid(Bitmap horizontalPattern, Prefab.Point upperleft, Grid grid)
        {
            for (int row = 0; row < horizontalPattern.Height; row++)
            {
                for (int col = 0; col < horizontalPattern.Width; col++)
                {
                    Prefab.Point location = new Prefab.Point(upperleft.X + col, upperleft.Y + row);
                    AddPixelToGrid(horizontalPattern[row, col], location, grid);
                }
            }

        }

        /// <summary>
        /// Adds either the left or right region to the grid.
        /// </summary>
        /// <param name="region"></param>
        /// <param name="upperleft"></param>
        /// <param name="grid"></param>
        public static void AddVerticalRegionToGrid(Bitmap verticalPattern, Prefab.Point upperleft, Grid grid)
        {

            for (int row = 0; row < verticalPattern.Height; row++)
            {
                for (int col = 0; col < verticalPattern.Width; col++)
                {
                    Prefab.Point location = new Prefab.Point(upperleft.X + col, upperleft.Y + row);
                    AddPixelToGrid(verticalPattern[row, col], location, grid);
                }
            }
        }

        /// <summary>
        /// Adds a pixel to the grid. It assignes the specified cell with the
        /// specified pixel value by adding a colored rectangle in that cell.
        /// </summary>
        /// <param name="pixel"></param>
        /// <param name="location"></param>
        /// <param name="grid"></param>
        public static void AddPixelToGrid(Int32 pixel, Prefab.Point location, Grid grid) {
            Rectangle rect = new Rectangle();

            if (pixel == Feature.TRANSPARENT_VALUE)
                rect.Fill = Brushes.Transparent;
            else
                rect.Fill = new SolidColorBrush(Color.FromArgb((byte)Bitmap.Alpha(pixel),
                    (byte)Bitmap.Red(pixel), (byte)Bitmap.Green(pixel), (byte)Bitmap.Blue(pixel)));

            rect.Stroke = Brushes.Black;
            rect.StrokeThickness = 0.5;
            grid.Children.Add(rect);

            Grid.SetRow(rect, location.Y);
            Grid.SetColumn(rect, location.X);
        }

        /// <summary>
        /// Renders a corner part to a grid at the specified location. Each pixel
        /// of the corner is used to color a cell of the grid.
        /// </summary>
        /// <param name="corner">The corner feature to render.</param>
        /// <param name="location">The location in the grid where the corner will be rendered.</param>
        /// <param name="grid">The grid object that will be populated with the corner's pixels</param>
        public static void AddFeatureToGrid(Feature feature, Prefab.Point location, Grid grid)
        {
            for (int row = 0; row < feature.Bitmap.Height; row++)
            {
                for (int col = 0; col < feature.Bitmap.Width; col++)
                {
                    AddPixelToGrid(feature.Bitmap[row, col], new Prefab.Point(location.X + col, location.Y + row), grid);
                }
            }
        }

        /// <summary>
        /// Adds an image to a grid. The ColumnSpan and RowSpan properties are set equal to the
        /// dimensions of the bitmap.
        /// </summary>
        /// <param name="image"></param>
        /// <param name="location"></param>
        /// <param name="grid"></param>
        public static void AddImageToGrid(Bitmap image, Prefab.Point location, Grid grid) {
            Image img = ViewablePrototypeItem.ToImage(image);

            grid.Children.Add(img);
            Grid.SetColumn(img, location.X);
            Grid.SetRow(img, location.Y);

            Grid.SetColumnSpan(img, image.Width);
            Grid.SetRowSpan(img, image.Height);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="visual"></param>
        /// <param name="location"></param>
        /// <param name="grid"></param>
        public static void AddVisualToGrid(UIElement visual, Prefab.Point location, Grid grid)
        {
            grid.Children.Add(visual);
            Grid.SetColumn(visual, location.X);
            Grid.SetRow(visual, location.Y);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bitmap"></param>
        /// <param name="location"></param>
        /// <param name="grid"></param>
        public static void AddEachPixelOfBitmapToGrid(Bitmap bitmap, Prefab.Point location, Grid grid)
        {
            for (int row = 0; row < bitmap.Height; row++)
            {
                for (int col = 0; col < bitmap.Width; col++)
                {
                    AddPixelToGrid(bitmap[row, col], new Prefab.Point(location.X + col, location.Y + row), grid);
                }
            }
        }
    }
        //private class VerticalScrollVisualizer : IPtypeVisualizer
        //{
        //    private PtypeToVisualConverter _converter = new PtypeToVisualConverter();
        //    public System.Windows.Media.Visual Visualize(Ptype ptype, object parameter = null)
        //    {
        //        Ptype.Hierarchical hptype = ptype as Ptype.Hierarchical;
        //        DockPanel panel = new DockPanel();
        //        StackPanel sp = new StackPanel();
        //        panel.Children.Add(sp);
        //        DockPanel.SetDock(sp, Dock.Right | Dock.Left | Dock.Top | Dock.Bottom);

        //        ContentControl cc = new ContentControl();
        //        cc.Content = (Visual)_converter.Convert(hptype.GetChild("top"), typeof(Visual), null, null);
        //        sp.Children.Add(cc);
        //        cc.Margin = new Thickness(0);
        //        cc.MaxHeight = 10;

        //        cc = new ContentControl();
        //        cc.Margin = new Thickness(2);
        //        cc.Content = _converter.Convert(hptype.GetChild("thumb"), typeof(Visual), null, null);
        //        sp.Children.Add(cc);
        //        cc.MaxWidth = 20;
        //        cc.MinHeight = 20;

        //        cc = new ContentControl();
        //        cc.Margin = new Thickness(0);
        //        cc.Content = _converter.Convert(hptype.GetChild("bottom"), typeof(Visual), null, null);
        //        sp.Children.Add(cc);
        //        cc.MaxHeight = 10;



        //        return panel;
        //    }
        //}
    }
}
