using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BioLink.Data.Model {

    public class GenusAvailableName : GUIDObject {

        public int BiotaID { get; set; }
        public string RefCode { get; set; }
        public string RefPage { get; set; }
        public string RefQual { get; set; }
        public int Designation { get; set; }
        public bool? TSCUChgComb { get; set; }
        public string TSFixationMethod { get; set; }
        public int? RefID { get; set; }
        public string TypeSpecies { get; set; }        
        public string AvailableNameStatus { get; set; }

    }
}
