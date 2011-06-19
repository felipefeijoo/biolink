using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace BioLink.Client.Utilities {

    public static class XmlExtensions {

        public static XmlElement AddNamedValue(this XmlElement parent, string name, string value) {
            var elem = parent.OwnerDocument.CreateElement(name);
            elem.InnerText = value;
            parent.AppendChild(elem);
            return elem;
        }

        public static XmlAttribute AddAttribute(this XmlElement element, string name, string value) {
            var attr = element.Attributes[name];
            if (attr == null) {
                attr = element.OwnerDocument.CreateAttribute(name);
                element.Attributes.Append(attr);
            }
            attr.Value = value;
            return attr;
        }

        public static string GetAttributeValue(this XmlElement element, string attrName, string @default = "") {
            if (element.HasAttribute(attrName)) {
                return element.Attributes[attrName].Value;
            }

            return @default;
        }

        public static string GetCData(this XmlElement element) {
            foreach (XmlNode child in element.ChildNodes) {
                if (child is XmlCDataSection) {
                    return (child as XmlCDataSection).Value;
                }
            }
            return null;
        }

    }
}
