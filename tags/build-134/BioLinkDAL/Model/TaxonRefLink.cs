using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BioLink.Data.Model {

    public class TaxonRefLink : BioLinkDataObject {

        public int RefLinkID { get; set; }
        public string RefLink { get; set; }
        public int? BiotaID { get; set; }
        public string FullName { get; set; }
        public string RefPage { get; set; }
        public string RefQual { get; set; }
        public bool? UseInReports { get; set; }

        public int Changes { get; set; }
    }
}
