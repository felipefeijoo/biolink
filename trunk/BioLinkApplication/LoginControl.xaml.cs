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
using System.Threading;
using BioLink.Client.Utilities;
using BioLink.Client.Extensibility;
using BioLink.Data;

namespace BioLinkApplication {
    /// <summary>
    /// Interaction logic for LoginControl.xaml
    /// </summary>
    public partial class LoginControl : UserControl {

        public static readonly RoutedEvent LoginSuccessfulEvent = EventManager.RegisterRoutedEvent("LoginSuccessful", RoutingStrategy.Bubble, typeof(LoginSuccessfulEventHandler), typeof(LoginControl));

        public LoginControl() {
            InitializeComponent();
        }

        public event RoutedEventHandler LoginSuccessful {
            add { AddHandler(LoginSuccessfulEvent, value); }
            remove { RemoveHandler(LoginSuccessfulEvent, value); }
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e) {                        
            DoLogin();
        }

        private void DoLogin() {
            btnCancel.Visibility = Visibility.Hidden;
            btnLogin.Visibility = Visibility.Hidden;

            ConnectionProfile profile = new ConnectionProfile();
            profile.Server = @"BIOLINKDEV-W7\SQLEXPRESS";
            profile.Database = "BiolinkDemo";
            profile.IntegratedSecurity = false;

            User user = new User(txtUsername.Text, txtPassword.Password, profile);
            
            string format = FindResource("LoginControl.Status.Connecting") as string;
            lblStatus.Content =  String.Format(format,  profile.Server);

            lblStatus.Foreground = SystemColors.ControlTextBrush;

            LoginAsync(user,
                () => { 
                    this.InvokeIfRequired(() => {                        
                        RaiseEvent(new LoginSuccessfulEventArgs(LoginControl.LoginSuccessfulEvent, user)); 
                    }); 
                }, 
                (errorMsg) => {
                    this.InvokeIfRequired(() => {
                        
                        btnCancel.Visibility = Visibility.Visible;
                        btnLogin.Visibility = Visibility.Visible;
                        string errorFormat = FindResource("LoginControl.Status.LoginFailed") as string;
                        lblStatus.Foreground = new SolidColorBrush(Colors.Red);
                        lblStatus.Content = String.Format(errorFormat, errorMsg);

                    });                
                }
            );
            
        }

        private void StatusUpdate(string format, params object[] args) {
            String message = String.Format(format, args);
            lblStatus.InvokeIfRequired(() => {
                lblStatus.Content = message;
            });
        }

        private void LoginAsync(User user, LoginSuccessfulDelegate onSuccess, LoginFailureDelegate onFailure) {
            Thread loginThread = new Thread(new ThreadStart(() => {

                StatusUpdate("Authenticating...");

                String message = "";
                if (user.Authenticate(out message)) {
                    if (onSuccess != null) {
                        onSuccess();
                    }
                } else {
                    if (onFailure != null) {
                        onFailure(message);
                    }
                }
            }));

            loginThread.IsBackground = true;
            loginThread.Start();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e) {
            Environment.Exit(0);
        }

        private delegate void LoginSuccessfulDelegate();
        private delegate void LoginFailureDelegate(string message);

    }

    public delegate void LoginSuccessfulEventHandler(object sender, LoginSuccessfulEventArgs e);

    public class LoginSuccessfulEventArgs : RoutedEventArgs {

        public User User { get; set; }

        public LoginSuccessfulEventArgs(RoutedEvent @event, User user)
            : base(@event) {
                this.User = user;
        }
    }

}
