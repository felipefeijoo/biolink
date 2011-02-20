using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BioLink.Data.Model {

    public class SpeciesAvailableName : AvailableName {

        public string PrimaryType { get; set; }
        public string SecondaryType { get; set; }
        public bool PrimaryTypeProbable { get; set; }
        public bool SecondaryTypeProbable { get; set; }

    }
}
