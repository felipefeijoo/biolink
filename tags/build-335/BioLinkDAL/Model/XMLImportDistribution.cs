using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BioLink.Data.Model {

    public class XMLImportDistribution : XMLImportObject {
        
        public XMLImportDistribution(System.Xml.XmlElement XMLRegionNode) : base(XMLRegionNode) { }

        public int TaxonID { get; set; }
        public int RegionID { get; set; }
    }
}
