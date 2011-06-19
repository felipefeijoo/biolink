using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BioLink.Client.Utilities {
    public interface IChangeable {
        bool IsChanged { get; set; }
    }
}
