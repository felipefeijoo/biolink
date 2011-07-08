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
using BioLink.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace BioLink.Client.Extensibility {
    /// <summary>
    /// Interaction logic for OptionalQuestionWindow.xaml
    /// </summary>
    public partial class OptionalQuestion : Window {

        public static bool AskOrDefault(Window parentWindow, string question, string configKey, string windowTitle) {
            var user = PluginManager.Instance.User;
            var askQuestion = Config.GetUser(user, configKey + ".AskQuestion", true);
            if (askQuestion) {
                var frm = new OptionalQuestion(question, configKey);
                frm.Owner = parentWindow;
                frm.Title = windowTitle;
                frm.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                return frm.ShowDialog().ValueOrFalse();
            } else {
                return Config.GetUser(user, configKey, true);
            }
        }

        public OptionalQuestion(string question, string configKey) {
            InitializeComponent();
            this.User = PluginManager.Instance.User;
            this.DataContext = this;
            lblQuestion.Text = question;
            this.ConfigurationKey = configKey;
        }

        public ImageSource MessageBoxImage {
            get {
                var bitmap = System.Drawing.SystemIcons.Information.ToBitmap();
                return GraphicsUtils.SystemDrawingBitmapToBitmapSource(bitmap);
            }
        }

        private void btnNo_Click(object sender, RoutedEventArgs e) {
            this.DialogResult = false;
            if (chkRemember.IsChecked.HasValue && chkRemember.IsChecked.Value) {
                Remember(false);
            }
            this.Close();

        }

        protected void Remember(bool remember) {
            Config.SetUser(User, ConfigurationKey + ".AskQuestion", false);
            Config.SetUser(User, ConfigurationKey, remember);
        }

        private void btnYes_Click(object sender, RoutedEventArgs e) {
            this.DialogResult = true;
            if (chkRemember.IsChecked.HasValue && chkRemember.IsChecked.Value) {
                Remember(true);
            }
        }

        protected User User { get; private set; }

        protected string ConfigurationKey { get; private set; }

        protected string Question { get; set; }

    }

}
