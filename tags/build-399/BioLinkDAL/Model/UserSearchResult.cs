using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BioLink.Data.Model {

    public class UserSearchResult : BioLinkDataObject {

        public int UserID { get; set; }
        public int GroupID { get; set; }
        [MappingInfo("User Name")]
        public string Username { get; set; }
        public string Group { get; set; }
        [MappingInfo("Full Name")]
        public string FullName { get; set; }
        public string Description { get; set; }
        public bool CanCreateUsers { get; set; }

        protected override System.Linq.Expressions.Expression<Func<int>> IdentityExpression {
            get { return () => this.UserID; }
        }

    }
}
