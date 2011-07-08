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
using BioLink.Client.Extensibility;
using BioLink.Data;

namespace BioLink.Client.Tools {
    /// <summary>
    /// Interaction logic for AdvancedPreferences.xaml
    /// </summary>
    public partial class AdvancedPreferences : Window {

        public AdvancedPreferences() {
            InitializeComponent();

            chkAskIDHistoryQuestion.IsChecked = Config.GetUser(User, "Material.DefaultRecordIDHistory.AskQuestion", true);
            chkDefaultIDHistoryCreation.IsChecked = Config.GetUser(User, "Material.DefaultRecordIDHistory", true);

            chkAskUpdateLocality.IsChecked = Config.GetUser(User, "Material.UpdateLocalityFromEGaz.AskQuestion", true);
            chkUpdateLocality.IsChecked = Config.GetUser(User, "Material.UpdateLocalityFromEGaz", true);

            txtMaxSearchResults.Text = Config.GetUser(User, "SearchResults.MaxSearchResults", 2000).ToString();
        }

        public User User {
            get { return PluginManager.Instance.User; }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e) {
            this.Close();
        }

        private void btnOK_Click(object sender, RoutedEventArgs e) {
            SavePreferences();
            this.Close();
        }

        private void SavePreferences() {
            Config.SetUser(User, "Material.DefaultRecordIDHistory.AskQuestion", chkAskIDHistoryQuestion.IsChecked.ValueOrFalse());
            Config.SetUser(User, "Material.DefaultRecordIDHistory", chkDefaultIDHistoryCreation.IsChecked.ValueOrFalse());

            Config.SetUser(User, "Material.UpdateLocalityFromEGaz.AskQuestion", chkAskUpdateLocality.IsChecked.ValueOrFalse());
            Config.SetUser(User, "Material.UpdateLocalityFromEGaz", chkUpdateLocality.IsChecked.ValueOrFalse());

            int maxResults = 0;
            if (Int32.TryParse(txtMaxSearchResults.Text, out maxResults)) {
                Config.SetUser(User, "SearchResults.MaxSearchResults", maxResults);
            }
        }
    }
}
