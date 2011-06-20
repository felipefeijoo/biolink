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
using BioLink.Data;
using BioLink.Data.Model;
using BioLink.Client.Utilities;
using System.Collections.ObjectModel;

namespace BioLink.Client.Material {
    /// <summary>
    /// Interaction logic for SiteDetails.xaml
    /// </summary>
    public partial class SiteDetails : DatabaseActionControl {

        private SiteViewModel _viewModel;

        #region Designer Constructor
        public SiteDetails() {
            InitializeComponent();
        }
        #endregion

        public SiteDetails(User user, int siteID)
            : base(user, "Site:" + siteID) {
            InitializeComponent();

            var list = new List<Ellipsoid>(GeoUtils.ELLIPSOIDS);
            cmbDatum.ItemsSource = new ObservableCollection<string>(list.ConvertAll((ellipsoid) => { return ellipsoid.Name; }));

            this.SiteID = siteID;

            // Radio button checked event handlers
            optNearestPlace.Checked += new RoutedEventHandler((s, e) => {
                txtLocality.IsEnabled = false;
                txtDirectionFrom.IsEnabled = true;
                txtDistanceFrom.IsEnabled = true;
                txtFrom.IsEnabled = true;
            });

            optLocality.Checked += new RoutedEventHandler((s, e) => {
                txtLocality.IsEnabled = true;
                txtDirectionFrom.IsEnabled = false;
                txtDistanceFrom.IsEnabled = false;
                txtFrom.IsEnabled = false;
            });

            var service = new MaterialService(user);
            var model = service.GetSite(siteID);
            _viewModel = new SiteViewModel(model);
            this.DataContext = _viewModel;

            tabSite.AddTabItem("Traits", new TraitControl(user, TraitCategoryType.Site, _viewModel));
            tabSite.AddTabItem("Notes", new NotesControl(user, TraitCategoryType.Site, _viewModel));
            tabSite.AddTabItem("Multimedia", new MultimediaControl(user, TraitCategoryType.Site, _viewModel));
            tabSite.AddTabItem("Ownership", new OwnershipDetails(_viewModel.Model));

            txtPosSource.BindUser(User, PickListType.Phrase, "Source", TraitCategoryType.Site);
            txtPosWho.BindUser(User, "tblSite", "vchrPosWho");
            txtPosOriginal.BindUser(User, PickListType.Phrase, "OriginalDetermination", TraitCategoryType.Site);

            txtElevUnits.BindUser(User, PickListType.Phrase, "Units", TraitCategoryType.Site);
            txtElevSource.BindUser(User, PickListType.Phrase, "Source", TraitCategoryType.Site);

            txtGeoEra.BindUser(User, PickListType.Phrase, "Geological Era", TraitCategoryType.Site);
            txtGeoPlate.BindUser(User, PickListType.Phrase, "Geological Plate", TraitCategoryType.Site);
            txtGeoStage.BindUser(User, PickListType.Phrase, "Geological State", TraitCategoryType.Site);

            txtGeoFormation.BindUser(User, PickListType.Phrase, "Geological Formation", TraitCategoryType.Site);
            txtGeoMember.BindUser(User, PickListType.Phrase, "Geological Member", TraitCategoryType.Site);
            txtGeoBed.BindUser(User, PickListType.Phrase, "Geological Bed", TraitCategoryType.Site);

            this.ctlX1.CoordinateValueChanged += new CoordinateValueChangedHandler((s, v) => { UpdateMiniMap(ctlY1.Value, ctlX1.Value); });
            this.ctlY1.CoordinateValueChanged += new CoordinateValueChangedHandler((s, v) => { UpdateMiniMap(ctlY1.Value, ctlX1.Value); });

            this.ctlX2.Visibility = System.Windows.Visibility.Hidden;
            this.ctlY2.Visibility = System.Windows.Visibility.Hidden;

            _viewModel.DataChanged += new DataChangedHandler(_viewModel_DataChanged);

            txtPoliticalRegion.BindUser(user, LookupType.Region);

            string llmode = Config.GetUser(User, "SiteDetails.LatLongFormat", LatLongInput.LatLongMode.DegreesMinutesSeconds.ToString());
            if (!String.IsNullOrEmpty(llmode)) {
                LatLongInput.LatLongMode mode = (LatLongInput.LatLongMode)Enum.Parse(typeof(LatLongInput.LatLongMode), llmode);
                SwitchLatLongFormat(mode);
            }

            optPoint.Checked += new RoutedEventHandler((s, e) => { UpdateGeomType(); });
            optLine.Checked += new RoutedEventHandler((s, e) => { UpdateGeomType(); });
            optBoundingBox.Checked += new RoutedEventHandler((s, e) => { UpdateGeomType(); });

            optCoordsNotSpecified.Checked += new RoutedEventHandler((s, e) => { UpdateGeomType(); });
            optEastingsNorthings.Checked += new RoutedEventHandler((s, e) => { UpdateGeomType(); });
            optLatLong.Checked += new RoutedEventHandler((s, e) => { UpdateGeomType(); });

            optElevDepth.Checked += new RoutedEventHandler(UpdateElevation);
            optElevElevation.Checked += new RoutedEventHandler(UpdateElevation);
            optElevNotSpecified.Checked += new RoutedEventHandler(UpdateElevation);

            if (model.PosY1.HasValue && model.PosX1.HasValue) {
                UpdateMiniMap(model.PosY1.Value, model.PosX1.Value);
            }

            this.PreviewDragOver += new DragEventHandler(site_PreviewDragEnter);
            this.PreviewDragEnter += new DragEventHandler(site_PreviewDragEnter);

            //this.Drop += new DragEventHandler(site_Drop);

            HookLatLongControl(ctlX1);
            HookLatLongControl(ctlY1);
            HookLatLongControl(ctlX2);
            HookLatLongControl(ctlY2);

        }

