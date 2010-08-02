using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Globalization;

using BioLink.Data.Model;
using BioLink.Client.Utilities;
using BioLink.Client.Extensibility;

namespace BioLink.Client.Taxa {

    public class TaxonViewModel : HierarchicalViewModelBase, ITaxon {

        private static Dictionary<string, IconMetaData> _TaxaIconMetaData = new Dictionary<string, IconMetaData>();
        private static Dictionary<string, BitmapSource> _ElemTypeIconCache = new Dictionary<string, BitmapSource>();

        private static Color DefaultBlue = Color.FromRgb(4, 4, 129);

        static TaxonViewModel() {                        
            AddIconBindings("HigherOrder", "H", DefaultBlue, "C", "CHRT", "D", "HO", "INC", "INO", "KING", "O", "P", "SBC", "SBD", "SKING", "SBO", "SBP", "SPC", "SPF", "SPO");
            AddIconBindings("Family", "F", Color.FromRgb(135,192,135), "F");
            AddIconBindings("SubFamily", "SF", Color.FromRgb(86,86,255), "SF");
            AddIconBindings("Genus", "G", Color.FromRgb(0, 148, 148), "G");
            AddIconBindings("SubGenus", "sg", Color.FromRgb(44, 135, 192), "SG");
            AddIconBindings("Species", "Sp", Color.FromRgb(174, 121, 59), "SP");
            AddIconBindings("SubSpecies", "ssp", Color.FromRgb(135, 135, 192), "SSP");
            AddIconBindings("Tribe", "T", Color.FromRgb(44, 135, 86), "T");
            AddIconBindings("Section", "SE", Color.FromRgb(0, 148, 148), "SCT");
            AddIconBindings("Series", "SR", Color.FromRgb(86, 135, 135), "SRS");
            AddIconBindings("SpeciesGroup", "SG", Color.FromRgb(255, 254, 193), "SGP");
            AddIconBindings("SuperTribe", "ST", Color.FromRgb(0, 0, 128), "ST");
            AddIconBindings("Form", "f", Color.FromRgb(135, 135, 192), "FM");
            AddIconBindings("SubForm", "sf", Color.FromRgb(177, 196, 255), "SFM");
            AddIconBindings("Variety", "V", Color.FromRgb(153, 153, 153), "V");
            AddIconBindings("SubVariety", "sv", Color.FromRgb(76, 76, 76), "SV");
            AddIconBindings("SubSection", "sse", Color.FromRgb(74, 148, 0), "SSCT");
            AddIconBindings("SubSeries", "ssr", Color.FromRgb(0, 92, 57), "SSRS");
            AddIconBindings("SubTribe", "sst", Color.FromRgb(71, 126, 161), "SBT");
            // Special pseudo ranks...
            AddIconBindings("SpeciesInquirenda", "SI", Color.FromRgb(0, 128, 128), "SI");
            AddIconBindings("IncertaeSedis", "IS", Color.FromRgb(139, 197, 89), "IS");
        }

        private static void AddIconBindings(string iconName, string caption, Color color, params string[] elemTypes) {
            foreach (string elemType in elemTypes) {
                IconMetaData md = new IconMetaData(iconName, caption, color);
                _TaxaIconMetaData.Add(elemType, md);
            }
        }

        public TaxonViewModel(TaxonViewModel parent, Taxon taxon)
            : base() {
            this.Parent = parent;
            this.Taxon = taxon;
            this.IsChanged = false;
            this.DataChanged += new DataChangedHandler(TaxonViewModel_DataChanged);
        }

        void TaxonViewModel_DataChanged() {
            // Force the icon to be reconstructed, possibly now with a changed badge/overlay
            _image = null;
            RaisePropertyChanged("Icon");
        }

        public Taxon Taxon { get; private set; }

        public override string Label {
            get { return TaxaFullName; }
        }

        public int? TaxaID {
            get { return Taxon.TaxaID; }
            set { SetProperty(() => Taxon.TaxaID, Taxon, value); }
        }

        public int? TaxaParentID {
            get { return Taxon.TaxaParentID; }
            set { SetProperty(() => Taxon.TaxaParentID, Taxon, value); }
        }

        public string Epithet {
            get { return Taxon.Epithet; }
            set { SetProperty(() => Taxon.Epithet, Taxon, value); }
        }

        public string TaxaFullName {
            get { return Taxon.TaxaFullName; }
            set { SetProperty(() => Taxon.TaxaFullName, Taxon, value); }
        }

        public string YearOfPub {
            get { return Taxon.YearOfPub; }
            set { SetProperty(() => Taxon.YearOfPub, Taxon, value); }
        }

        public string Author {
            get { return Taxon.Author; }
            set { SetProperty(() => Taxon.Author, Taxon, value); }
        }

        public string ElemType {
            get { return Taxon.ElemType; }
            set { SetProperty(() => Taxon.ElemType, Taxon, value); }
        }

        public string KingdomCode {
            get { return Taxon.KingdomCode; }
            set { SetProperty(() => Taxon.KingdomCode, Taxon, value); }
        }

        public bool? Unplaced {
            get { return Taxon.Unplaced; }
            set { SetProperty(() => Taxon.Unplaced, Taxon, value); }
        }

