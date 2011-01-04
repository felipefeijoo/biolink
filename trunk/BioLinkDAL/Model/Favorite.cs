using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BioLink.Data.Model {

    public abstract class Favorite : BioLinkDataObject {

        protected Favorite(FavoriteType type) {
            FavoriteID = -1;
            this.FavoriteType = type;
        }

        public string Username { get; set; }
        public int FavoriteID { get; set; }
        public int FavoriteParentID { get; set; }
        public bool IsGroup { get; set; }
        public string GroupName { get; set; }
        public virtual int NumChildren { get; set; }
        public bool IsGlobal { get; set; }
        public FavoriteType FavoriteType { get; set; }
        public int ID1 { get; set; }
        public string ID2 { get; set; }

        protected override System.Linq.Expressions.Expression<Func<int>> IdentityExpression {
            get { return () => this.FavoriteID; }
        }
    }

    public class TaxonFavorite : Favorite {

        public TaxonFavorite() : base(FavoriteType.Taxa) { }
        public int TaxaID { get; set; }
        public int TaxaParentID { get; set; }
        public string Epithet { get; set; }
        public string TaxaFullName { get; set; }
        public string YearOfPub { get; set; }
        public string KingdomCode { get; set; }
        public string ElemType { get; set; }
        public bool Unverified { get; set; }
        public bool Unplaced { get; set; }
        public int Order { get; set; }
        public string Rank { get; set; }
        public bool ChgComb { get; set; }
        public string NameStatus { get; set; }        
    }

    public class SiteFavorite : Favorite {

        public SiteFavorite() : base(FavoriteType.Site) { }

        public int ElemID { get; set; }
        public string Name { get; set; }
        public string ElemType { get; set; }        
    }

    public class ReferenceFavorite : Favorite {

        public ReferenceFavorite() : base(FavoriteType.Reference) {}

        public int RefID { get; set; }
        public string RefCode { get; set; }
        public string FullRTF { get; set; }
        public override int NumChildren {
            get { return (IsGroup ? base.NumChildren : 0); }
            set { base.NumChildren = value; }
        }
    }

    public class DistRegionFavorite : Favorite {

        public DistRegionFavorite() : base(FavoriteType.DistRegion) { }

        public int DistRegionID { get; set; }
        public int DistRegionParentID { get; set; }
        public string DistRegionName { get; set; }        
    }

    public class BiotaStorageFavorite : Favorite {

        public BiotaStorageFavorite() : base(FavoriteType.BiotaStorage) { }

        public int BiotaStorageID { get; set; }
        public int BiotaStorageParentID { get; set; }
        public string BiotaStorageName { get; set; }

        protected override System.Linq.Expressions.Expression<Func<int>> IdentityExpression {
            get { return () => this.FavoriteID; }
        }
    }

    public enum FavoriteType {
        Taxa,
        Site,
//        Character,  // No morphology at this stage...
        Reference,
        DistRegion,
        BiotaStorage
    }
}
