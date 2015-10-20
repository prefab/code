using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using Prefab;
using PrefabIdentificationLayers.Prototypes;

namespace SavedVideoInterpreter
{
    /// <summary>
    /// Interaction logic for PrototypeBrowser.xaml
    /// </summary>
    public partial class PrototypeBrowser : UserControl
    {

        public static readonly DependencyProperty PrototypeItemsProperty =
           DependencyProperty.Register("PrototypeItems",
           typeof(BindingList<ViewablePrototypeItem>), typeof(PrototypeBrowser));

        public static readonly DependencyProperty SelectedPrototypesProperty =
            DependencyProperty.Register("SelectedPrototypes",
            typeof(BindingList<ViewablePrototypeItem>), typeof(PrototypeBrowser));

        public static readonly DependencyProperty IsNotFirstPageProperty =
            DependencyProperty.Register("IsNotFirstPage",
            typeof(bool), typeof(PrototypeBrowser));

        public static readonly DependencyProperty IsNotLastPageProperty =
            DependencyProperty.Register("IsNotLastPage",
            typeof(bool), typeof(PrototypeBrowser));


        public event EventHandler PrototypeDeleteClicked;
        public event EventHandler AddPositiveClicked;
        public event EventHandler DeletePositiveClicked;
        public event EventHandler DeleteNegativeClicked;
        

        public BindingList<ViewablePrototypeItem> PrototypeItems
        {
            get
            {
                return (BindingList<ViewablePrototypeItem>)GetValue(PrototypeItemsProperty);
            }
            set
            {
                SetValue(PrototypeItemsProperty, value);

            }
        }

        public BindingList<ViewablePrototypeItem> SelectedPrototypes
        {
            get
            {
                return (BindingList<ViewablePrototypeItem>)GetValue(SelectedPrototypesProperty);
            }
            set
            {
                SetValue(SelectedPrototypesProperty, value);

            }
        }

        public PrototypeBrowser()
        {
            InitializeComponent();
            PrototypeItems = new BindingList<ViewablePrototypeItem>();
        }



        public void RemovePtypes(IEnumerable<string> ptypesremoved)
        {
            foreach (string ptype in ptypesremoved)
            {
                ViewablePrototypeItem prev = PrototypeItems.FirstOrDefault((i) => i.Guid == ptype);
                ViewablePrototypeItem selectedPrev = SelectedPrototypes.FirstOrDefault((i) => i.Guid == ptype);

                if (prev != null)
                {
                    PrototypeItems.Remove(prev);

                }

                if (selectedPrev != null)
                    SelectedPrototypes.Remove(selectedPrev);
            }
        }

        public void SetPtypes(string library, IEnumerable<Ptype> ptypes)
        {
            PrototypeItems.Clear();
            AddPtypes(library, ptypes);
        }

        public void AddPtypes(string library, IEnumerable<Ptype> ptypesadded)
        {

            PrototypeItems.RaiseListChangedEvents = false;
            foreach (Ptype ptype in ptypesadded)
            {

                ViewablePrototypeItem prev = PrototypeItems.FirstOrDefault((i) => i.Guid.Equals(ptype.Id));

                var examples = PtypeSerializationUtility.GetTrainingExamples(library, ptype.Id);
               
                if (prev == null)
                {
                    PrototypeItems.Insert(0, new ViewablePrototypeItem(ptype, library, examples.Positives, examples.Negatives));
                }
                else
                {
                    int index = PrototypeItems.IndexOf(prev);
                    PrototypeItems.Remove(prev);
                    PrototypeItems.Insert(index, new ViewablePrototypeItem(ptype, library, examples.Positives, examples.Negatives));
                }
            }

            PrototypeItems.RaiseListChangedEvents = true;
            PrototypeItems.ResetBindings();
            
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            if (PrototypeDeleteClicked != null)
                PrototypeDeleteClicked( (sender as MenuItem).DataContext, e);
            
        }

        private void AddPositiveExample_Click(object sender, RoutedEventArgs e)
        {
            if (AddPositiveClicked != null)
            {
                AddPositiveClicked((sender as MenuItem).DataContext, e);
            }
        }

        private void DeleteExample_Click(object sender, RoutedEventArgs e)
        {
            if (DeletePositiveClicked != null)
                DeletePositiveClicked((sender as Button).DataContext, e);
        }

        private void DeleteNegativeExample_Click(object sender, RoutedEventArgs e)
        {
            if (DeleteNegativeClicked != null)
                DeleteNegativeClicked((sender as Button).DataContext, e);
        }

        private void DeletePtypeButton_Click(object sender, RoutedEventArgs e)
        {
            if (PrototypeDeleteClicked != null)
                PrototypeDeleteClicked((sender as Button).DataContext, e);
        }

        private void AddPositiveButton_Click(object sender, RoutedEventArgs e)
        {
            if (AddPositiveClicked != null)
            {
                AddPositiveClicked((sender as Button).DataContext, e);
            }
        }


    }
}
