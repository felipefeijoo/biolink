using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Data;
using BioLink.Data.Model;
using System.IO;
using BioLink.Client.Utilities;
using System.Text.RegularExpressions;

namespace BioLink.Client.Tools {

    public static class LoanFormGenerator {

        public static string GenerateLoanForm(string template, Loan loan, List<LoanMaterial> material, List<Trait> traits, Contact originator, Contact requestor, Contact receiver) {            
            var sb = new StringBuilder();
            var reader = new StringReader(template);
            int i;
            var rtfbuffer = new StringBuilder();
            while ((i = reader.Read()) >= 0) {
                char? ch = (char)i;
                if (ch == '<') {
                    ch = SkipRTF(reader, rtfbuffer);
                    if (ch == '<') {
                        var placeHolder = ReadPlaceHolder(reader, rtfbuffer);
                        if (!string.IsNullOrEmpty(placeHolder)) {
                            var value = SubstitutePlaceHolder(placeHolder, loan, material, traits, originator, requestor, receiver);
                            if (!string.IsNullOrEmpty(value)) {
                                if (rtfbuffer.Length > 0) {
                                    sb.Append(rtfbuffer.ToString());
                                    rtfbuffer.Clear();
                                }
                                sb.Append(value);
                            }
                        }
                    } else {
                        sb.AppendFormat("<{0}{1}", rtfbuffer, ch);
                    }
                } else {
                    sb.Append(ch);
                }
            }

            return sb.ToString();
        }

        private static int CountTotalSpecimens(IEnumerable<LoanMaterial> material) {
            // Count the total number of specimens currently attached to this loan. Sometimes specimen counts are represented as "1 x adult" etc,
            // so we be a bit liberal in our interpretation.
            var specimenCountRegex = new Regex(@"[^\d]*(\d+)[^\d]*");
            var dblTotal = material.Sum(loanMaterial => {
                decimal subtotal = 0;
                var matcher = specimenCountRegex.Match(loanMaterial.NumSpecimens);
                while (matcher.Success) {
                    decimal d;
                    if (Decimal.TryParse(matcher.Groups[1].Value, out d)) {
                        subtotal += d;
                    }
                    matcher = matcher.NextMatch();
                }

                return subtotal;
            });

            return (int) dblTotal;
        }

        private static string ReadPlaceHolder(TextReader reader, StringBuilder rtfbuffer) {
            var sb = new StringBuilder();
            char? ch;

            while ((ch = SkipRTF(reader, rtfbuffer)).HasValue) {
                if (ch == '>') {
                    ch = SkipRTF(reader, rtfbuffer);
                    if (ch.HasValue && ch == '>') {
                        break;
                    } 
                    sb.AppendFormat("<{0}", ch);                    
                } else if (ch != '\n' && ch != '\r') {
                    sb.Append(ch);
                } 
            }

            return sb.ToString();
        }

        private static char? SkipRTF(TextReader reader, StringBuilder rtfbuffer) {
            int i;
            var incontrolword = false;
            while ((i = reader.Read()) >= 0) {
                var ch = (char)i;
                if (ch == '}' || ch == '{') {
                    incontrolword = false;
                    rtfbuffer.Append(ch);
                    continue;
                }

                if (ch == '\\') {
                    rtfbuffer.Append(ch);
                    incontrolword = true;
                    continue;
                }

                if (incontrolword) {
                    rtfbuffer.Append(ch);
                    if (ch == ' ') {
                        incontrolword = false;
                    }
                    continue;
                }

                return ch;
            }
            return null;
        }

        private static string SubstitutePlaceHolder(string key, Loan loan, IEnumerable<LoanMaterial> material, IEnumerable<Trait> traits, Contact originator, Contact requestor, Contact receiver) {
            var sb = new StringBuilder();

            // Special placeholders

            switch (key.ToLower()) {
                case "totalspecimencount":
                    return CountTotalSpecimens(material) + "";
            }

            if (key.Contains('(')) {
                // group...
                var collectionName = key.Substring(0, key.IndexOf('('));
                var fieldstr = key.Substring(key.IndexOf('(') + 1);
                var fields = fieldstr.Substring(0, fieldstr.Length - 1).Split(',');

                List<object> collection = null;
                if (collectionName.Equals("material", StringComparison.CurrentCultureIgnoreCase)) {
                    collection = new List<object>(material);
                } else if (collectionName.Equals("trait", StringComparison.CurrentCultureIgnoreCase)) {
                    collection = new List<object>(traits);
                }

                if (collection != null) {
                    foreach (Object obj in collection) {
                        int i = 0;
                        foreach (string field in fields) {
                            var value = GetPropertyValue(obj, field);
                            if (!string.IsNullOrEmpty(value)) {
                                sb.Append(RTFUtils.EscapeUnicode(value));
                            }
                            sb.Append(++i < fields.Length ? ", " : ". \\par\\pard ");
                        }
                    }
                }
            } else if (key.Contains(".")) {
                // is a property of a linked object (currently just the three different contacts attached to the loan)
                // e.g. receiver.EMail
                var bits = key.Split('.');

                if (bits.Length > 0) {
                    Object srcObject = null;
                    switch (bits[0].ToLower()) {
                        case "receiver":
                            srcObject = receiver;
                            break;
                        case "requestor":
                            srcObject = requestor;
                            break;
                        case "originator":
                            srcObject = originator;
                            break;
                    }

                    if (srcObject != null) {
                        var value = GetPropertyValue(srcObject, bits[1]);
                        if (!string.IsNullOrEmpty(value)) {
                            sb.Append(RTFUtils.EscapeUnicode(value));
                        }
                    }
                }

            } else {
                // single value from the Loan model...
                var value = GetPropertyValue(loan, key);
                if (!string.IsNullOrEmpty(value)) {
                    sb.Append(RTFUtils.EscapeUnicode(value));
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// This function attempts to extract the value of the named property from an object.
        /// It first tries the name as it is passed in. If no such property can be found, it then
        /// checks to see if the property passed starts with a known database column type prefix (e.g. vchr), and if it does,
        /// it strips that off, and tries again. In this manner vchrName and Name will both retrieve the 'Name' property (if it exists) of the object
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        private static string GetPropertyValue(object obj, string propertyName) {

            var p = obj.GetType().GetProperty(propertyName);
            if (p == null) {
                var prefixes = MapperBase.KNOWN_TYPE_PREFIXES.Split(',');
                foreach (string prefix in prefixes) {
                    if (propertyName.StartsWith(prefix, StringComparison.CurrentCultureIgnoreCase)) {
                        propertyName = propertyName.Substring(prefix.Length);
                        p = obj.GetType().GetProperty(propertyName);
                        break;
                    }
                }
            }
            if (p != null) {
                var val = p.GetValue(obj, null);
                if (val != null) {
                    if (val is DateTime) {
                        return string.Format("{0:d}", val);
                    }
                    return val.ToString();
                }
            }

            return null;
        }

    }

}
