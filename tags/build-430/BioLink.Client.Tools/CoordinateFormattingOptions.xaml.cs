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
using BioLink.Client.Utilities;
using BioLink.Client.Extensibility;
using BioLink.Data;
using BioLink.Data.Model;

namespace BioLink.Client.Tools {
    /// <summary>
    /// Interaction logic for CoordinateFormattingOptions.xaml
    /// </summary>
    public partial class CoordinateFormattingOptions : Window {

        public CoordinateFormattingOptions(CoordinateType coordType, string formatOption) {
            InitializeComponent();
            FormatSpecifier = formatOption;
            this.CoordinateType = coordType;

            if (formatOption.Equals(SupportService.COORD_FORMAT_DECIMAL_DEGREES)) {
                optDecDeg.IsChecked = true;
            } else if (formatOption.Equals(SupportService.COORD_FORMAT_DEGREES_DECIMAL_MINUTES)) {
                optDegDecM.IsChecked = true;
            } else {
                optDMS.IsChecked = true;
            }

            this.Title = string.Format("Format {0} options", coordType.ToString());

            UpdatePreview();
        }

        private void UpdatePreview() {
            var format = FormatSpecifier;
            if (string.IsNullOrWhiteSpace(format)) {
                format = SupportService.COORD_FORMAT_DMS;
            }

            double value = 35.33333;
            if (CoordinateType == Utilities.CoordinateType.Longitude) {
                value = 149.0333;
            }

            lblPreview.Content = String.Format("Preview: {0}", SupportService.FormatCoordinate(value, format, CoordinateType));
        }

        private void optDecDeg_Checked(object sender, RoutedEventArgs e) {
            FormatSpecifier = SupportService.COORD_FORMAT_DECIMAL_DEGREES;
            UpdatePreview();
        }

        private void optDMS_Checked(object sender, RoutedEventArgs e) {
            FormatSpecifier = SupportService.COORD_FORMAT_DMS;
            UpdatePreview();
        }

        private void optDegDecM_Checked(object sender, RoutedEventArgs e) {
            FormatSpecifier = SupportService.COORD_FORMAT_DEGREES_DECIMAL_MINUTES;
            UpdatePreview();
        }

        protected CoordinateType CoordinateType { get; private set; }

        public string FormatSpecifier { get; set; }

        private void btnCancel_Click(object sender, RoutedEventArgs e) {
            this.DialogResult = false;
            this.Close();
        }

        private void btnOk_Click(object sender, RoutedEventArgs e) {
            this.DialogResult = true;
            this.Close();
        }
    }
}
