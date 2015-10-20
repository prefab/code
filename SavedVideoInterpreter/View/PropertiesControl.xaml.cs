using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using Prefab;
using PrefabIdentificationLayers.Prototypes;
using Newtonsoft.Json;
using PrefabUtils;
using PrefabSingle;

namespace SavedVideoInterpreter
{
    /// <summary>
    /// Interaction logic for PropertiesControl.xaml
    /// </summary>
    public partial class PropertiesControl : UserControl
    {
        public static readonly DependencyProperty AnnotationsProperty = DependencyProperty.Register("Annotations", typeof(BindingList<AnnotationKeyValuePair>), typeof(PropertiesControl));
        public static readonly DependencyProperty PrototypeVisualProperty = DependencyProperty.Register("Prototype", typeof(ViewablePrototypeItem), typeof(PropertiesControl));
        public static readonly DependencyProperty StaticMetadataProperty = DependencyProperty.Register("StaticMetadata", typeof(string), typeof(PropertiesControl));
        public static DependencyProperty AnnotationLibrariesProperty = DependencyProperty.Register("AnnotationLibraries", typeof(BindingList<string>), typeof(PropertiesControl));
        public static DependencyProperty NodePathProperty = DependencyProperty.Register("NodePath", typeof(string), typeof(PropertiesControl));

        public event EventHandler<AnnotationChangedArgs> _annotationChanged;

        public class AnnotationChangedArgs : EventArgs
        {
            public Tree Node
            {
                get;
                private set;
            }

            public AnnotationKeyValuePair Annotation
            {
                get;
                private set;
            }

            public AnnotationChangedArgs(Tree node, AnnotationKeyValuePair annotation)
            {
                Node = node;
                Annotation = annotation;
            }
        }

        public class AnnotationDeletedArgs : EventArgs
        {
            public Tree Node
            {
                get;
                private set;
            }

            public AnnotationKeyValuePair Annotation
            {
                get;
                private set;
            }

            public AnnotationDeletedArgs(Tree node, AnnotationKeyValuePair annotation)
            {
                Node = node;
                Annotation = annotation;
            }
        }

        public class AnnotationAddedArgs : EventArgs
        {
            public string Value
            {
                get;
                private set;
            }

            public string Library
            {
                get;
                private set;
            }

            public AnnotationAddedArgs(string lib, string value)
            {
                Library = lib;
                Value = value;
            }


        }

        public event EventHandler<AnnotationChangedArgs> AnnotationChanged
        {
            add { _annotationChanged += value; }
            remove { _annotationChanged -= value; }
        }

        //private event EventHandler<AnnotationAddedArgs> _annotationAdded;

        //public event EventHandler<AnnotationAddedArgs> AnnotationAdded
        //{
        //    add { _annotationAdded += value; }
        //    remove { _annotationAdded -= value; }
        //}

        private event EventHandler<AnnotationDeletedArgs> _annotationDeleted;

        public event EventHandler<AnnotationDeletedArgs> AnnotationDeleted
        {
            add { _annotationDeleted += value; }
            remove { _annotationDeleted -= value; }
        }


        public string NodePath
        {
            get { return (string)GetValue(NodePathProperty); }
            set { SetValue(NodePathProperty, value); }
        }

        public string StaticMetadata
        {
            get
            {
                return (string)GetValue(StaticMetadataProperty);
            }

            set
            {
                SetValue(StaticMetadataProperty, value);
            }
        }

        public BindingList<AnnotationKeyValuePair> Annotations
        {
            get
            {
                return (BindingList<AnnotationKeyValuePair>)GetValue(AnnotationsProperty);
            }
            set
            {
                SetValue(AnnotationsProperty, value);
            }
        }


        public ViewablePrototypeItem Prototype
        {
            get
            {
                return (ViewablePrototypeItem)GetValue(PrototypeVisualProperty);
            }

            set
            {
                SetValue(PrototypeVisualProperty, value);
            }
        }

        public BindingList<string> AnnotationLibraries
        {
            get
            {
                return (BindingList<string>)GetValue(AnnotationLibrariesProperty);
            }
            set
            {
                SetValue(AnnotationLibrariesProperty, value);
            }
        }

        public Tree Node
        {
            get;
            private set;
        }

        public PropertiesControl()
        {
            DataContext = this;
            InitializeComponent();

            Annotations = new BindingList<AnnotationKeyValuePair>();
            AnnotationLibraries = new BindingList<string>();
        }

