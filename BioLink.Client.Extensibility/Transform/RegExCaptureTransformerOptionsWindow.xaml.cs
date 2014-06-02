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
using System.Text.RegularExpressions;
using BioLink.Client.Utilities;

namespace BioLink.Client.Extensibility {
    /// <summary>
    /// Interaction logic for RegExCaptureTransformerOptionsWindow.xaml
    /// </summary>
    public partial class RegExCaptureTransformerOptionsWindow : Window {

        public RegExCaptureTransformerOptionsWindow() {
            InitializeComponent();
        }

        public RegExCaptureTransformerOptionsWindow(RegExCaptureTransformerConfig config) {
            InitializeComponent();
            txtRegex.Text = config.Pattern;
            txtOutputFormat.Text = config.OutputFormat;
            chkUseDefault.IsChecked = config.DefaultIfNotMatch;
            txtDefault.Text = config.DefaultValue;
        }

        private void btnOK_Click(object sender, RoutedEventArgs e) {
            if (Validate()) {
                this.Pattern = txtRegex.Text;
                this.OutputFormat = txtOutputFormat.Text;
                this.DefaultIfNotMatch = chkUseDefault.IsChecked.GetValueOrDefault();
                this.DefaultValue = txtDefault.Text;
                this.DialogResult = true;
                this.Hide();
            }
        }

        private bool Validate() {
            try {
                var regex = new Regex(txtRegex.Text);
                return true;
            } catch (Exception ex) {
                ErrorMessage.Show(ex.Message);
            }
            return false;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e) {
            this.Hide();
        }

        public String Pattern { get; private set; }
        public String OutputFormat { get; private set; }
        public bool DefaultIfNotMatch { get; private set; }
        public String DefaultValue { get; private set; }
    }

}
