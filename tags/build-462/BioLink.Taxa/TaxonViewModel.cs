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

using BioLink.Data;
using BioLink.Data.Model;
using BioLink.Client.Utilities;
using BioLink.Client.Extensibility;

namespace BioLink.Client.Taxa {

    public class TaxonViewModel : HierarchicalViewModelBase {

        private const int _IconSize = 18;

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
            AddIconBindings("Form", "favorite", Color.FromRgb(135, 135, 192), "FM");
            AddIconBindings("SubForm", "sf", Color.FromRgb(177, 196, 255), "SFM");
            AddIconBindings("Variety", "V", Color.FromRgb(153, 153, 153), "V");
            AddIconBindings("SubVariety", "sv", Color.FromRgb(76, 76, 76), "SV");
            AddIconBindings("SubSection", "sse", Color.FromRgb(74, 148, 0), "SSCT");
            AddIconBindings("SubSeries", "ssr", Color.FromRgb(0, 92, 57), "SSRS");
            AddIconBindings("SubTribe", "sst", Color.FromRgb(71, 126, 161), "SBT");
            // Special pseudo ranks...
            AddIconBindings("SpeciesInquirenda", TaxonRank.SPECIES_INQUIRENDA, Color.FromRgb(0, 128, 128), TaxonRank.SPECIES_INQUIRENDA);
            AddIconBindings("IncertaeSedis", TaxonRank.INCERTAE_SEDIS, Color.FromRgb(139, 197, 89), TaxonRank.INCERTAE_SEDIS);
        }

        private static void AddIconBindings(string iconName, string caption, Color color, params string[] elemTypes) {
            foreach (string elemType in elemTypes) {
                IconMetaData md = new IconMetaData(iconName, caption, color);
                _TaxaIconMetaData.Add(elemType, md);
            }
        }

        private ImageSource _image;
        private TaxonLabelGenerator _labelGenerator;        

        public TaxonViewModel(HierarchicalViewModelBase parent, Taxon taxon, TaxonLabelGenerator labelGenerator, bool isRoot = false)
            : base() {
            this.Parent = parent;
            this.Taxon = taxon;
            this.IsChanged = false;
            this.DataChanged += new DataChangedHandler(TaxonViewModel_DataChanged);
            _labelGenerator = labelGenerator;
            this.IsRootNode = isRoot;
            TaxonLabel = GenerateLabel();
        }

        private string GenerateLabel() {
            if (_labelGenerator == null) {
                return Epithet;
            } else {
                return _labelGenerator(this);
            }
        }

        public bool IsRootNode { get; private set; }

        public override FrameworkElement TooltipContent {
            get {
                return new TaxonTooltipContent(PluginManager.Instance.User, TaxaID.Value);
            }
        }

        public void RegenerateLabel() {
            // Force the icon to be reconstructed, possibly now with a changed badge/overlay
            _image = null;
            RaisePropertyChanged("Icon");
            // Regenerate the label
            TaxonLabel = GenerateLabel();
        }

        void TaxonViewModel_DataChanged(ChangeableModelBase model) {
            RegenerateLabel();
        }

