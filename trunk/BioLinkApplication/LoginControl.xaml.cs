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

namespace BioLinkApplication {
    /// <summary>
    /// Interaction logic for LoginControl.xaml
    /// </summary>
    public partial class LoginControl : UserControl {

        public static readonly RoutedEvent LoginSuccessfulEvent = EventManager.RegisterRoutedEvent("LoginSuccessful", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(LoginControl));

        public LoginControl() {
            InitializeComponent();
        }

        public event RoutedEventHandler LoginSuccessful {
            add { AddHandler(LoginSuccessfulEvent, value); }
            remove { RemoveHandler(LoginSuccessfulEvent, value); }
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e) {
            RaiseEvent(new RoutedEventArgs(LoginControl.LoginSuccessfulEvent));
        }

    }

}
