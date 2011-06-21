using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BioLink.Data.Model {
    public class LoanReminder : GUIDObject {

        public int LoanReminderID { get; set; }
        public int LoanID { get; set; }
        public DateTime? Date { get; set; }
        public bool? Closed { get; set; }
        public string Description { get; set; }

        protected override System.Linq.Expressions.Expression<Func<int>> IdentityExpression {
            get { return () => LoanReminderID; }
        }

    }
}
