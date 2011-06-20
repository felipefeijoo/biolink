using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace BioLink.Data.Model {

    public class XMLImportRegion : XMLImportObject {

        public XMLImportRegion(XmlElement node) : base(node) { }

        public int ParentID { get; set; }
        public string RegionName { get; set; }
    }
}
