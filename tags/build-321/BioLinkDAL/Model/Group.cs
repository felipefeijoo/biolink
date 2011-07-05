using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BioLink.Data.Model {

    public class Group : BioLinkDataObject {

        [MappingInfo("Group ID")]
        public int GroupID { get; set; }

        [MappingInfo("Group Name")]
        public string GroupName { get; set; }

        protected override System.Linq.Expressions.Expression<Func<int>> IdentityExpression {
            get { return () => this.GroupID; }
        }

    }
}