        public int? Order {
            get { return Taxon.Order; }
            set { SetProperty(() => Taxon.Order, Taxon, value); }
        }

        public string Rank {
            get { return Taxon.Rank; }
            set { SetProperty(() => Taxon.Rank, Taxon, value); }
        }

        public bool? ChgComb {
            get { return Taxon.ChgComb; }
            set { SetProperty(() => Taxon.ChgComb, Taxon, value); }
        }

        public bool? Unverified {
            get { return Taxon.Unverified; }
            set { SetProperty(() => Taxon.Unverified, Taxon, value); }
        }

        public bool? AvailableName {
            get { return Taxon.AvailableName; }
            set { SetProperty(() => Taxon.AvailableName, Taxon, value); }
        }

        public bool? LiteratureName {
            get { return Taxon.LiteratureName; }
            set { SetProperty(() => Taxon.LiteratureName, Taxon, value); }
        }

        public string NameStatus {
            get { return Taxon.NameStatus; }
            set { SetProperty(() => Taxon.NameStatus, Taxon, value); }
        }

        public int? NumChildren {
            get { return Taxon.NumChildren; }
            set { SetProperty(() => Taxon.NumChildren, Taxon, value); }
        }

        private BitmapSource _image;

        private Pen AnimaliaBorder {
            get { return new Pen(new SolidColorBrush(Colors.Black), 2); }
        }

        private Pen PlantaeBorder {
            get { return new Pen(new SolidColorBrush(Color.FromRgb(184,71,19)), 2); }
        }

        public void BulkAddChildren(List<Taxon> taxa) {

            if (Children.Count == 1 && Children[0] is ViewModelPlaceholder) {
                Children.Clear();
            }

            foreach (Taxon taxon in taxa) {
                TaxonViewModel model = new TaxonViewModel(this, taxon);
                Children.Add(model);
            }
            
        }

        private BitmapSource ConstructIcon() {

            // Available names don't have icons
            if (AvailableName.GetValueOrDefault(false)) {
                return null;
            }

            // Also the top level container doesn't get an Icon either
            if (TaxaParentID < 0) {
                return null;
            }

            BitmapSource baseIcon = null;

            if (!IsChanged && _ElemTypeIconCache.ContainsKey(ElemType)) {
                baseIcon = _ElemTypeIconCache[ElemType];
            }

            if (baseIcon != null && !IsChanged) {
                return baseIcon;
            }

            if (baseIcon == null) {
                RenderTargetBitmap bmp = new RenderTargetBitmap(22, 22, 96, 96, PixelFormats.Pbgra32);
                DrawingVisual drawingVisual = new DrawingVisual();
                DrawingContext dc = drawingVisual.RenderOpen();

                IconMetaData md = null;
                if (_TaxaIconMetaData.ContainsKey(ElemType)) {
                    md = _TaxaIconMetaData[ElemType];
                }

                Color taxonColor = (md == null ? DefaultBlue : md.Color);
                string caption = (md == null ? "?" : md.Caption);

                Pen pen = new Pen(new SolidColorBrush(taxonColor), 2);
                Brush textBrush = new SolidColorBrush(Color.FromArgb(200, 0, 0, 0));
                Brush fillBrush = new SolidColorBrush(Color.FromArgb(20, taxonColor.R, taxonColor.G, taxonColor.B));
                Typeface typeface = new Typeface(new FontFamily("Palatino Linotype,Times New Roman"), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);

                dc.DrawRoundedRectangle(fillBrush, pen, new Rect(1, 1, 20, 20), 4, 4);
                FormattedText t = new FormattedText(caption, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, typeface, 10, textBrush);
                double originX = (bmp.Width / 2) - (t.Width / 2);
                double originY = (bmp.Height / 2) - (t.Height / 2);
                dc.DrawText(t, new Point(originX, originY));
                dc.Close();
                bmp.Render(drawingVisual);

                if (ElemType != null && !_ElemTypeIconCache.ContainsKey(ElemType)) {
                    _ElemTypeIconCache.Add(ElemType, bmp);
                }

                baseIcon = bmp;
            }

            
            if (IsChanged) {
                string assemblyName = this.GetType().Assembly.GetName().Name;
                return ImageCache.ApplyOverlay(baseIcon, String.Format("pack://application:,,,/{0};component/images/ChangedOverlay.png", assemblyName));
            }

            return baseIcon;
        }

        public override BitmapSource Icon {
            get {
                if (_image == null) {
                    _image = ConstructIcon();
                }
                return _image;
            }

            set {
                _image = value;
                RaisePropertyChanged("Icon");
            }
        }

        public string GetParentage() {
            String parentage = "";
            TraverseToTop((t) => {
                parentage = "/" + (t as TaxonViewModel).TaxaID + parentage;
            });
            return parentage;
        }

        public override string ToString() {
            return String.Format("TaxonViewModel: [{0}] {1}", ElemType, TaxaFullName);
        }

    }

    internal class IconMetaData {

        public IconMetaData(string name, string caption, Color color) {
            this.Name = name;
            this.Caption = caption;
            this.Color = color;
        }

        public string Name { get; set; }
        public string Caption { get; set; }
        public Color Color { get; set; }

    }

}
