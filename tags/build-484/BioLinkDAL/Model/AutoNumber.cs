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
