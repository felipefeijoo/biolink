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
    /// Interaction logic for LoanDetails.xaml
    /// </summary>
    public partial class LoanDetails : DatabaseActionControl {

        private LoanViewModel _viewModel;

        public LoanDetails(User user, ToolsPlugin plugin, int loanID) : base(user, "Loan:" + loanID) {
            InitializeComponent();

            this.Plugin = plugin;
            this.LoanID = loanID;

            this.ChangeContainerSet += new Action(LoanDetails_ChangeContainerSet);

        }

        public override bool Validate(List<string> messages) {

            if (!txtBorrower.ObjectID.HasValue || string.IsNullOrWhiteSpace(txtBorrower.Text)) {
                messages.Add("You must select a borrower for this loan");
            }

            return messages.Count == 0;
        }

        void LoanDetails_ChangeContainerSet() {
            txtLoanNumber.BindUser(User, "LoanNumber", "tblLoan", "vchrLoanNumber");
            txtTransferMethod.BindUser(User, PickListType.Phrase, "Transfer Method", TraitCategoryType.Contact);
            txtReturnType.BindUser(User, PickListType.Phrase, "Return Type", TraitCategoryType.Contact);

            txtBorrower.BindUser(User, LookupType.Contact);
            txtReceiver.BindUser(User, LookupType.Contact);
            txtAuthorizedBy.BindUser(User, LookupType.Contact);

            chkBorrowerIsReceiver.Checked += new RoutedEventHandler(chkBorrowerIsReceiver_Checked);
            chkBorrowerIsReceiver.Unchecked += new RoutedEventHandler(chkBorrowerIsReceiver_Unchecked);

            Loan model = null;
            if (LoanID >= 0) {
                var service = new LoanService(User);
                model = service.GetLoan(LoanID);
            } else {
                model = new Loan { LoanClosed = false };
                RegisterUniquePendingChange(new InsertLoanAction(model));
            }

            if (model != null) {
                _viewModel = new LoanViewModel(model);
                if (model.ReceiverID != 0 && model.ReceiverID == model.RequestorID) {
                    _viewModel.BorrowerIsReceiver = true;
                }

                this.DataContext = _viewModel;
                _viewModel.DataChanged += new DataChangedHandler(viewModel_DataChanged);
            }

        }

        void viewModel_DataChanged(ChangeableModelBase viewmodel) {
            if (_viewModel.LoanID >= 0) {
                RegisterUniquePendingChange(new UpdateLoanAction(_viewModel.Model));
            }
        }

        void chkBorrowerIsReceiver_Unchecked(object sender, RoutedEventArgs e) {
            txtReceiver.IsReadOnly = false;
        }

        void chkBorrowerIsReceiver_Checked(object sender, RoutedEventArgs e) {
            txtReceiver.Text = txtBorrower.Text;
            txtReceiver.ObjectID = txtBorrower.ObjectID;
            txtReceiver.IsReadOnly = true;
        }

        public int LoanID { get; private set; }
        public ToolsPlugin Plugin { get; private set; }

    }

    public class UpdateLoanAction : GenericDatabaseAction<Loan> {

        public UpdateLoanAction(Loan model) : base(model) { }

        protected override void ProcessImpl(User user) {
            var service = new LoanService(user);
            service.UpdateLoan(Model);
        }

    }

    public class InsertLoanAction : GenericDatabaseAction<Loan> {

        public InsertLoanAction(Loan model) : base(model) { }

        protected override void ProcessImpl(User user) {
            var service = new LoanService(user);
            Model.LoanID = service.InsertLoan(Model);
        }
    }
}
