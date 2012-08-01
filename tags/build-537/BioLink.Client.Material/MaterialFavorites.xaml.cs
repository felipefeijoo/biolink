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
using BioLink.Client.Utilities;
using BioLink.Client.Extensibility;
using BioLink.Data;
using BioLink.Data.Model;


namespace BioLink.Client.Material {
    /// <summary>
    /// Interaction logic for MaterialFavorites.xaml
    /// </summary>
    public partial class MaterialFavorites : FavoritesControl<SiteFavorite, SiteExplorerNodeViewModel> {

        #region Designer constructor
        public MaterialFavorites() {
            InitializeComponent();
        }
        #endregion

        public MaterialFavorites(User user, MaterialExplorer explorer)
            : base(user) {
            InitializeComponent();
            var provider = new MaterialFavoritesProvider(explorer, user, tvwFavorites);
            BindProvider(provider);
        }

        private void FavoriteName_EditingComplete(object sender, string text) {
            CompleteRename(sender, text);
        }
    }

    public class MaterialFavoritesProvider : IFavoritesProvider<SiteFavorite, SiteExplorerNodeViewModel> {

        public MaterialFavoritesProvider(MaterialExplorer explorer, User user, TreeView tree) {
            this.Explorer = explorer;
            this.FavoritesTree = tree;
            this.User = user;
        }

        public PinnableObject CreatePinnableObject(FavoriteViewModel<SiteFavorite> selected) {
            return Explorer.CreatePinnable(selected.Model.ID2, selected.Model.ID1);
        }

        public PinnableObject CreatePinnableObject(SiteExplorerNodeViewModel viewModel) {
            return Explorer.CreatePinnable(viewModel);
        }

        public List<SiteFavorite> GetTopFavorites(bool global) {
            var service = new SupportService(User);
            return service.GetTopSiteFavorites(global);
        }

        public List<SiteFavorite> GetFavoritesForParent(int parentID, bool global) {
            var service = new SupportService(User);
            return service.GetSiteFavorites(parentID, global);
        }

        public FavoriteViewModel<SiteFavorite> CreateViewModel(SiteFavorite model) {
            return new SiteFavoriteViewModel(model);
        }

        public List<HierarchicalViewModelBase> GetChildViewModels(FavoriteViewModel<SiteFavorite> parent) {
            var fav = parent as SiteFavoriteViewModel;
            var list = new List<HierarchicalViewModelBase>();
            if (fav != null) {
                var service = new MaterialService(User);
                var models = service.GetExplorerElementsForParent(fav.ElemID, fav.ElemType);
                list.AddRange(models.ConvertAll((m) => {
                    return new SiteExplorerNodeViewModel(m);
                }));
            }

            return list;
        }

        public List<HierarchicalViewModelBase> GetChildViewModels(SiteExplorerNodeViewModel parent) {
            var list = new List<HierarchicalViewModelBase>();
            var service = new MaterialService(User);
            var models = service.GetExplorerElementsForParent(parent.ElemID, parent.ElemType);
            list.AddRange(models.ConvertAll((m) => {
                return new SiteExplorerNodeViewModel(m);
            }));
            return list;
        }


        public ContextMenu GetContextMenu(HierarchicalViewModelBase selected) {
            if (selected is SiteExplorerNodeViewModel) {
                return SiteExplorerMenuBuilder.Build(selected as SiteExplorerNodeViewModel, Explorer);
            } else if (selected is SiteFavoriteViewModel) {
                var fav = selected as SiteFavoriteViewModel;
                return SiteExplorerMenuBuilder.BuildForFavorite(fav, Explorer);
            }

            return null;            
        }


        public FavoriteViewModel<SiteFavorite> CreateFavoriteViewModel(SiteExplorerNodeViewModel viewModel) {
            var model = new SiteFavorite();
            model.ID1 = viewModel.ElemID;
            model.ID2 = viewModel.ElemType;
            model.ElemID = viewModel.ElemID;
            model.ElemType = viewModel.ElemType;
            model.Name = viewModel.Name;
            return new SiteFavoriteViewModel(model);
        }

        public FavoriteViewModel<SiteFavorite> CreateFavoriteViewModel(SiteFavorite model) {
            return new SiteFavoriteViewModel(model);
        }

        public DatabaseCommand GetInsertAction(FavoriteViewModel<SiteFavorite> favViewModel) {
            return new InsertSiteFavoriteCommand(favViewModel.Model);
        }

        public DatabaseCommand RenameViewModel(SiteExplorerNodeViewModel vm, string text) {
            var action = Explorer.GetRenameActionForNode(vm);
            if (action != null) {
                vm.Name = text;
                return action;
            }
            return null;
        }

        public DatabaseCommand RenameFavorite(FavoriteViewModel<SiteFavorite> vm, string text) {
            if (!vm.IsGroup) {
                var fav = vm as SiteFavoriteViewModel;                
                SiteExplorerNode model = new SiteExplorerNode();
                model.ElemID = fav.ElemID;
                model.ElemType = fav.ElemType;
                model.Name = fav.Name;
                var node = new SiteExplorerNodeViewModel(model);
                var action = Explorer.GetRenameActionForNode(node);
                if (action != null) {
                    model.Name = text;
                    fav.Name = text;
                    return action;
                }
            }
            return null;
        }

        public TreeView FavoritesTree { get; private set; }
        internal User User { get; private set; }
        internal MaterialExplorer Explorer { get; private set; }



    }
}
