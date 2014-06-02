using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows;

namespace BioLink.Client.Extensibility {

    public abstract class ValueTransformer : IBioLinkExtension {
        public abstract String Key { get; }
        public abstract String Name { get; }
        public abstract String Description { get; }
        public abstract String Transform(String value, IRowSource row);
        public abstract bool HasOptions { get; }
        public abstract void ShowOptions(Window owner);
        public abstract object GetConfiguration();
        public abstract void RestoreFromConfiguration(object config);

        public virtual String DisplayString {
            get { return Name; }
        }

        public virtual void Dispose() {
        }

    }

}
