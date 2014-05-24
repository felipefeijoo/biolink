using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BioLink.Client.Extensibility {

    public interface IValueTransformer {
        String Name { get; }
        String Description { get; }
        String Transform(String value, IRowSource row);
    }

}
