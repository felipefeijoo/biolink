using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using BioLink.Client.Utilities;

namespace BioLink.Data {

    internal class XMLExportObject {

        private XmlDocument _xmlDoc;

        public XMLExportObject() {            
            InitXML();
        }

        private void InitXML() {
            _xmlDoc = new XmlDocument();
            var XMLParent = CreateNode(_xmlDoc, "XML");
            MetaRoot = CreateNode(XMLParent, "META");
            var XMLDataNode = CreateNode(XMLParent, "DATA");
            TaxaRoot = CreateNode(XMLDataNode, "TAXA");
            MaterialRoot = CreateNode(XMLDataNode, "MATERIAL");
            ReferenceRoot = CreateNode(XMLDataNode, "REFERENCES");
            MultimediaRoot = CreateNode(XMLDataNode, "MULTIMEDIALIST");
            AssociateRoot = CreateNode(XMLDataNode, "ASSOCIATES");
            JournalRoot = CreateNode(XMLDataNode, "JOURNALS");
            UnplacedTaxaRoot = CreateNode(TaxaRoot, "UNPLACEDTAXA");
        }

        public XmlDocument XMLDocument { 
            get { return _xmlDoc; } 
        }

        public XmlElement CreateNode(XmlNode parent, string tag) {
            var element = _xmlDoc.CreateElement(tag);
            parent.AppendChild(element);
            return element;
        }

        public XmlElement GetElementByGUID(string itemType, string guid) {
            return _xmlDoc.SelectSingleNode(string.Format("//{0}[@ID='{1}']", itemType, guid)) as XmlElement;
        }

        public XmlElement MetaRoot { get; private set; }
        public XmlElement TaxaRoot { get; private set; }
        public XmlElement MaterialRoot { get; private set; }
        public XmlElement ReferenceRoot { get; private set; }
        public XmlElement MultimediaRoot { get; private set; }
        public XmlElement AssociateRoot { get; private set; }
        public XmlElement JournalRoot { get; private set; }
        public XmlElement UnplacedTaxaRoot { get; private set; }


        internal void Save(string filename) {
            _xmlDoc.Save(filename);
        }
    }

    

}
