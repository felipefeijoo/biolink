using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Data.Model;
using BioLink.Client.Utilities;
using System.IO;

namespace BioLink.Client.Extensibility {


    public static class NameFormatter {

        private static string DEFAULT_MATERIAL_NAME_FORMAT = "{AccessionNumber|AccessionNo|RegistrationNumber|RegNo}[; ]{TaxaDesc}";
        private static string DEFAULT_SITE_VISIT_NAME_FORMAT = "{Collector}[ ]{DateStart|DateEnd}";

        public static string FormatMaterialName(Material material) {
            return Format(Config.GetProfile<string>(PluginManager.Instance.User, "Material.MaterialNameFormat", DEFAULT_MATERIAL_NAME_FORMAT), material);
        }

        public static string FormatMaterialName(RDEMaterial material) {
            return Format(Config.GetProfile<string>(PluginManager.Instance.User, "Material.MaterialNameFormat", DEFAULT_MATERIAL_NAME_FORMAT), material);
        }

        public static string FormatSiteVisitName(SiteVisit visit) {
            return Format(Config.GetProfile<string>(PluginManager.Instance.User, "Material.SiteVisitNameFormat", DEFAULT_SITE_VISIT_NAME_FORMAT), visit);
        }

        public static string FormatSiteVisitName(RDESiteVisit visit) {
            return Format(Config.GetProfile<string>(PluginManager.Instance.User, "Material.SiteVisitNameFormat", DEFAULT_SITE_VISIT_NAME_FORMAT), visit);
        }

        public static string Format(string formatString, object obj) {
            var sb = new StringBuilder();

            bool inBrace = false;
            var token = new StringBuilder();

            var reader = new StringReader(formatString);
            int ich;
            while ((ich = reader.Read()) >= 0) {
                char ch = (char)ich;
                int inext = reader.Peek();
                char next = inext >= 0 ? (char)inext : (char)0;

                switch (ch) {
                    case '{':
                        if (next == '{') {
                            reader.Read();
                            sb.Append(ch);
                        } else {
                            token.Clear();
                            inBrace = true;
                        }
                        break;
                    case '}':
                        if (next == '}') {
                            reader.Read();
                            sb.Append(ch);
                        } else {
                            
                            inBrace = false;
                            string separator = "";
                            if (next == '[') {
                                reader.Read(); // read the [
                                separator = reader.ReadUntilNext(']');
                            }
                            sb.Append(SubstituteToken(token.ToString(), obj, separator));
                        }
                        break;
                    default:
                        if (inBrace) {
                            token.Append(ch);
                        } else {
                            sb.Append(ch);
                        }
                        break;
                }
            }

            return sb.ToString();
        }

        private static string SubstituteToken(string token, object source, string separator = "") {

            string @default = "";
            List<string> candidates = new List<string>();            
            if (token.Contains('|')) {
                candidates.AddRange(token.Split('|'));
            } else {
                candidates.Add(token);
            }

            foreach (string candidate in candidates) {
                var p = source.GetType().GetProperty(candidate);
                if (p != null) {
                    var value = p.GetValue(source, null);
                    if (value != null) {
                        String strVal = null;
                        if (typeof(int?).IsAssignableFrom(p.PropertyType) && candidate.Contains("Date")) {
                            strVal = DateUtils.BLDateToStr((int)value);
                        } else {
                            strVal = value.ToString();
                        }

                        if (!string.IsNullOrWhiteSpace(strVal)) {
                            return strVal + separator;
                        }
                    }                    
                }
            }

            return @default;
        }

    }

    public static class StringReaderExtensions {

        public static string ReadUntilNext(this StringReader reader, char endToken) {
            var sb = new StringBuilder();
            int ich;
            while ((ich = reader.Read()) >= 0) {
                char ch = (char)ich;
                if (ch == endToken) {
                    return sb.ToString();
                } else {
                    sb.Append(ch);
                }
            }

            // Didn't find end token, return the rest of the string.
            return sb.ToString();
        }

    }
}
