using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BioLink.Data.Model {

    /// <summary>
    /// Represents the data held about individual users of biolink. Used by the security subsystem...
    /// </summary>
    public class BiolinkUser : BioLinkDataObject {
        
        [MappingInfo("User Name")]
        public string UserName { get; set; }
        [MappingInfo("Group Name")]
        public string GroupName { get; set; }
        [MappingInfo("Group ID")]
        public int GroupID { get; set; }
        [MappingInfo("Full Name")]
        public string FullName { get; set; }
        public string Description { get; set; }
        public string Notes { get; set; }
        public bool CanCreateUsers { get; set; }
        public string Password { get; set; }

        protected override System.Linq.Expressions.Expression<Func<int>> IdentityExpression {
            get { return () => 0; }
        }

    }
}
