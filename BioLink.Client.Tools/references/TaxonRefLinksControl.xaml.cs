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

namespace BioLink.Client.Tools {
    /// <summary>
    /// Interaction logic for TaxonRefLinksControl.xaml
    /// </summary>
    public partial class TaxonRefLinksControl : DatabaseActionControl, ILazyPopulateControl {

        private ObservableCollection<TaxonRefLinkViewModel> _model;

        #region Designer ctor
        public TaxonRefLinksControl() {
            InitializeComponent();
        }
        #endregion

        public TaxonRefLinksControl(User user, int referenceID)
            : base(user, "TaxonRefLinks:" + referenceID) {
            InitializeComponent();
            txtRefType.BindUser(User, PickListType.RefLinkType, "", TraitCategoryType.Taxon);
            txtTaxon.BindUser(User, LookupType.Taxon);
            this.ReferenceID = referenceID;
            ChangesCommitted += new PendingChangesCommittedHandler(TaxonRefLinksControl_ChangesCommitted);
            lst.SelectionChanged += new SelectionChangedEventHandler(lst_SelectionChanged);

        }

        void lst_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            detailsGrid.IsEnabled = lst.SelectedItem != null;
            detailsGrid.DataContext = lst.SelectedItem;
        }

        void TaxonRefLinksControl_ChangesCommitted(object sender) {
            LoadLinks();
        }

        private void DeleteSelected() {
            var viewmodel = lst.SelectedItem as TaxonRefLinkViewModel;
            if (viewmodel != null) {
                _model.Remove(viewmodel);
                RegisterPendingChange(new DeleteRefLinkAction(viewmodel.RefLinkID));
            }
        }

        private void AddNew() {
            var model = new TaxonRefLink();
            model.RefLinkID = -1;
            model.RefID = ReferenceID;
            model.RefLink = "<New Taxon Link>";
            var viewModel = new TaxonRefLinkViewModel(model);
            _model.Add(viewModel);

            lst.SelectedItem = viewModel;

            RegisterPendingChange(new InsertTaxonRefLinkAction(model));
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e) {
            DeleteSelected();
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e) {
            AddNew();
        }

        private void LoadLinks() {

            detailsGrid.IsEnabled = false;

            var service = new SupportService(User);
            var list = service.GetTaxonRefLinks(ReferenceID);
            _model = new ObservableCollection<TaxonRefLinkViewModel>(list.ConvertAll((model) => {
                var viewModel = new TaxonRefLinkViewModel(model);
                viewModel.DataChanged += new DataChangedHandler(viewModel_DataChanged);
                return viewModel;
            }));

            lst.ItemsSource = _model;

            if (_model.Count > 0) {
                lst.SelectedItem = _model[0];
            }

            IsPopulated = true;
        }

        void viewModel_DataChanged(ChangeableModelBase viewmodel) {
            var link = viewmodel as TaxonRefLinkViewModel;
            if (link != null) {
                RegisterUniquePendingChange(new UpdateTaxonRefLinkAction(link.Model));
            }
        }

        public bool IsPopulated { get; private set; }

        public void Populate() {
            if (!IsPopulated) {
                LoadLinks();
            }
        }


        #region Properties

        public int ReferenceID { get; private set;  }

        #endregion


    }
}
