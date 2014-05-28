using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace BioLink.Client.Utilities {

    public static class StringUtils {

        public static byte[] GetStringBytes(String str) {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }

        public static String BytesAsString(byte[] bytes) {
            char[] chars = new char[bytes.Length / sizeof(char)];
            System.Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
            return new string(chars);
        }

        public static String ToBase64String(String data) {
            return Convert.ToBase64String(GetStringBytes(data));
        }

        public static String FromBase64String(String base64) {
            return BytesAsString(Convert.FromBase64String(base64));
        }

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
