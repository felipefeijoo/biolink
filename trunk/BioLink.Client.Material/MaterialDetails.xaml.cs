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
    public partial class MaterialDetails : DatabaseActionControl {
        #region Designer Constructor
        public MaterialDetails() {
            InitializeComponent();
        }
        #endregion

        public MaterialDetails(User user, int materialID) : base(user, "Material:" + materialID) {
            InitializeComponent();
            var service = new MaterialService(user);
            var model = service.GetMaterial(materialID);
            var viewModel = new MaterialViewModel(model);
            viewModel.DataChanged += new DataChangedHandler(viewModel_DataChanged);
            this.DataContext = viewModel;

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
            txtIdentification.BindUser(User, LookupType.Taxon);

        }

        void viewModel_DataChanged(ChangeableModelBase viewmodel) {
            var mvm = viewmodel as MaterialViewModel;
            if (mvm != null) {
                RegisterUniquePendingChange(new UpdateMaterialAction(mvm));
            }

        }
    }
}
