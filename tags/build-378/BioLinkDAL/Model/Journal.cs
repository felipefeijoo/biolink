using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BioLink.Data.Model {

    public class Journal : OwnedDataObject {

        public int JournalID { get; set; }
        public string AbbrevName { get; set; }
        public string AbbrevName2 { get; set; }
        public string Alias { get; set; }
        public string FullName { get; set; }
        public string Notes { get; set; }

        protected override System.Linq.Expressions.Expression<Func<int>> IdentityExpression {
            get { return () => this.JournalID; }
        }
    }
}
