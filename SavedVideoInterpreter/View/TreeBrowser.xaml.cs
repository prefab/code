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
using System.Windows.Shapes;
using System.ComponentModel;

using System.Collections.ObjectModel;

using Prefab;

namespace SavedVideoInterpreter
{
    /// <summary>
    /// Interaction logic for PrototypeViewer.xaml
    /// </summary>
    public partial class TreeBrowser : UserControl
    {

        public static readonly DependencyProperty TreeNodesProperty =
           DependencyProperty.Register("TreeNodes",
           typeof(BindingList<ViewableTreeNode>), typeof(TreeBrowser));

        public BindingList<ViewableTreeNode> TreeNodes
        {
            get { return (BindingList<ViewableTreeNode>)GetValue(TreeNodesProperty); }
            set
            {
                SetValue(TreeNodesProperty, value);
            }
        }

        private event EventHandler _mouseUp;
        public event EventHandler ItemMouseUp
        {
            add
            {
                _mouseUp += value;
            }
            remove
            {
                _mouseUp -= value;
            }
        }

        public TreeBrowser()
        {
            InitializeComponent();
            TreeNodes = new BindingList<ViewableTreeNode>();  
        }

        private ViewableTreeNode GetCorrespondingViewableNode(Tree node, ViewableTreeNode currNode)
        {
            if (currNode.Node == node)
                return currNode;

            foreach (ViewableTreeNode child in currNode.Children)
            {
                ViewableTreeNode corresponding = GetCorrespondingViewableNode(node, child);
                if (corresponding != null)
                    return corresponding;
            }


            return null;
        }

        public TreeViewItem GetCorrespondingItem(Tree node)
        {
            if (TreeNodes.Count == 1)
            {
                ViewableTreeNode viewablenode = GetCorrespondingViewableNode(node, TreeNodes[0]);
                if (viewablenode != null)
                    return GetTreeViewItem(TreeViewControl, viewablenode);
            }
            return null;
        }


        private TreeViewItem GetTreeViewItem(ItemsControl container, object item)
        {
            if (container != null)
            {
                if (container.DataContext == item)
                {
                    return container as TreeViewItem;
                }

                // Expand the current container
                if (container is TreeViewItem && !((TreeViewItem)container).IsExpanded)
                {
                    container.SetValue(TreeViewItem.IsExpandedProperty, true);
                }

                // Try to generate the ItemsPresenter and the ItemsPanel.
                // by calling ApplyTemplate.  Note that in the 
                // virtualizing case even if the item is marked 
                // expanded we still need to do this step in order to 
                // regenerate the visuals because they may have been virtualized away.

                container.ApplyTemplate();
                ItemsPresenter itemsPresenter =
                    (ItemsPresenter)container.Template.FindName("ItemsHost", container);
                if (itemsPresenter != null)
                {
                    itemsPresenter.ApplyTemplate();
                }
                else
                {
                    // The Tree template has not named the ItemsPresenter, 
                    // so walk the descendents and find the child.
                    itemsPresenter = FindVisualChild<ItemsPresenter>(container);
                    if (itemsPresenter == null)
                    {
                        container.UpdateLayout();

                        itemsPresenter = FindVisualChild<ItemsPresenter>(container);
                    }
                }

                Panel itemsHostPanel = (Panel)VisualTreeHelper.GetChild(itemsPresenter, 0);


                // Ensure that the generator for this panel has been created.
                UIElementCollection children = itemsHostPanel.Children;

                for (int i = 0, count = container.Items.Count; i < count; i++)
                {
                    TreeViewItem subContainer;

                    subContainer =
                        (TreeViewItem)container.ItemContainerGenerator.
                        ContainerFromIndex(i);

                    // Bring the item into view to maintain the 
                    // same behavior as with a virtualizing panel.
                    subContainer.BringIntoView();

                    // Search the next level for the object.
                    TreeViewItem resultContainer = GetTreeViewItem(subContainer, item);
                    if (resultContainer != null)
                    {
                        return resultContainer;
                    }
                    else
                    {
                        // The object is not under this TreeViewItem
                        // so collapse it.
                        subContainer.IsExpanded = false;
                    }

                }
            }

            return null;
        }

        /// <summary>
        /// Search for an element of a certain type in the visual tree.
        /// </summary>
        /// <typeparam name="T">The type of element to find.</typeparam>
        /// <param name="visual">The parent element.</param>
        /// <returns></returns>
        private T FindVisualChild<T>(Visual visual) where T : Visual
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(visual); i++)
            {
                Visual child = (Visual)VisualTreeHelper.GetChild(visual, i);
                if (child != null)
                {
                    T correctlyTyped = child as T;
                    if (correctlyTyped != null)
                    {
                        return correctlyTyped;
                    }

                    T descendent = FindVisualChild<T>(child);
                    if (descendent != null)
                    {
                        return descendent;
                    }
                }
            }

            return null;
        }





        public void SelectNode(Tree node)
        {
            
            TreeViewItem item = GetCorrespondingItem(node);
            if (item != null)
            {
                item.IsSelected = true;
                //item.IsExpanded = true;
                item.IsEnabled = true;
                
                item.BringIntoView();


                ViewableTreeNode viewablenode = item.DataContext as ViewableTreeNode;
               
            }
        }

        public void DeselectNode(Tree node)
        {
            TreeViewItem item = GetCorrespondingItem(node);
            
            if(item != null)
                item.IsSelected = false;
        }

        private void ItemText_Mouseup(object sender, MouseButtonEventArgs e)
        {
            if (_mouseUp != null)
            {
                TextBlock tb = sender as TextBlock;
                ViewableTreeNode viewablenode = tb.DataContext as ViewableTreeNode ;
                IEnumerable<ViewableTreeNode> allitems = GetAllItems();
                foreach (ViewableTreeNode item in allitems)
                    item.IsSelected = false;

                viewablenode.IsSelected = true;

                _mouseUp(viewablenode, e);


            }
        }

        public void SelectNodeAndRectangle(Tree node)
        {
             TreeViewItem item = GetCorrespondingItem(node);
            if (item != null)
            {
                item.IsSelected = true;
                //item.IsExpanded = true;
                item.IsEnabled = true;
               
                item.BringIntoView();

                IEnumerable<ViewableTreeNode> allitems = GetAllItems();
                foreach (ViewableTreeNode vtn in allitems)
                    if(vtn.IsLocked)
                         vtn.IsSelected = false;

                ViewableTreeNode viewablenode = item.DataContext as ViewableTreeNode;
                viewablenode.IsSelected = true;
            }
            
        }

        internal IEnumerable<ViewableTreeNode> GetAllItems()
        {
            List<ViewableTreeNode> items = new List<ViewableTreeNode>();

            if(TreeNodes.Count == 1)
             GetAllItemsHelper(items, TreeNodes[0]);

            return items;
        }

        private void GetAllItemsHelper(List<ViewableTreeNode> items, ViewableTreeNode viewableTreeNode)
        {
            items.Add(viewableTreeNode);
            foreach (ViewableTreeNode child in viewableTreeNode.Children)
                GetAllItemsHelper(items, child);
        }

        internal void ClearSelected()
        {
            IEnumerable<ViewableTreeNode> nodes = GetAllItems();
            foreach (ViewableTreeNode item in nodes)
                item.IsSelected = false;
        }
    }

    
}
