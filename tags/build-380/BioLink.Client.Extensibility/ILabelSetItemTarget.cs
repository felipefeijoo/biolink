using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Data;
using BioLink.Client.Utilities;

namespace BioLink.Client.Extensibility {

    public interface ILabelSetItemTarget {
        void AddItemToLabelSet(PinnableObject item);
    }

}
