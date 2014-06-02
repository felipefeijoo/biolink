using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;


namespace BioLink.Client.Extensibility {

    public class RegExCaptureTransformer : ValueTransformer {

        protected RegExCaptureTransformerConfig Options { get; private set; }
        protected Regex Regex { get; private set; }
        protected int PlaceholderCount { get; private set; }

        public RegExCaptureTransformer() {
            UpdateConfig("^(.*)$", "{0}");
        }

        private bool UpdateConfig(String pattern, String outputFormat, bool defaultIfNotMatch = false, String defaultValue = "") {
            // Counts the number of placeholders in the output format
            Regex = new Regex(pattern);
            PlaceholderCount = Regex.Matches(outputFormat, @"(?<!\{)\{([0-9]+).*?\}(?!})").Cast<Match>().Max(m => int.Parse(m.Groups[1].Value)) + 1;

            Options = new RegExCaptureTransformerConfig { Pattern = pattern, OutputFormat = outputFormat, DefaultIfNotMatch = defaultIfNotMatch, DefaultValue = defaultValue };
            return true;
        }

        public override string Key {
            get { return "regexCapture"; }
        }

        public override string Name {
            get { return "RegEx capture"; }
        }

        public override string Description {
            get { return "Capture and re-format part(s) of the value"; }
        }

        public override string Transform(string value, IRowSource row) {

            if (this.Regex != null) {
                var m = this.Regex.Match(value);
                if (m.Success) {
                    var placeHolderValues = new String[PlaceholderCount];
                    for (int i = 0; i < PlaceholderCount; ++i) {

                        if (i < m.Groups.Count) {
                            placeHolderValues[i] = m.Groups[i].Value;
                        } else {
                            placeHolderValues[i] = "";
                        }
                    }
                    return String.Format(Options.OutputFormat, placeHolderValues);
                }
            }

            if (Options.DefaultIfNotMatch) {
                return Options.DefaultValue;
            }

            return value;
        }

        public override bool HasOptions {
            get { return true; }
        }

        public override void ShowOptions(System.Windows.Window owner) {
            var frm = new RegExCaptureTransformerOptionsWindow(Options);
            frm.Owner = owner;
            if (frm.ShowDialog().GetValueOrDefault()) {
                UpdateConfig(frm.Pattern, frm.OutputFormat, frm.DefaultIfNotMatch, frm.DefaultValue);
            }            
        }

        public override object GetConfiguration() {
            return Options;
        }

        public override void RestoreFromConfiguration(object config) {
            Options = config as RegExCaptureTransformerConfig;
        }

        public override string DisplayString {
            get {
                if (Options.DefaultIfNotMatch) {
                    return String.Format("Capture ('{0}' => '{1}') OR '{2}'", Options.Pattern, Options.OutputFormat, Options.DefaultValue);
                }
                return String.Format("Capture ('{0}' => '{1}')", Options.Pattern, Options.OutputFormat);
            }
        }

    }

    public class RegExCaptureTransformerConfig {
        public String Pattern { get; set; }
        public String OutputFormat { get; set; }
        public bool DefaultIfNotMatch { get; set; }
        public String DefaultValue { get; set; }
    }

}
