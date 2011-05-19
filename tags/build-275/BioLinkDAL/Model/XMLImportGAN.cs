using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace BioLink.Data.Model {

    public class XMLImportGAN : XMLImportObject {

        public XMLImportGAN(XmlElement node) : base(node) { }

        public int TaxonID { get; set; }

    }

    public class XMLImportGANIncludedSpecies : XMLImportObject {
        public XMLImportGANIncludedSpecies(XmlElement node) : base(node) { }
    }
}
