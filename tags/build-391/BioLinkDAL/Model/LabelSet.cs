using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BioLink.Data.Model {

    public class LabelSet : BioLinkDataObject {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Delimited { get; set; }

        protected override System.Linq.Expressions.Expression<Func<int>> IdentityExpression {
            get { return () => ID; }
        }
    }
}
