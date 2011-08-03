using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BioLink.Data  {

    /// <summary>
    /// Simple transfer object for values that have a label and an underlying code or value
    /// </summary>
    public class CodeLabelPair {

        public CodeLabelPair(string code, string label) {
            this.Code = code;
            this.Label = label;
        }

        public string Code { get; private set; }
        public string Label { get; private set; }

        public override string  ToString() {
            return Label;
        }

    }
}
