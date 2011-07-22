using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BioLink.Data {

    public class RTFReportBuilder {

        private StringBuilder _rtf = new StringBuilder();

        public const string RTF_HEADER = @"{\rtf1\ansi\deff0\deflang1033 {\fonttbl {\f0\fswiss\fcharset0 SYSTEM;}{\f1\froman\fcharset0 TIMES NEW ROMAN;}}";
        public const string RTF_COLOUR_TABLE = @"{\colortbl \red0\green0\blue0}";
        public const string RTF_PRE_TEXT = @"\paperw11895 \margr0\margl0\ATXph0 \plain \fs20 \f1 ";
        public const string RTF_PARA = @"\par ";
        public const string vbCRLF = "\n";

        public RTFReportBuilder Append(string format, params object[] args) {
            if (args.Length == 0) {
                _rtf.Append(format);
            } else {
                _rtf.AppendFormat(format, args);
            }
            return this;
        }

        public RTFReportBuilder Append(object obj) {
            _rtf.Append(obj);
            return this;
        }

        public RTFReportBuilder AppendCurrentDate() {
            _rtf.AppendFormat("{0:f}", DateTime.Now);
            return this;
        }

        public RTFReportBuilder Par() {
            _rtf.Append(RTF_PARA);
            return this;
        }

        public RTFReportBuilder NewLine() {
            _rtf.Append(vbCRLF);
            return this;            
        }

        public RTFReportBuilder Header() {
            _rtf.Append(RTF_HEADER);
            return this;
        }

        public RTFReportBuilder ColorTable() {
            _rtf.Append(RTF_COLOUR_TABLE);
            return this;
        }

        public RTFReportBuilder PreText() {
            _rtf.Append(RTF_PRE_TEXT);
            return this;
        }

        public string RTF { 
            get { return _rtf.ToString(); } 
        }

        public RTFReportBuilder AppendFullHeader() {
            Header().NewLine().ColorTable().NewLine().PreText();
            return this;
        }

        public RTFReportBuilder ReportHeading(string heading) {
            Append(@"\pard\fs36\b {0}\b0\pard\par\fs24 ", heading);
            return this;
        }

        public DataMatrix GetAsMatrix() {
            var results = new DataMatrix();
            results.Columns.Add(new MatrixColumn { Name = "RTF" });
            results.AddRow()[0] = _rtf.ToString();
            return results;
        }

    }
}
