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
using BioLink.Client.Utilities;
using BioLink.Client.Extensibility;
using BioLink.Data;
using BioLink.Data.Model;
using System.Threading;
using System.Collections.ObjectModel;
using Microsoft.Win32;

namespace BioLink.Client.Gazetteer {
    /// <summary>
    /// Interaction logic for Gazetteer.xaml
    /// </summary>
    public partial class Gazetteer : UserControl, IDisposable {
        
        GazetteerService _service;
        private ObservableCollection<PlaceNameViewModel> _searchModel = null;
        private GazetterPlugin _owner;
        private int _maximumSearchResults = 1000;

        private Point _startPoint;
        private bool _IsDragging;

        private OffsetControl _offsetControl;
        private DistanceDirectionControl _dirDistControl;
        private Action<SelectionResult> _selectionCallback;

        #region Designer CTOR
        public Gazetteer() {
            InitializeComponent();
            if (!this.IsDesignTime()) {
                throw new Exception("Wrong constructor!");
            }
        }
        #endregion

        public Gazetteer(GazetterPlugin owner) {
            InitializeComponent();
            _searchModel = new ObservableCollection<PlaceNameViewModel>();
            lstResults.ItemsSource = _searchModel;
            _owner = owner;
            btnDataInfo.IsEnabled = false;
            if (_owner != null) {
                string lastFile = Config.GetUser(_owner.User, "gazetteer.lastFile", "");
                if (!String.IsNullOrEmpty(lastFile)) {
                    LoadFile(lastFile);
                }
            }

            _offsetControl = new OffsetControl();
            _offsetControl.SelectedPlaceNameChanged += new Action<PlaceName>((place) => {
                UpdateMap();
            });

            _dirDistControl = new DistanceDirectionControl();

            lstResults.SelectionChanged += new SelectionChangedEventHandler(lstResults_SelectionChanged);

            lstResults.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(lvw_PreviewMouseLeftButtonDown);
            lstResults.PreviewMouseMove += new MouseEventHandler(lvw_PreviewMouseMove);

            optFindDistDir.Checked += new RoutedEventHandler(optFindDistDir_Checked);
            optFindLatLong.Checked += new RoutedEventHandler(optFindLatLong_Checked);

            optFindLatLong.IsChecked = true;

        }

        void lstResults_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            var place = lstResults.SelectedItem as PlaceNameViewModel;
            _offsetControl.DataContext = place;
            _dirDistControl.DataContext = place;
            UpdateMap();
        }

        void optFindLatLong_Checked(object sender, RoutedEventArgs e) {
            CalcOptionChanged();
        }

        void optFindDistDir_Checked(object sender, RoutedEventArgs e) {
            CalcOptionChanged();
        }

        private void CalcOptionChanged() {
            grpCalc.Content = null;
            if (optFindLatLong.IsChecked.GetValueOrDefault(false)) {
                grpCalc.Content = _offsetControl;
                grpCalc.Header = "Find Lat./Long. using Dist./Dir.";
            } else {
                grpCalc.Content = _dirDistControl;
                grpCalc.Header = "Find Dist./Dir. using Lat./Long.";
            }

        }

        void lvw_PreviewMouseMove(object sender, MouseEventArgs e) {
            CommonPreviewMouseMove(e, lstResults);
        }

