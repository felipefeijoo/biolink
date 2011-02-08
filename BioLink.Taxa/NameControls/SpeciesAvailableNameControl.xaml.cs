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
using BioLink.Client.Utilities;
using BioLink.Client.Extensibility;
using System.Collections.ObjectModel;


namespace BioLink.Client.Taxa {
    /// <summary>
    /// Interaction logic for SpeciesAvailableNameControl.xaml
    /// </summary>
    public partial class SpeciesAvailableNameControl : NameControlBase {

        private SpeciesAvailableNameViewModel _model;
        private ObservableCollection<SANTypeDataViewModel> _typeData;
        private Dictionary<string, SANTypeDataType> _SANTypeDataTypes = new Dictionary<string, SANTypeDataType>();

        #region Designer Constructor
        public SpeciesAvailableNameControl() {
            InitializeComponent();
        }
        #endregion

        public SpeciesAvailableNameControl(TaxonViewModel taxon, User user) 
            : base(taxon, user, "SpeciesAvailabeNames::" ) {

            InitializeComponent();

            txtReference.BindUser(user, LookupType.Reference);
            txtNameStatus.BindUser(user, PickListType.Phrase, "SAN Name Status", TraitCategoryType.Taxon);
            txtInstitution.BindUser(user, PickListType.Phrase, "Institution", TraitCategoryType.Taxon);
            txtSpecimen.BindUser(user, LookupType.Material);

            SpeciesAvailableName data = Service.GetSpeciesAvailableName(taxon.TaxaID.Value);
            if (data == null) {
                data = new SpeciesAvailableName { BiotaID = taxon.TaxaID.Value };
            }

            _model = new SpeciesAvailableNameViewModel(data);

            _model.DataChanged += new DataChangedHandler((changed) => {
                RegisterUniquePendingChange(new UpdateSanDatabaseAction(_model.Model));
            });

            cmbPrimaryType.SelectionChanged += new SelectionChangedEventHandler((source, e) => {
                var tstr = cmbPrimaryType.SelectedItem as string;
                if (_SANTypeDataTypes.ContainsKey(tstr)) {
                    var typedata = _SANTypeDataTypes[tstr];
                    cmbSecondaryType.ItemsSource = typedata.SecondaryTypes;                       
                }
            });

            List<SANTypeDataType> santypes = Service.GetSANTypeDataTypes(taxon.TaxaID.Value);
            foreach (SANTypeDataType type in santypes) {
                _SANTypeDataTypes[type.PrimaryType] = type;
            }

            cmbPrimaryType.ItemsSource = santypes.ConvertAll(st=>st.PrimaryType);

            var tdlist = Service.GetSANTypeData(taxon.TaxaID.Value);
            _typeData = new ObservableCollection<SANTypeDataViewModel>(tdlist.ConvertAll(d => {
                var viewmodel = new SANTypeDataViewModel(d);
                viewmodel.DataChanged +=new DataChangedHandler((changed) => {
                    if (viewmodel.SANTypeDataID >= 0) {
                        RegisterUniquePendingChange(new UpdateSANTypeDataAction(viewmodel.Model));
                    }
                    // todo
                });
                return viewmodel;
            }));
            lstTypeData.ItemsSource = _typeData;

            lstTypeData.SelectionChanged +=new SelectionChangedEventHandler((source, e) => {
                var availableTypes = new List<string>();
                availableTypes.Add(cmbPrimaryType.Text);
                var secondaries = cmbSecondaryType.ItemsSource as IEnumerable<string>;

                if (secondaries != null) {
                    availableTypes.AddRange(secondaries);
                }

                cmbType.ItemsSource = availableTypes;
                gridTypeData.DataContext = lstTypeData.SelectedItem;
            });

            this.DataContext = _model;


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
            var td = new SANTypeData();
            td.SANTypeDataID = -1;
            td.BiotaID = Taxon.TaxaID.Value;
            var viewModel = new SANTypeDataViewModel(td);
            viewModel.Museum = NextNewName("<New {0}>", _typeData, () => viewModel.Museum);            
            _typeData.Add(viewModel);
            lstTypeData.SelectedItem = viewModel;
            RegisterPendingChange(new InsertSANTypeDataAction(td));
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
                    RegisterPendingChange(new DeleteSANTypeDataAction(td.Model));
                }
            }
        }

        public TaxaService Service { get { return new TaxaService(User); } }

    }
}
