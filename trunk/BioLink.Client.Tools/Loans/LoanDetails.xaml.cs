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
        private OneToManyControl _reminders;

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

            if (string.IsNullOrWhiteSpace(txtLoanNumber.Text)) {
                messages.Add("You must enter or generate a loan number for this loan");
            }

            if (_reminders != null) {
                if (!_reminders.IsPopulated) {
                    _reminders.Populate();
                }
                if (_reminders.GetModel().Count == 0) {
                    AddLoanReminder();
                }
            }

            return messages.Count == 0;
        }

        private void AddLoanReminder() {
            if (_viewModel.DateDue.HasValue) {
                if (this.Question("Do you wish BioLink to automatically create a 'Loan due' reminder for this loan?", "Create automatic reminder")) {
                    HardDateConverter c = new HardDateConverter();
                    var reminder = new LoanReminder { LoanID = LoanID, Date = _viewModel.DateDue.Value, Description = "This loan was due back today", Closed = false};
                    RegisterPendingChange(new InsertLoanReminderAction(reminder, _viewModel.Model));
                    _reminders.GetModel().Add(new LoanReminderViewModel(reminder));
                }
            }
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

            tabLoan.AddTabItem("_Material", new OneToManyControl(new LoanMaterialControl(User, model)));
            tabLoan.AddTabItem("_Correspondence", new OneToManyControl(new LoanCorrespondenceControl(User, model)));
            _reminders = new OneToManyControl(new LoanRemindersControl(User, model));
            tabLoan.AddTabItem("_Reminders", _reminders);

            tabLoan.AddTabItem("_Traits", new TraitControl(User, TraitCategoryType.Loan, _viewModel));
            tabLoan.AddTabItem("_Notes", new NotesControl(User, TraitCategoryType.Loan, _viewModel));

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
