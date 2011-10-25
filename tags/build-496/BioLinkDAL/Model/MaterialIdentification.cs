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

    public class MaterialIdentification : GUIDObject {

        public int MaterialIdentID { get; set; }
        public int MaterialID { get; set; }
        public string Taxa { get; set; }
        public string IDBy { get; set; }
        public DateTime? IDDate { get; set; }
        public int IDRefID { get; set; }
        public string IDMethod { get; set; }
        public string IDAccuracy { get; set; }
        public string NameQual { get; set; }
        public string IDNotes { get; set; }
        public string IDRefPage { get; set; }
        public int? BasedOnID { get; set; }
        public string RefCode { get; set; }

        public int Changes { get; set; }


        protected override System.Linq.Expressions.Expression<Func<int>> IdentityExpression {
            get { return () => this.MaterialIdentID; }
        }
    }
}
