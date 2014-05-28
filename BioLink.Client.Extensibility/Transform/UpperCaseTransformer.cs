using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BioLink.Client.Extensibility {

    public class UpperCaseTransformer : IValueTransformer {

        public string Key {
            get { return "toUpperCase"; }
        }

        public string Name {
            get { return "Convert to uppercase"; }
        }

        public string Description {
            get { return "Converts all characters in input value to uppercase"; }
        }

        public string Transform(string value, IRowSource row) {
            if (!string.IsNullOrEmpty(value)) {
                return value.ToUpper();
            }
            return value;
        }

        public bool HasOptions {
            get { return false; }
        }

        public void ShowOptions() {
            throw new NotImplementedException();
        }

        public object GetConfiguration() {
            return new UpperCaseTransformerConfig { };
        }

        public void RestoreFromConfiguration(object config) {
            var c = config as UpperCaseTransformerConfig;
        }
    }

    public class UpperCaseTransformerConfig {        
    }
}
