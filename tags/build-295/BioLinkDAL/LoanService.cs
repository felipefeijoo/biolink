using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using BioLink.Data.Model;

namespace BioLink.Data {

    public class LoanService : BioLinkService  {

        public LoanService(User user) : base(user) { }

        public List<Contact> FindContacts(string filter, ContactSearchType searchType) {
            var what = "";
            switch (searchType) {
                case ContactSearchType.Institution:
                    what = "i";
                    break;
                case ContactSearchType.Surname:
                    what = "s";
                    break;
                default:
                    break;
            }

            var where = string.Format(" like '{0}%'", filter);
            var mapper = GetContactMapper();
            return StoredProcToList("spContactList", mapper, _P("vchrSearchWhere", what), _P("vchrWhereClause", where));
        }

        public List<Contact> ListContactsRange(string from, string to) {            
            var strWhere = "left(vchrName," + from.Length + ") between '" + from + "' and '" + to + "'";
            var mapper = new GenericMapperBuilder<Contact>().build();
            return StoredProcToList("spContactListRange", mapper, _P("vchrWhere", strWhere));
        }

        public Contact GetContact(int contactId) {
            var mapper = GetContactMapper();
            return StoredProcGetOne("spContactGet", mapper, _P("intContactID", contactId));
        }

        public void UpdateContact(Contact contact) {
            StoredProcUpdate("spContactUpdate",
                _P("intContactID", contact.ContactID),
                _P("vchrName", contact.Name),
                _P("vchrTitle", contact.Title),
                _P("vchrGivenName", contact.GivenName),
                _P("vchrPostalAddress", contact.StreetAddress),
                _P("vchrStreetAddress", contact.StreetAddress),
                _P("vchrInstitution", contact.Institution),
                _P("vchrJobTitle", contact.JobTitle),
                _P("vchrWorkPh", contact.WorkPh),
                _P("vchrWorkFax", contact.WorkFax),
                _P("vchrHomePh", contact.HomePh),
                _P("vchrEMail", contact.EMail));
        }

        public void DeleteContact(int contactID) {
            StoredProcUpdate("spContactDelete", _P("intContactID", contactID));
        }

        protected GenericMapper<Contact> GetContactMapper() {
            return new GenericMapperBuilder<Contact>().build();
        }


        public int InsertContact(Contact contact) {
            var retval = ReturnParam("NewContactID");
            StoredProcUpdate("spContactInsert",
                _P("vchrName", contact.Name),
                _P("vchrTitle", contact.Title),
                _P("vchrGivenName", contact.GivenName),
                _P("vchrPostalAddress", contact.StreetAddress),
                _P("vchrStreetAddress", contact.StreetAddress),
                _P("vchrInstitution", contact.Institution),
                _P("vchrJobTitle", contact.JobTitle),
                _P("vchrWorkPh", contact.WorkPh),
                _P("vchrWorkFax", contact.WorkFax),
                _P("vchrHomePh", contact.HomePh),
                _P("vchrEMail", contact.EMail),
                retval
                );

            int newContactID = (int)retval.Value;
            return newContactID;
        }

        public List<Loan> ListLoansForContact(int contactID) {
            var mapper = new GenericMapperBuilder<Loan>().build();
            return StoredProcToList("spLoanListForContact", mapper, _P("intContactID", contactID));
        }

        public static String FormatName(Contact contact) {
            return FormatName(contact.Title, contact.GivenName, contact.Name);
        }

        public static string FormatName(string title, string givenName, string surname) {
            var sb = new StringBuilder();
            if (!string.IsNullOrWhiteSpace(title)) {
                sb.Append(title).Append(" ");
            }

            if (!string.IsNullOrWhiteSpace(givenName)) {
                sb.Append(givenName).Append(" ");
            }

            sb.Append(surname);

            return sb.ToString();
        }

        public Loan GetLoan(int loanID) {
            var mapper = new GenericMapperBuilder<Loan>().build();
            return StoredProcGetOne("spLoanGet", mapper, _P("intLoanID", loanID));
        }

        public int InsertLoan(Loan loan) {
            var retval = ReturnParam("NewLoanID");
            StoredProcUpdate("spLoanInsert",
                _P("vchrLoanNumber", loan.LoanNumber),
                _P("intRequestorID", loan.RequestorID),
                _P("intReceiverID", loan.ReceiverID),
                _P("intOriginatorID", loan.OriginatorID),
                _P("dtDateInitiated", loan.DateInitiated),
                _P("dtDateDue", loan.DateDue),
                _P("vchrMethodOfTransfer", loan.MethodOfTransfer),
                _P("vchrPermitNumber", loan.PermitNumber),
                _P("vchrTypeOfReturn", loan.TypeOfReturn),
                _P("vchrRestrictions", loan.Restrictions),
                _P("dtDateClosed", loan.DateClosed),
                _P("bitLoanClosed", loan.LoanClosed),
                retval
            );

            return (int)retval.Value;
        }

        public void UpdateLoan(Loan loan) {
            StoredProcUpdate("spLoanUpdate",
                _P("intLoanID", loan.LoanID),
                _P("vchrLoanNumber", loan.LoanNumber),
                _P("intRequestorID", loan.RequestorID),
                _P("intReceiverID", loan.ReceiverID),
                _P("intOriginatorID", loan.OriginatorID),
                _P("dtDateInitiated", loan.DateInitiated),
                _P("dtDateDue", loan.DateDue),
                _P("vchrMethodOfTransfer", loan.MethodOfTransfer),
                _P("vchrPermitNumber", loan.PermitNumber),
                _P("vchrTypeOfReturn", loan.TypeOfReturn),
                _P("vchrRestrictions", loan.Restrictions),
                _P("dtDateClosed", loan.DateClosed),
                _P("bitLoanClosed", loan.LoanClosed)
            );
        }

