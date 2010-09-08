using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BioLink.Data.Model {

    public abstract class GUIDObject {
        public Nullable<Guid> GUID { get; set; }
    }

    public abstract class BiolinkDataObject : GUIDObject {

        public DateTime DateCreated { get; set; }
        public string WhoCreated { get; set; }
        public DateTime DateLastUpdated { get; set; }
        public string WhoLastUpdated { get; set; }

    }

}
