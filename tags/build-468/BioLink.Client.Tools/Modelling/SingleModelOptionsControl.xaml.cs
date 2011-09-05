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
    /// Interaction logic for SingleModelOptionsControl.xaml
    /// </summary>
    public partial class SingleModelOptionsControl : UserControl, IGridLayerBitmapOptions {
        public SingleModelOptionsControl() {
            InitializeComponent();

            var models = PluginManager.Instance.GetExtensionsOfType<DistributionModel>();
            cmbModelType.ItemsSource = models;
            cmbModelType.SelectedIndex = 0;
            txtFilename.Text = TempFileManager.NewTempFilename("grd", "model");

            cmbModelType.SelectionChanged += new SelectionChangedEventHandler(cmbModelType_SelectionChanged);

            Loaded += new RoutedEventHandler(SingleModelOptionsControl_Loaded);
            Unloaded += new RoutedEventHandler(SingleModelOptionsControl_Unloaded);
        }

        void SingleModelOptionsControl_Unloaded(object sender, RoutedEventArgs e) {
            // Save the color preferences
            Config.SetUser(PluginManager.Instance.User, "Modelling.SingleModel.LowColor", ctlLowColor.SelectedColor);
            Config.SetUser(PluginManager.Instance.User, "Modelling.SingleModel.HighColor", ctlHighColor.SelectedColor);
            Config.SetUser(PluginManager.Instance.User, "Modelling.SingleModel.NoColor", ctlNoValColor.SelectedColor);
            Config.SetUser(PluginManager.Instance.User, "Modelling.SingleModel.Cutoff", txtCutOff.Text);
            Config.SetUser(PluginManager.Instance.User, "Modelling.SingleModel.Intervals", txtIntervals.Text);
        }

        void SingleModelOptionsControl_Loaded(object sender, RoutedEventArgs e) {
            ctlLowColor.SelectedColor = Config.GetUser(PluginManager.Instance.User, "Modelling.SingleModel.LowColor", ctlLowColor.SelectedColor);
            ctlHighColor.SelectedColor = Config.GetUser(PluginManager.Instance.User, "Modelling.SingleModel.HighColor", ctlHighColor.SelectedColor);
            ctlNoValColor.SelectedColor = Config.GetUser(PluginManager.Instance.User, "Modelling.SingleModel.NoColor", ctlNoValColor.SelectedColor);
            txtCutOff.Text = Config.GetUser(PluginManager.Instance.User, "Modelling.SingleModel.Cutoff", txtCutOff.Text);
            txtIntervals.Text = Config.GetUser(PluginManager.Instance.User, "Modelling.SingleModel.Intervals", txtIntervals.Text);
        }

        void cmbModelType_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            var selected = cmbModelType.SelectedItem as DistributionModel;
            if (selected != null) {
                txtCutOff.IsEnabled = !selected.PresetCutOff.HasValue;
                txtIntervals.IsEnabled = !selected.PresetIntervals.HasValue;
            }
        }

        public int Intervals {
            get {
                var selected = cmbModelType.SelectedItem as DistributionModel;
                if (selected != null && selected.PresetIntervals.HasValue) {
                    return selected.PresetIntervals.Value;                    
                }
                return Int32.Parse(txtIntervals.Text);
            }
        }

        public double CutOff {
            get {
                var selected = cmbModelType.SelectedItem as DistributionModel;
                if (selected != null && selected.PresetCutOff.HasValue) {
                    return selected.PresetCutOff.Value;
                }
                return Double.Parse(txtCutOff.Text);
            }
        }

        public Color HighColor {
            get { return ctlHighColor.SelectedColor; }
        }

        public Color LowColor {
            get { return ctlLowColor.SelectedColor; }
        }

        public Color NoValueColor {
            get { return ctlNoValColor.SelectedColor; }
        }
    }

    public interface IGridLayerBitmapOptions {
        Color HighColor { get; }
        Color LowColor { get; }
        Color NoValueColor { get; }
    }

}
