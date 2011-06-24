using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace BioLink.Data.Model {

    public abstract class XMLImportObject {

        public XMLImportObject(XmlElement xmlNode) {
            if (xmlNode != null && xmlNode.HasAttribute("ID")) {
                GUID = xmlNode.Attributes["ID"].Value;
            }
        }

        [MappingInfo("GUID", Ignore=true)]
        public string GUID { get; set; }

        public string UpdateClause { get; set; }
        public string InsertClause { get; set; }
        public int ID { get; set; }
    }

    public class XMLImportJournal : XMLImportObject {

        public XMLImportJournal(XmlElement node) : base(node) { }
        
        public string FullName { get; set; }

    }
}
