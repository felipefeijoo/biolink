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
using BioLink.Client.Utilities;
using BioLink.Data;
using BioLink.Data.Model;
using System.Collections.ObjectModel;

namespace BioLink.Client.Taxa {
    /// <summary>
    /// Interaction logic for ChecklistReportOptions.xaml
    /// </summary>
    public partial class ChecklistReportOptions : Window {

        private ObservableCollection<SelectableRankName> _rankModel;

        public ChecklistReportOptions(User user, TaxonViewModel taxon) {
            InitializeComponent();
            this.User = user;
            txtTaxon.BindUser(user, LookupType.Taxon);
            txtTaxon.ObjectID = taxon.TaxaID.Value;
            txtTaxon.Text = taxon.TaxaFullName;

            var ranknames = Service.GetOrderedRanks();
            _rankModel = new ObservableCollection<SelectableRankName>( ranknames.Select((n) => {
                return new SelectableRankName(n, true);
            }));

            lstRanks.ItemsSource = _rankModel;

        }

        private void btnCancel_Click(object sender, RoutedEventArgs e) {
            this.DialogResult = false;
            this.Close();
        }

        private void btnOk_Click(object sender, RoutedEventArgs e) {
            this.DialogResult = true;
            this.Close();
        }

        protected TaxaService Service {
            get { return new TaxaService(User); }            
        }

        protected User User { get; private set; }

        private void button2_Click(object sender, RoutedEventArgs e) {
            _rankModel.ForEach((vm) => {
                vm.IsSelected = true;
            });
        }

        private void button1_Click(object sender, RoutedEventArgs e) {
            _rankModel.ForEach((vm) => {
                vm.IsSelected = false;
            });
        }

        public List<TaxonRankName> SelectedRanks {
            get {
                var list = new List<TaxonRankName>();

                _rankModel.ForEach((viewModel) => {
                    if (viewModel.IsSelected) {
                        list.Add(viewModel.Model);
                    }
                });

                return list;
            }
        }
        
    }

    public class SelectableRankName : GenericViewModelBase<TaxonRankName> {

        public SelectableRankName(TaxonRankName model, bool selected) : base(model, () => model.ObjectID.Value) {
            this.IsSelected = selected;
        }

        public string Code {
            get { return Model.Code; }
            set { SetProperty(() => Model.Code, value); }
        }

        public string LongName {
            get { return Model.LongName; }
            set { SetProperty(() => Model.LongName, value); }
        }

    }
}
