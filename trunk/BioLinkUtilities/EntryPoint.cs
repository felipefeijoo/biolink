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

namespace BioLink.Client.Utilities {

    /// <summary>
    /// <para>
    /// Helper class to make dealing with Legacy BioLink 'EntryPoints' easier
    ///</para>
    /// <para>
    /// Entry points are in the format <Name>?<optionkey>=<optionvalue>&...
    /// </para>
    /// <para>
    /// Entry points where used by the old BioLink as a form of 'inter-plugin' communication, and crude dispatch mechanism, very similar to a URI.
    /// </para>
    /// </summary>
    public class EntryPoint {

        private String _name;
        private List<KeyValuePair<string, string>> _parameters;

        public EntryPoint(string name) {
            _name = name;
            _parameters = new List<KeyValuePair<string, string>>();
        }

        /// <summary>
        /// Parses a stringified entry point into an EntryPoint instance that can be queried
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public static EntryPoint Parse(String uri) {

            var ep = new EntryPoint("");

            if (uri.Contains("?")) {
                ep._name = uri.Substring(0, uri.IndexOf("?"));
                string[] parambits = uri.Substring(uri.IndexOf("?") + 1).Split('&');
                foreach (String param in parambits) {
                    if (param.Contains("=")) {
                        string key = param.Substring(0, param.IndexOf("="));
                        string value = param.Substring(param.IndexOf("=") + 1);
                        KeyValuePair<string, string> kvp = new KeyValuePair<string, string>(key, value);
                        ep._parameters.Add(kvp);
                    } else {
                        KeyValuePair<string, string> kvp = new KeyValuePair<string, string>(param, param);
                        ep._parameters.Add(kvp);
                    }
                }
            } else {
                ep._name = uri;
            }

            return ep;
        }

        public void AddParameter(string name, string value) {
            _parameters.Add(new KeyValuePair<string,string>(name, value));
        }

        public bool HasParameter(string name) {
            foreach (KeyValuePair<string, string> kvp in _parameters) {
                if (kvp.Key.Equals(name)) {
                    return true;
                }
            }

            return false;
        }

        public String Name {
            get { return _name; }
        }

        public List<KeyValuePair<string, string>> Parameters {
            get { return _parameters; }
        }

        public String this[String param, String def] {
            get {
                foreach (KeyValuePair<string, string> p in _parameters) {
                    if (p.Key.Equals(param)) {
                        return p.Value;
                    }
                }

                return def;
            }
        }

        public String this[String param] {
            get {
                foreach (KeyValuePair<string, string> p in _parameters) {
                    if (p.Key.Equals(param)) {
                        return p.Value;
                    }
                }

                return "";
            }
        }

        public override string ToString() {
            var paramList = new StringBuilder();
            foreach (KeyValuePair<string, string> pair in _parameters) {
                paramList.AppendFormat("{0}={1}&", pair.Key, pair.Value);
            }
            paramList.Remove(paramList.Length - 1, 1);
            return string.Format("{0}?{1}", _name, paramList.ToString());
        }

    }
}
