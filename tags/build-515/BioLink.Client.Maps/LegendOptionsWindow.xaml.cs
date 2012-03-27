using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Shapes;
using System.Drawing;
using BioLink.Client.Utilities;
using BioLink.Client.Extensibility;

namespace BioLink.Client.Maps {
    /// <summary>
    /// Interaction logic for LegendOptionsWindow.xaml
    /// </summary>
    public partial class LegendOptionsWindow : Window {

        protected MapLegend Legend { get; private set; }

        public LegendOptionsWindow() {
            InitializeComponent();            
        }

        public LegendOptionsWindow(MapLegend legend) {
            InitializeComponent();
            this.Legend = legend;

            this.DataContext = this;

            BackgroundColor = Legend.BackgroundColor;
            BorderColor = Legend.BorderColor;
            BorderWidth = Legend.BorderWidth;
            LegendTitle = Legend.Title;
            TitleFont = Legend.TitleFont;
            NumberOfColumns = Legend.NumberOfColumns;
            ItemFont = Legend.ItemFont;

            Layers = new List<LegendItemDescriptor>(legend.GetLayerDescriptors().Values);
            Layers.Sort((a,b) => {
                return a.LayerName.CompareTo(b.LayerName);
            });

            GraphicsUtils.ApplyLegacyFont(lblTitleFont, TitleFont);
            GraphicsUtils.ApplyLegacyFont(lblItemFont, ItemFont);
        }

        public Color BackgroundColor { get; set; }
        public Color BorderColor { get; set; }
        public int BorderWidth { get; set; }
        public Font TitleFont { get; set; }
        public String LegendTitle { get; set; }
        public int NumberOfColumns { get; set; }
        public Font ItemFont { get; set; }

        public List<LegendItemDescriptor> Layers { get; set; }

        private void btnOk_Click(object sender, RoutedEventArgs e) {
            if (ApplyChanges()) {
                this.Close();
            }
        }

        private bool ApplyChanges() {

            Legend.BackgroundColor = BackgroundColor;
            Legend.BorderColor = BorderColor;
            Legend.BorderWidth = BorderWidth;
            Legend.Title = LegendTitle;
            Legend.TitleFont = GraphicsUtils.GetLegacyFont(lblTitleFont);
            Legend.NumberOfColumns = NumberOfColumns;
            Legend.ItemFont = GraphicsUtils.GetLegacyFont(lblItemFont);
            Legend.SaveToSettings();

            Legend.MapBox.Refresh();

            return true;
        }

        private void btnTitleFont_Click(object sender, RoutedEventArgs e) {
            SelectFont(lblTitleFont);
        }

        private void SelectFont(TextBlock control) {
            var fontDlg = new FontChooser();
            fontDlg.SelectedFontFamily = control.FontFamily;
            fontDlg.SelectedFontSize = control.FontSize;
            fontDlg.SelectedFontStyle = control.FontStyle;
            fontDlg.SelectedFontWeight = control.FontWeight;
            fontDlg.Owner = this;
            if (fontDlg.ShowDialog() == true) {
                control.FontFamily = fontDlg.SelectedFontFamily;
                control.FontSize = fontDlg.SelectedFontSize;
                control.FontStyle = fontDlg.SelectedFontStyle;
                control.FontWeight = fontDlg.SelectedFontWeight;
                control.TextDecorations = fontDlg.SelectedTextDecorations;
                control.Text = String.Format("{0}, {1} pt", control.FontFamily.Source, control.FontSize);
            }
        }

        private void btnItemFont_Click(object sender, RoutedEventArgs e) {
            SelectFont(lblItemFont);
        }

        private void btnApply_Click(object sender, RoutedEventArgs e) {
            ApplyChanges();
        }

    }
}
