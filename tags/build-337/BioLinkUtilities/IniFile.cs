using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;

namespace BioLink.Client.Utilities {

    public class IniFile {

        private static Regex SectionRegex = new Regex(@"^\s*[[](.*)[]]\s*$");
        private static Regex KeyRegex = new Regex("^(.*?)=(.*)$");

        public IniFile() {
            Sections = new Dictionary<string, IniFileSection>();
        }

        public void Clear() {
            Sections = new Dictionary<string, IniFileSection>();
        }

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

        public void Save(string filename) {
            using (var writer = new StreamWriter(filename)) {
                Write(writer);
            }
        }

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

        public Dictionary<string, IniFileSection> Sections;

        public IniFileSection GetSection(string name) {
            if (Sections.ContainsKey(name)) {
                return Sections[name];
            } else {
                var newSection = new IniFileSection { Name = name };
                Sections[name] = newSection;
                return newSection;
            }
        }

        public void SetValue(string section, string key, string value) {
            var s = GetSection(section);
            s.SetKey(key, value);
        }

        public void SetValue(string section, string key, int value) {
            var s = GetSection(section);
            s.SetKey(key, string.Format("{0}", value));
        }

        public void SetValue(string section, string key, double value) {
            var s = GetSection(section);
            s.SetKey(key, string.Format("{0}", value));
        }

        public void SetValue(string section, string key, bool value) {
            SetValue(section, key, string.Format("{0}", value ? "true" : "false")); 
        }

        public string GetValue(string section, string key, string @default = "") {
            var s = GetSection(section);
            return s.GetValue(key, @default);
        }

        public bool GetBool(string section, string key, bool @default = false) {
            var v = GetValue(section, key);
            if (string.IsNullOrWhiteSpace(v)) {
                return @default;
            } else {
                return v.StartsWith("t", StringComparison.CurrentCultureIgnoreCase) || v.StartsWith("Y", StringComparison.CurrentCultureIgnoreCase) || v.StartsWith("1");
            }
        }

        public int GetInt(string section, string key, int @default = 0) {
            var v = GetValue(section, key);
            if (string.IsNullOrWhiteSpace(v)) {
                return @default;
            } else {
                int result = @default;
                if (Int32.TryParse(v, out result)) {
                    return result;
                }
            }
            return @default;
        }

        public double GetFloat(string section, string key, double @default = 0) {
            var v = GetValue(section, key);
            if (string.IsNullOrWhiteSpace(v)) {
                return @default;
            } else {
                double result = @default;
                if (Double.TryParse(v, out result)) {
                    return result;
                }
            }
            return @default;
        }

    }

    public class IniFileSection {

        public IniFileSection() {
            Values = new Dictionary<string, IniFileValue>();
        }

        public void SetKey(string key, string value = "") {

            if (Values.ContainsKey(key)) {
                var oldkey = Values[key];
                oldkey.Value = value;
            } else {
                var newKey = new IniFileValue { Key = key, Value = value };
                Values[key] = newKey;
            }

        }

        public string GetValue(string key, string @default = "") {
            
            var iniKey = Values.FirstOrDefault((kvp) => {
                return kvp.Key.Equals(key, StringComparison.CurrentCultureIgnoreCase);
            });

            if (iniKey.Equals(default(KeyValuePair<string, IniFileValue>))) {
                return @default;
            }

            return iniKey.Value.Value;
        }

        public string Name { get; set; }

        public Dictionary<string, IniFileValue> Values;

    }

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
