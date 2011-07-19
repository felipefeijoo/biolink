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
    /// Interaction logic for TrapDetail.xaml
    /// </summary>
    public partial class TrapDetails : DatabaseActionControl {

        #region Designer Constructor

        public TrapDetails() {
            InitializeComponent();
        }

        #endregion

        public TrapDetails(User user, int trapID) : base(user, "Trap:" + trapID) {
            InitializeComponent();
            var service = new MaterialService(user);
            var model = service.GetTrap(trapID);
            var viewModel = new TrapViewModel(model);

            this.DataContext = viewModel;

            viewModel.DataChanged += new DataChangedHandler(viewModel_DataChanged);

            txtTrapType.BindUser(User, PickListType.Phrase, "Trap Type", TraitCategoryType.Trap);

            tabTrap.AddTabItem("Traits", new TraitControl(User, TraitCategoryType.Trap, viewModel));
            tabTrap.AddTabItem("Notes", new NotesControl(User, TraitCategoryType.Trap, viewModel));
            tabTrap.AddTabItem("Ownership", new OwnershipDetails(model));

        }

        void viewModel_DataChanged(ChangeableModelBase viewmodel) {
            RegisterUniquePendingChange(new UpdateTrapCommand((viewmodel as TrapViewModel).Model));
        }

    }
}
