using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace BioLink.Data.Model {
    public class XMLImportReference : XMLImportObject {

        public XMLImportReference(XmlElement node) : base(node) { }

        public string Code { get; set; }
        public string Author { get; set; }
        public string Year { get; set; }        
    }
}
