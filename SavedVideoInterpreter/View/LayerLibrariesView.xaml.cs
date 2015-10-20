using Microsoft.Win32;
using PrefabSingle;
using PythonHost;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
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

namespace SavedVideoInterpreter
{
    /// <summary>
    /// Interaction logic for LayerLibrariesView.xaml
    /// </summary>
    public partial class LayerLibrariesView : UserControl
    {

        private LayerInterpretationLogic _layerInterpretLogic;

        public static DependencyProperty LayerItemsProperty =
            DependencyProperty.Register("LayerChainItems",
            typeof(ObservableCollection<LayerChainItem>),
            typeof(LayerLibrariesView));

        public static DependencyProperty AllLayersProperty =
           DependencyProperty.Register("AllLayers", 
           typeof(ObservableCollection<LayerChainItem>),
           typeof(LayerLibrariesView));


        public static DependencyProperty LibrariesProperty = 
            DependencyProperty.Register("Libraries", 
            typeof(ObservableCollection<string>),
            typeof(LayerLibrariesView));



        public class LibraryAddedOrRemovedEventArgs : EventArgs
        {
            public bool Added
            {
                get;
                private set;
            }

            public LayerChainItem Layer
            {
                get;
                private set;
            }

            public string Library
            {
                get;
                private set;
            }

            public LibraryAddedOrRemovedEventArgs(bool added, LayerChainItem layer, string library)
            {
                Added = added;
                Layer = layer;
                Library = library;
            }
        }


        public ObservableCollection<LayerChainItem> LayerChainItems
        {
            get { return (ObservableCollection<LayerChainItem>)GetValue(LayerItemsProperty); }
            set { SetValue(LayerItemsProperty, value); }
        }

        public ObservableCollection<LayerChainItem> AllLayers
        {
            get { return (ObservableCollection<LayerChainItem>)GetValue(AllLayersProperty); }
            set { SetValue(AllLayersProperty, value); }
        }

        public event EventHandler<LibraryAddedOrRemovedEventArgs> ParameterAdded;
        public event EventHandler<LibraryAddedOrRemovedEventArgs> ParameterRemoved;
        public event EventHandler LayerMoved;
        public event EventHandler LayerDeleted;
        public event EventHandler LayerAdded;
        public event EventHandler ReloadLayersClicked;

        public ObservableCollection<string> Libraries
        {
            get { return (ObservableCollection<string>)GetValue(LibrariesProperty); }
            set { SetValue(LibrariesProperty, value); }
        }

        private void DeleteParameters_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            string lib = button.DataContext as string;
            LayerChainItem selectedLayer = ChainListBox.SelectedItem as LayerChainItem;

            Libraries.Remove(lib);
            int index = selectedLayer.ParameterKeys.IndexOf("libraries");
            var libs = selectedLayer.ParameterValues[index] as List<string>;

            libs.Remove(lib);

            if (libs.Count == 0)
            {
                selectedLayer.ParameterKeys.RemoveAt(index);
                selectedLayer.ParameterValues.RemoveAt(index);
            }
            if (ParameterRemoved != null)
                ParameterRemoved(this, new LibraryAddedOrRemovedEventArgs(false, selectedLayer, lib));
        }

        private void AddParameter_Click(object sender, RoutedEventArgs e)
        {
            
            
            LayerChainItem selectedLayer = ChainListBox.SelectedItem as LayerChainItem;

            if (selectedLayer != null)
            {
                if (selectedLayer.ParameterKeys.Contains("libraries"))
                {
                    int index = selectedLayer.ParameterKeys.IndexOf("libraries");
                    selectedLayer.ParameterKeys.RemoveAt(index);
                    selectedLayer.ParameterValues.RemoveAt(index);
                }


                selectedLayer.ParameterKeys.Add("libraries");

                if (!Libraries.Contains(LibraryBox.Text))
                    Libraries.Add(LibraryBox.Text);


                selectedLayer.ParameterValues.Add(new List<string>(Libraries));



                if (ParameterAdded != null)
                    ParameterAdded(this, new LibraryAddedOrRemovedEventArgs(true, selectedLayer, LibraryBox.Text));
            }
        }

