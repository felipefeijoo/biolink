using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BioLink.Data.Model {

    public class Trait : BiolinkDataObject {

        public int TraitID { get; set; }

        public int TraitCatID { get; set; }

        public int IntraCatID { get; set; }

        public int TraitTypeID { get; set; }

        public string Value { get; set; }

        public string Comment { get; set; }

        public System.Nullable<bool> UseInReports { get; set; }

    }
}
