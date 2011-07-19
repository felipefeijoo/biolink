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
using BioLink.Data;
using BioLink.Data.Model;
using BioLink.Client.Utilities;


namespace BioLink.Client.Taxa {
    /// <summary>
    /// Interaction logic for AvailableNameControl.xaml
    /// </summary>
    public partial class AvailableNameControl : NameControlBase {

        private AvailableNameViewModel _model;

        #region Designer Constructor
        public AvailableNameControl() {
            InitializeComponent();
        }
        #endregion

        public AvailableNameControl(TaxonViewModel taxon, User user) : base(taxon, user, "AvailableNames") {

            InitializeComponent();
            txtReference.BindUser(user, LookupType.Reference);
            txtNameStatus.BindUser(user, PickListType.Phrase, "ALN Name Status", TraitCategoryType.Taxon);
            

            var name = new TaxaService(user).GetAvailableName(taxon.TaxaID.Value);

            if (name == null) {
                name = new AvailableName();
                name.BiotaID = taxon.TaxaID.Value;
            }

            _model = new AvailableNameViewModel(name);
            this.DataContext = _model;

            _model.DataChanged += new DataChangedHandler((vm) => {
                RegisterUniquePendingChange(new UpdateAvailableNameAction(_model.Model));
            });

        }

        private void btnPhrase_Click(object sender, RoutedEventArgs e) {
            InsertPhrase(txtQual, "ALN Standard Phrases");
        }
    }

    public class UpdateAvailableNameAction : GenericDatabaseCommand<AvailableName> {

        public UpdateAvailableNameAction(AvailableName model)
            : base(model) {
        }

        protected override void ProcessImpl(User user) {
            var service = new TaxaService(user);
            service.InsertOrUpdateAvailableName(Model);
        }

    }

    public class AvailableNameViewModel : GenericViewModelBase<AvailableName> {

        public AvailableNameViewModel(AvailableName model) : base(model, null) { }

        public int BiotaID {
            get { return Model.BiotaID; }
            set { SetProperty(() => Model.BiotaID, value); }
        }

        public int? RefID {
            get { return Model.RefID; }
            set { SetProperty(() => Model.RefID, value); }
        }

        public string RefPage {
            get { return Model.RefPage; }
            set { SetProperty(() => Model.RefPage, value); }
        }

        public string RefQual {
            get { return Model.RefQual; }
            set { SetProperty(() => Model.RefQual, value); }
        }

        public string RefCode {
            get { return Model.RefCode; }
            set { SetProperty(() => Model.RefCode, value); }
        }

        public string AvailableNameStatus {
            get { return Model.AvailableNameStatus; }
            set { SetProperty(() => Model.AvailableNameStatus, value); }
        }

    }

}
