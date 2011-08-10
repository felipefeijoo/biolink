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

namespace BioLink.Client.Extensibility {
    /// <summary>
    /// Interaction logic for DateChooserWindow.xaml
    /// </summary>
    public partial class DateChooserWindow : Window {

        public DateChooserWindow() {
            InitializeComponent();
            cal.IsTodayHighlighted = true;
        }

        private void btnOK_Click(object sender, RoutedEventArgs e) {
            this.DialogResult = true;
            this.Close();
        }

        public DateTime? Date {
            get { return cal.SelectedDate; }
        }

    }
}
