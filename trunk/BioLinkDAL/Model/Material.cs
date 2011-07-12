using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BioLink.Data.Model {

    public class Material : OwnedDataObject {

        public int MaterialID { get; set; }

        [MappingInfo("Template")]
        public bool IsTemplate { get; set; }

        public string MaterialName { get; set; }

        public int SiteVisitID { get; set; }

        [MappingInfo("AccessionNo")]
        public string AccessionNumber { get; set; }

        [MappingInfo("RegNo")]
        public string RegistrationNumber { get; set; }

        [MappingInfo("CollectorNo")]
        public string CollectorNumber { get; set; }

        public int BiotaID { get; set; }

        [MappingInfo("IDBy")]
        public string IdentifiedBy { get; set; }

        [MappingInfo("IDDate")]
        public DateTime? IdentificationDate { get; set; }

        [MappingInfo("IDRefID")]
        public int IdentificationReferenceID { get; set; }

        [MappingInfo("IDMethod")]
        public string IdentificationMethod { get; set; }

        [MappingInfo("IDAccuracy")]
        public string IdentificationAccuracy { get; set; }

        [MappingInfo("IDNameQual")]
        public string IdentificationNameQualification { get; set; }

        [MappingInfo("IDNotes")]
        public string IdentificationNotes { get; set; }

        public string Institution { get; set; }

        public string CollectionMethod { get; set; }

        public string Abundance { get; set; }

        public string MacroHabitat { get; set; }

        public string MicroHabitat { get; set; }

        public string Source { get; set; }

        public int AssociateOf { get; set; }

        public string SpecialLabel { get; set; }

        public string OriginalLabel { get; set; }

        public int TrapID { get; set; }

        [MappingInfo("IDRefPage")]
        public string IdentificationRefPage { get; set; }

        public string TaxaDesc { get; set; }

        public int SiteID { get; set; }

        public string SiteName { get; set; }

        public string SiteVisitName { get; set; }

        public string RefCode { get; set; }

        public string TrapName { get; set; }

        public string LoadID { get; set; }

        public string TypeData { get; set; }

        public int? LoanID { get; set; }


        protected override System.Linq.Expressions.Expression<Func<int>> IdentityExpression {
            get { return () => this.MaterialID; }
        }
    }
}
