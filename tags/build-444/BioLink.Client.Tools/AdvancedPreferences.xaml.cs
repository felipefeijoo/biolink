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
using System.Reflection;

namespace BioLink.Client.Tools {
    /// <summary>
    /// Interaction logic for AdvancedPreferences.xaml
    /// </summary>
    public partial class AdvancedPreferences : Window {

        public AdvancedPreferences() {

            InitializeComponent();

            var questionFields = typeof(OptionalQuestions).GetMembers(BindingFlags.Public | BindingFlags.Static);
            
            for (int i = 0; i < questionFields.Length; ++i) {
                var questionField = questionFields[i] as FieldInfo;         
       
                var question = questionField.GetValue(null) as OptionalQuestion;
                if (question != null) {
                    gridOptionalQuestions.RowDefinitions.Add(new RowDefinition { Height = new GridLength(50) });
                    bool askQuestion = Config.GetUser(PluginManager.Instance.User, question.AskQuestionConfigurationKey, true);
                    bool defaultAnswer = Config.GetUser(PluginManager.Instance.User, question.ConfigurationKey, true);
                    var chkAskQuestion = new CheckBox { Content = question.ConfigurationText, IsChecked = askQuestion, VerticalAlignment = System.Windows.VerticalAlignment.Top };
                    chkAskQuestion.Margin = new Thickness(6, 0, 0, 0);
                    Grid.SetRow(chkAskQuestion, i);

                    var chkDefaultAnswer = new CheckBox { Content = "If not, is the default answer yes?", IsChecked = defaultAnswer, VerticalAlignment = System.Windows.VerticalAlignment.Top };
                    chkDefaultAnswer.Margin = new Thickness(30, 25, 0, 0);
                    Grid.SetRow(chkDefaultAnswer, i);

                    chkDefaultAnswer.Unloaded += new RoutedEventHandler((s, e) => {
                        Config.SetUser(PluginManager.Instance.User, question.AskQuestionConfigurationKey, chkAskQuestion.IsChecked.ValueOrFalse());
                        Config.SetUser(PluginManager.Instance.User, question.ConfigurationKey, chkDefaultAnswer.IsChecked.ValueOrFalse());
                    });

                    gridOptionalQuestions.Children.Add(chkAskQuestion);
                    gridOptionalQuestions.Children.Add(chkDefaultAnswer);
                }
            }

            txtMaxSearchResults.Text = Config.GetUser(User, "SearchResults.MaxSearchResults", 2000).ToString();

            chkCheckDuplicateAccessionNumbers.IsChecked = Config.GetGlobal("Material.CheckUniqueAccessionNumbers", true);

            chkUsePostHestControl.IsChecked = Config.GetUser(PluginManager.Instance.User, "Associates.UsePestHostControl", false);
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

            int maxResults = 0;
            if (Int32.TryParse(txtMaxSearchResults.Text, out maxResults)) {
                Config.SetUser(User, "SearchResults.MaxSearchResults", maxResults);
            }

            Config.SetGlobal("Material.CheckUniqueAccessionNumbers", chkCheckDuplicateAccessionNumbers.IsChecked.GetValueOrDefault(true));

            Config.SetUser(PluginManager.Instance.User, "Associates.UsePestHostControl", chkUsePostHestControl.IsChecked.ValueOrFalse());
        }
    }
}
