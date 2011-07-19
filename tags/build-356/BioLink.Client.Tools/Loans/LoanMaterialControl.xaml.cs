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

namespace BioLink.Client.Tools {
    /// <summary>
    /// Interaction logic for LoanMaterialControl.xaml
    /// </summary>
    public partial class LoanMaterialControl : OneToManyDetailControl {

        protected Loan Loan { get; private set; }

        public LoanMaterialControl(User user, Loan model) : base(user, "LoanMaterial:" + model.LoanID) {
            InitializeComponent();

            txtMaterial.BindUser(user, LookupType.Material);
            txtTaxon.BindUser(user, LookupType.Taxon);
            txtTaxon.EnforceLookup = false; // Any taxon name will do!

            Loan = model;

            lblClosed.Visibility = System.Windows.Visibility.Collapsed;
            chkReturned.IsEnabled = true;
            if (Loan.LoanClosed.HasValue && Loan.LoanClosed.Value) {
                lblClosed.Visibility = System.Windows.Visibility.Visible;
                chkReturned.IsEnabled = false;
            }

            this.DataContextChanged += new DependencyPropertyChangedEventHandler(LoanMaterialControl_DataContextChanged);
        }

        void LoanMaterialControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            var material = e.NewValue as LoanMaterialViewModel;
            if (material != null) {
                if (Loan.LoanClosed.ValueOrFalse() && !material.DateReturned.HasValue) {
                    material.Model.DateReturned = Loan.DateClosed;
                }
            }
        }

        public override ViewModelBase AddNewItem(out DatabaseCommand addAction) {            
            var model = new LoanMaterial() { Returned = false, NumSpecimens = "1", DateAdded=DateTime.Now };
            addAction = new InsertLoanMaterialAction(model, Loan);
            return new LoanMaterialViewModel(model);
        }


        public override DatabaseCommand PrepareDeleteAction(ViewModelBase viewModel) {
            return new DeleteLoanMaterialAction((viewModel as LoanMaterialViewModel).Model);
        }

        public override List<ViewModelBase> LoadModel() {
            var service = new LoanService(User);
            var list = service.GetLoanMaterial(Loan.LoanID);
            var model = new List<ViewModelBase>(list.Select((m) => {
                return new LoanMaterialViewModel(m);
            }));
            return model;
        }

        public override DatabaseCommand PrepareUpdateAction(ViewModelBase viewModel) {
            return new UpdateLoanMaterialAction((viewModel as LoanMaterialViewModel).Model);
        }

        public override FrameworkElement FirstControl {
            get { return txtMaterial; }
        }

        public override bool AcceptDroppedPinnable(PinnableObject pinnable) {
            return pinnable.LookupType == LookupType.Material;
        }

        public override void PopulateFromPinnable(ViewModelBase viewModel, PinnableObject pinnable) {
            var item = viewModel as LoanMaterialViewModel;
            if (item != null && pinnable.LookupType == LookupType.Material) {
                var service = new MaterialService(User);
                var material = service.GetMaterial(pinnable.ObjectID);
                if (material != null) {
                    var vm = PluginManager.Instance.GetViewModel(pinnable);
                    txtMaterial.ObjectID = pinnable.ObjectID;
                    txtMaterial.Text = vm.DisplayLabel;
                    txtTaxon.Text = material.TaxaDesc;
                }
            }
        }

    }

    public class DeleteLoanMaterialAction : GenericDatabaseCommand<LoanMaterial> {

        public DeleteLoanMaterialAction(LoanMaterial model) : base(model) { }

        protected override void ProcessImpl(User user) {
            var service = new LoanService(user);
            service.DeleteLoanMaterial(Model.LoanMaterialID);
        }

    }

    public class InsertLoanMaterialAction : GenericDatabaseCommand<LoanMaterial> {

        public InsertLoanMaterialAction(LoanMaterial model, Loan loan) : base(model) {
            Loan = loan;
        }

        protected override void ProcessImpl(User user) {
            var service = new LoanService(user);
            Model.LoanID = Loan.LoanID;
            Model.LoanMaterialID = service.InsertLoanMaterial(Model);
        }

        private Loan Loan { get; set; }
    }

    public class UpdateLoanMaterialAction : GenericDatabaseCommand<LoanMaterial> {
        public UpdateLoanMaterialAction(LoanMaterial model) : base(model) { }

        protected override void ProcessImpl(User user) {
            var service = new LoanService(user);
            service.UpdateLoanMaterial(Model);
        }
    }

    public class LoanMaterialViewModel : GenericViewModelBase<LoanMaterial> {

        public LoanMaterialViewModel(LoanMaterial model) : base(model, () => model.LoanMaterialID) { }

        public int LoanMaterialID {
            get { return Model.LoanMaterialID; }
            set { SetProperty(() => Model.LoanMaterialID, value); } 
        }

        public int LoanID {
            get { return Model.LoanID; }
            set { SetProperty(() => Model.LoanID, value); }
        }
        
        public int MaterialID {
            get { return Model.MaterialID; }
            set { SetProperty(() => Model.MaterialID, value); }
        }

        public string NumSpecimens {
            get { return Model.NumSpecimens; }
            set { SetProperty(() => Model.NumSpecimens, value); }
        }

        public string TaxonName { 
            get { return Model.TaxonName; }
            set { SetProperty(() => Model.TaxonName, value); }
        }

        public string MaterialDescription {
            get { return Model.MaterialDescription; }
            set { SetProperty(() => Model.MaterialDescription, value); }
        }

        public DateTime? DateAdded {
            get { return Model.DateAdded; }
            set { SetProperty(() => Model.DateAdded, value); }
        }

        public DateTime? DateReturned {
            get { return Model.DateReturned; }
            set { SetProperty(() => Model.DateReturned, value); }
        }

        public bool? Returned {
            get { return Model.Returned; }
            set { SetProperty(() => Model.Returned, value); }
        }

        public string MaterialName {
            get { return Model.MaterialName; }
            set { SetProperty(() => Model.MaterialName, value); }
        }

        public override string ToString() {
            return string.Format("{0}  {1}  ({2} specimens)", (MaterialID == 0 ? "<No Name>" : MaterialName), TaxonName, NumSpecimens);
        }

    }
}
