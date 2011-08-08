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

namespace BioLink.Client.Utilities {

    /// <summary>
    /// Helper class for asking simple questions and collection single string answers
    /// </summary>
    public partial class InputBox : Window {
        public InputBox() {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(InputBox_Loaded);
        }

        void InputBox_Loaded(object sender, RoutedEventArgs e) {
            txt.Focus();
        }

        public static void Show(Window owner, string title, object question, Action<string> handler) {
            Show(owner, title, question, null, handler);
        }

        public static void Show(Window owner, string title, object question, string prefill, Action<string> handler) {
            InputBox frm = new InputBox();
            frm.Owner = owner;
            frm.Title = title;
            frm.lblQuestion.Content = question;

            if (!String.IsNullOrEmpty(prefill)) {
                frm.txt.Text = prefill;
                frm.txt.SelectAll();
            }

            if (frm.ShowDialog().GetValueOrDefault(false)) {
                if (handler != null) {
                    handler(frm.txt.Text);
                }
            }
        }

        private void btnOK_Click(object sender, RoutedEventArgs e) {
            this.DialogResult = true;
            this.Hide();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e) {
            this.DialogResult = false;
            this.Hide();
        }

    }
}
