using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BioLink.Data.Model {

    public class LoanCorrespondence : GUIDObject {

        public int LoanCorrespondenceID { get; set; }
        public int LoanID { get; set; }
        public string Type { get; set; }
        public DateTime? Date { get; set; }
        public int SenderID { get; set; }
        public int RecipientID { get; set; }
        public string Description { get; set; }
        public string RefNo { get; set; }

        public string SenderTitle { get; set; }
        public string SenderGivenName { get; set; }
        public string SenderName { get; set; }

        public string RecipientTitle { get; set; }
        public string RecipientGivenName { get; set; }
        public string RecipientName { get; set; }

        protected override System.Linq.Expressions.Expression<Func<int>> IdentityExpression {
            get { return () => LoanCorrespondenceID; }
        }

    }
}
