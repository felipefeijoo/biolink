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
using System.Collections.ObjectModel;
using BioLink.Data;
using BioLink.Data.Model;
using BioLink.Client.Utilities;
using BioLink.Client.Extensibility;
using System.Threading;

namespace BioLink.Client.Taxa {

    
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class TaxonExplorer : UserControl {


        private Timer _timer;
        private IBioLinkPlugin _owner;

        private ObservableCollection<TaxonViewModel> _searchModel;

        public TaxonExplorer() {
            InitializeComponent();
        }

        

        public TaxonExplorer(IBioLinkPlugin owner) {
            InitializeComponent();
            _owner = owner;
            
            lstResults.Margin = treeView.Margin;
            lstResults.Visibility = Visibility.Hidden;
            _searchModel = new ObservableCollection<TaxonViewModel>();
            lstResults.ItemsSource = _searchModel;
            _timer = new Timer(new TimerCallback((obj) => {
                DoFind();
            }),null, Timeout.Infinite, Timeout.Infinite);

        }

        internal void SetModel(ObservableCollection<TaxonViewModel> model) {
            treeView.Items.Clear();            
            this.treeView.ItemsSource = model;
        }

        private void txtFind_TextChanged(object sender, TextChangedEventArgs e) {

            if (String.IsNullOrEmpty(txtFind.Text)) {                                
                treeView.Visibility = System.Windows.Visibility.Visible;
                lstResults.Visibility = Visibility.Hidden;
                _searchModel.Clear();
            } else {
                _timer.Change(Timeout.Infinite, Timeout.Infinite);
                _timer.Change(300, 300);
                _searchModel.Clear();
                treeView.Visibility = Visibility.Hidden;
                lstResults.Visibility = Visibility.Visible;
                
            }
        }

        private void DoFind() {
            if (_owner == null) {
                return;
            }
            _timer.Change(Timeout.Infinite, Timeout.Infinite);
            string searchTerm = null;
            txtFind.InvokeIfRequired(() => { searchTerm = txtFind.Text; });
            Logger.Debug("Searching for taxon matching {0}", searchTerm);
            List<Taxon> results = new TaxaService(_owner.User).FindTaxa(searchTerm);
            lstResults.InvokeIfRequired(() => {
                _searchModel.Clear();
                foreach (Taxon t in results) {
                    _searchModel.Add(new TaxonViewModel(null, t));
                }
            });
        }


    }
}
