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



namespace BioLink.Client.Tools {
    /// <summary>
    /// Interaction logic for PointSetDisplayOptionsWindow.xaml
    /// </summary>
    public partial class PointSetDisplayOptionsWindow : Window {

        private PointSetViewModel _pointSet;

        public PointSetDisplayOptionsWindow(PointSetViewModel pointSet) {
            InitializeComponent();
            _pointSet = pointSet;
            if (_pointSet != null) {
                ctlOptions.Shape = pointSet.PointShape;
                ctlOptions.Size = pointSet.Size;
                ctlOptions.Color = pointSet.PointColor;
                ctlOptions.DrawOutline = pointSet.DrawOutline;                
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e) {
            this.DialogResult = false;
            this.Close();
        }

        private void btnOK_Click(object sender, RoutedEventArgs e) {
            if (_pointSet != null) {
                _pointSet.PointShape = ctlOptions.Shape;
                _pointSet.Size = ctlOptions.Size;
                _pointSet.PointColor = ctlOptions.Color;
                _pointSet.DrawOutline = ctlOptions.DrawOutline;
                this.DialogResult = true;
            }
            this.Close();
        }

    }
}
