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

namespace BioLink.Data  {

    /// <summary>
    /// Simple transfer object for values that have a label and an underlying code or value
    /// </summary>
    public class CodeLabelPair {

        public CodeLabelPair(string code, string label) {
            this.Code = code;
            this.Label = label;
        }

        public string Code { get; private set; }
        public string Label { get; private set; }

        public override string  ToString() {
            return Label;
        }

    }
}
