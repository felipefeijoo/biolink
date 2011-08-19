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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace BioLink.Client.Utilities {

    /// <summary>
    /// Some useful extension methods for XML processing
    /// </summary>
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
            if (element == null) {
                return @default;
            }
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
