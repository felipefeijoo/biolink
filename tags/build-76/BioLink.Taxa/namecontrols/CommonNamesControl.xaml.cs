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
using BioLink.Data;
using BioLink.Data.Model;
using BioLink.Client.Extensibility;
using System.Collections.ObjectModel;

namespace BioLink.Client.Taxa {
    /// <summary>
    /// Interaction logic for CommonNamesControl.xaml
    /// </summary>
    public partial class CommonNamesControl : NameControlBase {

        private ObservableCollection<CommonNameViewModel> _model;

        #region Designer Constructor
        public CommonNamesControl() {
            InitializeComponent();
        }
        #endregion

        public CommonNamesControl(TaxonViewModel taxon, User user) 
            : base(taxon, user, "CommonNames") {

            InitializeComponent();

            txtReference.BindUser(user, LookupType.Reference);

            var list = Service.GetCommonNames(taxon.TaxaID.Value);
            _model = new ObservableCollection<CommonNameViewModel>(list.ConvertAll(name => {
                var vm = new CommonNameViewModel(name);
                vm.DataChanged += new DataChangedHandler((x) => {
                    if (vm.CommonNameID >= 0) {
                        RegisterPendingChange(new UpdateCommonNameAction(vm.Model));
                    }
                });
                return vm;
            }));
            lstNames.ItemsSource = _model;
            if (_model.Count > 0) {
                lstNames.SelectedIndex = 0;
            }
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e) {
            AddNewName();
        }

        private void AddNewName() {
            CommonName data = new CommonName();
            data.BiotaID = Taxon.TaxaID.Value;
            data.CommonNameID = -1;            
            var viewModel = new CommonNameViewModel(data);
            viewModel.Name = NextNewName("<Name name {0}>", _model, () => viewModel.Name);
            _model.Add(viewModel);
            lstNames.SelectedItem = viewModel;
            lstNames.ScrollIntoView(viewModel);
            RegisterPendingChange(new InsertCommonNameAction(data));
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e) {
            DeleteSelectedName();
        }

        private void DeleteSelectedName() {
            var name = lstNames.SelectedItem as CommonNameViewModel;
            if (name != null) {
                RegisterPendingChange(new DeleteCommonNameAction(name.Model));
                _model.Remove(name);
            }
        }

        protected TaxaService Service { get { return new TaxaService(User); } }

    }

}
