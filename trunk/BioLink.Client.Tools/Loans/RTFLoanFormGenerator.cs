using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Client.Extensibility;
using BioLink.Client.Utilities;
using BioLink.Data;
using BioLink.Data.Model;
using System.IO;
using System.Text.RegularExpressions;


namespace BioLink.Client.Tools {

    public class RTFLoanFormGenerator : AbstractLoanFormGenerator {

        protected override string NewLineSequence {
            get { return "\\par\\pard "; }
        }

        public override FileInfo GenerateLoanForm(Multimedia template, Loan loan, List<LoanMaterial> material, List<Trait> traits, Contact originator, Contact requestor, Contact receiver) {

            var user = PluginManager.Instance.User;
            var bytes = SupportService.GetMultimediaBytes(template.MultimediaID);
            var templateString = Encoding.ASCII.GetString(bytes);
            var sb = new StringBuilder();
            var reader = new StringReader(templateString);
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
            var content = sb.ToString();
            var filename = TempFileManager.NewTempFilename(".rtf");
            if (!string.IsNullOrWhiteSpace(filename)) {
                File.WriteAllText(filename, content);
                return new FileInfo(filename);            
            }

            return null;            
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
    }

}
