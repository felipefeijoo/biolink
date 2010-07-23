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
using System.Threading;
using System.Collections.ObjectModel;

namespace BioLink.Client.Gazetteer {
    /// <summary>
    /// Interaction logic for Gazetteer.xaml
    /// </summary>
    public partial class Gazetteer : UserControl {

        private Timer _timer;
        GazetteerService _service;
        private ObservableCollection<PlaceName> _searchModel = null;

        public Gazetteer() {
            InitializeComponent();
            _timer = new Timer(new TimerCallback((obj) => {
                DoFind();
            }), null, Timeout.Infinite, Timeout.Infinite);
            _searchModel = new ObservableCollection<PlaceName>();
            lstResults.ItemsSource = _searchModel;
        }

        private void btnOpen_Click(object sender, RoutedEventArgs e) {
            _service = new GazetteerService("c:/zz/Auslig.sqlite");
            lblFile.Content = _service.FileName;
        }

        private void DoFind() {

            try {
                lstResults.InvokeIfRequired(() => {
                    lstResults.Cursor = Cursors.Wait;
                });
                if (_service == null) {
                    MessageBox.Show("Select a gazetteer first!");
                    _timer.Change(Timeout.Infinite, Timeout.Infinite);
                    return;
                }

                _timer.Change(Timeout.Infinite, Timeout.Infinite);
                string searchTerm = null;
                txtFind.InvokeIfRequired(() => { searchTerm = txtFind.Text; });
                if (!String.IsNullOrEmpty(searchTerm)) {
                    List<PlaceName> results = _service.FindPlaceNames(searchTerm);
                    lstResults.InvokeIfRequired(() => {
                        _searchModel.Clear();
                        foreach (PlaceName place in results) {
                            _searchModel.Add(place);
                        }
                    });
                }
            } finally {
                lstResults.InvokeIfRequired(() => {
                    lstResults.Cursor = Cursors.Arrow;
                });
            }
        }

        private void txtFind_TextChanged(object sender, TextChangedEventArgs e) {
            if (String.IsNullOrEmpty(txtFind.Text)) {
                if (_searchModel != null) {
                    _timer.Change(Timeout.Infinite, Timeout.Infinite);
                    _searchModel.Clear();                    
                }
            } else {
                _timer.Change(Timeout.Infinite, Timeout.Infinite);
                _timer.Change(300, 300);
                if (_searchModel != null) {
                    _searchModel.Clear();
                }
            }

        }

    }
}
