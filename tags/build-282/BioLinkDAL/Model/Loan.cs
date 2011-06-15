using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BioLink.Data.Model {

    public class Loan : GUIDObject {

        public int LoanID { get; set; }
        public string LoanNumber { get; set; }
        public int RequestorID { get; set; }
        public int ReceiverID { get; set; }	
        public int OriginatorID { get; set; }	
        public DateTime? DateInitiated { get; set; }	
        public DateTime? DateDue { get; set; }
        public string MethodOfTransfer { get; set; }
        public string PermitNumber { get; set; }
        public string TypeOfReturn { get; set; }
        public string Restrictions { get; set; }
        public DateTime? DateClosed { get; set; }
        public bool? LoanClosed { get; set; }

        public string RequestorTitle { get; set; }
        public string RequestorGivenName { get; set; }
        public string RequestorName { get; set; }
        public string ReceiverTitle	{ get; set; }
        public string ReceiverGivenName	{ get; set; }
        public string ReceiverName { get; set; }
        public string OriginatorTitle { get; set; }
        public string OriginatorGivenName { get; set; }
        public string OriginatorName { get; set; }


        protected override System.Linq.Expressions.Expression<Func<int>> IdentityExpression {
            get { return () => this.LoanID; }
        }
    }
}
