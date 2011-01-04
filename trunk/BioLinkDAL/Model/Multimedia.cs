using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BioLink.Data.Model {

    public class Multimedia : OwnedDataObject {

        public int MultimediaID { get; set; }
        public string Name { get; set; }
        public string Number { get; set; }
        public string Artist { get; set; }
        public string DateRecorded { get; set; }
        public string Owner { get; set; }
        public string Copyright { get; set; }
        public string FileExtension { get; set; }


        protected override System.Linq.Expressions.Expression<Func<int>> IdentityExpression {
            get { return ()=>this.MultimediaID; }
        }
    }
}
