using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace BioLink.Data.Model {

    public class XMLImportTrap : XMLImportObject {

        public XMLImportTrap(XmlElement node) : base(node) { }

        public string TrapName { get; set; }
        public int SiteID { get; set; }
    }
}
