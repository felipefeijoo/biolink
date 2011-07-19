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
    /// Interaction logic for MaterialPartsControl.xaml
    /// </summary>
    public partial class MaterialPartsControl : DatabaseActionControl, ILazyPopulateControl {

        private ObservableCollection<ViewModelBase> _model;
        private bool _rdeMode;
        private bool _populated;

        #region Designer Constructor
        public MaterialPartsControl() {
            InitializeComponent();
        }
        #endregion

        public MaterialPartsControl(User user, ViewModelBase owner, bool rdeMode = false) : base(user, "MaterialParts:" + (owner == null ? -1 : owner.ObjectID) ) {
            InitializeComponent();
            _rdeMode = rdeMode;
            this.Owner = owner;

            plCondition.BindUser(User, PickListType.Phrase, "Specimen Condition", TraitCategoryType.Material);
            plCurationStatus.BindUser(User, PickListType.Phrase, "Curation Status", TraitCategoryType.Material);
            plGender.BindUser(User, PickListType.Phrase, "Gender", TraitCategoryType.Material);
            plLifeStage.BindUser(User, PickListType.Phrase, "Life Stage", TraitCategoryType.Material);
            plQualifier.BindUser(User, PickListType.Phrase, "Count Qualifier", TraitCategoryType.Material);
            plSampleType.BindUser(User, PickListType.Phrase, "Sample Type", TraitCategoryType.Material);
            plStorageMethod.BindUser(User, PickListType.Phrase, "Storage Method", TraitCategoryType.Material);
            plStorageSite.BindUser(User, PickListType.Phrase, "Storage Site", TraitCategoryType.Material);

            this.ChangesCommitted += new PendingChangesCommittedHandler(MaterialPartsControl_ChangesCommitted);

            lst.SelectionChanged += new SelectionChangedEventHandler(lst_SelectionChanged);
        }

        public void SetModel(ViewModelBase owner, ObservableCollection<ViewModelBase> model) {
            this.Owner = owner;
            _model = model;
            lst.ItemsSource = _model;

            if (_model != null && _model.Count > 0) {
                lst.SelectedItem = _model[0];
            }
        }

        public ObservableCollection<ViewModelBase> Model { 
            get {
                if (!_populated) {
                    Populate();
                }
                return _model; 
            } 
        }

        public void Populate() {
            if (!_populated) {
                LoadParts();
            }
        }

        private void LoadParts() {

            if (!_rdeMode && Owner != null) {
                detailGrid.IsEnabled = false;

                var service = new MaterialService(User);
                var list = service.GetMaterialParts(Owner.ObjectID.Value);

                _model = new ObservableCollection<ViewModelBase>(list.ConvertAll((part) => {
                    var viewmodel = new MaterialPartViewModel(part);
                    viewmodel.DataChanged += new DataChangedHandler(viewmodel_DataChanged);
                    return viewmodel;
                }));

                lst.ItemsSource = _model;

                if (_model.Count > 0) {
                    lst.SelectedItem = _model[0];
                }
            }

            _populated = true;
        }

        void lst_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            detailGrid.DataContext = lst.SelectedItem as MaterialPartViewModel;
            detailGrid.IsEnabled = detailGrid.DataContext != null;
        }

        void MaterialPartsControl_ChangesCommitted(object sender) {
            LoadParts();
        }

        void viewmodel_DataChanged(ChangeableModelBase viewmodel) {
            var part = viewmodel as MaterialPartViewModel;
            if (part != null) {
                RegisterUniquePendingChange(new UpdateMaterialPartCommand(part.Model));
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e) {
            DeleteSelected();
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e) {
            AddNew();
        }

        private void AddNew() {
            var part = new MaterialPart();
            part.MaterialID = Owner.ObjectID.Value;
            part.MaterialPartID = -1;
            part.PartName = "";

            var viewModel = new MaterialPartViewModel(part);
            _model.Add(viewModel);
            lst.SelectedItem = viewModel;

            RegisterPendingChange(new InsertMaterialPartCommand(part, Owner));
        }

        private void DeleteSelected() {
            var part = lst.SelectedItem as MaterialPartViewModel;
            if (part != null) {
                _model.Remove(part);
                RegisterUniquePendingChange(new DeleteMaterialPartCommand(part.Model));
            }
        }

        public ViewModelBase Owner { get; private set; }


        public bool IsPopulated {
            get { return _populated; }
        }

        public static readonly DependencyProperty IsReadOnlyProperty = DependencyProperty.Register("IsReadOnly", typeof(bool), typeof(MaterialPartsControl), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnIsReadOnlyChanged));

        public bool IsReadOnly {
            get { return (bool)GetValue(IsReadOnlyProperty); }
            set { SetValue(IsReadOnlyProperty, value); }
        }

        private static void OnIsReadOnlyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args) {

            var control = obj as MaterialPartsControl;
            if (control != null) {
                bool val = (bool)args.NewValue;
                control.detailGrid.IsEnabled = !val;
                control.btnAdd.IsEnabled = !val;
                control.btnDelete.IsEnabled = !val;
            }

        }

    }
}
