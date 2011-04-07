using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BioLink.Data {

    public class ReportConstants {

        public const string RTF_HEADER = @"{\rtf1\ansi\deff0\deflang1033 {\fonttbl {\f0\fswiss\fcharset0 SYSTEM;}{\f1\froman\fcharset0 TIMES NEW ROMAN;}}";
        public const string RTF_COLOUR_TABLE = @"{\colortbl \red0\green0\blue0}";
        public const string RTF_PRE_TEXT = @"\paperw11895 \margr0\margl0\ATXph0 \plain \fs20 \f1 ";
        public const string RTF_PARA = @"\par ";
        public const string vbCRLF = "\n";

        public static string ReportDateString() {
            return string.Format("{0:f}", DateTime.Now);
        }

    }

}