        public void SetProperties(Tree node, Tree root, Bitmap wholeScreenshot, string ptypeLib,
            PrefabInterpretationLogic interpLogic, Bitmap image = null, 
            IEnumerable<string> annotationLibs = null)
        {

            Annotations.Clear();
            AnnotationLibraries.Clear();

            List<string> keys = new List<string>(node.GetTags().Select(kvp => kvp.Key));

            foreach (string key in keys)
            {
                if (!key.Equals("ptype") && !key.Equals("invalidated"))
                {
                    object attribute = node[key];
                    if (attribute == null)
                        attribute = "";

                    AnnotationKeyValuePair annotation = new AnnotationKeyValuePair(key, attribute.ToString());
                    annotation.IsEditingChanged += new DependencyPropertyChangedEventHandler(annotation_IsEditingChanged);
                    Annotations.Add(annotation);
                }
            }

            
            
            Node = node;
            
            if (node.HasTag("type"))
            {
                switch (node["type"].ToString())
                {
                    case "ptype":

                         Ptype ptype = (Ptype)node["ptype"];
                         var exs  = PtypeSerializationUtility.GetTrainingExamples(ptypeLib, ptype.Id);
                         Prototype = new ViewablePrototypeItem(ptype, ptypeLib, exs.Positives, exs.Negatives, image);
                        break;

                    case "content":
                    case "feature":
                        Prototype = new ViewablePrototypeItem(null, ptypeLib, new List<ImageAnnotation>(), new List<ImageAnnotation>(), image);
                        break;

                    default:
                        break;
                }
            }
            
            SetAnnotationLibraries(annotationLibs);

            StaticMetadata = "x=" + node.Left + ", y=" + node.Top + ", width=" + node.Width + ", height=" + node.Height;

            NodePath = PathDescriptor.GetPath(node, root);

            if(image != null)
            {
                //string bitmapid = wholeScreenshot.GetHashCode().ToString();


                //var annotations = interpLogic.GetAnnotationsMatchingNode(node, root, bitmapid);

                //string annotationsStr = "[\n";

                //foreach (string lib in annotations.Keys)
                //{
                //    foreach (IAnnotation ia in annotations[lib])
                //    {
                //        annotationsStr += "{\"library\" : \"" + lib + "\"\n";
                //        annotationsStr += "\"id\" : \"" + ia.Id + "\"\n";
                //        annotationsStr += ", \"data\"" + JsonConvert.SerializeObject(ia.Data) + "\n";
                //        annotationsStr += "}";
                //        annotationsStr += ",";
                //    }
                //}

                //annotationsStr.Remove(annotationsStr.Length - 1);
                //annotationsStr += "\n]";
                //AnnotationValueBox.Text = annotationsStr;
            }
           
        }

        

        void annotation_IsEditingChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (_annotationChanged != null)
            {
                _annotationChanged( this, new AnnotationChangedArgs( Node, sender as AnnotationKeyValuePair) );
            }
        }


        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //if (e.ClickCount == 2)
            //{
            //    Window w = new Window();

            //    ViewablePrototypeItem item = (sender as Border).DataContext as ViewablePrototypeItem;
            //    PtypeToVisualConverter conv = new PtypeToVisualConverter();


            //    w.Content = conv.Convert(item.PrototypeVisual, typeof(Visual), null, null);
            //    w.Show();
            //    w.Title = item.PrototypeVisual.Id;
            //    w.Owner = this;
            //}
            //else
            //{
            //    ViewablePrototypeItem item = (sender as Border).DataContext as ViewablePrototypeItem;

            //    item.IsSelected = !item.IsSelected;
            //}
        }


        private void Image_MouseUp(object sender, MouseButtonEventArgs e)
        {
            //if (e.ClickCount == 2)
            //{
            //    ZoomWindow zw = new ZoomWindow();

            //    Example ex = (sender as Border).DataContext as Example;
            //    Image img = new Image();

            //    zw.ZoomedImage = new BitmapImage(new Uri(ex.Src, UriKind.RelativeOrAbsolute));
            //    zw.Title = ex.Src;
            //    zw.Show();
            //    zw.Owner = this;
            //}
            //else
            //{
            //    Example item = (sender as Border).DataContext as Example;
            //    item.IsSelected = !item.IsSelected;

            //}
        }

        private void EditableTextBlock_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            EditableTextBlock tb = sender as EditableTextBlock;
            AnnotationKeyValuePair kvp = tb.DataContext as AnnotationKeyValuePair;

            kvp.IsEditing = true;
        }

        private void Label_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Label label = sender as Label;
            AnnotationKeyValuePair kvp = label.DataContext as AnnotationKeyValuePair;
            kvp.IsEditing = true;
        }


        internal void SetAnnotationLibraries(IEnumerable<string> librarynames)
        {
            AnnotationLibraries.Clear();
            foreach (string lib in librarynames)
            {
                AnnotationLibraries.Add(lib);
            }

            //if (AnnotationLibraries.Count > 0)
            //    AnnotationLibComboBox.SelectedIndex = 0;
        }

        private void SubmitAnnotationClick(object sender, RoutedEventArgs e)
        {
            //if (_annotationAdded != null)
            //{
            //    _annotationAdded(this, new AnnotationAddedArgs(Node, AnnotationLibComboBox.SelectedItem.ToString(), 
            //        AnnotationKeyBox.Text, AnnotationValueBox.Text));
            //}

            //AnnotationKeyBox.Text = "";
            //AnnotationValueBox.Text = "";

            
        }

        private void DeleteAnnotationClick(object sender, RoutedEventArgs e)
        {
            if (_annotationDeleted != null)
            {
                MenuItem item = sender as MenuItem;
                AnnotationKeyValuePair pair = item.DataContext as AnnotationKeyValuePair;
                
                _annotationDeleted(this, new AnnotationDeletedArgs(Node, pair));
            }
        }
    }
}
