using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BioLink.Client.Extensibility {

    public interface IValueTransformer {
        String name { get; }
        String description { get; }
        String transform(String value, IRowSource row);
    }

}
