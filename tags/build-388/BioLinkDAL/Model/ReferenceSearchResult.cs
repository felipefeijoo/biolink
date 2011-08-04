using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BioLink.Data.Model {

    /// <summary>
    /// 
    /// The reference search result actually doesn't have a GUID, but it needs to extend from because
    /// downstream classes expect that all data objects have guids...
    /// 
    /// </summary>
    public class ReferenceSearchResult : GUIDObject {

        public int RefID { get; set; }
        public string RefCode { get; set; }
        public string Author { get; set; }
        public string YearOfPub { get; set; }
        public string Title { get; set; }
        public string RefType { get; set; }
        public string RefRTF { get; set; }

        protected override System.Linq.Expressions.Expression<Func<int>> IdentityExpression {
            get { return () => this.RefID; }
        }
    }
}
