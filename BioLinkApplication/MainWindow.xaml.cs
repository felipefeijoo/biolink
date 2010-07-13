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
using System.Net;


namespace BioLinkApplication {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {

        private static MainWindow _instance;

        public MainWindow() {
            System.Threading.Thread.CurrentThread.Name = "Init Thread";
            Logger.Debug("Main window created: User {0} on {1} (CLR {2})", Environment.UserName, Environment.MachineName, Environment.Version );
            InitializeComponent();
            CodeTimer.DefaultStopAction += (name, elapsed) => { Logger.Debug("{0} took {1} milliseconds.", name, elapsed.TotalMilliseconds); };
            _instance = this;            
        }

        public static MainWindow Instance {
            get { return _instance; }
        }

        private void loginControl1_LoginSuccessful(object sender, RoutedEventArgs e) {
            contentGrid.Children.Clear();
            BiolinkHost host = new BiolinkHost();            
            contentGrid.Children.Add(host);
            host.StartUp();
        }

    }
}