        private void HookLatLongControl(LatLongInput ctl) {
            HookTextBox(ctl.txtDegrees);
            HookTextBox(ctl.txtMinutes);
            HookTextBox(ctl.txtSeconds);
        }

        private void HookTextBox(System.Windows.Controls.TextBox box) {
            box.AllowDrop = true;

            box.PreviewDragEnter += new DragEventHandler(PositionControl_PreviewDragEnter);
            box.PreviewDragOver += new DragEventHandler(PositionControl_PreviewDragEnter);
            box.PreviewDrop += new DragEventHandler(site_Drop);
        }

        void PositionControl_PreviewDragEnter(object sender, DragEventArgs e) {
            var pinnable = e.Data.GetData(PinnableObject.DRAG_FORMAT_NAME) as PinnableObject;
            if (pinnable != null) {
                if (pinnable.LookupType == LookupType.PlaceName) {
                    e.Effects = DragDropEffects.Link;
                }
            } else {
                e.Effects = DragDropEffects.None;
            }
            e.Handled = true;
        }

        void site_PreviewDragEnter(object sender, DragEventArgs e) {
            e.Effects = DragDropEffects.None;
        }

        void site_Drop(object sender, DragEventArgs e) {

            var latLongCtl = (sender as Control).FindParent<LatLongInput>();
            bool setX1Y1 = true;
            if (latLongCtl != null) {
                if (latLongCtl.Name == ctlX2.Name || latLongCtl.Name == ctlY2.Name) {
                    setX1Y1 = false;
                }
            }

            var pinnable = e.Data.GetData(PinnableObject.DRAG_FORMAT_NAME) as PinnableObject;
            if (pinnable != null && pinnable.LookupType == LookupType.PlaceName) {
                PlaceName placeName = pinnable.GetState<PlaceName>();
                if (!setX1Y1) {
                    ctlY2.Value = placeName.Latitude;
                    ctlX2.Value = placeName.Longitude;
                } else {
                    ctlY1.Value = placeName.Latitude;
                    ctlX1.Value = placeName.Longitude;

                    UpdateLocality(placeName);             
                }

            }
            e.Handled = true;
        }

