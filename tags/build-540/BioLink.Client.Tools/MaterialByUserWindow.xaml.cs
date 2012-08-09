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
using BioLink.Data;
using BioLink.Data.Model;

namespace BioLink.Client.Tools {

    public partial class MaterialByUserWindow : Window {

        public MaterialByUserWindow(Action<List<LabelSetItem>> successAction = null) {
            InitializeComponent();
            this.SuccessAction = successAction;
            var service = new SupportService(User);
            var list = service.GetUsers();
            var model = new List<string>(list.Select((user) => {
                return user.Username;
            }));
            model.Insert(0, "<All Users>");
            cmbUser.ItemsSource = model;
            Loaded += new RoutedEventHandler(MaterialByUserWindow_Loaded);
        }

        void MaterialByUserWindow_Loaded(object sender, RoutedEventArgs e) {

            ctlStartDate.SetToToday();
            ctlEndDate.SetToToday();

            cmbUser.SelectedIndex = 0;
        }

        public User User {
            get { return PluginManager.Instance.User; }
        }

        protected Action<List<LabelSetItem>> SuccessAction { get; private set; }

        private void btnCancel_Click(object sender, RoutedEventArgs e) {
            this.DialogResult = false;
            this.Close();
        }

        private void btnLoad_Click(object sender, RoutedEventArgs e) {

            if (string.IsNullOrEmpty(ctlStartDate.Date)) {
                ErrorMessage.Show("Please supply a start date");
                return;
            }

            if (string.IsNullOrEmpty(ctlEndDate.Date)) {
                ErrorMessage.Show("Please supply a end date");
                return;
            }

            StatusMessage("Searching for items...");

            using (new OverrideCursor(Cursors.Wait)) {

                var service = new SupportService(User);
                var results = service.ListLabelSetItemsForUser(cmbUser.SelectedItem as string, ctlStartDate.GetDateAsDateTime(), ctlEndDate.GetDateAsDateTime());
                if (results.Count > 0) {
                    if (SuccessAction != null) {
                        StatusMessage("Add " + results.Count + " items to list...");
                        SuccessAction(results);
                        StatusMessage("Done.");
                    }
                    this.DialogResult = true;
                    this.Close();                
                } else {
                    InfoBox.Show("No matching items found.", "No results", this);
                }

            }
            
        }

        private void StatusMessage(string message) {
            lblStatus.Content = message;
            lblStatus.UpdateLayout();
        }

    }
}
