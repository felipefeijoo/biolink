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
using System.Windows.Shapes;
using BioLink.Data.Model;

namespace BioLink.Client.Taxa {
    /// <summary>
    /// Interaction logic for ConvertTaxon.xaml
    /// </summary>
    public partial class DragDropOptions : Window {

        private TaxaPlugin _owner;

        public DragDropOptions() {
            InitializeComponent();
        }

        public DragDropOptions(TaxaPlugin owner) {
            _owner = owner;
            InitializeComponent();
            Title = owner.GetCaption("DragDropOptions.Title");
        }


        public TaxonRank ShowChooseConversion(TaxonRank sourceRank, List<TaxonRank> choices) {
            // Prepare the form...
            grid.RowDefinitions[0].Height = Zero;
            grid.RowDefinitions[1].Height = Forty;
            grid.RowDefinitions[2].Height = Forty;

            optConvert.Content = _owner.GetCaption("DragDropOptions.lblConvert", sourceRank.LongName);
            optConvert.IsChecked = true;
            cmbRanks.ItemsSource = choices;
            cmbRanks.SelectedIndex = 0;
            if (ShowDialog().GetValueOrDefault(false)) {
                return cmbRanks.SelectedItem as TaxonRank;
            }
            return null;
        }

        private static GridLength Star = new GridLength(1, GridUnitType.Star);
        private static GridLength Zero = new GridLength(0);
        private static GridLength Forty = new GridLength(40);

        internal DragDropAction ShowChooseMergeOrConvert(TaxonDropContext context) {

            optMerge.Content = _owner.GetCaption("DragDropOptions.lblMerge", context.Source.Epithet, context.Target.Epithet);

            List<TaxonRank> conversionOptions = new List<TaxonRank>();
            if (context.TargetChildRank == null) {
                conversionOptions.AddRange(context.TaxaPlugin.Service.GetChildRanks(context.TargetRank));
            } else {
                conversionOptions.Add(context.TargetChildRank);
            }

            // Prepare the form depending on how many conversion options there all
            if (conversionOptions.Count == 0) {
                // No conversion options - only show the merge option
                grid.RowDefinitions[0].Height = new GridLength(80);
                grid.RowDefinitions[1].Height = Zero;
                grid.RowDefinitions[2].Height = Zero;                
            } else if (conversionOptions.Count == 1) {
                var targetRank = conversionOptions[0];
                grid.RowDefinitions[0].Height = new GridLength(80);
                grid.RowDefinitions[1].Height = Forty;
                grid.RowDefinitions[2].Height = Zero;
                optConvert.Content = _owner.GetCaption("DragDropOptions.lblConvertAsChild", targetRank.LongName, context.Target.Epithet);
            } else {                
                cmbRanks.ItemsSource = conversionOptions;
                cmbRanks.SelectedIndex = 0;
                grid.RowDefinitions[0].Height = new GridLength(80);
                grid.RowDefinitions[1].Height = Forty;
                grid.RowDefinitions[2].Height = Forty;                
                optConvert.Content = _owner.GetCaption("DragDropOptions.lblConvert", context.SourceRank.LongName);
            }

            DragDropAction result = null;

            optMerge.IsChecked = true;
            if (ShowDialog().GetValueOrDefault(false)) {
                if (optMerge.IsChecked.GetValueOrDefault(false)) {
                    result = new MergeDropAction(context, chkCreateIDRecord.IsChecked.GetValueOrDefault(false));
                } else {

                    TaxonRank convertToRank = null;
                    if (conversionOptions.Count == 1) {
                        convertToRank = conversionOptions[0];
                    } else {
                        convertToRank = cmbRanks.SelectedItem as TaxonRank;
                    }

                    result = new ConvertingMoveDropAction(context, convertToRank);
                }
            }

            return result;
        }

        private void btnOk_Click(object sender, RoutedEventArgs e) {
            this.DialogResult = true;
            this.Hide();
        }

        private void optMerge_Click(object sender, RoutedEventArgs e) {
            chkCreateIDRecord.IsEnabled = optMerge.IsChecked.GetValueOrDefault(false);
        }
    }
}
