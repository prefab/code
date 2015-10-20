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
using Microsoft.Win32;
using System.Reflection;
using System.Threading;
using System.IO;
using System.Runtime.Remoting;
using System.Windows.Threading;
using System.ComponentModel;
using System.Configuration;

using Prefab;


namespace SavedVideoInterpreter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class LoadDialog : UserControl
    {
        
        public LoadDialog()
        {
            DataContext = this;
            InitializeComponent();
        }


        //public event EventHandler<AsyncPrototypeLoader.LoadResults> LoadComplete;
        //private PrefabWithDatabase _prefab;

        //public void LoadPrototypes(PrefabWithDatabase prefab)
        //{
        //    Visibility = System.Windows.Visibility.Visible;
        //    _prefab = prefab;
        //    prefab.LoadProgressChanged += new ProgressChangedEventHandler(LoadPtypes_ProgressChanged);
        //    prefab.LoadProgressComplete += new EventHandler<AsyncPrototypeLoader.LoadResults>(LoadPtypes_RunWorkerCompleted);
        //    prefab.LoadPtypesFromDBAsync();
        //}




        //private void LoadPtypes_RunWorkerCompleted(object sender, AsyncPrototypeLoader.LoadResults e)
        //{
        //    LoadProgress.Visibility = System.Windows.Visibility.Hidden;
        //    LoadProgressLabel.Visibility = System.Windows.Visibility.Hidden;
        //    if (LoadComplete != null)
        //    {
        //        LoadComplete(sender, e);
        //    }

        //    Visibility = System.Windows.Visibility.Hidden;
         
        //}

        private void LoadPtypes_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            string state = e.UserState as string;
            switch (state)
            {
                case "prototypes":
                    LoadProgressLabel.Content = "Loading Prototypes...";
                    LoadProgress.IsIndeterminate = false;
                    LoadProgress.Minimum = 0;
                    LoadProgress.Maximum = 100;
                    LoadProgress.Value = e.ProgressPercentage;
                    break;

                case "rebuilding":
                    LoadProgress.IsIndeterminate = true;
                    LoadProgressLabel.Content = "Building data structures...";
                    break;

                case "connecting":
                    LoadProgress.IsIndeterminate = true;
                    LoadProgressLabel.Content = "Connecting to database...";
                    break;


                case "cancel":
                    Visibility = System.Windows.Visibility.Hidden;
                    break;

            }
        }


        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            //if (_prefab != null)
            //{
            //    _prefab.Cancel();
            //}
        }

    }

    
}
