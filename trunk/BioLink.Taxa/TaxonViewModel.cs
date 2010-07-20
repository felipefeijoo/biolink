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

    [Notify]
    public class TaxonViewModel : HierarchicalViewModelBase, ITaxon {

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
        }
       
        public Taxon Taxon { get; private set; }

        public override string Label {
            get { return TaxaFullName; }
        }

        public new bool IsChanged { get; set; }


        public int? TaxaID {
            get { return Taxon.TaxaID; }
            set { Taxon.TaxaID = value; }
        }

        public int? TaxaParentID {
            get { return Taxon.TaxaParentID; }
            set { Taxon.TaxaParentID = value; }
        }

        public string Epithet {
            get { return Taxon.Epithet; }
            set { Taxon.Epithet = value; }
        }

        public string TaxaFullName {
            get { return Taxon.TaxaFullName; }
            set { Taxon.TaxaFullName = value; }
        }

        public string YearOfPub {
            get { return Taxon.YearOfPub; }
            set { Taxon.YearOfPub = value; }
        }

        public string Author {
            get { return Taxon.Author; }
            set { Taxon.Author = value; }
        }

        public string ElemType {
            get { return Taxon.ElemType; }
            set { Taxon.ElemType = value; }
        }

        public string KingdomCode {
            get { return Taxon.KingdomCode; }
            set { Taxon.KingdomCode = value; }
        }

        public bool? Unplaced {
            get { return Taxon.Unplaced; }
            set { Taxon.Unplaced = value; }
        }

        public int? Order {
            get { return Taxon.Order; }
            set { Taxon.Order = value; }
        }

        public string Rank {
            get { return Taxon.Rank; }
            set { Taxon.Rank = value; }
        }

        public bool? ChgComb {
            get { return Taxon.ChgComb; }
            set { Taxon.ChgComb = value; }
        }

        public bool? Unverified {
            get { return Taxon.Unverified; }
            set { Taxon.Unverified = value; }
        }

        public bool? AvailableName {
            get { return Taxon.AvailableName; }
            set { Taxon.AvailableName = value; }
        }

        public bool? LiteratureName {
            get { return Taxon.LiteratureName; }
            set { Taxon.LiteratureName = value; }
        }

        public string NameStatus {
            get { return Taxon.NameStatus; }
            set { Taxon.NameStatus = value; }
        }

        public int? NumChildren {
            get { return Taxon.NumChildren; }
            set { Taxon.NumChildren = value; }
        }

        private BitmapSource _image;

        public override BitmapSource Icon {
            get {
                if (_image == null || IsChanged) {

                    // Available names don't have icons
                    if (AvailableName.HasValue && AvailableName.Value) {
                        return null;
                    }

                    string assemblyName = this.GetType().Assembly.GetName().Name;
                    string uri = null;
                    if (_TaxaIconNames.ContainsKey(this.ElemType)) {
                        string iconName = _TaxaIconNames[this.ElemType];
                        uri = String.Format("pack://application:,,,/{0};component/images/{1}.png", assemblyName, iconName);
                        _image = ImageCache.GetImage(uri);
                    } else {
#if DEBUG
                        RenderTargetBitmap bmp = new RenderTargetBitmap(50, 20, 96, 96, PixelFormats.Pbgra32);
                        FormattedText text = new FormattedText("[" + this.ElemType + "]", new CultureInfo("en-us"), FlowDirection.LeftToRight, new Typeface(new FontFamily("Tahoma"), FontStyles.Normal, FontWeights.Normal, new FontStretch()), 10, new SolidColorBrush(Colors.Black));
                        DrawingVisual drawingVisual = new DrawingVisual();
                        DrawingContext drawingContext = drawingVisual.RenderOpen();
                        BitmapSource icon = ImageCache.GetImage(String.Format("pack://application:,,,/{0};component/images/UnknownTaxa.png", assemblyName));
                        drawingContext.DrawImage(icon, new Rect(new Point(0, 0), new Point(20, 20)));
                        drawingContext.DrawText(text, new Point(20, 0));
                        drawingContext.Close();
                        bmp.Render(drawingVisual);
                        _image = bmp;
#else
                            _image = ImageCache.GetImage(String.Format("pack://application:,,,/{0};component/images/UnknownTaxa.png", assemblyName));
#endif
                    }

                    if (KingdomCode.Equals("P")) {
                        _image = ImageCache.ApplyOverlay(_image, String.Format("pack://application:,,,/{0};component/images/PlantOverlay.png", assemblyName));
                    }

                    if (IsChanged) {
                        _image = ImageCache.ApplyOverlay(_image, String.Format("pack://application:,,,/{0};component/images/ChangedOverlay.png", assemblyName));
                    }

                }

                return _image;
            }

            set {
                _image = value;
            }
        }
    }
}
