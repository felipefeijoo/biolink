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
using System.Text.RegularExpressions;
using System.IO;

namespace BioLink.Client.Utilities {

    /// <summary>
    /// <para>
    /// Provides a convenient way to read and write legacy "INI" files (circa Windows 3.1 etc)
    /// </para>
    /// <para>
    /// Ini files are line based text files, broken into sections by [section headers]
    /// Key value pairs are described as Key=Value
    /// </para>
    /// </summary>
    public class IniFile {

        private static readonly Regex SectionRegex = new Regex(@"^\s*[[](.*)[]]\s*$");
        private static readonly Regex KeyRegex = new Regex("^(.*?)=(.*)$");

        // An ini file is a keyed collection of sections, and a section is a collection of key value pairs
        public Dictionary<string, IniFileSection> Sections;

        /// <summary>
        /// Creates a new empty ini file
        /// </summary>
        public IniFile() {
            Sections = new Dictionary<string, IniFileSection>();
        }

        /// <summary>
        /// Clear the contents of this Ini file
        /// </summary>
        public void Clear() {
            Sections = new Dictionary<string, IniFileSection>();
        }

        /// <summary>
        /// Attempts to load the contents of filename as an IniFile
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="clearBefore"></param>
        public void Load(string filename, bool clearBefore = true) {

            if (clearBefore) {
                Clear();
            }

            var section = GetSection("(General)");

            using (var reader = new StreamReader(filename)) {

                string currentLine = reader.ReadLine();
                while (currentLine != null) {

                    var matcher = SectionRegex.Match(currentLine);
                    if (matcher.Success) {
                        section = GetSection(matcher.Groups[1].Value);
                    } else {
                        matcher = KeyRegex.Match(currentLine);
                        if (matcher.Success) {
                            section.SetKey(matcher.Groups[1].Value, matcher.Groups[2].Value);
                        }
                    }
                    currentLine = reader.ReadLine();
                }
            }

        }

        /// <summary>
        /// Dumps the contents of this ini file to disk
        /// </summary>
        /// <param name="filename"></param>
        public void Save(string filename) {
            using (var writer = new StreamWriter(filename)) {
                Write(writer);
            }
        }

        /// <summary>
        /// Writes the contents of this ini file to the supplied writer
        /// </summary>
        /// <param name="writer"></param>
        public void Write(TextWriter writer) {

            foreach (IniFileSection section in Sections.Values) {
                if (section.Name == "(General)" && section.Values.Count == 0) {
                    // ignore
                } else {
                    writer.WriteLine(string.Format("[{0}]", section.Name));
                    foreach (IniFileValue key in section.Values.Values) {
                        writer.WriteLine(string.Format("{0}={1}", key.Key, key.Value));
                    }
                    writer.WriteLine();
                }
            }

        }

        /// <summary>
        /// Retrieve a section object by name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public IniFileSection GetSection(string name) {
            if (Sections.ContainsKey(name)) {
                return Sections[name];
            }
            var newSection = new IniFileSection { Name = name };
            Sections[name] = newSection;
            return newSection;
        }

        /// <summary>
        /// Set a single value in a section (string value)
        /// </summary>
        /// <param name="section"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void SetValue(string section, string key, string value) {
            var s = GetSection(section);
            s.SetKey(key, value);
        }

        /// <summary>
        /// Set an int value into a section
        /// </summary>
        /// <param name="section"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void SetValue(string section, string key, int value) {
            var s = GetSection(section);
            s.SetKey(key, string.Format("{0}", value));
        }

        /// <summary>
        /// Set a double value
        /// </summary>
        /// <param name="section"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void SetValue(string section, string key, double value) {
            var s = GetSection(section);
            s.SetKey(key, string.Format("{0}", value));
        }

        /// <summary>
        /// Set a bool value
        /// </summary>
        /// <param name="section"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void SetValue(string section, string key, bool value) {
            SetValue(section, key, string.Format("{0}", value ? "true" : "false")); 
        }

        /// <summary>
        ///  Get a string value (all values in an ini file are ultimately strings
        /// </summary>
        /// <param name="section"></param>
        /// <param name="key"></param>
        /// <param name="default"></param>
        /// <returns></returns>
        public string GetValue(string section, string key, string @default = "") {
            var s = GetSection(section);
            return s.GetValue(key, @default);
        }

        /// <summary>
        /// Get a boolean value
        /// </summary>
        /// <param name="section"></param>
        /// <param name="key"></param>
        /// <param name="default"></param>
        /// <returns></returns>
        public bool GetBool(string section, string key, bool @default = false) {
            var v = GetValue(section, key);
            if (string.IsNullOrWhiteSpace(v)) {
                return @default;
            }
            return v.StartsWith("t", StringComparison.CurrentCultureIgnoreCase) || v.StartsWith("Y", StringComparison.CurrentCultureIgnoreCase) || v.StartsWith("1");
        }

        /// <summary>
        /// Get an int value
        /// </summary>
        /// <param name="section"></param>
        /// <param name="key"></param>
        /// <param name="default"></param>
        /// <returns></returns>
        public int GetInt(string section, string key, int @default = 0) {
            var v = GetValue(section, key);
            if (string.IsNullOrWhiteSpace(v)) {
                return @default;
            }
            int result;
            if (Int32.TryParse(v, out result)) {
                return result;
            }
            return @default;
        }

        /// <summary>
        /// Get a float value
        /// </summary>
        /// <param name="section"></param>
        /// <param name="key"></param>
        /// <param name="default"></param>
        /// <returns></returns>
        public double GetFloat(string section, string key, double @default = 0) {
            var v = GetValue(section, key);
            if (string.IsNullOrWhiteSpace(v)) {
                return @default;
            }
            double result;
            if (Double.TryParse(v, out result)) {
                return result;
            }
            return @default;
        }

    }

    /// <summary>
    /// Ini files are basically a keyed collection of sections, and a section is a collection of Key Value pairs. Order of key value pairs is not significant
    /// </summary>
    public class IniFileSection {

        // Creates a new sections
        public IniFileSection() {
            Values = new Dictionary<string, IniFileValue>();
        }

        /// <summary>
        /// Set a value in the section
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void SetKey(string key, string value = "") {

            if (Values.ContainsKey(key)) {
                var oldkey = Values[key];
                oldkey.Value = value;
            } else {
                var newKey = new IniFileValue { Key = key, Value = value };
                Values[key] = newKey;
            }

        }

        /// <summary>
        /// Retrieve a value
        /// </summary>
        /// <param name="key"></param>
        /// <param name="default"></param>
        /// <returns></returns>
        public string GetValue(string key, string @default = "") {
            
            var iniKey = Values.FirstOrDefault(kvp => kvp.Key.Equals(key, StringComparison.CurrentCultureIgnoreCase));

            if (iniKey.Equals(default(KeyValuePair<string, IniFileValue>))) {
                return @default;
            }

            return iniKey.Value.Value;
        }

        /// <summary>
        /// The name of the section
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The collection of Key Value pairs
        /// </summary>
        public Dictionary<string, IniFileValue> Values;

    }

    /// <summary>
    /// Individual data elements within an Ini file are basically key value pairs
    /// </summary>
    public class IniFileValue {

        public IniFileValue() {
            Comments = new List<string>();
        }

        public string Key { get; set; }
        public string Value { get; set; }

        public void AddComment(string comment) {
            Comments.Add(comment);
        }

        public List<string> Comments { get; private set; }
    }
}
