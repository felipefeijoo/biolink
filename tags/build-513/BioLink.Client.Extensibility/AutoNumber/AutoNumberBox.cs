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
using System.Windows.Forms;
using BioLink.Client.Utilities;
using BioLink.Data;

namespace BioLink.Client.Extensibility {

    public class AutoNumberBox : EllipsisTextBox {

        public AutoNumberBox() {
            InitializeComponent();
            Click += new System.Windows.RoutedEventHandler(AutoNumberBox_Click);
        }

        public void BindUser(User user, string autoNumberCategory, string table, string field) {
            this.AutoNumberCategory = autoNumberCategory;
            this.AutoNumberTable = table;
            this.AutoNumberField = field;
            this.User = user;

        }

        void AutoNumberBox_Click(object sender, System.Windows.RoutedEventArgs e) {
            ShowAutoNumber();
        }

        private void ShowAutoNumber() {

            Debug.Assert(User != null, "User has not been set!");

            var frm = new AutoNumberOptions(User, AutoNumberCategory, AutoNumberTable, AutoNumberField);
            frm.Owner = this.FindParentWindow();
            if (frm.ShowDialog().ValueOrFalse()) {
                txt.Text = frm.AutoNumber;                     
            }
        }

        public void GenerateNumber() {
            Debug.Assert(User != null, "User has not been set!");

            var frm = new AutoNumberOptions(User, AutoNumberCategory, AutoNumberTable, AutoNumberField);
            frm.Owner = this.FindParentWindow();
            if (frm.GenerateNumber()) {
                txt.Text = frm.AutoNumber;
            }

        }

        public string AutoNumberCategory { get; set; }

        public string AutoNumberTable { get; set; }

        public string AutoNumberField { get; set; }

        public User User { get; private set; }

    }
}
