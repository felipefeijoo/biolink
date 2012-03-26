/*******************************************************************************
 * Copyright (C) 2011 Atlas of Living Australia
 * All Rights Reserved.
 * 
 * The contents of this file are subject to the Mozilla Public
 * License Version 1.1 (the "License"); you may not use this file
 * except in compliance with the License. You may obtain a copy of
 * the License at http://www.mozilla.org/MPL/
 * 
 * Software distributed under the License is distributed on an "AS
 * IS" basis, WITHOUT WARRANTY OF ANY KIND, either express or
 * implied. See the License for the specific language governing
 * rights and limitations under the License.
 ******************************************************************************/
using System.Windows;
using System.Windows.Media;
using BioLink.Client.Utilities;
using BioLink.Data;

namespace BioLink.Client.Extensibility {
    /// <summary>
    /// Interaction logic for OptionalQuestionWindow.xaml
    /// </summary>
    public partial class OptionalQuestionWindow : Window {

        public static bool AskOrDefault(Window parentWindow, OptionalQuestion question, params object[] args) {
            var user = PluginManager.Instance.User;
            var askQuestion = Config.GetUser(user, question.AskQuestionConfigurationKey, true);
            if (askQuestion) {
                var frm = new OptionalQuestionWindow(question, args);
                frm.Owner = parentWindow;
                frm.Title = question.QuestionTitle;
                frm.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                return frm.ShowDialog().ValueOrFalse();
            } else {
                return Config.GetUser(user, question.ConfigurationKey, true);
            }
        }

        protected OptionalQuestionWindow(OptionalQuestion question, params object[] args) {
            InitializeComponent();
            this.User = PluginManager.Instance.User;
            this.DataContext = this;
            lblQuestion.Text = string.Format(question.QuestionText, args);
            this.Question = question;
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
            Config.SetUser(User, Question.AskQuestionConfigurationKey, false);
            Config.SetUser(User, Question.ConfigurationKey, remember);
        }

        private void btnYes_Click(object sender, RoutedEventArgs e) {
            this.DialogResult = true;
            if (chkRemember.IsChecked.HasValue && chkRemember.IsChecked.Value) {
                Remember(true);
            }
        }

        protected User User { get; private set; }

        protected OptionalQuestion Question { get; private set; }

    }

}