        public Taxon Taxon { get; private set; }        

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
            set { 
                SetProperty(() => Taxon.Unverified, Taxon, value);
                RaisePropertyChanged("DisplayLabel");
            }
        }

        public bool? AvailableName {
            get { return Taxon.AvailableName; }
            set {
                if (SetProperty(() => Taxon.AvailableName, Taxon, value)) {
                    RaisePropertyChanged("IsAvailableOrLiteratureName");
                }
            }
        }

        public bool? LiteratureName {
            get { return Taxon.LiteratureName; }
            set {
                if (SetProperty(() => Taxon.LiteratureName, Taxon, value)) {
                    RaisePropertyChanged("IsAvailableOrLiteratureName");
                }
            }
        }

        public string NameStatus {
            get { return Taxon.NameStatus; }
            set { SetProperty(() => Taxon.NameStatus, Taxon, value); }
        }

        public override int NumChildren {
            get { return Taxon.NumChildren; }
            set { SetProperty(() => Taxon.NumChildren, Taxon, value); }
        }

        public string Parentage {
            get { return Taxon.Parentage; }            
        }

        public string DistQual {
            get { return Taxon.DistQual; }
            set { SetProperty(() => Taxon.DistQual, Taxon, value); }
        }

        public DateTime DateCreated {
            get { return Taxon.DateCreated; }
            set { SetProperty(() => Taxon.DateCreated, Taxon, value); }
        }

        public string WhoCreated {
            get { return Taxon.WhoCreated; }
            set { SetProperty(() => Taxon.WhoCreated, Taxon, value); }
        }

        public DateTime DateLastUpdated {
            get { return Taxon.DateLastUpdated; } 
            set { SetProperty( () => Taxon.DateLastUpdated, Taxon, value); }
        }

        public string WhoLastUpdated {
            get { return Taxon.WhoLastUpdated; }
            set { SetProperty(() => Taxon.WhoLastUpdated, Taxon, value); } 
        }

        private Pen AnimaliaBorder {
            get { return new Pen(new SolidColorBrush(Colors.Black), 2); }
        }

        private Pen PlantaeBorder {
            get { return new Pen(new SolidColorBrush(Color.FromRgb(184,71,19)), 2); }
        }

        public override String DisplayLabel {
            get {
                return GenerateLabel();
            }
        }

        public string TaxonLabel {
            get { return (string)GetValue(TaxonLabelProperty); }
            set { SetValue(TaxonLabelProperty, value); }
        }

        public static readonly DependencyProperty TaxonLabelProperty = DependencyProperty.Register("TaxonLabel", typeof(string), typeof(TaxonViewModel), new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.Journal | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(OnTaxonLabelChanged)));

        private static void OnTaxonLabelChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args) {
        }


        public void BulkAddChildren(List<Taxon> taxa, TaxonLabelGenerator labelGenerator) {

            if (Children.Count == 1 && Children[0] is ViewModelPlaceholder) {
                Children.Clear();
            }

            foreach (Taxon taxon in taxa) {                
                TaxonViewModel model = new TaxonViewModel(this, taxon, labelGenerator);
                Children.Add(model);
            }
            
        }

        private ImageSource ConstructIcon() {
            // The top level container nodes don'note get icons...
            if (IsRootNode) {
                return null;
            }

            return ConstructIcon(IsAvailableOrLiteratureName, ElemType, IsChanged);
        }

        public static ImageSource ConstructIcon(bool isAvailableOrLiteratureName, string elemType, bool isChanged) {


            // This is used to construct image uri's, if required...
            string assemblyName = typeof(TaxonViewModel).Assembly.GetName().Name;

            // Available and Literature names don'note have icons either
            if (isAvailableOrLiteratureName) {
                // Unless they've been changed, in which they get the 
                if (isChanged) {
                    return ImageCache.GetImage(String.Format("pack://application:,,,/{0};component/images/ChangedOverlay.png", assemblyName));
                } else {                    
                    return null;
                }
            }

            BitmapSource baseIcon = null;

            if (elemType == null) {
                return null;
            }

            if (_ElemTypeIconCache.ContainsKey(elemType)) {
                baseIcon = _ElemTypeIconCache[elemType];
            }

            if (baseIcon != null && !isChanged) {
                return baseIcon;
            }

            if (baseIcon == null) {
                RenderTargetBitmap bmp = new RenderTargetBitmap(_IconSize, _IconSize, 96, 96, PixelFormats.Pbgra32);
                DrawingVisual drawingVisual = new DrawingVisual();
                DrawingContext dc = drawingVisual.RenderOpen();

                IconMetaData md = null;
                if (_TaxaIconMetaData.ContainsKey(elemType)) {
                    md = _TaxaIconMetaData[elemType];
                }

                Color taxonColor = (md == null ? Colors.Red : md.Color);
                string caption = (md == null ? "?" : md.Caption);

                Pen pen = new Pen(new SolidColorBrush(taxonColor), 2);
                Brush textBrush = new SolidColorBrush(Color.FromArgb(200, 0, 0, 0));
                Brush fillBrush = new SolidColorBrush(Color.FromArgb(20, taxonColor.R, taxonColor.G, taxonColor.B));
                Typeface typeface = new Typeface(new FontFamily("Palatino Linotype,Times New Roman"), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);

                dc.DrawRoundedRectangle(fillBrush, pen, new Rect(1, 1, _IconSize - 2, _IconSize - 2), 4, 4);
                FormattedText t = new FormattedText(caption, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, typeface, 10, textBrush);
                double originX = (bmp.Width / 2) - (t.Width / 2);
                double originY = (bmp.Height / 2) - (t.Height / 2);
                dc.DrawText(t, new Point(originX, originY));
                dc.Close();
                bmp.Render(drawingVisual);

                if (elemType != null && !_ElemTypeIconCache.ContainsKey(elemType)) {
                    _ElemTypeIconCache.Add(elemType, bmp);
                }

                baseIcon = bmp;
            }

            
            if (isChanged) {                
                return ImageCache.ApplyOverlay(baseIcon, String.Format("pack://application:,,,/{0};component/images/ChangedOverlay.png", assemblyName));
            }

            return baseIcon;
        }

        public override ImageSource Icon {
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

        public override string GetParentage() {
            String parentage = "";
            TraverseToTop((t) => {
                if (t is TaxonViewModel) {
                    parentage = "/" + (t as TaxonViewModel).TaxaID + parentage;
                } else {
                    parentage = "/0" + parentage;
                }
            });
            return parentage;
        }

        public override string ToString() {
            return String.Format("TVM: [{0}-{2}] {1} <Order={3}>", ElemType, DisplayLabel, TaxaID, Order);
        }

        public bool IsAvailableOrLiteratureName {
            get {
                return AvailableName.GetValueOrDefault(false) || LiteratureName.GetValueOrDefault(false);
            }
        }

        public override int? ObjectID {
            get { return Taxon.TaxaID; }
        }

        public string DefaultSortOrder {
            get {
                string styleFactor = "2";

                if (AvailableName.ValueOrFalse()) {
                    styleFactor = "0";
                } else if (LiteratureName.ValueOrFalse()) {
                    styleFactor = "1";
                } else if (TaxonRank.INCERTAE_SEDIS.Equals(ElemType, StringComparison.CurrentCultureIgnoreCase)) {
                    styleFactor = "3";
                } else if (TaxonRank.SPECIES_INQUIRENDA.Equals(ElemType, StringComparison.CurrentCultureIgnoreCase)) {
                    styleFactor = "4";
                } else if (Unplaced.ValueOrFalse()) {
                    styleFactor = "5";
                }

                string strOrder = string.Format("{0:0000000}", Order.GetValueOrDefault(0));

                if (AvailableName.ValueOrFalse()) {
                    string strYearOfPub = YearOfPub;
                    if (string.IsNullOrWhiteSpace(YearOfPub) || YearOfPub.Length < 4) {
                        strYearOfPub = "0000";
                    }
                    return string.Format("{0}{1}{2}{3}", styleFactor, strYearOfPub, Epithet, Author);                        
                } else {
                    return string.Format("{0}{1}{2}", styleFactor, Epithet, Author);
                }
            }
        }


    }

    public delegate string TaxonLabelGenerator(TaxonViewModel taxon);

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
