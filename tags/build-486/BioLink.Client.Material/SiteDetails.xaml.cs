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
using BioLink.Data;
using BioLink.Data.Model;
using BioLink.Client.Utilities;
using System.Collections.ObjectModel;

namespace BioLink.Client.Material {
    /// <summary>
    /// Interaction logic for SiteDetails.xaml
    /// </summary>
    public partial class SiteDetails : DatabaseCommandControl {

        private SiteViewModel _viewModel;

        #region Designer Constructor
        public SiteDetails() {
            InitializeComponent();
        }
        #endregion

        public SiteDetails(User user, int siteID, bool readOnly) : base(user, "Site:" + siteID) {
            InitializeComponent();
            this.SiteID = siteID;

            this.IsReadOnly = readOnly;
            var list = new List<Ellipsoid>(GeoUtils.ELLIPSOIDS);
            cmbDatum.ItemsSource = new ObservableCollection<string>(list.ConvertAll((ellipsoid) => { return ellipsoid.Name; }));

            if (GoogleEarth.IsInstalled()) {
                coordGrid.ColumnDefinitions[7].Width = new GridLength(23);
            } else {
                coordGrid.ColumnDefinitions[7].Width = new GridLength(0);
            }

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

            var service = new MaterialService(User);
            var model = service.GetSite(SiteID);
            _viewModel = new SiteViewModel(model);
            this.DataContext = _viewModel;

            tabSite.AddTabItem("Traits", new TraitControl(User, TraitCategoryType.Site, _viewModel) { IsReadOnly = readOnly });
            tabSite.AddTabItem("Notes", new NotesControl(User, TraitCategoryType.Site, _viewModel) { IsReadOnly = readOnly });
            tabSite.AddTabItem("Multimedia", new MultimediaControl(User, TraitCategoryType.Site, _viewModel) { IsReadOnly = readOnly });
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

            txtPoliticalRegion.BindUser(User, LookupType.Region);

            string llmode = Config.GetUser(User, "SiteDetails.LatLongFormat", LatLongMode.DegreesMinutesSeconds.ToString());
            if (!String.IsNullOrEmpty(llmode)) {
                LatLongMode mode = (LatLongMode)Enum.Parse(typeof(LatLongMode), llmode);
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

            this.Loaded += new RoutedEventHandler(SiteDetails_Loaded);
        }

        void SiteDetails_Loaded(object sender, RoutedEventArgs evt) {
            _viewModel.DataChanged += new DataChangedHandler(_viewModel_DataChanged);
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

            var updateLocality = OptionalQuestions.UpdateLocalityQuestion.Ask(this.FindParentWindow(), _viewModel.Locality, place);

            if (updateLocality) {
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
                btnGE2.Visibility = System.Windows.Visibility.Collapsed;
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
                btnGE2.Visibility = System.Windows.Visibility.Visible;
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
            RegisterUniquePendingChange(new UpdateSiteCommand(_viewModel.Model));
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
                    LatLongMode mode = (LatLongMode)Enum.Parse(typeof(LatLongMode), lltype);
                    SwitchLatLongFormat(mode);
                }
            }
        }

        private void SwitchLatLongFormat(LatLongMode latLongMode) {
            ctlX1.Mode = latLongMode;
            ctlX2.Mode = latLongMode;
            ctlY1.Mode = latLongMode;
            ctlY2.Mode = latLongMode;

            mnuDecimalDegrees.IsChecked = false;
            mnuDDM.IsChecked = false;
            mnuDMS.IsChecked = false;

            switch (latLongMode) {
                case LatLongMode.DecimalDegrees:
                    mnuDecimalDegrees.IsChecked = true;
                    break;
                case LatLongMode.DegreesDecimalMinutes:
                    mnuDDM.IsChecked = true;
                    break;
                case LatLongMode.DegreesMinutesSeconds:
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

        private void btnGE1_Click(object sender, RoutedEventArgs e) {
            GeoCode(ctlY1, ctlX1, true);
        }

        private void btnGE2_Click(object sender, RoutedEventArgs e) {
            GeoCode(ctlY2, ctlX2, false);
        }

        private void GeoCode(LatLongInput ctlLat, LatLongInput ctlLon, bool acceptElevation) {
            if (GoogleEarth.IsInstalled()) {
                GoogleEarth.GeoTag((lat, lon, alt) => {
                    ctlLat.Value = lat;
                    ctlLon.Value = lon;
                    txtPosSource.Text = "Google Earth";
                    if (alt.HasValue && acceptElevation) {
                        if (OptionalQuestions.UpdateElevationQuestion.Ask(this.FindParentWindow(), alt.Value)) {
                            optElevElevation.IsChecked = true;
                            txtElevUpper.Text = alt.Value + "";
                            txtElevUnits.Text = "m";
                            txtElevSource.Text = "Google Earth";
                        }
                    }
                }, ctlLat.Value, ctlLon.Value);
            }

        }

        private void imgMap_MouseDown(object sender, MouseButtonEventArgs e) {
            if (e.LeftButton == MouseButtonState.Pressed && e.ClickCount == 2) {
                ShowOnMap();
            }
        }

        private void ShowOnMap() {
            var map = PluginManager.Instance.GetMap();
            if (map != null) {
                map.Show();
                map.DropAnchor(ctlX1.Value, ctlY1.Value, txtLocality.Text);
            }
        }

        public static readonly DependencyProperty IsReadOnlyProperty = DependencyProperty.Register("IsReadOnly", typeof(bool), typeof(SiteDetails), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(OnIsReadOnlyChanged)));

        public bool IsReadOnly {
            get { return (bool)GetValue(IsReadOnlyProperty); }
            set { SetValue(IsReadOnlyProperty, value); }
        }

        private static void OnIsReadOnlyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args) {
            var control = (SiteDetails) obj;
            if (control != null) {
                var readOnly = (bool)args.NewValue;
                control.SetReadOnlyRecursive(readOnly);
            }
        }


    }

}