        private void UpdateLocality(PlaceName placeName) {
            string place = placeName.Name;
            if (placeName.PlaceNameType == PlaceNameType.OffsetAndDirection) {
                place = string.Format("{0} {1} {2} of {3}", placeName.Offset, placeName.Units, placeName.Direction, placeName.Name);
            }
            if (this.Question(string.Format("Do you wish to update the locality from '{0}' to '{1}'?", _viewModel.Locality, place), "Update locality?")) {
                if (placeName.PlaceNameType == PlaceNameType.OffsetAndDirection) {
                    optNearestPlace.IsChecked = true;
                    _viewModel.Locality = placeName.Name;
                    txtDistanceFrom.Text = string.Format("{0} {1}", placeName.Offset, placeName.Units);
                    txtDirectionFrom.Text = placeName.Direction;
                } else {
                    optLocality.IsChecked = true;
                }
                _viewModel.Locality = placeName.Name;
            }
        }

        void UpdateElevation(object sender, RoutedEventArgs e) {
            lblDepth.IsEnabled = false;
            txtElevDepth.IsEnabled = false;
            if (optElevNotSpecified.IsChecked.ValueOrFalse()) {
                txtElevUpper.Text = null;
                txtElevLower.Text = null;
                txtElevDepth.Text = null;
                txtElevError.Text = null;
            }

            if (optElevDepth.IsChecked.ValueOrFalse()) {
                lblDepth.IsEnabled = true;
                txtElevDepth.IsEnabled = true;
            }

        }

        private void UpdateGeomType() {
            if (_viewModel.PosAreaType == 1) {
                lblStart.Content = "Coordinates:";
                lblEnd.Visibility = System.Windows.Visibility.Hidden;
                ctlX2.Visibility = System.Windows.Visibility.Hidden;
                ctlY2.Visibility = System.Windows.Visibility.Hidden;
                ctlEastingsNorthings2.Visibility = System.Windows.Visibility.Collapsed;
                btnEgaz2.Visibility = System.Windows.Visibility.Collapsed;
                if (_viewModel.PosCoordinates == 2) {
                    ctlX1.Visibility = System.Windows.Visibility.Collapsed;
                    ctlY1.Visibility = System.Windows.Visibility.Collapsed;
                    ctlEastingsNorthings1.Visibility = System.Windows.Visibility.Visible;
                } else {
                    ctlX1.Visibility = System.Windows.Visibility.Visible;
                    ctlY1.Visibility = System.Windows.Visibility.Visible;
                    ctlEastingsNorthings1.Visibility = System.Windows.Visibility.Collapsed;
                }
            } else {
                lblStart.Content = "Start:";
                lblEnd.Visibility = System.Windows.Visibility.Visible;
                btnEgaz2.Visibility = System.Windows.Visibility.Visible;
                if (_viewModel.PosCoordinates == 2) {
                    ctlX1.Visibility = System.Windows.Visibility.Collapsed;
                    ctlY1.Visibility = System.Windows.Visibility.Collapsed;
                    ctlX2.Visibility = System.Windows.Visibility.Collapsed;
                    ctlY2.Visibility = System.Windows.Visibility.Collapsed;
                    ctlEastingsNorthings1.Visibility = System.Windows.Visibility.Visible;
                    ctlEastingsNorthings2.Visibility = System.Windows.Visibility.Visible;
                } else {
                    ctlX1.Visibility = System.Windows.Visibility.Visible;
                    ctlY1.Visibility = System.Windows.Visibility.Visible;
                    ctlX2.Visibility = System.Windows.Visibility.Visible;
                    ctlY2.Visibility = System.Windows.Visibility.Visible;
                    ctlEastingsNorthings1.Visibility = System.Windows.Visibility.Collapsed;
                    ctlEastingsNorthings2.Visibility = System.Windows.Visibility.Collapsed;
                }

            }

        }

        private void UpdateMiniMap(double latitude, double longitude) {
            UpdateMiniMap(latitude, longitude, imgMap);
            UpdateMiniMap(latitude, longitude, imgMiniMap2);
        }

