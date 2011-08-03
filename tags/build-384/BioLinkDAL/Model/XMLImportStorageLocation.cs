using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace BioLink.Data.Model {

    public class XMLImportStorageLocation : XMLImportObject {

        public XMLImportStorageLocation(XmlElement node) : base(node) { }
        public int TaxonID { get; set; }
        public int LocationID { get; set; }

    }
}
