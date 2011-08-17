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

namespace BioLink.Client.Extensibility {
    /// <summary>
    /// Interaction logic for PointSetOptionsWindow.xaml
    /// </summary>
    public partial class PointSetOptionsWindow : Window {

        public PointSetOptionsWindow(string caption, Func<MapPointSet> generator) {
            InitializeComponent();
            this.Generator = generator;
            this.Caption = caption;
            this.Title = "Point options - " + caption;
        }

        protected Func<MapPointSet> Generator { get; private set; }

        protected string Caption { get; private set; }

        public MapPointSet Points { get; private set; }

        private void btnOK_Click(object sender, RoutedEventArgs e) {

            btnCancel.IsEnabled = false;
            btnOK.IsEnabled = false;
            lblStatus.Content = "Generating points...";
            this.Cursor = Cursors.Wait;
            JobExecutor.QueueJob(() => {
                if (Generator != null) {
                    Points = Generator();
                    this.InvokeIfRequired(() => {
                        Points.PointColor = shapeOptions.Color;
                        Points.PointShape = shapeOptions.Shape;
                        Points.Size = shapeOptions.Size;
                        Points.DrawOutline = shapeOptions.DrawOutline;                        
                    });
                }
                this.InvokeIfRequired(() => {
                    lblStatus.Content = "";
                    this.DialogResult = true;
                    this.Close();
                    this.Cursor = Cursors.Arrow;
                });
            });
            
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e) {
            this.Close();
        }

    }

}
