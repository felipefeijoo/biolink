/*******************************************************************************
 * Copyright (C) 2011 Atlas of Living Australia
 * All Rights Reserved.
 * 
 * The contents of this file are subject to the Mozilla Public
 * License Version 1.1 (the "License"); you may not use this file
 * except in compliance with the License. You may obtain a copy of
 * the License at http://www.mozilla.org/MPL/
 * 
 * Software distributed under the License is distributed on an "AS
 * IS" basis, WITHOUT WARRANTY OF ANY KIND, either express or
 * implied. See the License for the specific language governing
 * rights and limitations under the License.
 ******************************************************************************/
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
    public partial class LoanDetails : DatabaseCommandControl {

        private LoanViewModel _viewModel;
        private OneToManyControl _reminders;
        private GenerateLoanFormControl _loanFormGenerator;

        public LoanDetails(User user, ToolsPlugin plugin, int loanID) : base(user, "Loan:" + loanID) {
            InitializeComponent();

            this.Plugin = plugin;
            this.LoanID = loanID;
            this.ChangeContainerSet += new Action(LoanDetails_ChangeContainerSet);
            this.ChangesCommitted += new PendingChangesCommittedHandler(LoanDetails_ChangesCommitted);
            txtBorrower.ObjectIDChanged += new ObjectIDChangedHandler(txtBorrower_ObjectIDChanged);
            
        }

        void txtBorrower_ObjectIDChanged(object source, int? objectID) {

            if (Preferences.UseLoanPermitNumberTrait.Value) {
                if (objectID.HasValue) {
                    var supportService = new SupportService(User);
                    var traits = supportService.GetTraits("contact", objectID.Value);
                    var permitNumberTrait = traits.Find((trait) => {
                        return trait.Name.Equals("Permit Number", StringComparison.CurrentCultureIgnoreCase);
                    });

                    if (permitNumberTrait != null) {
                        txtPermitNumber.Text = permitNumberTrait.Value;
                    }
                }
            }
        }

        void LoanDetails_ChangesCommitted(object sender) {
            // Keep the loan id in sync with the view model (in the case of new loans).
            LoanID = _viewModel.LoanID;
        }

        public override void Dispose() {
            if (_loanFormGenerator != null) {
                _loanFormGenerator.Close();
            }
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
                    RegisterPendingChange(new InsertLoanReminderCommand(reminder, _viewModel.Model));
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
                RegisterUniquePendingChange(new InsertLoanCommand(model));
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

            var window = this.FindParentWindow() as ControlHostWindow;
            if (window != null) {
                var button = new Button { Width = 130, Height = 23, Content = "_Generate Loan Form..." };
                window.AddCustomButton(button);
                button.Click += new RoutedEventHandler((source, e) => {
                    ShowLoanForms();
                });
            }
            

        }

        private void ShowLoanForms() {

            if (LoanID < 0 || HasPendingChanges) {
                ErrorMessage.Show("You must save/apply any changes to this loan before generating forms");
                return;
            }

            if (_loanFormGenerator == null) {
                _loanFormGenerator = new GenerateLoanFormControl(User, Plugin, LoanID);
                _loanFormGenerator.Owner = this.FindParentWindow();
                _loanFormGenerator.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                _loanFormGenerator.Closed += new EventHandler((source, e) => {
                    _loanFormGenerator = null;
                });
            } 
            
            _loanFormGenerator.Show();
        }

        void viewModel_DataChanged(ChangeableModelBase viewmodel) {
            if (_viewModel.LoanID >= 0) {
                RegisterUniquePendingChange(new UpdateLoanCommand(_viewModel.Model));
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

        private void CheckBox_Checked(object sender, RoutedEventArgs e) {
            DateTime dt = new DateTime(1900, 1, 1);
            if (!_viewModel.DateClosed.HasValue || _viewModel.DateClosed.Value.CompareTo(dt) < 0) {
                _viewModel.DateClosed = DateTime.Now;
            }
        }

    }

    public class UpdateLoanCommand : GenericDatabaseCommand<Loan> {

        public UpdateLoanCommand(Loan model) : base(model) { }

        protected override void ProcessImpl(User user) {
            var service = new LoanService(user);
            service.UpdateLoan(Model);
        }

        protected override void BindPermissions(PermissionBuilder required) {
            required.None();
        }

    }

    public class InsertLoanCommand : GenericDatabaseCommand<Loan> {

        public InsertLoanCommand(Loan model) : base(model) { }

        protected override void ProcessImpl(User user) {
            var service = new LoanService(user);
            Model.LoanID = service.InsertLoan(Model);
        }

        protected override void BindPermissions(PermissionBuilder required) {
            required.None();
        }

        public override void Validate(ValidationMessages messages) {
            var service = new LoanService(PluginManager.Instance.User);
            // Search for loans with this loan number...
            var existing = service.FindLoans(Model.LoanNumber, "l", false);
            if (existing != null && existing.Count > 0) {
                messages.Warn("A loan already exists with this loan number.");
            }
        }

    }
}
