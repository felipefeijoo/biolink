using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BioLink.Data.Model {

    public class GenusAvailableName : AvailableName {
        public int Designation { get; set; }
        public bool? TSCUChgComb { get; set; }
        public string TSFixationMethod { get; set; }
        public string TypeSpecies { get; set; }        
    }
}
