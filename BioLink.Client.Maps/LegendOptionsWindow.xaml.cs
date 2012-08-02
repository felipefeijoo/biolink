using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Drawing;
using BioLink.Client.Utilities;
using BioLink.Client.Extensibility;

namespace BioLink.Client.Maps {
    /// <summary>
    /// Interaction logic for LegendOptionsWindow.xaml
    /// </summary>
    public partial class LegendOptionsWindow : Window {

        private Color _backgroundColor;
        private Color _borderColor;
        private int _borderWidth;
        private String _legendTitle;
        private int _numberOfColumns;

        public LegendOptionsWindow() {
            InitializeComponent();            
        }

        public LegendOptionsWindow(MapLegend legend) {
            InitializeComponent();
            Legend = legend;

            DataContext = this;

            BackgroundColor = Legend.BackgroundColor;
            BorderColor = Legend.BorderColor;
            BorderWidth = Legend.BorderWidth;
            LegendTitle = Legend.Title;
            TitleFont = Legend.TitleFont;
            NumberOfColumns = Legend.NumberOfColumns;
            ItemFont = Legend.ItemFont;

            Layers = new List<LegendItemDescriptorViewModel>();
            foreach (LegendItemDescriptor desc in legend.GetLayerDescriptors().Values) {
                var model = new LegendItemDescriptor {IsVisible = desc.IsVisible, LayerName = desc.LayerName, Title = desc.Title, TitleColor = desc.TitleColor};
                var viewModel = new LegendItemDescriptorViewModel(model);
                Layers.Add(viewModel);
                viewModel.DataChanged += viewModel_DataChanged;
            }

            Layers.Sort((a,b) => String.CompareOrdinal(a.LayerName, b.LayerName));

            Loaded += (sender, e) => { if (lstLayers.Items.Count > 0) lstLayers.SelectedIndex = 0; };
            

            GraphicsUtils.ApplyLegacyFont(lblTitleFont, TitleFont);
            GraphicsUtils.ApplyLegacyFont(lblItemFont, ItemFont);

            btnApply.IsEnabled = false;

            PropertyChanged += LegendOptionsWindow_PropertyChanged;
        }

        void viewModel_DataChanged(ChangeableModelBase viewmodel) {
            setDirty();
        }

        void LegendOptionsWindow_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            setDirty();
        }

        protected MapLegend Legend { get; private set; }


        public Color BackgroundColor {
            get { return _backgroundColor; }
            set { SetProperty("BackgroundColor", ref _backgroundColor, value);  }
        }

        public Color BorderColor {
            get { return _borderColor; }
            set { SetProperty("BorderColor", ref _borderColor, value); }
        }

        public int BorderWidth {
            get { return _borderWidth; }
            set { SetProperty("BorderWidth", ref _borderWidth, value); }
        }   
     
        public String LegendTitle { 
            get { return _legendTitle; }
            set { SetProperty("LegendTitle", ref _legendTitle, value); }
        }

        public int NumberOfColumns {
            get { return _numberOfColumns; }
            set { SetProperty("NumberOfColumns", ref _numberOfColumns, value); }
        }

        public Font ItemFont { get; set; }
        public Font TitleFont { get; set; }

        public List<LegendItemDescriptorViewModel> Layers { get; set; }

        private void btnOk_Click(object sender, RoutedEventArgs e) {
            if (ApplyChanges()) {
                Close();
            }
        }

        private void setDirty() {            
            btnApply.IsEnabled = true;
        }

        private bool ApplyChanges() {

            Legend.BackgroundColor = BackgroundColor;
            Legend.BorderColor = BorderColor;
            Legend.BorderWidth = BorderWidth;
            Legend.Title = LegendTitle;
            Legend.TitleFont = GraphicsUtils.GetLegacyFont(lblTitleFont);
            Legend.NumberOfColumns = NumberOfColumns;
            Legend.ItemFont = GraphicsUtils.GetLegacyFont(lblItemFont);

            var origLayers = Legend.GetLayerDescriptors();

            foreach (LegendItemDescriptorViewModel desc in Layers) {
                var orig = origLayers[desc.LayerName];
                orig.Title = desc.Title;
                orig.IsVisible = desc.IsVisible;
                orig.TitleColor = desc.TitleColor;                
            }

            Legend.SaveToSettings();

            Legend.MapBox.Refresh();
            
            btnApply.IsEnabled = false;

            return true;
        }

        private void btnTitleFont_Click(object sender, RoutedEventArgs e) {
            SelectFont(lblTitleFont);
        }

        private void SelectFont(TextBlock control) {
            var fontDlg = new FontChooser { SelectedFontFamily = control.FontFamily, SelectedFontSize = control.FontSize, SelectedFontStyle = control.FontStyle, SelectedFontWeight = control.FontWeight, Owner = this};
            if (fontDlg.ShowDialog() == true) {
                control.FontFamily = fontDlg.SelectedFontFamily;
                control.FontSize = fontDlg.SelectedFontSize;
                control.FontStyle = fontDlg.SelectedFontStyle;
                control.FontWeight = fontDlg.SelectedFontWeight;
                control.TextDecorations = fontDlg.SelectedTextDecorations;

                var points = GraphicsUtils.PixelsToPoints(this, control.FontSize);
                control.Text = String.Format("{0}, {1} pt", control.FontFamily.Source, points);

                setDirty();
            }
        }

        private void btnItemFont_Click(object sender, RoutedEventArgs e) {
            SelectFont(lblItemFont);
        }

        private void btnApply_Click(object sender, RoutedEventArgs e) {
            ApplyChanges();
        }

        /// <summary>
        /// Helper function to be called from within property setters that will automatically fire the property changed event (if required),
        /// and sets the dirty flag
        /// </summary>
        /// <typeparam name="T">Type of the property</typeparam>
        /// <param name="propertyName">Name of the property being set</param>
        /// <param name="backingField">A ref to the property backing member</param>
        /// <param name="value">The new value</param>
        /// <param name="changeAgnostic"> </param>
        /// <returns>true if the property has changed</returns>
        protected bool SetProperty<T>(string propertyName, ref T backingField, T value, bool changeAgnostic = false) {
            var changed = !EqualityComparer<T>.Default.Equals(backingField, value);
            if (changed) {
                backingField = value;
                RaisePropertyChanged(propertyName);
                if (!SuspendChangeMonitoring && !changeAgnostic) {
                    IsChanged = true;
                }
            }
            return changed;
        }

        protected void RaisePropertyChanged(string propertyName) {
            if (PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public bool IsChanged { get; set; }


        public bool SuspendChangeMonitoring { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

    }
}
