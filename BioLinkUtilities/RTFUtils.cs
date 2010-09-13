using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Itenso.Rtf;
using Itenso.Rtf.Parser;
using Itenso.Rtf.Converter.Text;
using Itenso.Rtf.Support;

namespace BioLink.Client.Utilities {
    public static class RTFUtils {

        public static string StripMarkup(string rtf) {

            if (!String.IsNullOrEmpty(rtf) && rtf.StartsWith("{")) {
                try {
                    var l = new PlainTextListener();
                    RtfParser parser = new RtfParser(l);
                    parser.Parse(new RtfSource(rtf));
                    return l.PlainText;
                } catch (Exception) {
                    return rtf;
                }
            } else {
                return rtf;
            }
        }
    }

    class PlainTextListener : IRtfParserListener {

        private StringBuilder _builder = new StringBuilder();
        private int _groupDepth;

        public void GroupBegin() {
            _groupDepth++;
        }

        public void GroupEnd() {
            _groupDepth--;
        }

        public void ParseBegin() {
            _groupDepth = 0;
        }

        public void ParseEnd() {            
        }

        public void ParseFail(RtfException reason) {            
        }

        public void ParseSuccess() {            
        }

        public void TagFound(IRtfTag tag) {
            switch (tag.Name) {
                case "fonttbl":
                    int targetDepth = _groupDepth - 1;
                    this.CollectPredicate = () => {
                        if (_groupDepth <= targetDepth) {
                            CollectPredicate = null;
                            return true;
                        } else {
                            return false;
                        }
                    };
                    break;
                case "par":
                case "sect":
                case "line":
                case "emspace":
                case "enspace":
                    _builder.Append(" ");
                    break;
                case "tab":
                    _builder.Append("\t");
                    break;
                case "emdash":
                case "endash":
                    _builder.Append("-");
                    break;
                case "lquote":
                case "rquote":               
                    _builder.Append("'");
                    break;
                case "ldblquote":
                case "rdblquote":
                    _builder.Append("\"");
                    break;
            }
        }

        public void TextFound(IRtfText text) {
            if (CollectPredicate == null || CollectPredicate()) {
                _builder.Append(text);
            }
        }

        public String PlainText {
            get {                
                return _builder.ToString();                
            }
        }

        private Func<bool> CollectPredicate { get; set; }
    }
}
