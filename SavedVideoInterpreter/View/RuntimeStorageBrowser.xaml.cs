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
    /// Interaction logic for RuntimeStorageBrowser.xaml
    /// </summary>
    public partial class RuntimeStorageBrowser : Window
    {
        private bool _isSearching;

        public static readonly DependencyProperty StoredItemsProperty = DependencyProperty.Register("StoredItems", typeof(ObservableCollection<GridItem>), typeof(RuntimeStorageBrowser));

        public ObservableCollection<GridItem> StoredItems
        {
            get { return (ObservableCollection<GridItem>)GetValue(StoredItemsProperty); }
            set { SetValue(StoredItemsProperty, value); }
        }

        public RuntimeStorageBrowser()
        {
            InitializeComponent();
            StoredItems = new ObservableCollection<GridItem>();
           
            DataContext = this;
            _allItems = new List<GridItem>();
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            Visibility = Visibility.Hidden;
        }

        private List<GridItem> _allItems;

        private bool _allowEdit;


        public void AddItem(GridItem item)
        {
            _allItems.Add(item);
            if (!_isSearching)
                StoredItems.Add(item);
        }

        public void RemoveItem(GridItem item)
        {
            _allItems.Remove(item);
            if (StoredItems.Contains(item))
                StoredItems.Remove(item);
        }

        public void ClearItems()
        {
            _allItems.Clear();
            StoredItems.Clear();
        }

        public bool AllowEdit
        {
            get { return _allowEdit; }
            set
            {

                _allowEdit = value;

                if (_allowEdit)
                {
                    //ActionsText.Visibility = Visibility.Visible;
                    foreach (GridItem item in StoredItems)
                        item.Visibility = System.Windows.Visibility.Visible;
                }
                else
                {
                    //ActionsText.Visibility = Visibility.Collapsed;
                    foreach (GridItem item in StoredItems)
                        item.Visibility = System.Windows.Visibility.Collapsed;
                }
            }
        }

        private void Search_Click(object sender, RoutedEventArgs e)
        {
            StoredItems.Clear();
            foreach (GridItem item in _allItems)
            {
                if (item.DocumentName.Contains(QueryBox.Text) || item.Data.Contains(QueryBox.Text))
                {
                    StoredItems.Add(item);
                }
            }
            _isSearching = true;
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            StoredItems.Clear();
            foreach (GridItem item in _allItems)
                StoredItems.Add(item);

            _isSearching = false;
        }
    }
}
