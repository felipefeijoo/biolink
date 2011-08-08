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
using BioLink.Client.Utilities;
using BioLink.Client.Extensibility;
using BioLink.Data;
using BioLink.Data.Model;

namespace BioLink.Client.Tools {
    /// <summary>
    /// Interaction logic for LoanRemindersControl.xaml
    /// </summary>
    public partial class LoanRemindersControl : OneToManyControllerEditor {

        public LoanRemindersControl(User user, Loan loan) : base(user)  {
            InitializeComponent();
            this.Loan = loan;
        }

        public override ViewModelBase AddNewItem(out DatabaseCommand addAction) {
            var model = new LoanReminder() { Closed = false };
            addAction = new InsertLoanReminderCommand(model, Loan);
            return new LoanReminderViewModel(model);
        }


        public override DatabaseCommand PrepareDeleteAction(ViewModelBase viewModel) {
            var rc = viewModel as LoanReminderViewModel;
            if (rc != null) {
                return new DeleteLoanReminderCommand(rc.Model);
            }
            return null;
        }

        public override List<ViewModelBase> LoadModel() {
            var service = new LoanService(User);
            var list = service.GetLoanReminders(Loan.LoanID);
            return new List<ViewModelBase>(list.Select((m) => {
                return new LoanReminderViewModel(m);
            }));
        }

        public override DatabaseCommand PrepareUpdateAction(ViewModelBase viewModel) {
            var rc = viewModel as LoanReminderViewModel;
            if (rc != null) {
                return new UpdateLoanReminderCommand(rc.Model);
            }
            return null;
        }

        protected Loan Loan { get; private set; }

    }

    public class InsertLoanReminderCommand : GenericDatabaseCommand<LoanReminder> {

        public InsertLoanReminderCommand(LoanReminder model, Loan loan) : base(model) {
            this.Loan = loan;
        }

        protected override void ProcessImpl(User user) {
            var service = new LoanService(user);
            Model.LoanID = Loan.LoanID;
            service.InsertLoanReminder(Model);
        }

        protected Loan Loan { get; private set; }

        protected override void BindPermissions(PermissionBuilder required) {
            required.None();
        }
    }

    public class UpdateLoanReminderCommand : GenericDatabaseCommand<LoanReminder> {

        public UpdateLoanReminderCommand(LoanReminder model) : base(model) { }

        protected override void ProcessImpl(User user) {
            var service = new LoanService(user);
            service.UpdateLoanReminder(Model);
        }

        protected override void BindPermissions(PermissionBuilder required) {
            required.None();
        }
    }

    public class DeleteLoanReminderCommand : GenericDatabaseCommand<LoanReminder> {

        public DeleteLoanReminderCommand(LoanReminder model) : base(model) { }

        protected override void ProcessImpl(User user) {
            var service = new LoanService(user);
            service.DeleteLoanReminder(Model.LoanReminderID);
        }

        protected override void BindPermissions(PermissionBuilder required) {
            required.None();
        }

    }

    public class LoanReminderViewModel : GenericViewModelBase<LoanReminder> {

        public LoanReminderViewModel(LoanReminder model) : base(model, () => model.LoanID) { }

        public override string DisplayLabel {
            get { return ToString(); }            
        }

        public override ImageSource Icon {
            get {
                if (IsOverdue) {
                    return ImageCache.GetImage("pack://application:,,,/BioLink.Client.Extensibility;component/images/Reminder_Overdue.png");
                } else {
                    return ImageCache.GetImage("pack://application:,,,/BioLink.Client.Extensibility;component/images/Reminder.png");
                }
            }
            
            set {  }
        }

        public int LoanReminderID {
            get { return Model.LoanReminderID; }
            set { SetProperty(() => Model.LoanReminderID, value); }
        }

        public int LoanID {
            get { return Model.LoanID; }
            set { SetProperty(() => Model.LoanID, value); }
        }

        public DateTime? Date {
            get { return Model.Date; }
            set { SetProperty(() => Model.Date, value); }
        }

        public bool? Closed {
            get { return Model.Closed; }
            set { SetProperty(() => Model.Closed, value); }
        }

        public string Description {
            get { return Model.Description; }
            set { SetProperty(() => Model.Description, value); }
        }

        public override string ToString() {
            return string.Format("{0:d}  {1}", Date, Description);
        }

        public bool IsOverdue {
            get {
                if (!Date.HasValue || Closed.ValueOrFalse()) {
                    return false;
                }
                return DateTime.Compare(DateTime.Now, Date.Value) > 0; 
            }
        }

        public string DateStr {
            get {
                if (!Date.HasValue) {
                    return "";
                }

                return string.Format("{0:d}", Date);
            }
        }

    }
}
