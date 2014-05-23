using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BioLink.Client.Extensibility {

    public class UpperCaseTransformer : IValueTransformer {

        public string name {
            get { return "Convert to uppercase"; }
        }

        public string description {
            get { return "Converts all characters in input value to uppercase"; }
        }

        public string transform(string value, IRowSource row) {
            if (!string.IsNullOrEmpty(value)) {
                return value.ToUpper();
            }
            return value;
        }

    }
}
