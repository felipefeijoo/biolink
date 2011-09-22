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
using System.Xml;

namespace BioLink.Data.Model {

    public class XMLImportSite : XMLImportObject {

        public XMLImportSite(XmlElement node) : base(node) { }

        public int LocalityType { get; set; }
        public string Locality { get; set; }
        public double? Y1 { get; set; }
        public double? X1 { get; set; }
    }
}
