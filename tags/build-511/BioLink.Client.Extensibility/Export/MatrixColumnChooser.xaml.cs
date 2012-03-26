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
using BioLink.Data;

namespace BioLink.Client.Extensibility.Export {
    /// <summary>
    /// Interaction logic for MatrixColumnChooser.xaml
    /// </summary>
    public partial class MatrixColumnChooser : Window {

        public MatrixColumnChooser(DataMatrix matrix, string caption) {
            InitializeComponent();
            this.Matrix = matrix;
            lblCaption.Content = caption;

            var names = matrix.Columns.FindAll((col) => { return !col.IsHidden; }).Select((col) => {
                return col.Name;
            });

            cmbColumn.ItemsSource = names;

            if (names != null && names.Count() > 0) {
                cmbColumn.SelectedItem = names.ElementAt(0);
            }

        }

        public DataMatrix Matrix { get; private set; }

        private void btnCancel_Click(object sender, RoutedEventArgs e) {
            this.DialogResult = false;
            Hide();
        }

        private void btnOK_Click(object sender, RoutedEventArgs e) {
            if (cmbColumn.SelectedItem != null) {
                this.DialogResult = true;
                Hide();
            }
        }

        public String SelectedColumn {
            get { return cmbColumn.SelectedItem as string; }
        }

    }
}
