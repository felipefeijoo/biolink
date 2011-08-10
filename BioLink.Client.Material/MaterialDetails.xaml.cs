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

namespace BioLink.Client.Material {
    /// <summary>
    /// Interaction logic for MaterialDetails.xaml
    /// </summary>
    public partial class MaterialDetails : DatabaseCommandControl {

        private IdentificationHistoryControl _historyControl;

        private MaterialViewModel _viewModel;

        #region Designer Constructor
        public MaterialDetails() {
            InitializeComponent();
        }
        #endregion

        public MaterialDetails(User user, int materialID, bool readOnly) : base(user, "Material:" + materialID) {
            InitializeComponent();
            var service = new MaterialService(user);
            var model = service.GetMaterial(materialID);
            _viewModel = new MaterialViewModel(model);
            _viewModel.DataChanged += new DataChangedHandler(viewModel_DataChanged);
            this.DataContext = _viewModel;

            this.IsReadOnly = readOnly;
            

            // General tab
            txtAccessionNumber.BindUser(User, "MaterialAccessionNo", "tblMaterial", "vchrAccessionNo");            
            txtRegistrationNumber.BindUser(User, "MaterialRegNo", "tblMaterial", "vchrRegNo" );
            txtCollectorNo.BindUser(User, "MaterialCollectorNo", "tblMaterial", "vchrCollectorNo");

            txtAbundance.BindUser(user, PickListType.Phrase, "Material Abundance", TraitCategoryType.Material);
            txtSource.BindUser(user, PickListType.Phrase, "Material Source", TraitCategoryType.Material);
            txtInstitution.BindUser(user, PickListType.Phrase, "Institution", TraitCategoryType.Material);
            txtCollectionMethod.BindUser(user, PickListType.Phrase, "Collection Method", TraitCategoryType.Material);
            txtMacroHabitat.BindUser(user, PickListType.Phrase, "Macro Habitat", TraitCategoryType.Material);
            txtMicroHabitat.BindUser(user, PickListType.Phrase, "Micro Habitat", TraitCategoryType.Material);

            txtTrap.BindUser(User, LookupType.Trap);

            // Identification tab
            txtIdentification.BindUser(User, LookupType.Taxon, LookupOptions.TaxonExcludeAvailableNames);
            txtIdentification.ObjectIDChanged += new ObjectIDChangedHandler(txtIdentification_ObjectIDChanged);
            txtIdentifiedBy.BindUser(User, "tblMaterial", "vchrIDBy");
            txtReference.BindUser(User, LookupType.Reference);
            txtAccuracy.BindUser(User, PickListType.Phrase, "Identification Accuracy", TraitCategoryType.Material);
            txtMethod.BindUser(User, PickListType.Phrase, "Identification Method", TraitCategoryType.Material);
            txtNameQual.BindUser(User, PickListType.Phrase, "Identification Qualifier", TraitCategoryType.Material);

            _historyControl = new IdentificationHistoryControl(user, materialID);
            _historyControl.Margin = new Thickness(0);
            tabIDHistory.Content = _historyControl;

            var partsControl = new MaterialPartsControl(User, _viewModel) { IsReadOnly = readOnly };

            tabMaterial.AddTabItem("Subparts", partsControl);
            tabMaterial.AddTabItem("Associates", new OneToManyControl(new AssociatesOneToManyController(User, TraitCategoryType.Material, _viewModel)) { IsReadOnly = readOnly });
            tabMaterial.AddTabItem("Events", new CurationEventsControl(User, materialID, partsControl));
            tabMaterial.AddTabItem("Labels", new MaterialLabelsControl(_viewModel));
            tabMaterial.AddTabItem("Traits", new TraitControl(User, TraitCategoryType.Material, _viewModel) { IsReadOnly = readOnly });
            tabMaterial.AddTabItem("Notes", new NotesControl(User, TraitCategoryType.Material, _viewModel) { IsReadOnly = readOnly });
            tabMaterial.AddTabItem("Multimedia", new MultimediaControl(User, TraitCategoryType.Material, _viewModel) { IsReadOnly = readOnly });
            tabMaterial.AddTabItem("Ownership", new OwnershipDetails(model));
            if (!model.IsTemplate) {
                tabMaterial.AddTabItem("Summary", new MaterialSummary(User, _viewModel));
            }
        }

        public override bool Validate(List<string> messages) {

            if (Config.GetGlobal("Material.CheckUniqueAccessionNumbers", true)) {
                if (!string.IsNullOrWhiteSpace(_viewModel.AccessionNumber)) {

                    var service = new MaterialService(User);

                    var candidateIds = service.GetMaterialIdsByAccessionNo(_viewModel.AccessionNumber);
                    var firstDuplicate = candidateIds.FirstOrDefault((id) => {
                        return id != _viewModel.MaterialID;
                    });

                    if (firstDuplicate > 0) {
                        messages.Add("There is already material in the database with Accession number " + _viewModel.AccessionNumber + " (Material ID " + firstDuplicate + ")");
                    }

                }
            }

            return messages.Count == 0;
        }

        void txtIdentification_ObjectIDChanged(object source, int? objectID) {

            if (!_viewModel.IsTemplate) {
                var addHistory = OptionalQuestions.MaterialIDHistoryQuestion.Ask(this.FindParentWindow());
                if (addHistory) {
                    _historyControl.AddHistoryFromMaterial(_viewModel);
                    // Clear id fields...
                    _viewModel.IdentificationAccuracy = "";
                    _viewModel.IdentificationDate = null;
                    _viewModel.IdentificationMethod = "";
                    _viewModel.IdentificationNameQualification = "";
                    _viewModel.IdentificationNotes = "";
                    _viewModel.IdentificationReferenceID = 0;
                    _viewModel.IdentificationRefPage = "";
                    _viewModel.IdentifiedBy = "";
                }
            }
        }

        void viewModel_DataChanged(ChangeableModelBase viewmodel) {
            var mvm = viewmodel as MaterialViewModel;
            if (mvm != null) {
                RegisterUniquePendingChange(new UpdateMaterialCommand(mvm.Model));
            }

        }

        private void btnAddToLabelSet_Click(object sender, RoutedEventArgs e) {
            AddToLabelSet();
        }

        private void AddToLabelSet() {
            if (_viewModel != null) {
                if (_viewModel.IsChanged || _viewModel.MaterialID <= 0) {
                    ErrorMessage.Show("This material has unsaved changes. Please apply the changes before trying again.");
                    return;
                }

                // make a pinnable...
                var pinnable = new PinnableObject(MaterialPlugin.MATERIAL_PLUGIN_NAME, Data.LookupType.Material, _viewModel.MaterialID);
                var target = PluginManager.Instance.FindAdaptorForPinnable<ILabelSetItemTarget>(pinnable);
                if (target != null) {
                    target.AddItemToLabelSet(pinnable);
                }
            }
        }

        public static readonly DependencyProperty IsReadOnlyProperty = DependencyProperty.Register("IsReadOnly", typeof(bool), typeof(MaterialDetails), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(OnIsReadOnlyChanged)));

        public bool IsReadOnly {
            get { return (bool)GetValue(IsReadOnlyProperty); }
            set { SetValue(IsReadOnlyProperty, value); }
        }

        private static void OnIsReadOnlyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args) {
            var control = (MaterialDetails)obj;
            if (control != null) {
                var readOnly = (bool)args.NewValue;
                control.SetReadOnlyRecursive(readOnly);
            }
        }


    }
}
