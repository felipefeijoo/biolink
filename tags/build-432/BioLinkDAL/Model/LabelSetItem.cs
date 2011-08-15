using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BioLink.Data.Model {

    public class LabelSetItem : BioLinkDataObject {

        public int SetID { get; set; }
        public int LabelItemID { get; set;}
        public int ItemID { get; set; }
        public string ItemType { get; set; }
        public int SiteID { get; set; }
        public int VisitID { get; set; }
        public int MaterialID { get; set; }
        public string Region { get; set; }
        public int LocalType { get; set; }
        public string Local { get; set; }
        public string DistanceFromPlace { get; set; }
        public string DirFromPlace { get; set; }
        public int AreaType { get; set; }
        public double? Long { get; set; }
        public double? Lat { get; set; }
        public double? Long2 { get; set; }
        public double? Lat2 { get; set; }
        public string Collectors { get; set; }
        public int DateType { get; set; }
        public int? StartDate { get; set; }	
        public int? EndDate { get; set; }
        public string CasualDate { get; set; }
        public string AccessionNo { get; set; }
        public string TaxaFullName { get; set; }
        public int PrintOrder { get; set; }
        public int NumCopies { get; set; }

        protected override System.Linq.Expressions.Expression<Func<int>> IdentityExpression {
            get { return () => LabelItemID; }
        }
    }
}
