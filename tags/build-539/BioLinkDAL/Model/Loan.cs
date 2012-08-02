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

    public class Loan : OwnedDataObject {

        public int LoanID { get; set; }
        public string LoanNumber { get; set; }
        public int RequestorID { get; set; }
        public int ReceiverID { get; set; }	
        public int OriginatorID { get; set; }	
        public DateTime? DateInitiated { get; set; }	
        public DateTime? DateDue { get; set; }
        public string MethodOfTransfer { get; set; }
        public string PermitNumber { get; set; }
        public string TypeOfReturn { get; set; }
        public string Restrictions { get; set; }
        public DateTime? DateClosed { get; set; }
        public bool? LoanClosed { get; set; }

        public string RequestorTitle { get; set; }
        public string RequestorGivenName { get; set; }
        public string RequestorName { get; set; }
        public string RequestorInstitution { get; set; }
        public string RequestorPostalAddress { get; set; }
        public string RequestorStreetAddress { get; set; }
        public string ReceiverTitle	{ get; set; }
        public string ReceiverGivenName	{ get; set; }
        public string ReceiverName { get; set; }
        public string ReceiverInstitution { get; set; }
        public string ReceiverPostalAddress	{ get; set; }
        public string ReceiverStreetAddress { get; set; }
        public string OriginatorTitle { get; set; }
        public string OriginatorGivenName { get; set; }
        public string OriginatorName { get; set; }
		public string OriginatorPostalAddress { get; set; }
        public string OriginatorStreetAddress { get; set; }



        protected override System.Linq.Expressions.Expression<Func<int>> IdentityExpression {
            get { return () => this.LoanID; }
        }
    }
}
