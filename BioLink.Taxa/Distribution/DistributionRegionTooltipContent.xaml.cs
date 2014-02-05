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

namespace BioLink.Client.Taxa {
    /// <summary>
    /// Interaction logic for DistributionRegionTooltipContent.xaml
    /// </summary>
    public partial class DistributionRegionTooltipContent : UserControl {

        public DistributionRegionTooltipContent(User user, int regionId) {
            InitializeComponent();
            this.User = user;
            this.DistRegionID = regionId;
            Loaded += new RoutedEventHandler(DistributionRegionTooltipContent_Loaded);
        }

        void DistributionRegionTooltipContent_Loaded(object sender, RoutedEventArgs e) {
            var service = new SupportService(User);
            var fullPath = service.GetDistributionFullPath(DistRegionID);

            var icon = ImageCache.GetImage("pack://application:,,,/BioLink.Client.Extensibility;component/images/DistributionRegion.png");

            imgIcon.Source = icon;


            if (fullPath != null) {
                var regions = fullPath.Split('\\');

                for (int i = 0; i < regions.Length; ++i) {
                    var region = regions[i];
                    var parentPanel = new StackPanel() { Orientation = Orientation.Horizontal, Margin = new Thickness(i * 15, i * 25, 0, 0) };
                    var parentIcon = new Image() { VerticalAlignment = System.Windows.VerticalAlignment.Top, UseLayoutRounding = true, SnapsToDevicePixels = true, Stretch = Stretch.None, Margin = new Thickness(6, 0, 6, 0) };
                    parentIcon.Source = icon;
                    parentPanel.Children.Add(parentIcon);
                    var weight = FontWeights.Normal;

                    if (i == regions.Length - 1) {
                        weight = FontWeights.Bold;
                        lblHeader.Content = region;
                    }

                    var txt = new TextBlock() { VerticalAlignment = System.Windows.VerticalAlignment.Top, Text = region, FontWeight = weight };
                    parentPanel.Children.Add(txt);
                    grdAncestry.Children.Add(parentPanel);

                }
            }

            lblSystem.Content = String.Format("Distribution Region ID: {0}", DistRegionID);

        }


        protected User User { get; private set; }
        protected int DistRegionID { get; private set; }
    }
}
