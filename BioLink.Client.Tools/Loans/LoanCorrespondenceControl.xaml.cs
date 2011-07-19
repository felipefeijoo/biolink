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
    /// Interaction logic for LoanCorrespondenceControl.xaml
    /// </summary>
    public partial class LoanCorrespondenceControl : OneToManyDetailControl {

        public LoanCorrespondenceControl(User user, Loan loan) : base(user, "LoanCorrespondence:" + loan.LoanID) {
            InitializeComponent();
            txtRefNo.BindUser(user, "LoanCorrespondence", "tblLoanCorrespondence", "vchrRefNo");
            txtSender.BindUser(user, LookupType.Contact);
            txtRecipient.BindUser(user, LookupType.Contact);
            txtType.BindUser(user, PickListType.Phrase, "Correspondence Type", TraitCategoryType.Loan);
            this.Loan = loan;
        }

        public override ViewModelBase AddNewItem(out DatabaseCommand addAction) {
            var model = new LoanCorrespondence() {  };
            addAction = new InsertLoanCorrespondenceCommand(model, Loan);
            return new LoanCorrespondenceViewModel(model);
        }


        public override DatabaseCommand PrepareDeleteAction(ViewModelBase viewModel) {
            var lc = viewModel as LoanCorrespondenceViewModel;
            if (lc != null) {
                return new DeleteLoanCorrespondenceCommand(lc.Model);
            }
            return null;
        }

        public override List<ViewModelBase> LoadModel() {
            var service = new LoanService(User);
            var list = service.GetLoanCorrespondence(Loan.LoanID);
            return new List<ViewModelBase>(list.Select((m) => {
                return new LoanCorrespondenceViewModel(m);
            }));
        }

        public override DatabaseCommand PrepareUpdateAction(ViewModelBase viewModel) {
            var lc = viewModel as LoanCorrespondenceViewModel;
            if (lc != null) {
                return new UpdateLoanCorrespondenceCommand(lc.Model);
            }
            return null;            
        }

        public override FrameworkElement FirstControl {
            get { return txtRefNo; }
        }

        protected Loan Loan { get; private set; }

    }

    public class InsertLoanCorrespondenceCommand : GenericDatabaseCommand<LoanCorrespondence> {

        public InsertLoanCorrespondenceCommand(LoanCorrespondence model, Loan loan) : base(model) {
            Loan = loan;
        }

        protected override void ProcessImpl(User user) {
            var service = new LoanService(user);
            Model.LoanID = Loan.LoanID;
            service.InsertLoanCorrespondence(Model);
        }

        protected override void BindPermissions(PermissionBuilder required) {
            required.None();
        }

        protected Loan Loan { get; private set; }
    }

    public class UpdateLoanCorrespondenceCommand : GenericDatabaseCommand<LoanCorrespondence> {
        public UpdateLoanCorrespondenceCommand(LoanCorrespondence model) : base(model) { }

        protected override void ProcessImpl(User user) {
            var service = new LoanService(user);
            service.UpdateLoanCorrespondence(Model);
        }

        protected override void BindPermissions(PermissionBuilder required) {
            required.None();
        }

    }

    public class DeleteLoanCorrespondenceCommand : GenericDatabaseCommand<LoanCorrespondence> {

        public DeleteLoanCorrespondenceCommand(LoanCorrespondence model) : base(model) { }

        protected override void ProcessImpl(User user) {
            var service = new LoanService(user);
            service.DeleteLoanCorrespondence(Model.LoanCorrespondenceID);
        }

        protected override void BindPermissions(PermissionBuilder required) {
            required.None();
        }

    }

    public class LoanCorrespondenceViewModel : GenericViewModelBase<LoanCorrespondence> {

        public LoanCorrespondenceViewModel(LoanCorrespondence model) : base(model, ()=>model.LoanCorrespondenceID) { }

        public int LoanCorrespondenceID {
            get { return Model.LoanCorrespondenceID; }
            set { SetProperty(() => Model.LoanCorrespondenceID, value); }
        }

        public int LoanID {
            get { return Model.LoanID; }
            set { SetProperty(() => Model.LoanID, value); }
        }

        public string Type {
            get { return Model.Type; }
            set { SetProperty(() => Model.Type, value); }
        }

        public DateTime? Date {
            get { return Model.Date; }
            set { SetProperty(() => Model.Date, value); }
        }

        public int SenderID {
            get { return Model.SenderID; }
            set { SetProperty(() => Model.SenderID, value); }
        }

        public int RecipientID {
            get { return Model.RecipientID; }
            set { SetProperty(() => Model.RecipientID, value); }
        }

        public string Description {
            get { return Model.Description; }
            set { SetProperty(() => Model.Description, value); }
        }

        public string RefNo {
            get { return Model.RefNo; }
            set { SetProperty(() => Model.RefNo, value); }
        }

        public string SenderTitle {
            get { return Model.SenderTitle; }
            set { SetProperty(() => Model.SenderTitle, value); }
        }

        public string SenderGivenName {
            get { return Model.SenderGivenName; }
            set { SetProperty(() => Model.SenderGivenName, value); }
        }

        public string SenderName {
            get { return Model.SenderName; }
            set { SetProperty(() => Model.SenderName, value); }
        }

        public string RecipientTitle {
            get { return Model.RecipientTitle; }
            set { SetProperty(() => Model.RecipientTitle, value); }
        }

        public string RecipientGivenName {
            get { return Model.RecipientGivenName; }
            set { SetProperty(() => Model.RecipientGivenName, value); }
        }

        public string RecipientName {
            get { return Model.RecipientName; }
            set { SetProperty(() => Model.RecipientName, value); }
        }

        public string SenderFullName {
            get { return LoanService.FormatName(Model.SenderTitle, Model.SenderGivenName, Model.SenderName); }
        }

        public string RecipientFullName {
            get { return LoanService.FormatName(Model.RecipientTitle, Model.RecipientGivenName, Model.RecipientName); }
        }

        public override string ToString() {
            return String.Format("{0}  {1:d} From: {2} To: {3}", RefNo, Date, SenderFullName, RecipientFullName); 
        }

    }
}
