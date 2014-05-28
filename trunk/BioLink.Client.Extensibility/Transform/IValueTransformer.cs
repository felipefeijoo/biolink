using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace BioLink.Client.Extensibility {

    public interface IValueTransformer {
        String Key { get; }
        String Name { get; }
        String Description { get; }
        String Transform(String value, IRowSource row);
        bool HasOptions { get; }
        void ShowOptions();
        object GetConfiguration();
        void RestoreFromConfiguration(object config);
    }

}
