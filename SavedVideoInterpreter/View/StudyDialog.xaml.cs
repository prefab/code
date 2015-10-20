using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace SavedVideoInterpreter
{
    /// <summary>
    /// Interaction logic for StudyDialog.xaml
    /// </summary>
    public partial class StudyDialog : Window
    {

        
        public StudyDialog()
        {
            InitializeComponent();
            DataContext = this;
            
        }

        public string GetStudyType()
        {
            try
            {
                return ((ComboBoxItem)StudyTypeBox.SelectedItem).Content.ToString();
            }
            catch
            {
                return "debug";
            }
         
        }

        public string GetParticipantId()
        {
            return ParticipantIdField.Text;
        }

        private void Continue_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

    }
}