        void lvw_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            _startPoint = e.GetPosition(lstResults);
        }

        private void CommonPreviewMouseMove(MouseEventArgs e, ListBox listView) {

            if (_startPoint == null) {
                return;
            }

            if (e.LeftButton == MouseButtonState.Pressed && !_IsDragging) {
                Point position = e.GetPosition(listView);
                if (Math.Abs(position.X - _startPoint.X) > SystemParameters.MinimumHorizontalDragDistance || Math.Abs(position.Y - _startPoint.Y) > SystemParameters.MinimumVerticalDragDistance) {
                    if (listView.SelectedItem != null) {

                        ListBoxItem item = listView.ItemContainerGenerator.ContainerFromItem(listView.SelectedItem) as ListBoxItem;
                        if (item != null) {
                            StartDrag(e, listView, item);
                        }
                    }
                }
            }
        }

        private void StartDrag(MouseEventArgs mouseEventArgs, ListBox listbox, ListBoxItem item) {

            var selected = listbox.SelectedItem as PlaceNameViewModel;
            if (selected != null) {
                var data = new DataObject("Pinnable", selected);

                var pinnable = new PinnableObject(GazetterPlugin.GAZETTEER_PLUGIN_NAME, LookupType.PlaceName, 0, selected.Model);
                data.SetData(PinnableObject.DRAG_FORMAT_NAME, pinnable);
                data.SetData(DataFormats.Text, selected.DisplayLabel);

                try {
                    _IsDragging = true;
                    DragDrop.DoDragDrop(item, data, DragDropEffects.Copy | DragDropEffects.Move | DragDropEffects.Link);
                } finally {
                    _IsDragging = false;
                }
            }

            InvalidateVisual();
        }


        private void LoadFile(string filename) {
            try {
                _service = new GazetteerService(filename);
                lblFile.Content = _service.FileName;
                btnOpen.Content = _owner.GetCaption("Gazetteer.btnOpen.change");
                btnDataInfo.IsEnabled = true;
                // now populate the Divisions combo box...
                var divisions = _service.GetDivisions();
                cmbDivision.ItemsSource = divisions;
                cmbDivision.SelectedIndex = 0;
            } catch (Exception ex) {
                ErrorMessage.Show(ex.ToString());
            }
        }

        private void btnOpen_Click(object sender, RoutedEventArgs e) {
            ChooseFile();            
        }

        private void ChooseFile() {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Title = _owner.GetCaption("Gazetteer.FileOpen.Title");
            dlg.Filter = "Gazetteer files (*.gaz)|*.gaz|All files (*.*)|*.*";
            bool? result = dlg.ShowDialog();
            if (result.GetValueOrDefault(false)) {
                LoadFile(dlg.FileName);
            }
        }


        private void delayedTriggerTextbox1_TypingPaused(string text) {
            DoSearch(text);
        }

        private void DoSearch(string text) {
            try {

                if (_service == null) {
                    return;
                }

                lstResults.InvokeIfRequired(() => {
                    lstResults.Cursor = Cursors.Wait;
                    lblResults.Content = _owner.GetCaption("Gazetteer.Search.Searching");
                });


                if (!String.IsNullOrEmpty(text)) {
                    List<PlaceName> results = null;

                    bool limit = false;
                    chkLimit.InvokeIfRequired(() => {
                        limit = chkLimit.IsChecked.HasValue && chkLimit.IsChecked.Value;
                    });

                    string division = "";
                    if (limit) {
                        cmbDivision.InvokeIfRequired(() => {
                            CodeLabelPair selected = cmbDivision.SelectedItem as CodeLabelPair;
                            if (selected != null) {
                                division = selected.Code;
                            }
                        });
                    }

                    if (limit && (!String.IsNullOrEmpty(division))) {
                        results = _service.FindPlaceNamesLimited(text, division, _maximumSearchResults + 1);
                    } else {
                        results = _service.FindPlaceNames(text, _maximumSearchResults + 1);
                    }

                    lstResults.InvokeIfRequired(() => {
                        if (results.Count > _maximumSearchResults) {
                            lblResults.Content = _owner.GetCaption("Gazetteer.Search.Results.TooMany", _maximumSearchResults);
                        } else {
                            lblResults.Content = _owner.GetCaption("Gazetteer.Search.Results", results.Count);
                        }

                        _offsetControl.Clear();
                        _dirDistControl.Clear();
                        _searchModel.Clear();                        
                        foreach (PlaceName place in results) {
                            _searchModel.Add(new PlaceNameViewModel(place));
                        }
                    });
                } else {
                    lblResults.Content = "";
                }
            } catch (Exception ex) {
                GlobalExceptionHandler.Handle(ex);
            } finally {
                lstResults.InvokeIfRequired(() => {
                    lstResults.Cursor = Cursors.Arrow;
                });
            }

        }

        private void delayedTriggerTextbox1_TextChanged(object sender, TextChangedEventArgs e) {
            if (_searchModel != null) {
                _searchModel.Clear();
                lblResults.Content = "";
            }
        }

        private void delayedTriggerTextbox1_KeyUp(object sender, KeyEventArgs e) {
            if (e.Key == Key.Down) {
                lstResults.SelectedIndex = 0;
                if (lstResults.SelectedItem != null) {
                    ListBoxItem item = lstResults.ItemContainerGenerator.ContainerFromItem(lstResults.SelectedItem) as ListBoxItem;
                    item.Focus();
                }
            }
        }


        public void Dispose() {
            if (_service != null) {
                Config.SetUser(_owner.User, "gazetteer.lastFile", _service.Filename);
                _service.Dispose();
            }
        }

        private void chkLimit_Checked(object sender, RoutedEventArgs e) {
            cmbDivision.IsEnabled = true;
            DoSearch(txtFind.Text);
        }

        private void chkLimit_Unchecked(object sender, RoutedEventArgs e) {
            cmbDivision.IsEnabled = false;
            DoSearch(txtFind.Text);
        }

        private void cmbDivision_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            DoSearch(txtFind.Text);
        }

        private void button1_Click(object sender, RoutedEventArgs e) {
            _offsetControl.Clear();
        }

        private void button2_Click(object sender, RoutedEventArgs e) {
            ShowMap();
        }

        private IMapProvider _map;

        public void ShowMap() {
            var providers = PluginManager.Instance.GetExtensionsOfType<IMapProvider>();
            if (providers != null && providers.Count > 0) {
                _map = providers[0];
                if (_map != null) {
                    _map.Show();
                    UpdateMap();
                }
            }
        }

        private void UpdateMap() {
            var selected = lstResults.SelectedItem as PlaceNameViewModel;
            if (_map != null) {
                _map.HideAnchor();
                _map.ClearPoints();

                if (selected != null) {
                    _map.DropAnchor(selected.Longitude, selected.Latitude, selected.Name);
                    if (_offsetControl.IsVisible && _offsetControl.OffsetPlace != null) {
                        var offset = _offsetControl.OffsetPlace;

                        MapPoint p = new MapPoint();
                        p.Latitude = offset.Latitude;
                        p.Longitude = offset.Longitude;
                        p.Label = string.Format("{0} {1} {2} of {3}", offset.Offset, offset.Units, offset.Direction, offset.Name);
                        var set = new ListMapPointSet("Offset");
                        set.DrawLabels = true;
                        set.Add(p);

                        _map.PlotPoints(set);

                    }

                } 
            }
        }

        public void BindSelectCallback(Action<SelectionResult> selectionFunc) {
            if (selectionFunc != null) {
                btnSelect.Visibility = Visibility.Visible;
                btnSelect.IsEnabled = true;
                _selectionCallback = selectionFunc;
            } else {
                ClearSelectCallback();
            }
        }

        public void ClearSelectCallback() {
            _selectionCallback = null;
            btnSelect.Visibility = Visibility.Hidden;
        }

        private void btnSelect_Click(object sender, RoutedEventArgs e) {
            DoSelect();
        }

        private void DoSelect() {

            PlaceName result = null;
            if (optFindLatLong.IsChecked.GetValueOrDefault(false)) {
                result = _offsetControl.OffsetPlace;
            }

            if (result == null) {
                var selected = lstResults.SelectedItem as PlaceNameViewModel;
                if (selected != null) {
                    result = selected.Model;
                }
            }

            if (result != null && _selectionCallback != null) {
                var selResult = new SelectionResult {
                    ObjectID = null,
                    DataObject = result,
                    Description = result.Name
                };

                _selectionCallback(selResult);
            }

        }

        private void Image_ImageFailed(object sender, ExceptionRoutedEventArgs e) {
            ShowGazetteerInfo();
        }

        private void ShowGazetteerInfo() {
            if (_service != null) {
                var info = _service.GetGazetteerInfo();
                if (info != null) {
                    var frm = new GazetteerInfoForm(info);
                    frm.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                    frm.Owner = PluginManager.Instance.ParentWindow;
                    frm.ShowDialog();
                }
            }
        }

        private void btnDataInfo_Click(object sender, RoutedEventArgs e) {
            ShowGazetteerInfo();
        }

    }
    
}