        public void DeleteLoan(int loanId) {
            StoredProcUpdate("spLoanDelete", _P("intLoanID", loanId));
        }

        public List<LoanMaterial> GetLoanMaterial(int loanId) {
            var mapper = new GenericMapperBuilder<LoanMaterial>().build();
            return StoredProcToList("spLoanMaterialList", mapper, _P("intLoanID", loanId));
        }

        public LoanMaterial GetSingleLoanMaterial(int loanMaterialID) {
            var mapper = new GenericMapperBuilder<LoanMaterial>().build();
            return StoredProcGetOne("spLoanMaterialGet", mapper, _P("intLoanMaterialID", loanMaterialID));
        }

        public int InsertLoanMaterial(LoanMaterial m) {
            var retval = ReturnParam("NewLoanMaterialID");
            StoredProcUpdate("spLoanMaterialInsert",
                _P("intLoanID", m.LoanID),
                _P("intMaterialID", m.MaterialID),
                _P("vchrNumSpecimens", m.NumSpecimens),
                _P("vchrTaxonName", m.TaxonName),
                _P("vchrMaterialDescription", m.MaterialDescription),
                _P("dtDateAdded", m.DateAdded),
                _P("dtDateReturned", m.DateReturned),
                _P("bitReturned", m.Returned),
                retval
            );
            return (int)retval.Value;
        }

        public void UpdateLoanMaterial(LoanMaterial m) {
            StoredProcUpdate("spLoanMaterialUpdate",
                _P("intLoanMaterialID", m.LoanMaterialID),
                _P("intLoanID", m.LoanID),
                _P("intMaterialID", m.MaterialID),
                _P("vchrNumSpecimens", m.NumSpecimens),
                _P("vchrTaxonName", m.TaxonName),
                _P("vchrMaterialDescription", m.MaterialDescription),
                _P("dtDateAdded", m.DateAdded),
                _P("dtDateReturned", m.DateReturned),
                _P("bitReturned", m.Returned));                
        }

        public void DeleteLoanMaterial(int loanMaterialID) {
            StoredProcUpdate("spLoanMaterialDelete", _P("intLoanMaterialID", loanMaterialID));
        }

        public List<LoanCorrespondence> GetLoanCorrespondence(int loanId) {
            var mapper = new GenericMapperBuilder<LoanCorrespondence>().build();
            return StoredProcToList("spLoanCorrList", mapper, _P("intLoanID", loanId));
        }

        public void InsertLoanCorrespondence(LoanCorrespondence c) {

            StoredProcUpdate("spLoanCorrInsert",
                _P("intLoanID", c.LoanID),
                _P("vchrRefNo", c.RefNo),
                _P("vchrType", c.Type),
                _P("dtDate", c.Date),
                _P("intSenderID", c.SenderID),
                _P("intRecipientID", c.RecipientID),
                _P("txtDescription", c.Description));
        }

        public void UpdateLoanCorrespondence(LoanCorrespondence c) {
            StoredProcUpdate("spLoanCorrUpdate",
                _P("intLoanCorrespondenceID", c.LoanCorrespondenceID),
                _P("intLoanID", c.LoanID),
                _P("vchrRefNo", c.RefNo),
                _P("vchrType", c.Type),
                _P("dtDate", c.Date),
                _P("intSenderID", c.SenderID),
                _P("intRecipientID", c.RecipientID),
                _P("txtDescription", c.Description)
            );
        }

        public void DeleteLoanCorrespondence(int loanCorrespondenceId) {
            StoredProcUpdate("spLoanCorrDelete", _P("intLoanCorrespondenceID", loanCorrespondenceId));
        }


        public List<Loan> FindLoans(string filter, string what, bool findOpenLoansOnly) {
            var mapper = new GenericMapperBuilder<Loan>().build();
            filter = filter.Replace("*", "%");
            var list = StoredProcToList("spLoanFind", mapper, _P("vchrFieldType", what), _P("vchrFieldValue", filter), _P("bitOnlyActiveLoans", findOpenLoansOnly));
            return list;
        }

        public List<LoanReminder> GetLoanReminders(int loanId) {
            var mapper = new GenericMapperBuilder<LoanReminder>().build();
            return StoredProcToList("spLoanReminderList", mapper, _P("intLoanID", loanId));
        }

        public int InsertLoanReminder(LoanReminder r) {
            var retval = ReturnParam("NewLoanReminderID");
            StoredProcUpdate("spLoanReminderInsert",
                _P("intLoanID", r.LoanID),
                _P("dtDate", r.Date),
                _P("bitClosed", r.Closed),
                _P("txtDescription", r.Description),
                retval
            );

            return (int)retval.Value;
        }

        public void UpdateLoanReminder(LoanReminder r) {
            StoredProcUpdate("spLoanReminderUpdate",
                _P("intLoanReminderID", r.LoanReminderID),
                _P("intLoanID", r.LoanID),
                _P("dtDate", r.Date),
                _P("bitClosed", r.Closed),
                _P("txtDescription", r.Description)
            );
        }

        public void DeleteLoanReminder(int loanReminderId) {
            StoredProcUpdate("spLoanReminderDelete", _P("intLoanReminderID", loanReminderId));
        }

        public List<LoanReminderEx> GetRemindersDue(DateTime dateTime) {
            var mapper = new GenericMapperBuilder<LoanReminderEx>().build();
            return StoredProcToList("spLoanReminderDue", mapper, _P("dtDueByDate", dateTime));
        }
    }

    public enum ContactSearchType {
        All, Surname, Institution
    }
    
}
