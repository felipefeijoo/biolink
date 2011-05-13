using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace BioLink.Data.Model {

    public class XMLImportCommonName : XMLImportObject {

        public XMLImportCommonName(XmlElement node) : base(node) { }

        public string CommonName { get; set; }
    }
}
