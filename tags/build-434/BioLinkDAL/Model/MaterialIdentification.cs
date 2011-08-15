using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BioLink.Data.Model {

    public class MaterialIdentification : GUIDObject {

        public int MaterialIdentID { get; set; }
        public int MaterialID { get; set; }
        public string Taxa { get; set; }
        public string IDBy { get; set; }
        public DateTime? IDDate { get; set; }
        public int IDRefID { get; set; }
        public string IDMethod { get; set; }
        public string IDAccuracy { get; set; }
        public string NameQual { get; set; }
        public string IDNotes { get; set; }
        public string IDRefPage { get; set; }
        public int? BasedOnID { get; set; }
        public string RefCode { get; set; }

        public int Changes { get; set; }


        protected override System.Linq.Expressions.Expression<Func<int>> IdentityExpression {
            get { return () => this.MaterialIdentID; }
        }
    }
}
