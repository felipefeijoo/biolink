using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BioLink.Data.Model {

    [Serializable()]
    public class MaterialPart : GUIDObject {

        public int MaterialID { get; set; }
        public int MaterialPartID { get; set; }
        public string PartName { get; set; }
        public string SampleType { get; set;}
        public int? NoSpecimens { get; set; }
        public string NoSpecimensQual { get; set; }
        public string Lifestage	{ get; set; }
        public string Gender { get; set;}
        public string RegNo	{ get; set; }
        public string Condition	{ get; set; }
        public string StorageSite { get; set; }
        public string StorageMethod { get; set; }
        public string CurationStatus { get; set; }
        public string NoOfUnits	{ get; set; }
        [IgnoreRTFFormattingChanges]
        public string Notes	{ get; set; }
        public bool OnLoan { get; set; }
        public int? BasedOnID { get; set; }

        public int? Changes { get; set; }


        protected override System.Linq.Expressions.Expression<Func<int>> IdentityExpression {
            get { return () => this.MaterialPartID; }
        }
    }
}
