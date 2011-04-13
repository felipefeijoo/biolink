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
using BioLink.Data;

namespace BioLinkApplication {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : ChangeContainerWindow {

        private static MainWindow _instance;
        private BiolinkHost _hostControl;

        public MainWindow() {
            System.Threading.Thread.CurrentThread.Name = "Init Thread";
            Logger.Debug("Main window created: User {0} on {1} (CLR {2})", Environment.UserName, Environment.MachineName, Environment.Version );
            InitializeComponent();
            CodeTimer.DefaultStopAction += (name, elapsed) => { Logger.Debug("{0} took {1} milliseconds.", name, elapsed.TotalMilliseconds); };
            _instance = this;

            var v = this.GetType().Assembly.GetName().Version;
            Title = String.Format("BioLink {0}.{1}.{2}", v.Major, v.Minor, v.Revision);
            ShowLogin();
        }

        public static MainWindow Instance {
            get { return _instance; }
        }

        public bool Shutdown() {
            Logger.Debug("Shutting down initiated");
            if (_hostControl != null) {
                if (_hostControl.RequestShutdown()) {
                    Logger.Debug("Disposing Host control");
                    _hostControl.Dispose();
                    Logger.Debug("Exiting.");

                    Environment.Exit(0);
                } else {
                    return false;
                }
            } else {
                Logger.Debug("Host control is null. Exiting.");
                Environment.Exit(0);
            }

            return false;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            if (!Shutdown()) {
                Logger.Debug("Shutdown aborted.");
                e.Cancel = true;
            }
        }

        private void ShowLogin() {
            contentGrid.Children.Clear();
            LoginControl login = new LoginControl();
            login.LoginSuccessful += new RoutedEventHandler(login_LoginSuccessful);
            contentGrid.Children.Add(login);
        }

        void login_LoginSuccessful(object sender, RoutedEventArgs e) {
            contentGrid.Children.Clear();
            _hostControl = new BiolinkHost();
            _hostControl.User = (e as LoginSuccessfulEventArgs).User;
            this.User = _hostControl.User;
            contentGrid.Children.Add(_hostControl);

            Title = String.Format("BioLink - {0}\\{1} ({2})", User.ConnectionProfile.Server, User.ConnectionProfile.Database, User.Username);
            _hostControl.StartUp();
        }

        public bool LogOut() {
            if (_hostControl.RequestShutdown()) {
                _hostControl.Visibility = System.Windows.Visibility.Hidden;
                _hostControl.Dispose();
                _hostControl = null;
                ShowLogin();
                return true;
            } else {
                return false;
            }
        }

    }
}
