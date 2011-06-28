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
