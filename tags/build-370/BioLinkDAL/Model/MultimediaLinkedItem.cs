using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BioLink.Data.Model {

    public class MultimediaLinkedItem : GUIDObject {

        public int MultimediaLinkID { get; set; }
        public int MultimediaTypeID { get; set; }
        public int CatID { get; set; }
        public int IntraCatID { get; set; }
        public int MultiMediaID { get; set; }
        public string Caption { get; set; }
        public bool UseInReport { get; set; }
        public string CategoryName { get; set; }

        protected override System.Linq.Expressions.Expression<Func<int>> IdentityExpression {
            get { return () => MultimediaLinkID; }
        }

    }
}
