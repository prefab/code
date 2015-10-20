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
using System.Windows.Interop;

namespace ScreenVideoCapture
{
    /// <summary>
    /// Interaction logic for WindowHighlight.xaml
    /// </summary>
    public partial class WindowHighlight : Window
    {
        public WindowHighlight()
        {
            InitializeComponent();
        }

        public IntPtr Handle
        {
            get;
            private set;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Handle = new WindowInteropHelper(this).Handle;
        }
    }
}
