using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Client.Utilities;
using BioLink.Client.Extensibility;
using BioLink.Data;
using BioLink.Data.Model;
using System.Windows.Media;

namespace BioLink.Client.Tools {
    public class LoanViewModel : GenericViewModelBase<Loan> {

        public LoanViewModel(Loan model) : base(model, ()=>model.LoanID) { }

        public override string DisplayLabel {
            get {
                var refNumber = String.IsNullOrEmpty(LoanNumber) ? PermitNumber : LoanNumber;
                return string.Format("{0}  {1}  Due: {2}", refNumber, RequestedBy, DueDateStr);
            }
        }

        public override ImageSource Icon {
            get {
                if (base.Icon == null) {
                    if (IsOverdue) {
                        return ImageCache.GetImage("pack://application:,,,/BioLink.Client.Extensibility;component/images/Loan_Overdue.png");                        
                    } else {
                        return ImageCache.GetImage("pack://application:,,,/BioLink.Client.Extensibility;component/images/Loan.png");
                    }
                }
                return base.Icon;
            }
            set {
                base.Icon = value;
            }
        }

        public override System.Windows.FrameworkElement TooltipContent {
            get {
                return new LoanTooltipContent(LoanID, this);
            }
        }

        public int LoanID {
            get { return Model.LoanID; }
            set { SetProperty(() => Model.LoanID, value); }
        }

        public string LoanNumber {
            get { return Model.LoanNumber; }
            set { SetProperty(() => Model.LoanNumber, value); }
        }
        public int RequestorID {
            get { return Model.RequestorID; }
            set { 
                SetProperty(() => Model.RequestorID, value);
                if (_borrowerIsReceiver) {
                    SetProperty(() => Model.ReceiverID, value);
                }
            }
        }

        public int ReceiverID {
            get { return Model.ReceiverID; }
            set { SetProperty(() => Model.ReceiverID, value); }
        }

        public int OriginatorID {
            get { return Model.OriginatorID; }
            set { SetProperty(() => Model.OriginatorID, value); }
        }

        public DateTime? DateInitiated {
            get { return Model.DateInitiated; }
            set { SetProperty(() => Model.DateInitiated, value); }
        }

        public DateTime? DateDue {
            get { return Model.DateDue; }
            set { SetProperty(() => Model.DateDue, value); }
        }

        public string MethodOfTransfer {
            get { return Model.MethodOfTransfer; }
            set { SetProperty(() => Model.MethodOfTransfer, value); }
        }

        public string PermitNumber {
            get { return Model.PermitNumber; }
            set { SetProperty(() => Model.PermitNumber, value); }
        }

        public string TypeOfReturn {
            get { return Model.TypeOfReturn; }
            set { SetProperty(() => Model.TypeOfReturn, value); }
        }

        public string Restrictions {
            get { return Model.Restrictions; }
            set { SetProperty(() => Model.Restrictions, value); }
        }

        public DateTime? DateClosed {
            get { return Model.DateClosed; }
            set { SetProperty(() => Model.DateClosed, value); }
        }

        public bool? LoanClosed {
            get { return Model.LoanClosed; }
            set { SetProperty(()=>Model.LoanClosed, value); }
        }

        public string RequestorTitle {
            get { return Model.RequestorTitle; }
        }

        public string RequestorGivenName {
            get { return Model.RequestorGivenName; }
        }

        public string RequestorName {
            get { return Model.RequestorName; }
        }

        public string ReceiverTitle {
            get { return Model.ReceiverTitle; }
        }

        public string ReceiverGivenName {
            get { return Model.ReceiverGivenName; }
        }

        public string ReceiverName {
            get { return Model.ReceiverName; }
        }

        public string OriginatorTitle {
            get { return Model.OriginatorTitle; }
        }

        public string OriginatorGivenName {
            get { return Model.OriginatorGivenName; }
        }

        public string OriginatorName {
            get { return Model.OriginatorName; }
        }

        public string RequestedBy {
            get { return LoanService.FormatName(RequestorTitle, RequestorGivenName, RequestorName); }
        }

        public string ReceivedBy {
            get { return LoanService.FormatName(ReceiverTitle, ReceiverGivenName, ReceiverName); }
        }

        public string AuthorizedBy {
            get { return LoanService.FormatName(OriginatorTitle, OriginatorGivenName, OriginatorName); }
        }

        public bool IsOverdue {
            get {
                if (LoanClosed.ValueOrFalse()) {
                    return false;
                } else {
                    var now = DateTime.Now;
                    return (DateDue.HasValue && DateTime.Compare(now, DateDue.Value) > 0);
                }

            }
        }

        private bool _borrowerIsReceiver;

        public bool BorrowerIsReceiver {
            get { return _borrowerIsReceiver; }
            set { SetProperty("BorrowerIsReceiver", ref _borrowerIsReceiver, value); }
        }

        public string Status {
            get {
                if (LoanClosed.ValueOrFalse()) {
                    return "Closed";
                } else {
                    return "Open" + (IsOverdue ? " (Overdue)" : "");
                }
            }
        }

        public string StartDateStr {
            get { return DateUtils.ShortDate(DateInitiated); }
        }

        public string DueDateStr {
            get { return DateUtils.ShortDate(DateDue); }
        }

    }

    public class LoanTooltipContent : TooltipContentBase {

        public LoanTooltipContent(int objectId, ViewModelBase viewModel) : base(objectId, viewModel) { }

        protected override void GetDetailText(OwnedDataObject model, TextTableBuilder builder) {
            var loan = model as Loan;
            builder.Add("Loan number", loan.LoanNumber);
            builder.Add("Permit number", loan.PermitNumber);
            builder.Add("Date initiated", loan.DateInitiated);
            builder.Add("Date due", loan.DateDue);
            builder.AddFormat("Requested by", LoanService.FormatName(loan.RequestorTitle, loan.RequestorGivenName, loan.RequestorName));
            builder.AddFormat("Received by", LoanService.FormatName(loan.ReceiverTitle, loan.RequestorGivenName, loan.ReceiverName));
            builder.AddFormat("Authorized by", LoanService.FormatName(loan.OriginatorTitle, loan.OriginatorGivenName, loan.OriginatorName));
            builder.AddFormat("Status", (ViewModel as LoanViewModel).Status);
        }

        protected override OwnedDataObject GetModel() {
            var service = new LoanService(User);
            return service.GetLoan(ObjectID);
        }
    }
}
