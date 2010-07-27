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
using System.Threading;
using System.Collections.ObjectModel;

namespace BioLink.Client.Gazetteer {
    /// <summary>
    /// Interaction logic for Gazetteer.xaml
    /// </summary>
    public partial class Gazetteer : UserControl, IDisposable {
        
        GazetteerService _service;
        private ObservableCollection<PlaceName> _searchModel = null;
        private GazetterPlugin _owner;
        private int _maximumSearchResults = 1000;        

        public Gazetteer() {
            InitializeComponent();
            _searchModel = new ObservableCollection<PlaceName>();
            lstResults.ItemsSource = _searchModel;
        }

        public Gazetteer(GazetterPlugin owner) {
            InitializeComponent();
            _searchModel = new ObservableCollection<PlaceName>();
            lstResults.ItemsSource = _searchModel;
            _owner = owner;
            if (_owner != null) {
                string lastFile = Preferences.GetUser(_owner.User, "gazetteer.lastFile", "");
                if (!String.IsNullOrEmpty(lastFile)) {
                    LoadFile(lastFile);
                }
            }
        }

        private void LoadFile(string filename) {
            try {
                _service = new GazetteerService(filename);
                lblFile.Content = _service.FileName;
                btnOpen.Content = _owner.GetCaption("Gazetteer.btnOpen.change");
                // now populate the Divisions combo box...
                List<CodeLabelPair> divisions = _service.GetDivisions();
                cmbDivision.ItemsSource = divisions;
                cmbDivision.SelectedIndex = 0;
            } catch (Exception ex) {
                ErrorMessage.Show(ex.ToString());
            }
        }

        private void btnOpen_Click(object sender, RoutedEventArgs e) {
            LoadFile("c:/zz/Auslig.sqlite");          
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

                        _searchModel.Clear();
                        foreach (PlaceName place in results) {
                            _searchModel.Add(place);
                        }
                    });
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
                Preferences.SetUser(_owner.User, "gazetteer.lastFile", _service.Filename);
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

    }
}
