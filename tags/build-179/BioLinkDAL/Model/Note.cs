using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BioLink.Data.Model {

    public class Note : GUIDObject {

        public string NoteCategory { get; set; }

        public int IntraCatID { get; set; }

        public int NoteID { get; set; }

        public string NoteType { get; set; }

        [MappingInfo("Note")]
        public string NoteRTF { get; set; }

        public string Author { get; set; }

        public string Comments { get; set; }

        public bool UseInReports { get; set; }

        public int RefID { get; set; }

        public string RefCode { get; set; }

        public string RefPages { get; set; }

        protected override System.Linq.Expressions.Expression<Func<int>> IdentityExpression {
            get { return ()=>this.NoteID; }
        }
    }

}
