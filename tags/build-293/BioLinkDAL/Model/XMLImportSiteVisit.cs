using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace BioLink.Data.Model {

    public class XMLImportSiteVisit : XMLImportObject {

        public XMLImportSiteVisit(XmlElement node) : base(node) { }

        public int SiteID { get; set; }
        public string Collector { get; set; }
        public int? StartDate { get; set; }

    }
}
