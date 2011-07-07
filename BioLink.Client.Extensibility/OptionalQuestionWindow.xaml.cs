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
    public partial class OptionalQuestionWindow : Window {

        public OptionalQuestionWindow(string question) {
            InitializeComponent();
            this.User = PluginManager.Instance.User;
            this.DataContext = this;
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
            Config.SetUser(User, "Material.ShowRecordIDHistoryQuestion", false);
            Config.SetUser(User, "Material.DefaultRecordIDHistory", remember);
        }

        protected User User { get; private set; }

        private void btnYes_Click(object sender, RoutedEventArgs e) {
            this.DialogResult = true;
            if (chkRemember.IsChecked.HasValue && chkRemember.IsChecked.Value) {
                Remember(true);
            }
        }

    }

}
