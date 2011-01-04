using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BioLink.Data.Model {

    public class PhraseCategory : OwnedDataObject {

        public int CategoryID { get; set; }

        public string Category { get; set; }

        public bool Fixed { get; set; }

        protected override System.Linq.Expressions.Expression<Func<int>> IdentityExpression {
            get { return () => this.CategoryID; }
        }

    }

    public class Phrase : OwnedDataObject {

        public int PhraseID { get; set; }

        public int PhraseCatID { get; set; }

        public string PhraseText { get; set; }

        protected override System.Linq.Expressions.Expression<Func<int>> IdentityExpression {
            get { return () => this.PhraseID; }
        }
    }

}
