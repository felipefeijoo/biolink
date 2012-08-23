/*******************************************************************************
 * Copyright (C) 2011 Atlas of Living Australia
 * All Rights Reserved.
 * 
 * The contents of this file are subject to the Mozilla Public
 * License Version 1.1 (the "License"); you may not use this file
 * except in compliance with the License. You may obtain a copy of
 * the License at http://www.mozilla.org/MPL/
 * 
 * Software distributed under the License is distributed on an "AS
 * IS" basis, WITHOUT WARRANTY OF ANY KIND, either express or
 * implied. See the License for the specific language governing
 * rights and limitations under the License.
 ******************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using BioLink.Client.Extensibility;
using BioLink.Client.Utilities;
using BioLink.Data;
using BioLink.Data.Model;

namespace BioLink.Client.Tools {
    /// <summary>
    /// Interaction logic for ReferenceFavorites.xaml
    /// </summary>
    public partial class ReferenceFavorites : FavoritesControl<ReferenceFavorite, ReferenceSearchResultViewModel> {
        #region Designer Ctor
        public ReferenceFavorites() {
            InitializeComponent();
        }
        #endregion

        public ReferenceFavorites(User user, ReferenceManager refManager)
            : base(user) {
            InitializeComponent();
            BindProvider(new ReferenceFavoritesProvider(user, tvwFavorites, refManager));
            ChangesCommitted += new PendingChangesCommittedHandler(ReferenceFavorites_ChangesCommitted);

        }

        void ReferenceFavorites_ChangesCommitted(object sender) {
            ReloadFavorites();
        }

        private void FavoriteName_EditingComplete(object sender, string text) {
            CompleteRename(sender, text);
        }

    }

    public class ReferenceFavoritesProvider : IFavoritesProvider<ReferenceFavorite, ReferenceSearchResultViewModel> {

        public ReferenceFavoritesProvider(User user, TreeView tree, ReferenceManager refManager) {
            this.User = user;
            this.FavoritesTree = tree;
            this.RefManager = refManager;
        }

        public PinnableObject CreatePinnableObject(FavoriteViewModel<ReferenceFavorite> viewModel) {
            return new PinnableObject(ToolsPlugin.TOOLS_PLUGIN_NAME, LookupType.Reference, viewModel.Model.RefID, viewModel.DisplayLabel);
        }

        public PinnableObject CreatePinnableObject(ReferenceSearchResultViewModel viewModel) {
            return new PinnableObject(ToolsPlugin.TOOLS_PLUGIN_NAME, LookupType.Reference, viewModel.RefID, viewModel.DisplayLabel);
        }

        public TreeView FavoritesTree { get; private set; }

        public List<ReferenceFavorite> GetTopFavorites(bool global) {
            var service = new SupportService(User);
            return service.GetTopReferenceFavorites(global);
        }

        public List<ReferenceFavorite> GetFavoritesForParent(int parentID, bool global) {
            var service = new SupportService(User);
            return service.GetReferenceFavorites(parentID, global);
        }

        public FavoriteViewModel<ReferenceFavorite> CreateViewModel(ReferenceFavorite model) {
            return new ReferenceFavoriteViewModel(model);
        }

        public List<HierarchicalViewModelBase> GetChildViewModels(FavoriteViewModel<ReferenceFavorite> parent) {
            return new List<HierarchicalViewModelBase>();
        }

        public List<HierarchicalViewModelBase> GetChildViewModels(ReferenceSearchResultViewModel parent) {
            return new List<HierarchicalViewModelBase>();
        }

        public ContextMenu GetContextMenu(HierarchicalViewModelBase selected) {
            ContextMenuBuilder builder = new ContextMenuBuilder(null);

            int refId = -1;
            string refCode = null;
            if (selected is ReferenceFavoriteViewModel) {
                var fav = selected as ReferenceFavoriteViewModel;
                if (!fav.IsGroup) {
                    refId = fav.RefID;
                    refCode = fav.RefCode;
                } else {
                    builder.New("Export favorite group").Handler(() => ExportReferenceFavorite(fav)).End();
                }
            } else if (selected is ReferenceSearchResultViewModel) {
                var vm = selected as ReferenceSearchResultViewModel;
                refId = vm.RefID;
                refCode = vm.RefCode;
            }

            builder.New("Add New...").Handler(() => RefManager.Owner.AddNewReference()).End();

            if (refId >= 0) {
                builder.Separator();
                builder.New("Delete").Handler(() => RefManager.DeleteReference(refId, refCode)).End();
                builder.Separator();
                builder.New("Edit Details...").Handler(() => RefManager.Owner.EditReference(refId)).End();
            }

            return builder.ContextMenu;
        }

        public void GetFavoriteRefIds(ReferenceFavoriteViewModel fav, List<Int32> refIds) {
            if (fav == null) {
                return;
            }

            if (fav.IsGroup) {
                if (!fav.IsExpanded) {
                    fav.IsExpanded = true;
                }
                foreach (var vm in fav.Children) {
                    GetFavoriteRefIds(vm as ReferenceFavoriteViewModel, refIds);
                }
            } else {
                var refId = fav.RefID;
                if (refId > 0 && !refIds.Contains(refId)) {
                    refIds.Add(refId);
                }
            }

        }

        private void ExportReferenceFavorite(ReferenceFavoriteViewModel favorite) {

            if (favorite == null) {
                return;
            }

            var plugin = PluginManager.Instance.GetPluginByName("Tools");
            var refIds = new List<Int32>();
            GetFavoriteRefIds(favorite, refIds);

            if (refIds.Count == 0) {
                return;
            }

            var report = new ReferencesReport(User, favorite.DisplayLabel, refIds);
            PluginManager.Instance.RunReport(plugin, report);
        }

        public FavoriteViewModel<ReferenceFavorite> CreateFavoriteViewModel(ReferenceSearchResultViewModel viewModel) {
            var model = new ReferenceFavorite();
            model.RefCode = viewModel.RefCode;
            model.RefID = viewModel.RefID;
            model.FullRTF = viewModel.RefRTF;
            return new ReferenceFavoriteViewModel(model);
        }

        public FavoriteViewModel<ReferenceFavorite> CreateFavoriteViewModel(ReferenceFavorite model) {
            return new ReferenceFavoriteViewModel(model);
        }

        public DatabaseCommand GetInsertAction(FavoriteViewModel<ReferenceFavorite> favViewModel) {
            return new InsertReferenceFavoriteCommand(favViewModel.Model);
        }

        public DatabaseCommand RenameViewModel(ReferenceSearchResultViewModel vm, string text) {
            throw new NotImplementedException();
        }

        public DatabaseCommand RenameFavorite(FavoriteViewModel<ReferenceFavorite> vm, string text) {
            throw new NotImplementedException();
        }

        public User User { get; private set; }

        public ReferenceManager RefManager{ get; private set; }

    }

}
