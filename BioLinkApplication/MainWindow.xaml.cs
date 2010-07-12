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
using BioLink.Utilities;
using BioLink.Client.Extensibility;

namespace BioLinkApplication {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
            CodeTimer.DefaultStopAction += (name, elapsed) => { Debug.Log("{0} took {1} milliseconds.", name, elapsed.TotalMilliseconds); };
        }

        private void loginControl1_LoginSuccessful(object sender, RoutedEventArgs e) {
            contentGrid.Children.Clear();
            BiolinkHost host = new BiolinkHost();            
            contentGrid.Children.Add(host);
            host.StartUp();
        }

    }
}
