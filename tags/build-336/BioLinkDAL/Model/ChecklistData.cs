using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BioLink.Data.Model {

    public class ChecklistData {

        public int BiotaID { get; set; }
        public int IndentLevel { get; set; }
        public string DisplayName { get; set; }
        public string Author { get; set; }
        public string Type { get; set; }
        public string Rank { get; set; }
        public bool Verified { get; set; }        
        public bool Unplaced { get; set; }
        public bool AvailableName { get; set; }
        public bool LiteratureName { get; set; }
        public string RankCategory { get; set; }

    }
}
