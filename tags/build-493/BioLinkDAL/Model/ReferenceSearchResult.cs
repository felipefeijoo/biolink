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
