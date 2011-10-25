using System.Collections.Generic;
using System.Windows;
using BioLink.Data;
using BioLink.Data.Model;
using BioLink.Client.Utilities;
using BioLink.Client.Extensibility;
using System.Collections.ObjectModel;


namespace BioLink.Client.Taxa {
    /// <summary>
    /// Interaction logic for SpeciesAvailableNameControl.xaml
    /// </summary>
    public partial class SpeciesAvailableNameControl : NameControlBase {

        private readonly SpeciesAvailableNameViewModel _model;
        private readonly ObservableCollection<SANTypeDataViewModel> _typeData;
        private readonly Dictionary<string, SANTypeDataType> _SANTypeDataTypes = new Dictionary<string, SANTypeDataType>();

        #region Designer Constructor
        public SpeciesAvailableNameControl() {
            InitializeComponent();
        }
        #endregion

        public SpeciesAvailableNameControl(TaxonViewModel taxon, User user) : base(taxon, user, "SpeciesAvailabeNames::" ) {

            InitializeComponent();

            txtReference.BindUser(user, LookupType.Reference);
            txtNameStatus.BindUser(user, PickListType.Phrase, "SAN Name Status", TraitCategoryType.Taxon);
            txtInstitution.BindUser(user, PickListType.Phrase, "Institution", TraitCategoryType.Taxon);
            txtSpecimen.BindUser(user, LookupType.Material);


            int taxaId = taxon.TaxaID.GetValueOrDefault(-1);
            SpeciesAvailableName data = Service.GetSpeciesAvailableName(taxaId) ?? new SpeciesAvailableName { BiotaID = taxaId };

            _model = new SpeciesAvailableNameViewModel(data);

            _model.DataChanged += changed => RegisterUniquePendingChange(new UpdateSanDatabaseCommand(_model.Model));

            cmbPrimaryType.SelectionChanged += (source, e) => {
                                                   var tstr = cmbPrimaryType.SelectedItem as string;
                                                   if (tstr != null && _SANTypeDataTypes.ContainsKey(tstr)) {
                                                       var typedata = _SANTypeDataTypes[tstr];
                                                       cmbSecondaryType.ItemsSource = typedata.SecondaryTypes;                       
                                                   }
                                               };

            if (taxon.TaxaID != null) {
                List<SANTypeDataType> santypes = Service.GetSANTypeDataTypes(taxon.TaxaID.Value);
                foreach (SANTypeDataType type in santypes) {
                    _SANTypeDataTypes[type.PrimaryType] = type;
                }

                cmbPrimaryType.ItemsSource = santypes.ConvertAll(st=>st.PrimaryType);
            }

            var tdlist = Service.GetSANTypeData(taxaId);
            _typeData = new ObservableCollection<SANTypeDataViewModel>(tdlist.ConvertAll(d => {
                var viewmodel = new SANTypeDataViewModel(d);
                viewmodel.DataChanged +=changed => {
                                            if (viewmodel.SANTypeDataID >= 0) {
                                                RegisterUniquePendingChange(new UpdateSANTypeDataCommand(viewmodel.Model));
                                            }
                                        };
                return viewmodel;
            }));
            lstTypeData.ItemsSource = _typeData;

            lstTypeData.SelectionChanged +=(source, e) => {

                gridTypeData.IsEnabled = lstTypeData.SelectedItem != null;

                var availableTypes = new List<string> {cmbPrimaryType.Text};
                var secondaries = cmbSecondaryType.ItemsSource as IEnumerable<string>;

                if (secondaries != null) {
                    availableTypes.AddRange(secondaries);
                }

                cmbType.ItemsSource = availableTypes;
                gridTypeData.DataContext = lstTypeData.SelectedItem;
            };

            DataContext = _model;

            gridTypeData.IsEnabled = false;


            this.BackgroundInvoke(() => {
                if (_typeData.Count > 0) {
                    lstTypeData.SelectedIndex = 0;
                }
            });
            
        }

        private void btnPhrase_Click(object sender, RoutedEventArgs e) {
            InsertPhrase(txtQual, "ALN Standard Phrases");
        }

        public void AddNewSANTypeData() {
            var td = new SANTypeData {SANTypeDataID = -1, BiotaID = Taxon.TaxaID.GetValueOrDefault(-1)};
            var viewModel = new SANTypeDataViewModel(td);
            viewModel.Museum = NextNewName("<New {0}>", _typeData, () => viewModel.Museum);            
            _typeData.Add(viewModel);
            lstTypeData.SelectedItem = viewModel;
            RegisterPendingChange(new InsertSANTypeDataCommand(td));
        }

        private void btnAddNew_Click(object sender, RoutedEventArgs e) {
            AddNewSANTypeData();
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e) {
            DeleteSelectedSANTypeData();
        }

        private void DeleteSelectedSANTypeData() {
            var td = lstTypeData.SelectedItem as SANTypeDataViewModel;
            if (td != null) {
                if (_typeData.Remove(td)) {
                    RegisterPendingChange(new DeleteSANTypeDataCommand(td.Model));
                }
            }
        }

        public TaxaService Service { get { return new TaxaService(User); } }

        public static readonly DependencyProperty IsReadOnlyProperty = DependencyProperty.Register("IsReadOnly", typeof(bool), typeof(SpeciesAvailableNameControl), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnIsReadOnlyChanged));

        public bool IsReadOnly {
            get { return (bool)GetValue(IsReadOnlyProperty); }
            set { SetValue(IsReadOnlyProperty, value); }
        }

        private static void OnIsReadOnlyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args) {
            var control = (SpeciesAvailableNameControl)obj;
            if (control != null) {
                var readOnly = (bool)args.NewValue;
                control.txtAccessionNumber.IsReadOnly = readOnly;
                control.txtInstitution.IsReadOnly = readOnly;
                control.txtLocality.IsReadOnly = readOnly;
                control.txtMaterial.IsReadOnly = readOnly;
                control.txtNameStatus.IsReadOnly = readOnly;
                control.txtPage.IsReadOnly = readOnly;
                control.txtQual.IsReadOnly = readOnly;
                control.txtReference.IsReadOnly = readOnly;
                control.txtSpecimen.IsReadOnly = readOnly;
                control.btnAddNew.IsEnabled = !readOnly;
                control.btnDelete.IsEnabled = !readOnly;
                control.btnPhrase.IsEnabled = !readOnly;
                control.cmbPrimaryType.IsEnabled = !readOnly;
                control.cmbSecondaryType.IsEnabled = !readOnly;
                control.cmbType.IsEnabled = !readOnly;

                control.chkIDConfirmed.IsEnabled = !readOnly;
                control.chkPrimaryProbable.IsEnabled = !readOnly;
                control.chkSecondaryProbable.IsEnabled = !readOnly;
            }
        }

    }
}
