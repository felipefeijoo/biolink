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
using BioLink.Client.Extensibility;
using System.Collections.ObjectModel;

namespace BioLink.Client.Taxa {
    /// <summary>
    /// Interaction logic for CommonNamesControl.xaml
    /// </summary>
    public partial class CommonNamesControl : DatabaseActionControl {

        private ObservableCollection<CommonNameViewModel> _model;

        #region Designer Constructor
        public CommonNamesControl() {
            InitializeComponent();
        }
        #endregion

        public CommonNamesControl(TaxonViewModel taxon, User user) 
            : base(user, "Taxon::CommonNames::" + taxon.TaxaID) {

            InitializeComponent();

            txtReference.BindUser(user, LookupType.Reference);

            var list = Service.GetCommonNames(taxon.TaxaID.Value);
            _model = new ObservableCollection<CommonNameViewModel>(list.ConvertAll(name => {
                var vm = new CommonNameViewModel(name);
                vm.DataChanged += new DataChangedHandler((x) => {
                    if (vm.CommonNameID >= 0) {
                        RegisterPendingChange(new UpdateCommonNameAction(vm));
                    }
                });
                return vm;
            }));
            lstNames.ItemsSource = _model;
            if (_model.Count > 0) {
                lstNames.SelectedIndex = 0;
            }
        }

        protected TaxaService Service { get { return new TaxaService(User); } }
    }

    public class UpdateCommonNameAction : GenericDatabaseAction<CommonNameViewModel> {

        public UpdateCommonNameAction(CommonNameViewModel model)
            : base(model) {
        }

        protected override void ProcessImpl(User user) {
            var service = new TaxaService(user);
            service.UpdateCommonName(Model.Model);
        }

    }

}
