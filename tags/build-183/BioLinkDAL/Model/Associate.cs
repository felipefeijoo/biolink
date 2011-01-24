using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BioLink.Data.Model {

    [Serializable()]
    public class Associate : BioLinkDataObject {

        public int AssociateID { get; set; }
        public int FromIntraCatID { get; set; }
        public int FromCatID { get; set; }
        public int? ToIntraCatID	{ get; set; } 
        public int? ToCatID { get; set; }
        public string AssocDescription { get; set;}
        public string RelationFromTo { get; set; }
        public string RelationToFrom { get; set; }
        public int PoliticalRegionID { get; set; }
        public string Source { get; set; }
        public int RefID { get; set; }
        public string RefPage { get; set; }
        public bool Uncertain { get; set; }
        [IgnoreRTFFormattingChanges]
        public string Notes { get; set; }
        public string AssocName { get; set; }
        public string RefCode { get; set; }
        public string PoliticalRegion { get; set;}
        public string Direction { get; set; }
        public string FromCategory { get; set; }
        public string ToCategory { get; set; }
        public Guid AssocGUID { get; set; }
        public Guid RegionGUID { get; set; }

        public int Changes { get; set; }

        protected override System.Linq.Expressions.Expression<Func<int>> IdentityExpression {
            get { return () => this.AssociateID; }
        }
    }
}
