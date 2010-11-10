using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Data.Model;

namespace BioLink.Data {

    public class DeleteFavoriteAction : DatabaseAction {

        public DeleteFavoriteAction(int favoriteId) {
            this.FavoriteID = favoriteId;
        }

        protected override void ProcessImpl(User user) {
            var service = new SupportService(user);
            service.DeleteFavorite(FavoriteID);
        }

        public int FavoriteID { get; set; }

    }

    public class InsertFavoriteGroupAction : GenericDatabaseAction<Favorite> {

        public InsertFavoriteGroupAction(Favorite model, FavoriteType favoriteType)
            : base(model) {
            this.FavoriteType = favoriteType;
        }


        protected override void ProcessImpl(User user) {
            var service = new SupportService(user);
            int favId = service.InsertFavoriteGroup(FavoriteType, Model.FavoriteParentID, Model.GroupName, Model.IsGlobal);
            Model.FavoriteID = favId;
        }

        internal FavoriteType FavoriteType { get; private set; }
    }

    public class RenameFavoriteGroupAction : GenericDatabaseAction<Favorite> {

        public RenameFavoriteGroupAction(Favorite model)
            : base(model) {
        }

        protected override void ProcessImpl(User user) {
            var service = new SupportService(user);            
            service.RenameFavoriteGroup(Model.FavoriteID, Model.GroupName);            
        }
    }
}
