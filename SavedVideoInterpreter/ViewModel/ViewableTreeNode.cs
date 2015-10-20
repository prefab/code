using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Prefab;
using System.ComponentModel;
using System.Windows;


namespace SavedVideoInterpreter
{
    public class ViewableTreeNode : DependencyObject
    {
        public static readonly DependencyProperty ChildrenProperty = DependencyProperty.Register("Children", typeof(BindingList<ViewableTreeNode>), typeof(ViewableTreeNode));
        public static readonly DependencyProperty IdProperty = DependencyProperty.Register("Id", typeof(string), typeof(ViewableTreeNode));
        public static readonly DependencyProperty SelectedProperty = DependencyProperty.Register("IsSelected", typeof(bool), typeof(ViewableTreeNode));

        public Tree Node
        {
            get;
            private set;
        }

        public ViewableTreeNode(Tree node)
        {
            Node = node;
            Children = new BindingList<ViewableTreeNode>();
            foreach (Tree child in node.GetChildren())
            {
                Children.Add(new ViewableTreeNode(child));
            }

            if (node.HasTag("type")){

                Id = "{x=" + node.Left + ", y=" + node.Top + ", width=" + node.Width + ", height=" + node.Height + "}";

                //switch(node["type"].ToString()){

                //    case "ptype":
                //        Id = ((string)node["ptype_id"]);
                //        break;

                //    default:
                //        Id = node["type"].ToString();
                //        break;
                //}
                

            }

            else
                Id = "node";
        }

        public string Id
        {
            get { return (string)GetValue(IdProperty); }
            set
            {
                SetValue(IdProperty, value);
            }
        }

        public BindingList<ViewableTreeNode> Children
        {
            get { return (BindingList<ViewableTreeNode>)GetValue(ChildrenProperty); }
            set
            {
                SetValue(ChildrenProperty, value);
            }
        }

        public bool IsSelected
        {
            get { return (bool)GetValue(SelectedProperty); }
            set { SetValue(SelectedProperty, value); }
        }

        public bool IsLocked
        {
            get;
            set;
        }
    }
}
