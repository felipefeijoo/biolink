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

    public abstract class XMLImportObject {

        public XMLImportObject(XmlElement xmlNode) {
            if (xmlNode != null && xmlNode.HasAttribute("ID")) {
                GUID = xmlNode.Attributes["ID"].Value;
            }
        }

        [MappingInfo("GUID", Ignore=true)]
        public string GUID { get; set; }

        public string UpdateClause { get; set; }
        public string InsertClause { get; set; }
        public int ID { get; set; }
    }

    public class XMLImportJournal : XMLImportObject {

        public XMLImportJournal(XmlElement node) : base(node) { }
        
        public string FullName { get; set; }

    }
}
