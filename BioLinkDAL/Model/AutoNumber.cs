using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BioLink.Data.Model {

    public class AutoNumber : GUIDObject {

        public int AutoNumberCatID { get; set; }
        public string Category { get; set; }
        public string Name { get; set; }
        public string Prefix { get; set; }
        public string Postfix { get; set; }
        public int NumLeadingZeros { get; set; }
        public int LastNumber { get; set; }
        public bool Locked { get; set; }
        public bool EnsureUnique { get; set; }

        protected override System.Linq.Expressions.Expression<Func<int>> IdentityExpression {
            get { return () => this.AutoNumberCatID; }
        }
    }

    public class NewAutoNumber : AutoNumber {
        public int NewNumber { get; set; }

        public string FormattedNumber {

            get {
                string format = null;
                if (NumLeadingZeros > 0) {
                    format = string.Format("{{0}}{{1:{0}}}{{2}}", new string('0', NumLeadingZeros));
                } else {
                    format = "{0}{1}{2}";
                }
                return string.Format(format, Prefix, NewNumber, Postfix);
            }

        }
    }
    
}
