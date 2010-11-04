using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BioLink.Data.Model {

    // Place holder class representing all Biolink Transfer Objects (TO)
    public abstract class BioLinkDataObject {
    }

    // Biolink data objects that have a GUID column
    public abstract class GUIDObject : BioLinkDataObject {
        public Nullable<Guid> GUID { get; set; }
    }

    // Biolink data objects that have ownership columns
    public abstract class OwnedDataObject : GUIDObject {
        public DateTime DateCreated { get; set; }
        public string WhoCreated { get; set; }
        public DateTime DateLastUpdated { get; set; }
        public string WhoLastUpdated { get; set; }

    }

}
