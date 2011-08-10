using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BioLink.Data.Model {

    public class Reference : OwnedDataObject {

        public int RefID { get; set; }
        public string RefCode { get; set; }
        public string Author { get; set; }
        public string Title { get; set; }
        public string BookTitle { get; set; }
        public string Editor { get; set; }
        public string RefType { get; set; }
        public string YearOfPub { get; set; }
        public string ActualDate { get; set; }
        public int? JournalID { get; set; }
        public string PartNo { get; set; }
        public string Series { get; set; }
        public string Publisher { get; set; }
        public string Place { get; set; }
        public string Volume { get; set; }
        public string Pages { get; set; }
        public string TotalPages { get; set; }
        public string Possess { get; set; }
        public string Source { get; set; }
        public string Edition { get; set; }
        public string ISBN { get; set; }
        public string ISSN { get; set; }
        public string Abstract { get; set; }
        public string FullText { get; set; }
        public string FullRTF { get; set; }
        public int? StartPage { get; set; }
        public int? EndPage { get; set; }
        public string JournalName { get; set; }

        protected override System.Linq.Expressions.Expression<Func<int>> IdentityExpression {
            get { return () => this.RefID; }
        }

    }

    public class ReferenceImport : Reference {

        public string JournalAbbrevName { get; set; }
        public string JournalFullName { get; set; }
        public string DateQual { get; set; }

    }
}
