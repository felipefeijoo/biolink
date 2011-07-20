using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Data.Model;

namespace BioLink.Data {

    public class DeleteFavoriteCommand : DatabaseCommand {

        public DeleteFavoriteCommand(int favoriteId) {
            this.FavoriteID = favoriteId;
        }

        protected override void ProcessImpl(User user) {
            var service = new SupportService(user);
            service.DeleteFavorite(FavoriteID);
        }

        public int FavoriteID { get; set; }

        public override string ToString() {
            return string.Format("Delete Favorite: ID={0}", FavoriteID);
        }

        protected override void BindPermissions(PermissionBuilder required) {
            required.None();
        }

    }

    public class InsertFavoriteGroupCommand : GenericDatabaseCommand<Favorite> {

        public InsertFavoriteGroupCommand(Favorite model)
            : base(model) {
        }

        protected override void ProcessImpl(User user) {
            var service = new SupportService(user);
            int favId = service.InsertFavoriteGroup(Model.FavoriteType, Model.FavoriteParentID, Model.GroupName, Model.IsGlobal);
            Model.FavoriteID = favId;
        }

        public override string ToString() {
            return string.Format("Insert Favorite Group: Name={0}, IsGlobal={1}", Model.GroupName, Model.IsGlobal);
        }

        protected override void BindPermissions(PermissionBuilder required) {
            required.None();
        }

    }

    public class RenameFavoriteGroupCommand : GenericDatabaseCommand<Favorite> {

        public RenameFavoriteGroupCommand(Favorite model) : base(model) { }

        protected override void ProcessImpl(User user) {
            var service = new SupportService(user);            
            service.RenameFavoriteGroup(Model.FavoriteID, Model.GroupName);            
        }

        public override string ToString() {
            return string.Format("Rename Favorite Group: ID={0}, GroupName={1}", Model.FavoriteID, Model.GroupName);
        }


        protected override void BindPermissions(PermissionBuilder required) {
            required.None();
        }

    }

    public class MoveFavoriteCommand : GenericDatabaseCommand<Favorite> {

        public MoveFavoriteCommand(Favorite model, Favorite newParent) : base(model) {
            this.NewParent = newParent;
        }

        protected override void ProcessImpl(User user) {
            var service = new SupportService(user);
            int parentId = 0;
            if (NewParent != null) {
                parentId = NewParent.FavoriteID;
            }
            // There is an issue with the stored procedure in that it will not correctly move child items back up to the root.
            // The reason for this is that the stored proc attempts to match the username of the target with source by selecting
            // the username or the target element (username is column for favorites, regardless if its global or not...)
            // Anyway when it attempts to get the username for the root elements, it gets a null, and updates the user for the source to null
            // making it irretrievable. 
            // The solution for this (without patching the database) is for these cases actually to do the move manually (i.e. delete and re-insert)
            if (parentId > 0) {
                service.MoveFavorite(Model.FavoriteID, parentId);
            } else {
                // first delete the existing favorite...
                service.DeleteFavorite(Model.FavoriteID);
                // then insert it again with a revised parent id...
                if (Model.IsGroup) {
                    service.InsertFavoriteGroup(Model.FavoriteType, Model.FavoriteParentID, Model.GroupName, Model.IsGlobal);
                } else {
                    service.InsertFavorite(Model.FavoriteType, 0, Model.ID1, Model.ID2, Model.IsGlobal);
                }
            }
        }

        public Favorite NewParent { get; private set; }

        public override string ToString() {
            return string.Format("Move Favorite: ID={0}, NewParentID={1}", Model.FavoriteID, NewParent.FavoriteID);
        }


        protected override void BindPermissions(PermissionBuilder required) {
            required.None();
        }
    }

    public class InsertTaxonFavoriteCommand : GenericDatabaseCommand<TaxonFavorite> {

        public InsertTaxonFavoriteCommand(TaxonFavorite model)
            : base(model) {
        }

        protected override void ProcessImpl(User user) {
            var service = new SupportService(user);
            var newID = service.InsertFavorite(FavoriteType.Taxa, Model.FavoriteParentID, Model.TaxaID, "", Model.IsGlobal);
            Model.FavoriteID = newID;
        }

        public override string ToString() {
            return string.Format("Insert Taxon Favorite: TaxaID={0}, IsGlobal={1}", Model.TaxaID, Model.IsGlobal);
        }


        protected override void BindPermissions(PermissionBuilder required) {
            required.None();
        }
    }

    public class InsertSiteFavoriteCommand : GenericDatabaseCommand<SiteFavorite> {

        public InsertSiteFavoriteCommand(SiteFavorite model)
            : base(model) {
        }

        protected override void ProcessImpl(User user) {
            var service = new SupportService(user);
            var newID = service.InsertFavorite(FavoriteType.Site, Model.FavoriteParentID, Model.ElemID, Model.ElemType, Model.IsGlobal);
            Model.FavoriteID = newID;
        }

        public override string ToString() {
            return string.Format("Insert Site Favorite: SiteID={0}, IsGlobal={1}", Model.ElemID, Model.IsGlobal);
        }



        protected override void BindPermissions(PermissionBuilder required) {
            required.None();
        }
    }

    public class InsertReferenceFavoriteCommand : GenericDatabaseCommand<ReferenceFavorite> {

        public InsertReferenceFavoriteCommand(ReferenceFavorite model)
            : base(model) {
        }

        protected override void ProcessImpl(User user) {
            var service = new SupportService(user);
            var newID = service.InsertFavorite(FavoriteType.Reference, Model.FavoriteParentID, Model.RefID, "", Model.IsGlobal);
            Model.FavoriteID = newID;
        }

        public override string ToString() {
            return string.Format("Insert Reference Favorite: RefID={0}, IsGlobal={1}", Model.RefID, Model.IsGlobal);
        }

        protected override void BindPermissions(PermissionBuilder required) {
            required.None();
        }

    }
}
