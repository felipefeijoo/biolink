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
using BioLink.Data.Model;
using BioLink.Client.Utilities;
using BioLink.Client.Extensibility;

namespace BioLink.Client.Extensibility {
    /// <summary>
    /// Interaction logic for LookupResults.xaml
    /// </summary>
    public partial class LookupResults : Window {

        private List<LookupResult> _model;

        public LookupResults(List<LookupResult> model) {
            InitializeComponent();
            _model = model;
            lst.ItemsSource = _model;
            lst.SelectedIndex = 0;
            Loaded += new RoutedEventHandler(LookupResults_Loaded);
        }

        void LookupResults_Loaded(object sender, RoutedEventArgs e) {
            Keyboard.Focus(lst);
        }

        public LookupResult SelectedItem {
            get { return lst.SelectedItem as LookupResult; }
        }

        private void btnAccept_Click(object sender, RoutedEventArgs e) {
            if (lst.SelectedItem != null) {
                this.DialogResult = true;
                this.Close();
            }
        }

        private void lst_MouseDoubleClick(object sender, MouseButtonEventArgs e) {

            DependencyObject src = (DependencyObject)(e.OriginalSource);
            while (!(src is Control)) {
                src = VisualTreeHelper.GetParent(src);
            }

            if (src != null && src is ListBoxItem) {
                if (lst.SelectedItem != null) {
                    this.DialogResult = true;
                    this.Close();
                }
            }
        }

    }
}
