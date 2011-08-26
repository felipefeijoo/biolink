﻿using System;
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

namespace BioLink.Client.Tools {
    /// <summary>
    /// Interaction logic for SpeciesRichnessOptions.xaml
    /// </summary>
    public partial class SpeciesRichnessOptions : UserControl, IGridLayerBitmapOptions {
        public SpeciesRichnessOptions() {
            InitializeComponent();
            var models = PluginManager.Instance.GetExtensionsOfType<DistributionModel>();
            cmbModel.ItemsSource = models;
            cmbModel.SelectedIndex = 0;
            txtFilename.Text = TempFileManager.NewTempFilename("grd", "richness");

            cmbModel.SelectionChanged += new SelectionChangedEventHandler(cmbModel_SelectionChanged);
            this.Loaded += new RoutedEventHandler(SpeciesRichnessOptions_Loaded);
            this.Unloaded += new RoutedEventHandler(SpeciesRichnessOptions_Unloaded);

        }

        void SpeciesRichnessOptions_Unloaded(object sender, RoutedEventArgs e) {
            // Save the color preferences
            Config.SetUser(PluginManager.Instance.User, "Modelling.SpeciesRichness.LowColor", ctlLowValueColor.SelectedColor);
            Config.SetUser(PluginManager.Instance.User, "Modelling.SpeciesRichness.HighColor", ctlHighValueColor.SelectedColor);
            Config.SetUser(PluginManager.Instance.User, "Modelling.SpeciesRichness.NoColor", ctlNoValueColor.SelectedColor);
            Config.SetUser(PluginManager.Instance.User, "Modelling.SpeciesRichness.Cutoff", txtCutOff.Text);
            Config.SetUser(PluginManager.Instance.User, "Modelling.SpeciesRichness.RetainIntermediate", chkRetainFiles.IsChecked);
        }

        void SpeciesRichnessOptions_Loaded(object sender, RoutedEventArgs e) {
            ctlLowValueColor.SelectedColor = Config.GetUser(PluginManager.Instance.User, "Modelling.SpeciesRichness.LowColor", ctlLowValueColor.SelectedColor);
            ctlHighValueColor.SelectedColor = Config.GetUser(PluginManager.Instance.User, "Modelling.SpeciesRichness.HighColor", ctlHighValueColor.SelectedColor);
            ctlNoValueColor.SelectedColor = Config.GetUser(PluginManager.Instance.User, "Modelling.SpeciesRichness.NoColor", ctlNoValueColor.SelectedColor);
            txtCutOff.Text = Config.GetUser(PluginManager.Instance.User, "Modelling.SpeciesRichness.Cutoff", txtCutOff.Text);
            chkRetainFiles.IsChecked = Config.GetUser(PluginManager.Instance.User, "Modelling.SpeciesRichness.RetainIntermediate", chkRetainFiles.IsChecked);            
        }

        void cmbModel_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            var selected = cmbModel.SelectedItem as DistributionModel;
            if (selected != null) {
                txtCutOff.IsEnabled = !selected.PresetCutOff.HasValue;
            }            
        }

        public double CutOff {
            get {
                var selected = cmbModel.SelectedItem as DistributionModel;
                if (selected != null && selected.PresetCutOff.HasValue) {
                    return selected.PresetCutOff.Value;
                }
                return Double.Parse(txtCutOff.Text);
            }
        }

        public Color HighColor {
            get { return ctlHighValueColor.SelectedColor; }
        }

        public Color LowColor {
            get { return ctlLowValueColor.SelectedColor; }
        }

        public Color NoValueColor {
            get { return ctlNoValueColor.SelectedColor; }
        }

        public string OutputFilename {
            get { return txtFilename.Text; }
        }

        public DistributionModel SelectedModel {
            get { return cmbModel.SelectedItem as DistributionModel; }
        }

    }
}
