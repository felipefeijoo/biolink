using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace BioLink.Data.Model {

    public class XMLImportSAN : XMLImportObject {

        public XMLImportSAN(XmlElement node) : base(node) { }

        public int BiotaID { get; set; }
    
    }

    public class XMLImportSANType : XMLImportObject {

        public XMLImportSANType(XmlElement node) : base(node) { }

    }
}
