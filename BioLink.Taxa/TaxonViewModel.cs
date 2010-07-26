using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Globalization;

using BioLink.Data.Model;
using BioLink.Client.Utilities;
using BioLink.Client.Extensibility;

namespace BioLink.Client.Taxa {
    
    public class TaxonViewModel : HierarchicalViewModelBase {

        private static Dictionary<string, string> _TaxaIconNames = new Dictionary<string, string>();

        static TaxonViewModel() {
            AddIconBindings("HigherOrder", "C", "CHRT", "D", "HO", "INC", "INO", "KING", "O", "P", "SBC", "SBD", "SKING", "SBO", "SBP", "SPC", "SPF", "SPO");
            AddIconBindings("Family", "F");
            AddIconBindings("SubFamily", "SF");
            AddIconBindings("Genus", "G");
            AddIconBindings("SubGenus", "SG");
            AddIconBindings("Species", "SP");
            AddIconBindings("SubSpecies", "SSP");
            AddIconBindings("Tribe", "T");
            AddIconBindings("Section", "SCT");
            AddIconBindings("Series", "SRS");
            AddIconBindings("SpeciesGroup", "SGP");
            AddIconBindings("SuperTribe", "ST");
            AddIconBindings("Form", "FM");
            AddIconBindings("SubForm", "SFM");
            AddIconBindings("Variety", "V");
            AddIconBindings("SubVariety", "SV");
            AddIconBindings("SubSection", "SSCT");
            AddIconBindings("SubSeries", "SSRS");
            AddIconBindings("SubTribe", "SBT");
        }

        private static void AddIconBindings(string iconName, params string[] elemTypes) {
            foreach (string elemType in elemTypes) {
                _TaxaIconNames.Add(elemType, iconName);
            }
        }

        public TaxonViewModel(TaxonViewModel parent, Taxon taxon) : base() {
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
            set { SetProperty(() => Taxon.TaxaParentID ,Taxon,  value); }
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

        private BitmapSource ConstructIcon() {
            BitmapSource newimage = null;

            // Available names don't have icons, nor do elements missing an element type
            if ((AvailableName.HasValue && AvailableName.Value) || ElemType == null) {
                return newimage;
            }

            string assemblyName = this.GetType().Assembly.GetName().Name;
            string uri = null;
            if (_TaxaIconNames.ContainsKey(this.ElemType)) {
                string iconName = _TaxaIconNames[this.ElemType];
                uri = String.Format("pack://application:,,,/{0};component/images/{1}.png", assemblyName, iconName);
                newimage = ImageCache.GetImage(uri);
            } else {
#if xDEBUG
                        RenderTargetBitmap bmp = new RenderTargetBitmap(50, 20, 96, 96, PixelFormats.Pbgra32);
                        FormattedText text = new FormattedText("[" + this.ElemType + "]", new CultureInfo("en-us"), FlowDirection.LeftToRight, new Typeface(new FontFamily("Tahoma"), FontStyles.Normal, FontWeights.Normal, new FontStretch()), 10, new SolidColorBrush(Colors.Black));
                        DrawingVisual drawingVisual = new DrawingVisual();
                        DrawingContext drawingContext = drawingVisual.RenderOpen();
                        BitmapSource icon = ImageCache.GetImage(String.Format("pack://application:,,,/{0};component/images/UnknownTaxa.png", assemblyName));
                        drawingContext.DrawImage(icon, new Rect(new Point(0, 0), new Point(20, 20)));
                        drawingContext.DrawText(text, new Point(20, 0));
                        drawingContext.Close();
                        bmp.Render(drawingVisual);
                        SetProperty("Icon", ref _image, bmp);                                                
#else
                newimage = ImageCache.GetImage(String.Format("pack://application:,,,/{0};component/images/UnknownTaxa.png", assemblyName));                
#endif
            }

            if (KingdomCode.Equals("P")) {
                newimage = ImageCache.ApplyOverlay(newimage, String.Format("pack://application:,,,/{0};component/images/PlantOverlay.png", assemblyName));
            }

            if (IsChanged) {
                newimage = ImageCache.ApplyOverlay(newimage, String.Format("pack://application:,,,/{0};component/images/ChangedOverlay.png", assemblyName));
            }
            return newimage;
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

}
