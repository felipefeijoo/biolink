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

namespace BioLink.Client.Material {
    /// <summary>
    /// Interaction logic for CurationEventsControl.xaml
    /// </summary>
    public partial class CurationEventsControl : DatabaseActionControl, ILazyPopulateControl {

        private ObservableCollection<CurationEventViewModel> _model;
        private bool _populated = false;
        private MaterialPartsControl _partsControl;

        #region Designer Ctor
        public CurationEventsControl() {
            InitializeComponent();
        }
        #endregion

        public CurationEventsControl(User user, int materialID, MaterialPartsControl partsControl) : base(user, "CurationEvents:" + materialID) {
            InitializeComponent();
            _partsControl = partsControl;
            MaterialID = materialID;
            detailsGrid.IsEnabled = false;
            lstEvents.SelectionChanged += new SelectionChangedEventHandler(lstEvents_SelectionChanged);

            txtEventType.BindUser(user, PickListType.Phrase, "Event Type", TraitCategoryType.Material);
            txtCurator.BindUser(user, PickListType.Phrase, "Curator", TraitCategoryType.Material);

            if (partsControl != null) {
                LoadPartNames();
                partsControl.Model.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(Model_CollectionChanged);

            }
            ChangesCommitted += new PendingChangesCommittedHandler(CurationEventsControl_ChangesCommitted);
        }

        private void LoadPartNames() {            
            var list = new List<String>();
            list.Add("");
            if (_partsControl != null) {
                foreach (MaterialPartViewModel vm in _partsControl.Model) {
                    list.Add(vm.PartName);
                }
            }
            cmbSubpart.ItemsSource = list;
        }

        void Model_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            LoadPartNames();
        }

        void CurationEventsControl_ChangesCommitted(object sender) {
            LoadEvents();
        }

        void lstEvents_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            var selection = lstEvents.SelectedItem as CurationEventViewModel;
            detailsGrid.IsEnabled = selection != null;
            detailsGrid.DataContext = selection;
        }

        void viewModel_DataChanged(ChangeableModelBase viewmodel) {
            var e = viewmodel as CurationEventViewModel;
            if (e != null) {
                RegisterUniquePendingChange(new UpdateCurationEventCommand(e.Model));
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e) {
            DeleteSelected();
        }

        private void DeleteSelected() {
            var selected = lstEvents.SelectedItem as CurationEventViewModel;
            if (selected != null) {
                _model.Remove(selected);
                RegisterPendingChange(new DeleteCurationEventCommand(selected.Model));
            }
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e) {
            AddNew();
        }

        private void AddNew() {
            CurationEvent model = new CurationEvent();
            model.CurationEventID = -1;
            model.MaterialID = MaterialID;
            model.EventType = "<New event>";

            var viewModel = new CurationEventViewModel(model);
            _model.Add(viewModel);

            lstEvents.SelectedItem = viewModel;

            RegisterPendingChange(new InsertCurationEventCommand(model));
        }

        public bool IsPopulated {
            get { return _populated; }
        }

        public void Populate() {
            if (!_populated) {
                LoadEvents();
            }
        }

        protected void LoadEvents() {
            detailsGrid.IsEnabled = false;

            var service = new MaterialService(User);
            var list = service.GetCurationEvents(MaterialID);
            _model = new ObservableCollection<CurationEventViewModel>(list.ConvertAll((model) => {
                var viewModel = new CurationEventViewModel(model);
                viewModel.DataChanged += new DataChangedHandler(viewModel_DataChanged);
                return viewModel;
            }));

            lstEvents.ItemsSource = _model;

            if (_model.Count > 0) {
                lstEvents.SelectedItem = _model[0];
            }
            _populated = true;
        }

        #region Properties

        public int MaterialID { get; set; }

        #endregion

    }
}
