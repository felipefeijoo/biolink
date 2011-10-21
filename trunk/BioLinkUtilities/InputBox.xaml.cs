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
using System.Windows;

namespace BioLink.Client.Utilities {

    /// <summary>
    /// Helper class for asking simple questions and collection single string answers
    /// </summary>
    public partial class InputBox : Window {
        public InputBox() {
            InitializeComponent();
            Loaded += InputBox_Loaded;
        }

        void InputBox_Loaded(object sender, RoutedEventArgs e) {
            txt.Focus();
        }

        public static void Show(Window owner, string title, object question, Action<string> handler) {
            Show(owner, title, question, null, handler);
        }

        public static void Show(Window owner, string title, object question, string prefill, Action<string> handler) {
            var frm = new InputBox {Owner = owner, Title = title, lblQuestion = {Content = question}};

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
            DialogResult = true;
            Hide();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e) {
            DialogResult = false;
            Hide();
        }

    }
}
