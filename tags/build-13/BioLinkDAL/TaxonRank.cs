using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BioLink.Data.Model {

    public class TaxonRank : BiolinkDataObject {

        public string KingdomCode { get; set; }

        public string Code { get; set; }

        public string LongName { get; set; }

        public string TextBeforeInFullName { get; set; }

        public string TextAfterInFullName { get; set; }

        public System.Nullable<bool> JoinToParentInFullName { get; set; }

        public string ChecklistDisplayAs { get; set; }

        public System.Nullable<byte> AvailableNameAllowed { get; set; }

        public System.Nullable<bool> UnplacedAllowed { get; set; }

        public System.Nullable<bool> ChgCombAllowed { get; set; }

        public System.Nullable<bool> LituratueNameAllowed { get; set; }

        public string Category { get; set; }

        public string ValidChildList { get; set; }

        public string ValidEndingList { get; set; }

        public System.Nullable<int> Order { get; set; }
        
    }
}
