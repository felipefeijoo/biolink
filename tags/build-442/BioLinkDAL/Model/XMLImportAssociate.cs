using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace BioLink.Data.Model {
    public class XMLImportAssociate : XMLImportObject {

        public XMLImportAssociate(XmlElement node) : base(node) { }

        public int FromCatID { get; set; }
        public int FromIntraCatID { get; set; }
        public int ToCatID { get; set; }
        public int ToIntraCatID { get; set; }
        public string AssocDescription { get; set; }
        public string RelationFromTo { get; set; }
        public string RelationToFrom { get; set; }

    }
}
