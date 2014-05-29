using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace BioLink.Client.Extensibility {

    public abstract class ValueTransformer {
        public abstract String Key { get; }
        public abstract String Name { get; }
        public abstract String Description { get; }
        public abstract String Transform(String value, IRowSource row);
        public abstract bool HasOptions { get; }
        public abstract void ShowOptions();
        public abstract object GetConfiguration();
        public abstract void RestoreFromConfiguration(object config);
    }

}