        public LayerLibrariesView()
        {
            InitializeComponent();
            DataContext = this;
        }


        public void SetLayerChainItems(PrefabInterpretationLogic logic)
        {
            LayerChainItems = new ObservableCollection<LayerChainItem>();

            if (logic is LayerInterpretationLogic)
            {
                LayerInterpretationLogic layerLog = logic as LayerInterpretationLogic;
                _layerInterpretLogic = layerLog;
                List<string> layernames = new List<string>();
                List<Dictionary<string, object>> parameters = new List<Dictionary<string, object>>();

                ChainLoader.GetLayerNamesAndParameters(layerLog.LayerDirectory, layernames, parameters);

                
                

                    for (int i = 0; i < layernames.Count; i ++ )
                    {
                        
                        LayerChainItem item = new LayerChainItem();
                        item.Name = System.IO.Path.GetFileName(layernames[i]);
                        item.RelativePath = layernames[i];

                        item.FullPath = System.IO.Path.GetFullPath(layerLog.LayerDirectory + "/" + layernames[i]);

                        foreach (string key in parameters[i].Keys)
                        {

                            object val = parameters[i][key];
                            if (val != null)
                            {
                                item.ParameterKeys.Add(key);
                                item.ParameterValues.Add(val);

                            }
                        }

                        LayerChainItems.Add(item);
                    }
                

                //List<string> dirs = new List<string>();
                //dirs.Add(layerLog.LayerDirectory);
                //var alllayers = ChainLoader.GetAllLayerNames(dirs);

                //AllLayers = new ObservableCollection<LayerChainItem>();

                //foreach (string layer in alllayers)
                //{
                //    LayerChainItem item = new LayerChainItem();
                //    item.Name = System.IO.Path.GetFileName(layer);
                //    string path = System.IO.Path.GetDirectoryName(layer);
                    
                //    if (System.IO.Path.Equals(path, layerLog.LayerDirectory))
                //    {
                //        item.RelativePath = item.Name;
                //    }else
                //         item.RelativePath = layer;

                //    item.FullPath = System.IO.Path.GetFullPath(layerLog.LayerDirectory + "/" + layer);

                //    AllLayers.Add(item);

                //}
            }
            else
            {
                LayerChainItem item = new LayerChainItem();
                item.Name = "prefab_identification_layers";
                item.RelativePath = "..\\prefab_identification_layers";
                item.FullPath =  System.IO.Path.GetFullPath(logic.LayerDirectory + "/" + item.RelativePath);
                item.DeleteButtonVisibility = System.Windows.Visibility.Collapsed;
                LayerChainItems.Add(item);
                
                item = new LayerChainItem();
                item.Name = "interpret_tree.py";
                item.RelativePath = "interpret_tree.py";
                item.FullPath =  System.IO.Path.GetFullPath(logic.LayerDirectory + "/" + item.RelativePath);
                item.DeleteButtonVisibility = System.Windows.Visibility.Collapsed;
                LayerChainItems.Add(item);

                AddLayerButton.Visibility = Visibility.Collapsed;
                LibrariesPanel.Visibility = Visibility.Collapsed;
            }
        }

