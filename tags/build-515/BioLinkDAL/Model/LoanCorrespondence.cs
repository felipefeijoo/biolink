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

    public class LoanCorrespondence : GUIDObject {

        public int LoanCorrespondenceID { get; set; }
        public int LoanID { get; set; }
        public string Type { get; set; }
        public DateTime? Date { get; set; }
        public int SenderID { get; set; }
        public int RecipientID { get; set; }
        public string Description { get; set; }
        public string RefNo { get; set; }

        public string SenderTitle { get; set; }
        public string SenderGivenName { get; set; }
        public string SenderName { get; set; }

        public string RecipientTitle { get; set; }
        public string RecipientGivenName { get; set; }
        public string RecipientName { get; set; }

        protected override System.Linq.Expressions.Expression<Func<int>> IdentityExpression {
            get { return () => LoanCorrespondenceID; }
        }

    }
}