        private void UpdateMiniMap(double latitude, double longitude, Image imgControl) {

            if (imgControl.Width == 0 || imgControl.Height == 0) {
                return;
            }

            double meridian = imgControl.Width / 2.0;
            double equator = imgControl.Height / 2.0;
            double x = meridian + ((longitude / 180) * meridian);
            double y = equator - ((latitude / 90) * equator);

            string assemblyName = this.GetType().Assembly.GetName().Name;
            var img = ImageCache.GetPackedImage(@"images\World.png", assemblyName);

            RenderTargetBitmap bmp = new RenderTargetBitmap((int)img.Width, (int)img.Height, 96, 96, PixelFormats.Pbgra32);
            DrawingVisual drawingVisual = new DrawingVisual();
            DrawingContext dc = drawingVisual.RenderOpen();

            dc.DrawImage(img, new Rect(0, 0, (int)imgControl.Width, (int)imgControl.Height));
            var brush = new SolidColorBrush(Colors.Red);
            var pen = new Pen(brush, 1);

            dc.DrawEllipse(brush, pen, new Point(x, y), 2, 2);

            dc.Close();
            bmp.Render(drawingVisual);

            imgControl.Source = bmp;
        }

        void _viewModel_DataChanged(ChangeableModelBase viewmodel) {
            RegisterUniquePendingChange(new UpdateSiteAction(_viewModel.Model));
        }

        private void mnuDecimalDegrees_Click(object sender, RoutedEventArgs e) {
            SwitchLatLongFormat(sender);
        }

        private void mnuDMS_Click(object sender, RoutedEventArgs e) {
            SwitchLatLongFormat(sender);
        }

        private void mnuDDM_Click(object sender, RoutedEventArgs e) {
            SwitchLatLongFormat(sender);
        }

        private void SwitchLatLongFormat(object sender) {
            var mnu = sender as MenuItem;
            if (mnu != null) {
                var lltype = mnu.Tag as string;
                if (lltype != null) {
                    LatLongInput.LatLongMode mode = (LatLongInput.LatLongMode)Enum.Parse(typeof(LatLongInput.LatLongMode), lltype);
                    SwitchLatLongFormat(mode);
                }
            }
        }

        private void SwitchLatLongFormat(LatLongInput.LatLongMode latLongMode) {
            ctlX1.Mode = latLongMode;
            ctlX2.Mode = latLongMode;
            ctlY1.Mode = latLongMode;
            ctlY2.Mode = latLongMode;

            mnuDecimalDegrees.IsChecked = false;
            mnuDDM.IsChecked = false;
            mnuDMS.IsChecked = false;

            switch (latLongMode) {
                case LatLongInput.LatLongMode.DecimalDegrees:
                    mnuDecimalDegrees.IsChecked = true;
                    break;
                case LatLongInput.LatLongMode.DegreesDecimalMinutes:
                    mnuDDM.IsChecked = true;
                    break;
                case LatLongInput.LatLongMode.DegreesMinutesSeconds:
                    mnuDMS.IsChecked = true;
                    break;
            }

            Config.SetUser(User, "SiteDetails.LatLongFormat", latLongMode.ToString());
        }


        #region Properties

        public int SiteID { get; private set; }

        #endregion

        private void btnEgaz1_Click(object sender, RoutedEventArgs e) {
            LaunchGazetteer(ctlY1, ctlX1, true);
        }

        private void btnEgaz2_Click(object sender, RoutedEventArgs e) {
            LaunchGazetteer(ctlY2, ctlX2, false);
        }

        private void LaunchGazetteer(LatLongInput lat, LatLongInput lon, bool updateLocality) {
            PluginManager.Instance.StartSelect<PlaceName>((result) => {
                var place = result.DataObject as PlaceName;
                if (place != null) {
                    lat.Value = place.Latitude;
                    lon.Value = place.Longitude;
                    txtPosSource.Text = "EGaz";
                    if (updateLocality) {
                        UpdateLocality(place);
                    }
                }
            });
        }

    }

}