        private void ChainListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Libraries = new ObservableCollection<string>();
            LayerChainItem item = ChainListBox.SelectedItem as LayerChainItem;
            if (item != null)
            {
                if(item.ParameterKeys.Contains("libraries")){

                    int index = item.ParameterKeys.IndexOf("libraries");

                    IEnumerable<string> libs = item.ParameterValues[index] as IEnumerable<string>;

                    foreach(string lib in libs)
                        Libraries.Add(lib);
                }
            }
        }


        private void listBox1_Drop(object sender, DragEventArgs e)
        {
            ListBox parent = sender as ListBox;
            LayerChainItem data = e.Data.GetData(typeof(LayerChainItem)) as LayerChainItem;
            LayerChainItem objectToPlaceBefore = GetObjectDataFromPoint(parent, e.GetPosition(parent)) as LayerChainItem;
            if (data != null && objectToPlaceBefore != null && data != objectToPlaceBefore)
            {
                int index = LayerChainItems.IndexOf(objectToPlaceBefore);
                LayerChainItems.Remove(data);
                LayerChainItems.Insert(index, data);
                this.ChainListBox.SelectedItem = data;

                if (LayerMoved != null)
                    LayerMoved(this, e);
            }
        }

        private void listBox1_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //ListBox parent = sender as ListBox;
            //LayerChainItem data = GetObjectDataFromPoint(parent, e.GetPosition(parent)) as LayerChainItem;
            //if (data != null)
            //{
            //    DragDrop.DoDragDrop(parent, data, DragDropEffects.Move);
            //}

            //e.Handled = false;
        }

        private static object GetObjectDataFromPoint(ListBox source, System.Windows.Point point)
        {
            UIElement element = source.InputHitTest(point) as UIElement;
            if (element != null)
            {
                object data = DependencyProperty.UnsetValue;
                while (data == DependencyProperty.UnsetValue)
                {
                    data = source.ItemContainerGenerator.ItemFromContainer(element);
                    if (data == DependencyProperty.UnsetValue)
                        element = VisualTreeHelper.GetParent(element) as UIElement;
                    if (element == source)
                        return null;
                }
                if (data != DependencyProperty.UnsetValue)
                    return data;
            }

            return null;
        }

        private void DeleteLayer_Click(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;
            LayerChainItem item = b.DataContext as LayerChainItem;

            LayerChainItems.Remove(item);


            if (LayerDeleted != null)
                LayerDeleted(this, e);

        }

        private void ChainListBox_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                ListBox parent = sender as ListBox;
                LayerChainItem data = GetObjectDataFromPoint(parent, e.GetPosition(parent)) as LayerChainItem;
                if (data != null)
                {
                    DragDrop.DoDragDrop(parent, data, DragDropEffects.Move);

                }
            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //if (AddLayerComboBox.SelectedIndex >= 0)
            //{
            //    LayerChainItem chainitem = AllLayers[AddLayerComboBox.SelectedIndex];

            //    LayerChainItem cpy = new LayerChainItem();
            //    cpy.Name = chainitem.Name;
            //    cpy.RelativePath = chainitem.RelativePath;
            //    cpy.FullPath = chainitem.FullPath;
            //    LayerChainItems.Add(cpy);

            //    if (LayerAdded != null)
            //        LayerAdded(this, e);
            //}
            //else
            //    AddLayerComboBox.Text = "Add a Layer";
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if(ReloadLayersClicked != null)
                ReloadLayersClicked(this, e);
        }

        private void ListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ListBox parent = sender as ListBox;
            LayerChainItem data = GetObjectDataFromPoint(parent, e.GetPosition(parent)) as LayerChainItem;
            if (data != null)
            {
                Process.Start(data.FullPath);
            }
        }

        private void AddLayerButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Python Files, Dlls, Directories | *.py; *.dll";
            ofd.Multiselect = true;
            if (ofd.ShowDialog().Value)
            {
                foreach (string filename in ofd.FileNames)
                {
                    LayerChainItem item = new LayerChainItem();
                    item.Name = System.IO.Path.GetFileName(filename);
                    item.FullPath = System.IO.Path.GetFullPath(filename);
                    item.RelativePath = GetRelativePath(item.FullPath, System.IO.Path.GetFullPath(_layerInterpretLogic.LayerDirectory));
                    LayerChainItems.Add(item);   
                }

                if (LayerAdded != null)
                    LayerAdded(this, e);
            }
        }

        string GetRelativePath(string filespec, string folder)
        {
            Uri pathUri = new Uri(filespec);
            // Folders must end in a slash
            if (!folder.EndsWith(System.IO.Path.DirectorySeparatorChar.ToString()))
            {
                folder += System.IO.Path.DirectorySeparatorChar;
            }
            Uri folderUri = new Uri(folder);
            return Uri.UnescapeDataString(folderUri.MakeRelativeUri(pathUri).ToString().Replace('/', System.IO.Path.DirectorySeparatorChar));
        }
    }
}
