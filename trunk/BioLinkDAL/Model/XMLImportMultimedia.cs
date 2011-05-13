using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace BioLink.Data.Model {

    public class XMLImportMultimedia : XMLImportObject {

        public XMLImportMultimedia(XmlElement node) : base(node) { }

        public byte[] ImageData { get; set; }
    }
}
