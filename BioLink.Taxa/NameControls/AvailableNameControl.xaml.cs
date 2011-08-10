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
                RegisterUniquePendingChange(new UpdateAvailableNameCommand(_model.Model));
            });

        }

        private void btnPhrase_Click(object sender, RoutedEventArgs e) {
            InsertPhrase(txtQual, "ALN Standard Phrases");
        }

        public static readonly DependencyProperty IsReadOnlyProperty = DependencyProperty.Register("IsReadOnly", typeof(bool), typeof(AvailableNameControl), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(OnIsReadOnlyChanged)));

        public bool IsReadOnly {
            get { return (bool)GetValue(IsReadOnlyProperty); }
            set { SetValue(IsReadOnlyProperty, value); }
        }

        private static void OnIsReadOnlyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args) {
            var control = (AvailableNameControl)obj;
            if (control != null) {
                var readOnly = (bool)args.NewValue;
                control.txtNameStatus.IsReadOnly = readOnly;
                control.txtPage.IsReadOnly = readOnly;
                control.txtQual.IsReadOnly = readOnly;
                control.txtReference.IsReadOnly = readOnly;
                control.btnPhrase.IsEnabled = !readOnly;
            }
        }

    }

    public class UpdateAvailableNameCommand : GenericDatabaseCommand<AvailableName> {

        public UpdateAvailableNameCommand(AvailableName model)
            : base(model) {
        }

        protected override void ProcessImpl(User user) {
            var service = new TaxaService(user);
            service.InsertOrUpdateAvailableName(Model);
        }

        protected override void BindPermissions(PermissionBuilder required) {
            required.Add(PermissionCategory.SPIN_TAXON, PERMISSION_MASK.UPDATE);
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
