using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BioLink.Data.Model {

    public class RefLink : GUIDObject {
       
        public int RefLinkID { get; set; }        
        public int RefID { get; set; }
        public int? IntraCatID { get; set; }
        public string RefPage { get; set; }
        public string RefQual { get; set; }
        public int? Order { get; set; }
        public bool? UseInReport { get; set; }
        public string RefLinkType { get; set; }
        public string RefCode { get; set; }
        public string FullRTF { get; set; }
        public int? StartPage { get; set; }
        public int? EndPage { get; set; }

        protected override System.Linq.Expressions.Expression<Func<int>> IdentityExpression {
            get { return () => this.RefLinkID; }
        }
    }
}
