using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BioLink.Data.Model {
    public class XMLImportReference : XMLImportObject {
        public string Code { get; set; }
        public string Author { get; set; }
        public string Year { get; set; }        
    }
}
