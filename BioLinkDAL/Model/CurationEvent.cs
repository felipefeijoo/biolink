using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BioLink.Data.Model {

    public class CurationEvent : GUIDObject {

        public int CurationEventID { get; set; }
        public int MaterialID { get; set; }
        public string SubPartName { get; set; }
        public string Who { get; set; }
        public DateTime? When { get; set; }
        public string EventType { get; set; }
        public string EventDesc { get; set; }
        public int Changes { get; set; }

    }
}
