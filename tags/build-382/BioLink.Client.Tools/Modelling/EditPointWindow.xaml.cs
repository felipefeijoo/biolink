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
using BioLink.Client.Extensibility;
using BioLink.Client.Utilities;

namespace BioLink.Client.Tools {
    /// <summary>
    /// Interaction logic for EditPointWindow.xaml
    /// </summary>
    public partial class EditPointWindow : Window {

        public EditPointWindow(PointViewModel point) {
            InitializeComponent();
            this.Point = point;
            if (point != null) {
                txtLatitude.Text = point.Latitude.ToString();
                txtLongitude.Text = point.Longitude.ToString();
            }

            Loaded += new RoutedEventHandler(EditPointWindow_Loaded);
        }

        void EditPointWindow_Loaded(object sender, RoutedEventArgs e) {
            txtLongitude.Focus();
        }

        protected PointViewModel Point { get; set; }

        private void btnOK_Click(object sender, RoutedEventArgs e) {
            if (Validate()) {
                if (Point != null) {
                    Point.Latitude = Double.Parse(txtLatitude.Text);
                    Point.Longitude = Double.Parse(txtLongitude.Text);
                    this.DialogResult = true;
                }
                this.Close();
            }
        }

        private bool Validate() {
            double d;
            if (!double.TryParse(txtLongitude.Text, out d)) {
                ErrorMessage.Show("Invalid longitude. Must be a number");
                return false;
            }

            if (!double.TryParse(txtLatitude.Text, out d)) {
                ErrorMessage.Show("Invalid latitude. Must be a number");
                return false;
            }

            return true;

        }

        private void btnCancel_Click(object sender, RoutedEventArgs e) {
            this.Close();
        }
    }
}
