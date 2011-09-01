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

namespace BioLinkApplication {
    /// <summary>
    /// Interaction logic for ThirdPartyComponentControl.xaml
    /// </summary>
    public partial class ThirdPartyComponentControl : UserControl {
        public ThirdPartyComponentControl(Credit credit) {
            InitializeComponent();
            this.DataContext = credit;
        }

        private void TextBlock_MouseUp(object sender, MouseButtonEventArgs e) {
            SystemUtils.ShellExecute(txtURL.Text);
        }
    }
}
