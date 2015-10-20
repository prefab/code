using Prefab;
using PrefabIdentificationLayers.Models;
using PrefabIdentificationLayers.Prototypes;
using PrefabIdentificationLayers.Regions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SavedVideoInterpreter
{
    /// <summary>
    /// Interaction logic for HelloWorldOverlay.xaml
    /// </summary>
    public partial class HelloWorldOverlay : UserControl
    {

        public static readonly DependencyProperty BackgroundOverlayImageProperty =
            DependencyProperty.Register("BackgroundOverlayImage", typeof(WriteableBitmap), typeof(HelloWorldOverlay));

        public static readonly DependencyProperty TextLocationsProperty =
            DependencyProperty.Register("TextLocations", typeof(ObservableCollection<TextBlockRect>), typeof(HelloWorldOverlay));
        public WriteableBitmap BackgroundOverlayImage
        {
            get
            {
                return (WriteableBitmap)GetValue(BackgroundOverlayImageProperty);
            }

            set
            {
                SetValue(BackgroundOverlayImageProperty, value);
            }
        }
        public ObservableCollection<TextBlockRect> TextLocations
        {
            get
            {
                return (ObservableCollection<TextBlockRect>)GetValue(TextLocationsProperty);
            }

            set
            {
                SetValue(TextLocationsProperty, value);
            }
        }
        private delegate void UpdateDel(VideoInterpreter.VideoCapture frame, Tree root, List<Tree> textNodes, IBoundingBox invalidated);
        private UpdateDel _update;
        private Bitmap _backgroundBitmap;



        public HelloWorldOverlay()
        {
            InitializeComponent();
            DataContext = this;

            TextLocations = new ObservableCollection<TextBlockRect>();
            BackgroundOverlayImage = new WriteableBitmap((int)System.Windows.SystemParameters.VirtualScreenWidth * 2,
                            (int)System.Windows.SystemParameters.VirtualScreenHeight * 2, 96, 96, PixelFormats.Bgra32, null);
            _backgroundBitmap = Bitmap.FromDimensions((int)SystemParameters.VirtualScreenHeight, (int)SystemParameters.VirtualScreenWidth);
            _update = new UpdateDel(UIThreadUpdate);
        }



        private void UIThreadUpdate(VideoInterpreter.VideoCapture frame, Tree root, List<Tree> textNodes, IBoundingBox invalidated)
        {
            SetTextBoxes(textNodes, root);
            RenderImageThatMasksText(frame, invalidated);
        }


        public void WriteBackgroundAndRender(Tree tree)
        {
            VideoInterpreter.VideoCapture capture = tree["videocapture"] as VideoInterpreter.VideoCapture;
            IBoundingBox invalidatedpixels = tree["invalidated"] as IBoundingBox;

            //Todo: this is not working properly - returning null when there is a change...
            List<Tree> contentRegionBoxes = GetContentRegionBoxes(tree);
            if (invalidatedpixels != null)
            {
                _backgroundBitmap.Width = tree.Width;
                _backgroundBitmap.Height = tree.Height;
                
                WriteBackgroundPixels(_backgroundBitmap,contentRegionBoxes, tree);
                Dispatcher.BeginInvoke(_update, capture, tree, contentRegionBoxes, tree);
            }
            else
            {
                WriteBackgroundPixels(_backgroundBitmap, contentRegionBoxes, tree);
                Dispatcher.BeginInvoke(_update, capture, tree, contentRegionBoxes, null);
            }
        }

        private static double GetRightBoundary(Tree node, Tree parent)
        {
            IEnumerable<Tree> sibs = Tree.GetSiblings(node, parent);
            int mindist = int.MaxValue;
            Tree closestToRight = null;
            foreach (Tree sib in sibs)
            {
                
                int dist = 0;
                if (BoundingBox.IsToTheRight(sib, node, out dist))
                {
                    if (dist < mindist)
                    {
                        mindist = dist;
                        closestToRight = sib;
                    }
                }
            }

            if (closestToRight == null)
            {
                return parent.Left + parent.Width;
            }
            else
                return closestToRight.Left;
        }

        private static double GetLeftBoundary(Tree node, Tree parent)
        {
            IEnumerable<Tree> sibs = Tree.GetSiblings(node, parent);
            int mindist = int.MaxValue;
            Tree closestToLeft = null;
            foreach (Tree sib in sibs)
            {

                int dist = 0;
                if (BoundingBox.IsToTheLeft(sib, node, out dist))
                {
                    if (dist < mindist)
                    {
                        mindist = dist;
                        closestToLeft = sib;
                    }
                }
            }

            if (closestToLeft == null)
            {
                return parent.Left;
            }
            else
                return closestToLeft.Left + closestToLeft.Width;
        }

        private int GetTopBoundary(Tree node, Tree parent)
        {
            IEnumerable<Tree> sibs = Tree.GetSiblings(node, parent);
            int mindist = int.MaxValue;
            Tree closestAbove = null;
            foreach (Tree sib in sibs)
            {

                int dist = 0;
                if (BoundingBox.IsAlignedVertically(sib, node) && BoundingBox.IsAbove(sib, node, out dist))
                {
                    if (dist < mindist)
                    {
                        mindist = dist;
                        closestAbove = sib;
                    }
                }
            }

            if (closestAbove == null)
            {
                return parent.Top;
            }
            else
                return closestAbove.Top + closestAbove.Height;
        }


        private int GetBottomBoundary(Tree node, Tree parent)
        {
            IEnumerable<Tree> sibs = Tree.GetSiblings(node, parent);
            int mindist = int.MaxValue;
            Tree closestBelow = null;
            foreach (Tree sib in sibs)
            {
                int dist = 0;
                if (BoundingBox.IsAlignedVertically(sib, node) && BoundingBox.IsBelow(sib, node, out dist))
                {
                    if (dist < mindist)
                    {
                        mindist = dist;
                        closestBelow = sib;
                    }
                }
            }

            if (closestBelow == null)
            {
                return parent.Top + parent.Height;
            }
            else
                return closestBelow.Top;
        }

        private void SetTextBoxes(List<Tree> contentRegionBoxes, Tree root)
        {
            TextLocations.Clear();
            foreach(Tree node in contentRegionBoxes)
            {
                Tree parent = Tree.GetParent(node, root);
                var rect = new TextBlockRect();
                rect.TreeNode = node;
                rect.Color = Brushes.Black;
                rect.FontFamily = new System.Windows.Media.FontFamily("Segoe UI");
                rect.FontWeight = FontWeights.Regular;
                int leftBoundary = node.Left; //(int)GetLeftBoundary(node, parent);
                rect.Width = node.Width;//(int)GetRightBoundary(node, parent) - leftBoundary - 4;
                int topBoundary = node.Top;//GetTopBoundary(node, parent);
                rect.Top = node.Top;
                rect.Height = node.Height;//GetBottomBoundary(node, parent) - topBoundary - 8;
                rect.Left = leftBoundary + 1;
                rect.Alignment = TextAlignment.Left;
                rect.Text = "Annuler";

                //}
                //else
                //{

                //    rect.Top = occurrence.TopOffset - 4;
                //    rect.Alignment = TextAlignment.Left;

                //    int toTheRight = GetToTheRight(occurrence, parent);
                //    int margin = 35;
                //    if (toTheRight < parent.LeftOffset + parent.Width)
                //        margin = 5;

                //    rect.Width += toTheRight - (rect.Left + rect.Width) - margin;

                //}



                    rect.FontSize = 12;
                    rect.FontSize = CalculateMaximumFontSize(rect.FontSize, rect.FontSize - 4, 1, rect.Text,
                         rect.FontFamily, new System.Windows.Size(rect.Width, rect.Height), new Thickness(0));

                    //rect.Top = topBoundary + 4;
                    //rect.Left = parent.LeftOffset + (parent.Width - rect.Width) / 2;
                    //rect.Alignment = TextAlignment.Center;


                //if(rect.Width >= 30)
                    TextLocations.Add(rect);
                //TextLocations.Add(   new SelectableBoundingBox.WithTreeNode(node, root, node.Left, node.Top, node.Width, node.Height, null) );
            }
        }

        /// <summary>
        /// Calculates a maximum font size that will fit in a given space
        /// </summary>
        /// <param name="maximumFontSize">The maximum (and default) font size</param>
        /// <param name="minimumFontSize">The minimum size the font is capped at</param>
        /// <param name="reductionStep">The step to de-increment font size with. 
        /// A higher step is less expensive, whereas a lower step sizes font with greater accuracy</param>
        /// <param name="text">The string to measure</param>
        /// <param name="fontFamily">The font family the string provided should be measured in</param>
        /// <param name="containerAreaSize">The total area of the containing area for font placement, specified as a size</param>
        /// <param name="containerInnerMargin">An internal margin to specify the distance the font should keep from the edges of the container</param>
        /// <returns>The caculated maximum font size</returns>
        public static Double CalculateMaximumFontSize(Double maximumFontSize, Double minimumFontSize,
            Double reductionStep, String text, FontFamily fontFamily, System.Windows.Size containerAreaSize, Thickness containerInnerMargin)
        {
            // set fontsize to maimumfont size
            Double fontSize = maximumFontSize;

            // holds formatted text - Culture info is setup for US-Engish, this can be changed for different language sets
            FormattedText formattedText = new FormattedText(text, System.Globalization.CultureInfo.GetCultureInfo("en-us"),
                FlowDirection.LeftToRight, new Typeface(fontFamily.ToString()), fontSize, Brushes.Black);
            formattedText.MaxTextWidth = containerAreaSize.Width;

            // hold the maximum internal space allocation as an absolute value
            Double maximumInternalHeight = containerAreaSize.Height - (containerInnerMargin.Top + containerInnerMargin.Bottom);



            // if measured font is too big for container size, with regard for internal margin
            if (formattedText.Height > maximumInternalHeight)
            {
                do
                {
                    // reduce font size by step
                    fontSize -= reductionStep;

                    // set fontsize ready for re-measure
                    formattedText.SetFontSize(fontSize);
                }
                while ((formattedText.Height > maximumInternalHeight) && (fontSize > minimumFontSize));
            }

            // return ammended fontsize
            return fontSize;
        }
        

        private void RenderImageThatMasksText(VideoInterpreter.VideoCapture frame, IBoundingBox invalidatedRegion)
        {
            frame.CopyToWriteableBitmap(BackgroundOverlayImage, invalidatedRegion);
            BackgroundOverlay.Width = frame.Width;
            BackgroundOverlay.Height = frame.Height;

            foreach (var textLoc in TextLocations)
            {
                CopyBitmapToWriteableBitmap(_backgroundBitmap, BackgroundOverlayImage, textLoc.TreeNode);
            }
        }


        public static void CopyBitmapToWriteableBitmap(Bitmap src, WriteableBitmap dest, IBoundingBox regionToUpdate)
        {
            int stride = 4 * src.Width;
            System.Windows.Int32Rect rect = new Int32Rect(regionToUpdate.Left, regionToUpdate.Top,
                regionToUpdate.Width, regionToUpdate.Height);
            dest.WritePixels(rect, src.Pixels, stride, src.Width * regionToUpdate.Top + regionToUpdate.Left);
            //CopyToWriteableBitmap(dest, new System.Windows.Int32Rect(regionToUpdate.LeftOffset, regionToUpdate.TopOffset, regionToUpdate.Width, regionToUpdate.Height));
        
            
            
        }

        




        private List<Tree> GetContentRegionBoxes(Tree tree)
        {
            List<Tree> nodes = new List<Tree>();
            List<Tree> contentRegionBoxes = new List<Tree>();
            Tree.AddNodesToCollection(tree, nodes);

            foreach (Tree node in nodes)
            {
                if (node["type"] != null && node["type"].Equals("grouped_text")) //(node.ContainsAttribute("hello_world") && node["hello_world"].Equals("true"))
                {
                    contentRegionBoxes.Add(node);
                }
                else
                {
                    Tree parent = Tree.GetParent(node, tree);
                    if (parent != null && parent["type"] != null && parent["type"].Equals("ptype")
                        && node["is_text"] != null && bool.Parse(node["is_text"].ToString()))
                        contentRegionBoxes.Add(node);
                }
            }

            return contentRegionBoxes;
        }

        private void WriteBackgroundPixels(Bitmap destination, List<Tree> textLocations, Tree root)
        {
            foreach(var content in textLocations)
            {
                Tree contentNode = content;
                Tree parent = Tree.GetParent(contentNode, root);
                Ptype ptype = parent["ptype"] as Ptype;

                if (ptype != null)
                {
                    Model model = ptype.Model;
                    if (model.Name.Equals("ninepart"))
                    {
                        PrefabIdentificationLayers.Models.NinePart.Finder finder = (PrefabIdentificationLayers.Models.NinePart.Finder)model.Finder;
                        finder.WriteBackgroundOver(parent, contentNode, destination, contentNode.Top, contentNode.Left);
                    }
                }
            }
        }


    }
}
