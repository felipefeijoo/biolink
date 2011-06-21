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
using System.Drawing;

namespace BioLink.Client.Utilities {
    /// <summary>
    /// Interaction logic for DiscardChangesWindow.xaml
    /// </summary>
    public partial class DiscardChangesWindow : Window {

        public DiscardChangesWindow() {
            InitializeComponent();
            imgIcon.Source = GraphicsUtils.SystemDrawingImageToBitmapSource(SystemIcons.Question.ToBitmap());
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
        }

        private void btnDiscard_Click(object sender, RoutedEventArgs e) {
            this.DialogResult = true;
            this.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e) {
            this.DialogResult = false;
            this.Close();
        }
    }
}
