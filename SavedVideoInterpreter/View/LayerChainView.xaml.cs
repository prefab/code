using Prefab;
using PrefabSingle;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
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
using PythonHost;
namespace SavedVideoInterpreter
{
    /// <summary>
    /// Interaction logic for LayerChainView.xaml
    /// </summary>
    public partial class LayerChainView : UserControl
    {
        public static DependencyProperty LayerItemsProperty =
            DependencyProperty.Register("LayerChainItems", typeof(ObservableCollection<LayerChainItem>), typeof(LayerChainView));

        public static DependencyProperty AllLayersProperty =
           DependencyProperty.Register("AllLayers", typeof(ObservableCollection<LayerChainItem>), typeof(LayerChainView));


        public static DependencyProperty ParametersProperty = DependencyProperty.Register("Parameters", typeof(BindingList<ParameterKeyValuePair>), typeof(LayerChainView));

        public class ParameterAddedOrRemovedEventArgs : EventArgs
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

            public ParameterKeyValuePair Parameter
            {
                get;
                private set;
            }

            public ParameterAddedOrRemovedEventArgs(bool added, LayerChainItem layer, ParameterKeyValuePair parameter)
            {
                Added = added;
                Layer = layer;
                Parameter = parameter;
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

        public event EventHandler<ParameterAddedOrRemovedEventArgs> ParameterAdded;
        public event EventHandler<ParameterAddedOrRemovedEventArgs> ParameterRemoved;
        public event EventHandler LayerMoved;
        public event EventHandler LayerDeleted;
        public event EventHandler LayerAdded;
        public event EventHandler ReloadLayersClicked;

        public BindingList<ParameterKeyValuePair> Parameters
        {
            get { return (BindingList<ParameterKeyValuePair>)GetValue(ParametersProperty); }
            set { SetValue(ParametersProperty, value); }
        }

        private void DeleteParameters_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            ParameterKeyValuePair param = button.DataContext as ParameterKeyValuePair;
            LayerChainItem selectedLayer = ChainListBox.SelectedItem as LayerChainItem;

            int index = selectedLayer.ParameterKeys.IndexOf(param.Key.Value);

            selectedLayer.ParameterKeys.RemoveAt(index);
            selectedLayer.ParameterValues.RemoveAt(index);

            Parameters.Remove(param);

            if (ParameterRemoved != null)
                ParameterRemoved(this, new ParameterAddedOrRemovedEventArgs(false, selectedLayer, param));
        }

        private void AddParameter_Click(object sender, RoutedEventArgs e)
        {
            ParameterKeyValuePair param = new ParameterKeyValuePair(KeyBox.Text, ValueBox.Text);
            LayerChainItem selectedLayer = ChainListBox.SelectedItem as LayerChainItem;

            if (selectedLayer.ParameterKeys.Contains(KeyBox.Text))
            {
                int index = selectedLayer.ParameterKeys.IndexOf(KeyBox.Text);
                selectedLayer.ParameterKeys.RemoveAt(index);
                selectedLayer.ParameterValues.RemoveAt(index);
            }
            selectedLayer.ParameterKeys.Add(KeyBox.Text);
            selectedLayer.ParameterValues.Add(ValueBox.Text);

            Parameters.Add(param);

            if (ParameterAdded != null)
                ParameterAdded(this, new ParameterAddedOrRemovedEventArgs(true, selectedLayer, param));
        }

        public LayerChainView()
        {
            InitializeComponent();
            DataContext = this;
        }


        public void SetLayerChainItems(PrefabInterpretationLogic logic)
        {
            if (logic is LayerInterpretationLogic)
            {
                LayerInterpretationLogic layerLog = logic as LayerInterpretationLogic;
                List<string> layernames = new List<string>();
                List<Dictionary<string, object>> parameters = new List<Dictionary<string, object>>();

                ChainLoader.GetLayerNamesAndParameters(layerLog.LayerDirectory, layernames, parameters);

                
                LayerChainItems = new ObservableCollection<LayerChainItem>();
                if (layerLog.Layers != null)
                {
                    for (int i = 0; i < layernames.Count; i ++ )
                    {
                        LayerChainItem item = new LayerChainItem();
                        item.Name = System.IO.Path.GetFileNameWithoutExtension(layernames[i]);
                        item.FullPath = layernames[i];
                        foreach (string key in parameters[i].Keys)
                        {

                            string val = parameters[i][key] as string;
                            if (val != null)
                            {
                                item.ParameterKeys.Add(key);
                                item.ParameterValues.Add(val);

                            }
                        }

                        LayerChainItems.Add(item);
                    }
                }

                //List<string> dirs = new List<string>();
                //dirs.Add(layerLog.LayerDirectory);
                //var alllayers = ChainLoader.GetAllLayerNames(dirs);

                //AllLayers = new ObservableCollection<LayerChainItem>();

                //foreach (string layer in alllayers)
                //{
                //    LayerChainItem item = new LayerChainItem();
                //    item.Name = System.IO.Path.GetFileNameWithoutExtension(layer);
                //    string path = System.IO.Path.GetDirectoryName(layer);
                    
                //    if (System.IO.Path.Equals(path, layerLog.LayerDirectory))
                //    {
                //        item.FullPath = item.Name;
                //    }else
                //         item.FullPath = layer;

                //    AllLayers.Add(item);

                //}
            }
            else
            {
                Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        private void ChainListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Parameters = new BindingList<ParameterKeyValuePair>();
            LayerChainItem item = ChainListBox.SelectedItem as LayerChainItem;
            if (item != null)
            {
                for (int i = 0; i < item.ParameterKeys.Count; i++)
                {
                    Parameters.Add(new ParameterKeyValuePair(item.ParameterKeys[i], item.ParameterValues[i].ToString()));


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
            if (AddLayerComboBox.SelectedIndex >= 0)
            {
                LayerChainItem chainitem = AllLayers[AddLayerComboBox.SelectedIndex];

                LayerChainItem cpy = new LayerChainItem();
                cpy.Name = chainitem.Name;
                cpy.FullPath = chainitem.FullPath;

                LayerChainItems.Add(cpy);

                if (LayerAdded != null)
                    LayerAdded(this, e);
            }
            else
                AddLayerComboBox.Text = "Add a Layer";
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if(ReloadLayersClicked != null)
                ReloadLayersClicked(this, e);
        }
    }
}
