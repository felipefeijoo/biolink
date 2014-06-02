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
    /// Interaction logic for ParseDateTransformerOptionsWindow.xaml
    /// </summary>
    public partial class ParseDateTransformerOptionsWindow : Window {

        public ParseDateTransformerOptionsWindow() {
            InitializeComponent();
        }

        public ParseDateTransformerOptionsWindow(ParseDateTransformerConfig options) {
            InitializeComponent();
            txtFormat.Text = options.InputFormat;
            chkGuess.IsChecked = options.AutoDetectFormat;
        }


        private void btnCancel_Click(object sender, RoutedEventArgs e) {
            this.Hide();
        }

        private void btnOK_Click(object sender, RoutedEventArgs e) {
            if (!string.IsNullOrWhiteSpace(txtFormat.Text) || chkGuess.IsChecked.GetValueOrDefault()) {
                this.DialogResult = true;
                this.InputFormat = txtFormat.Text;
                this.AutoDetectFormat = chkGuess.IsChecked.GetValueOrDefault();
                this.Hide();
            }
        }

        public String InputFormat { get; private set; }
        public Boolean AutoDetectFormat { get; private set; }

    }
}
