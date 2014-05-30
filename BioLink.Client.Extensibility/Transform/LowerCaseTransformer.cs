using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace BioLink.Client.Extensibility {

    public class LowerCaseTransformer : ValueTransformer {

        public override string Key {
            get { return "toLowerCase"; }
        }

        public override string Name {
            get { return "Convert to lower case"; }
        }

        public override string Description {
            get { return "Converts all characters in input value to lower case"; }
        }

        public override string Transform(string value, IRowSource row) {
            if (!string.IsNullOrEmpty(value)) {
                return value.ToLower();
            }
            return value;
        }

        public override bool HasOptions {
            get { return false; }
        }

        public override void ShowOptions(Window owner) {
            throw new NotImplementedException();
        }

        public override object GetConfiguration() {
            return new LowerCaseTransformerConfig { };
        }

        public override void RestoreFromConfiguration(object config) {
            var c = config as LowerCaseTransformerConfig;
        }
    }

    public class LowerCaseTransformerConfig {
    }
}
