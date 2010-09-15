using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BioLink.Data.Model {
    public class ReferenceSearchResult {

        public int RefID { get; set; }
        public string RefCode { get; set; }
        public string Author { get; set; }
        public string YearOfPub { get; set; }
        public string Title { get; set; }
        public string RefType { get; set; }
        public string RefRTF { get; set; }

    }
}
