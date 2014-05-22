using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace BioLink.Client.Utilities {

    public static class StringUtils {

        public static String RemoveAll(String source, params char[] chars) {
            var sb = new StringBuilder();
            var list = new List<char>(chars);

            foreach (char ch in source) {
                if (!list.Contains(ch)) {
                    sb.Append(ch);
                }
            }

            return sb.ToString();
        }

        public static string SubstitutePlaceholders(string text, Dictionary<String, String> values) {
            var sb = new StringBuilder();
            bool inBrace = false;
            var token = new StringBuilder();
            var reader = new StringReader(text);
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
                            // At this point 'token' will contain a key to lookup in the map of values
                            var key = token.ToString();
                            if (values.ContainsKey(key)) {
                                sb.Append(values[key]);
                            } else {
                                sb.Append("?" + key + "?");
                            }                            
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
    }
}
