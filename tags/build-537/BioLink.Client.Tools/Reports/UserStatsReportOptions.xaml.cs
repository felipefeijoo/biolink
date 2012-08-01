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
    /// <summary>
    /// Interaction logic for UserStatsReportOptions.xaml
    /// </summary>
    public partial class UserStatsReportOptions : Window {

        public UserStatsReportOptions(User user) {
            InitializeComponent();
            this.User = user;

            UserSearchResult allUsers = new UserSearchResult { Username = "(All Users)", FullName = "", UserID = -1 };

            var service = new SupportService(user);
            var model = service.GetUsers();
            model.Insert(0, allUsers);

            cmbUser.ItemsSource = model;
            cmbUser.SelectedIndex = 0;

            StartDate = DateTime.Now;
            EndDate = DateTime.Now;
            this.DataContext = this;
        }

        protected User User { get; private set; }

        private void btnCancel_Click(object sender, RoutedEventArgs e) {
            this.DialogResult = false;
            this.Close();
        }

        public String Username {
            get {
                var selected = cmbUser.SelectedItem as UserSearchResult;
                if (selected == null) {
                    return "";
                } else {
                    return (selected.UserID < 0 ? "" : selected.Username);
                }
            }
        }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        private void btnOK_Click(object sender, RoutedEventArgs e) {
            if (Validate()) {
                this.DialogResult = true;
                this.Close();
            }
        }

        private bool Validate() {
            if (String.IsNullOrWhiteSpace(dtStart.Date)) {
                ErrorMessage.Show("Please select a start date");
                return false;
            }

            if (String.IsNullOrWhiteSpace(dtEnd.Date)) {
                ErrorMessage.Show("Please select an end date");
                return false;
            }

            return true;
        }

    }
}
