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
using BioLink.Client.Utilities;

namespace BioLink.Client.Taxa {
    /// <summary>
    /// Interaction logic for CommonNamesControl.xaml
    /// </summary>
    public partial class CommonNamesControl : NameControlBase, ILazyPopulateControl {

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
            lstNames.SelectionChanged += new SelectionChangedEventHandler(lstNames_SelectionChanged);

            ChangesCommitted += new PendingChangesCommittedHandler(CommonNamesControl_ChangesCommitted);

        }

        void CommonNamesControl_ChangesCommitted(object sender) {
            LoadNames();
        }

        void lstNames_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            detailGrid.IsEnabled = lstNames.SelectedItem != null;
        }

        private void LoadNames() {
            detailGrid.IsEnabled = false;
            var list = Service.GetCommonNames(Taxon.TaxaID.Value);
            _model = new ObservableCollection<CommonNameViewModel>(list.ConvertAll(name => {
                var vm = new CommonNameViewModel(name);
                vm.DataChanged += new DataChangedHandler((x) => {
                    RegisterPendingChange(new UpdateCommonNameAction(vm.Model));
                });
                return vm;
            }));
            lstNames.ItemsSource = _model;
            if (_model.Count > 0) {
                lstNames.SelectedIndex = 0;
            }

            IsPopulated = true;
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


        public bool IsPopulated { get; private set; }

        public void Populate() {
            if (!IsPopulated) {
                LoadNames();
            }
        }
    }

}
