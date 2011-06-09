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
    }

    public enum ContactSearchType {
        All, Surname, Institution
    }
    
}
