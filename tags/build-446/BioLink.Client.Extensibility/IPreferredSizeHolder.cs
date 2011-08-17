using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BioLink.Client.Extensibility {

    public interface IPreferredSizeHolder {

        int PreferredHeight { get; }
        int PreferredWidth { get; }

    }
}
