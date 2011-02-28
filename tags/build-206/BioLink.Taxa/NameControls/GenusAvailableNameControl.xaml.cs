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
using BioLink.Client.Extensibility;
using BioLink.Data.Model;
using BioLink.Client.Utilities;
using System.Collections.ObjectModel;

namespace BioLink.Client.Taxa {
    /// <summary>
    /// Interaction logic for GenusAvailableNameControl.xaml
    /// </summary>
    public partial class GenusAvailableNameControl : NameControlBase {

        private GenusAvailableNameViewModel _model;
        private ObservableCollection<GANIncludedSpeciesViewModel> _includedSpecies;

        #region designer constructor
        public GenusAvailableNameControl() {
            InitializeComponent();
        }
        #endregion

        public GenusAvailableNameControl(TaxonViewModel taxon, User user)
            : base(taxon, user, "GenusAvailableNames") {

            InitializeComponent();

            txtReference.BindUser(user, LookupType.Reference);
            txtNameStatus.BindUser(user, PickListType.Phrase, "GAN Name Status", TraitCategoryType.Taxon);
            txtFixationMethod.BindUser(user, PickListType.Phrase, "Fixation Method", TraitCategoryType.Taxon);

            var data = Service.GetGenusAvailableName(taxon.TaxaID.Value);
            if (data == null) {
                data = new GenusAvailableName();
                data.BiotaID = taxon.TaxaID.Value;                
            }
            _model = new GenusAvailableNameViewModel(data);

            this.DataContext = _model;
            _model.DataChanged += new DataChangedHandler((vm) => {
                RegisterUniquePendingChange(new UpdateGenusAvailableNameAction(_model.Model));
                EnableTabs();
            });

            LoadIncludedSpeciesModel();

            EnableTabs();

            // The "insert included species" stored proc does not return the new identity for the
            // included species record, so we need to reload the model on update to get the ids...
            this.ChangesCommitted += new PendingChangesCommittedHandler((s) => {
                LoadIncludedSpeciesModel();
            });      

        }

        private void LoadIncludedSpeciesModel() {
            var incSpecies = Service.GetGenusAvailableNameIncludedSpecies(Taxon.TaxaID.Value);
            _includedSpecies = new ObservableCollection<GANIncludedSpeciesViewModel>(incSpecies.ConvertAll((d) => {
                var vm = new GANIncludedSpeciesViewModel(d);
                vm.DataChanged += new DataChangedHandler((changedvm) => {
                    if (d.GANISID < 0) {
                        // don'note need to do anything, because the pending insert will use the most recent changes
                    } else {
                        RegisterUniquePendingChange(new UpdateGANIncludedSpeciesAction(d));
                    }

                });
                return vm;
            }));
            lstIncludedSpecies.ItemsSource = _includedSpecies;
        }

        private void EnableTabs() {
            switch (_model.Designation) {
                case 0:                    
                    tabTypeSpecies.IsEnabled = true;
                    tabIncludedSpecies.IsEnabled = false;
                    break;
                case 1: 
                case 3:
                    tabTypeSpecies.IsEnabled = false;
                    tabIncludedSpecies.IsEnabled = false;
                    break;
                case 2:                    
                    tabTypeSpecies.IsEnabled = false;
                    tabIncludedSpecies.IsEnabled = true;
                    break;                
                default:
                    throw new Exception("Unexpected Designation value: " + _model.Designation);
            }

            

            var selectedTab = tabControl.SelectedItem as TabItem;
            if (selectedTab != null) {
                if (!selectedTab.IsEnabled) {
                    (tabControl.Items[0] as TabItem).IsSelected = true;
                }
            }
        }

        protected TaxaService Service { get { return new TaxaService(User); } }

        private void btnInsertPhrase_Click(object sender, RoutedEventArgs e) {
            InsertPhrase(txtQual, "GAN Standard Phrases");
        }

        private void btnDeleteIncludedSpecies_Click(object sender, RoutedEventArgs e) {
            var item = (sender as Button).Tag as GANIncludedSpeciesViewModel;
            if (item != null) {
                if (this.Question("Are you sure you wish to delete included species '" + item.IncludedSpecies + "'", "Delete included species?")) {
                    // Delete
                    RegisterPendingChange(new DeleteGANIncludedSpeciesAction(item.Model));
                    _includedSpecies.Remove(item);
                }
            }
        }

        private void btnAddIncludedSpecies_Click(object sender, RoutedEventArgs e) {
            AddIncludedSpecies();
        }

        private void AddIncludedSpecies() {
            GANIncludedSpecies data = new GANIncludedSpecies();
            data.GANISID = -1;
            data.BiotaID = this.Taxon.TaxaID.Value;
            data.IncludedSpecies = "<Included Species>";
            var vm = new GANIncludedSpeciesViewModel(data);
            _includedSpecies.Add(vm);
            RegisterPendingChange(new InsertGANIncludedSpeciesAction(data));
        }

    }

    public class GANDesignationTypeConverter : IValueConverter {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            string param = parameter as string;
            if (!string.IsNullOrEmpty(param)) {
                int val;
                if (int.TryParse(param, out val)) {
                    return val.Equals(value);
                }
            }

            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            int val;
            if (int.TryParse(parameter as string, out val)) {
                return val;
            }
            throw new Exception("Unexepected paramter value: " + parameter);
        }

    }

}
