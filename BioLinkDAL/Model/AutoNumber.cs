using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BioLink.Data.Model {

    public class AutoNumber : GUIDObject {

        public int AutoNumberCatID { get; set; }
        public string Category { get; set; }
        public string Name { get; set; }
        public string Prefix { get; set; }
        public string Postfix { get; set; }
        public int NumLeadingZeros { get; set; }
        public int LastNumber { get; set; }
        public bool Locked { get; set; }
        public bool EnsureUnique { get; set; }
    }

    public class NewAutoNumber : AutoNumber {
        public int NewNumber { get; set; }
    }
    
}
