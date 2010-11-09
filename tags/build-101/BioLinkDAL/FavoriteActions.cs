using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
}
