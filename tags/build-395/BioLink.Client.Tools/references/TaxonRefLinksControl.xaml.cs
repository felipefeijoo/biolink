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
    public partial class TaxonRefLinksControl : OneToManyDetailControl {

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
        }

        public override ViewModelBase AddNewItem(out DatabaseCommand addAction) {
            var model = new TaxonRefLink();
            model.RefLinkID = -1;
            model.RefID = ReferenceID;
            model.RefLink = "<New Taxon Link>";
            addAction = new InsertTaxonRefLinkCommand(model);
            return new TaxonRefLinkViewModel(model);
        }

        public override DatabaseCommand PrepareDeleteAction(ViewModelBase viewModel) {
            var link = viewModel as TaxonRefLinkViewModel;
            if (link != null) {
                return new DeleteTaxonRefLinkCommand(link.Model);
            }
            return null;
        }

        public override List<ViewModelBase> LoadModel() {
            var service = new SupportService(User);
            var list = service.GetTaxonRefLinks(ReferenceID);
            return list.ConvertAll((model) => {
                return (ViewModelBase)new TaxonRefLinkViewModel(model);
            });
        }

        public override DatabaseCommand PrepareUpdateAction(ViewModelBase viewModel) {
            var link = viewModel as TaxonRefLinkViewModel;
            if (link != null) {
                return new UpdateTaxonRefLinkCommand(link.Model);
            }
            return null;
        }

        #region Properties

        public int ReferenceID { get; private set;  }

        public override FrameworkElement FirstControl {
            get { return this.txtRefType; }
        }

        #endregion

    }
}
