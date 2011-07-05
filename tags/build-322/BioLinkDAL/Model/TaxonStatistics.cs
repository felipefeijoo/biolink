using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BioLink.Data.Model {

    public class TaxonStatistics {

        public int Sites { get; set; }
        public int SiteVisits { get; set; }
        public int Material { get; set; }
        public int Specimens { get; set; }
        public int Multimedia { get; set; }
        public int TypeSpecimens { get; set; }
        public int Notes { get; set; }
        public int References { get; set; }

        public int TotalItems { get; set; }

    }
}
