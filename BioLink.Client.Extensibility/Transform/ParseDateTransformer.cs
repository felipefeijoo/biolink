using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Globalization;
using BioLink.Client.Utilities;

namespace BioLink.Client.Extensibility {

    public class ParseDateTransformer : ValueTransformer {

        public ParseDateTransformer() {
            Options = new ParseDateTransformerConfig { InputFormat = "yyyy-MM-dd", AutoDetectFormat = true };
        }

        protected ParseDateTransformerConfig Options { get; set; }

        public override string Key {
            get { return "toBLDate"; }
        }

        public override string Name {
            get {                 
                return "Parse as date"; 
            }
        }

        public override string DisplayString {
            get {
                var l = new List<String>();
                var bits = Options.InputFormat.Split('|');
                bits.ForEach(bit => {
                    l.Add(bit);
                });
                if (Options.AutoDetectFormat) {
                    l.Add("auto");
                }
                return String.Format("Parse date ({0})", l.Join("|"));
            }
        }

        public override string Description {
            get { return "Attempts to convert the value into a valid BioLink date"; }
        }

        public override string Transform(string value, IRowSource row) {            
            List<String> patterns = new List<String>();
            DateTime dt;
            if (!String.IsNullOrWhiteSpace(Options.InputFormat)) {
                var bits = Options.InputFormat.Split('|');                
                foreach (String pattern in bits) {
                    if (!String.IsNullOrWhiteSpace(pattern)) {
                        if (DateTime.TryParseExact(value, pattern.Trim(), CultureInfo.InvariantCulture, DateTimeStyles.None, out dt)) {
                            return DateUtils.DateToBLDate(dt);
                        }
                    }
                }
            }

            if (Options.AutoDetectFormat) {
                if (DateTime.TryParse(value, out dt)) {
                    return DateUtils.DateToBLDate(dt);
                }
            }

            return "0";
        }

        public override bool HasOptions {
            get { return true; }
        }

        public override void ShowOptions(Window owner) {

            var frm = new ParseDateTransformerOptionsWindow(Options);
            if (frm.ShowDialog().GetValueOrDefault()) {
                Options.InputFormat = frm.InputFormat;
                Options.AutoDetectFormat = frm.AutoDetectFormat;
            }

        }

        public override object GetConfiguration() {
            return Options;
        }

        public override void RestoreFromConfiguration(object config) {
            Options = config as ParseDateTransformerConfig;
        }

    }

    public class ParseDateTransformerConfig {
        public String InputFormat { get; set; }
        public bool AutoDetectFormat { get; set; }
    }

}
