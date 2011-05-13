using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace BioLink.Data.Model {

    public class XMLImportMultimediaLink : XMLImportObject {

        public XMLImportMultimediaLink(XmlElement node) : base(node) { }

        public string Caption { get; set; }
    }
}
