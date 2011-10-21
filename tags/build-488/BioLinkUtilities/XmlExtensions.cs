/*******************************************************************************
 * Copyright (C) 2011 Atlas of Living Australia
 * All Rights Reserved.
 * 
 * The contents of this file are subject to the Mozilla Public
 * License Version 1.1 (the "License"); you may not use this file
 * except in compliance with the License. You may obtain a copy of
 * the License at http://www.mozilla.org/MPL/
 * 
 * Software distributed under the License is distributed on an "AS
 * IS" basis, WITHOUT WARRANTY OF ANY KIND, either express or
 * implied. See the License for the specific language governing
 * rights and limitations under the License.
 ******************************************************************************/

using System.Linq;
using System.Xml;

namespace BioLink.Client.Utilities {
    /// <summary>
    /// Some useful extension methods for XML processing
    /// </summary>
    public static class XmlExtensions {
        public static XmlElement AddNamedValue(this XmlElement parent, string name, string value) {
            if (parent.OwnerDocument != null) {
                XmlElement elem = parent.OwnerDocument.CreateElement(name);
                elem.InnerText = value;
                parent.AppendChild(elem);
                return elem;
            }
            return null;
        }

        public static XmlAttribute AddAttribute(this XmlElement element, string name, string value) {
            XmlAttribute attr = element.Attributes[name];
            if (attr == null) {
                if (element.OwnerDocument != null) {
                    attr = element.OwnerDocument.CreateAttribute(name);
                    element.Attributes.Append(attr);
                }
            }
            if (attr != null) {
                attr.Value = value;                
            }
            return attr;
        }

        public static string GetAttributeValue(this XmlElement element, string attrName, string @default = "") {
            if (element == null) {
                return @default;
            }
            if (element.HasAttribute(attrName)) {
                return element.Attributes[attrName].Value;
            }

            return @default;
        }

        public static string GetCData(this XmlElement element) {
            return element.ChildNodes.OfType<XmlCDataSection>().Select(child => (child as XmlCDataSection).Value).FirstOrDefault();
        }
    }
}