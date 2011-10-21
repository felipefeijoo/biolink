/*******************************************************************************
 * Copyright (C) 2011 Atlas of Living Australia
 * All Rights Reserved.
 * 
 * The contents of this file are subject to the Mozilla Public
 * License Version 1.1 (the "License"); you may not use this file
 * except in compliance with the License. You may obtain a copy of
 * the License at http://www.mozilla.org/MPL/
 * 
 * Software distributed under the License is distributed on an "AS
 * IS" basis, WITHOUT WARRANTY OF ANY KIND, either express or
 * implied. See the License for the specific language governing
 * rights and limitations under the License.
 ******************************************************************************/
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
