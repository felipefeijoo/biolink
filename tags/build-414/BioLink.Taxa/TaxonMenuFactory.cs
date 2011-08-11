using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using BioLink.Client.Extensibility;
using BioLink.Client.Utilities;
using System.Windows.Input;
using BioLink.Data.Model;
using BioLink.Data;

namespace BioLink.Client.Taxa {

    internal class TaxonMenuFactory {

        private MenuItemBuilder _builder;

        public TaxonMenuFactory(TaxonViewModel taxon, TaxonExplorer explorer, MessageFormatterFunc formatter) {
            this.Taxon = taxon;
            this.Explorer = explorer;
            this.FormatterFunc = formatter;
            _builder = new MenuItemBuilder(formatter);
        }

        public ContextMenu BuildExplorerMenu() {

            ContextMenuBuilder builder = new ContextMenuBuilder(FormatterFunc);
            

            if (Explorer.IsUnlocked) {
                if (!Taxon.IsRootNode) {
                    builder.New("TaxonExplorer.menu.Delete", Taxon.DisplayLabel).Handler(() => { Explorer.DeleteTaxon(Taxon); });
                    builder.New("TaxonExplorer.menu.Rename", Taxon.DisplayLabel).Handler(() => { Explorer.RenameTaxon(Taxon); });
                    builder.Separator();
                    builder.New("TaxonExplorer.menu.ChangeRank", Taxon.DisplayLabel).Handler(() => { Explorer.ChangeRank(Taxon); });
                    if (Taxon.AvailableName == true) {
                        builder.New("TaxonExplorer.menu.ChangeToLiterature", Taxon.DisplayLabel).Handler(() => { Explorer.ChangeAvailable(Taxon); });
                    } else if (Taxon.LiteratureName == true) {
                        builder.New("TaxonExplorer.menu.ChangeToAvailable", Taxon.DisplayLabel).Handler(() => { Explorer.ChangeAvailable(Taxon); });
                    } 

                }

                MenuItem addMenu = BuildAddMenuItems();
                if (addMenu != null && addMenu.Items.Count > 0) {
                    builder.Separator();
                    builder.AddMenuItem(addMenu);
                }

            } else {
                builder.New("TaxonExplorer.menu.Unlock").Handler(() => { Explorer.btnLock.IsChecked = true; });
            }

            builder.Separator();

            builder.New("TaxonExplorer.menu.ExpandAll").Handler(() => {
                JobExecutor.QueueJob(() => {
                    Explorer.tvwAllTaxa.InvokeIfRequired(() => {
                        try {
                            using (new OverrideCursor(Cursors.Wait)) {                                
                                Explorer.ExpandChildren(Taxon);
                            }
                        } catch (Exception ex) {
                            GlobalExceptionHandler.Handle(ex);
                        } 
                    });
                });
            });

            MenuItem sortMenu = BuildSortMenuItems();
            if (sortMenu != null && sortMenu.HasItems) {
                builder.Separator();
                builder.AddMenuItem(sortMenu);
            }

            builder.Separator();
            builder.AddMenuItem(CreateFavoriteMenuItems());

            if (!Explorer.IsUnlocked) {
                builder.Separator();                
                builder.New("TaxonExplorer.menu.Refresh").Handler(() => Explorer.Refresh()).End();
            }

            if (!Taxon.IsRootNode) {                
                MenuItem reports = CreateReportMenuItems();
                if (reports != null && reports.HasItems) {
                    builder.Separator();
                    builder.New("Distribution _Map").Handler(() => { Explorer.DistributionMap(Taxon); }).End();
                    builder.AddMenuItem(reports);
                }

                builder.Separator();
                builder.New("Export (XML)").Handler(() => { Explorer.XMLIOExport(Taxon.TaxaID.Value); }).End();

                builder.Separator();
                var pinnable = Explorer.Owner.CreatePinnableTaxon(Taxon.TaxaID.Value);
                builder.New("_Pin to pin board").Handler(() => { PluginManager.Instance.PinObject(pinnable); }).End();
                builder.Separator();
                builder.New("_Permissions...").Handler(() => Explorer.EditBiotaPermissions(Taxon)).End();
                builder.Separator();                
                builder.New("_Edit Name...").Handler(() => { Explorer.EditTaxonName(Taxon); });
                builder.New("_Edit Details...").Handler(() => { Explorer.EditTaxonDetails(Taxon.TaxaID); }).End();
            }

            return builder.ContextMenu;        
        }

        private MenuItem CreateFavoriteMenuItems() {
            MenuItem add = _builder.New("Add to favorites").MenuItem;
            add.Items.Add(_builder.New("User specific").Handler(() => { Explorer.AddToFavorites(Taxon, false); }).MenuItem);
            add.Items.Add(_builder.New("Global").Handler(() => { Explorer.AddToFavorites(Taxon, true); }).MenuItem);
            return add;
        }

        private MenuItem CreateReportMenuItems() {
            MenuItem reports = _builder.New("Reports").MenuItem;
            var taxa = new List<TaxonViewModel>();
            taxa.Add(Taxon);
            var list = Explorer.Owner.GetReportsForTaxon(taxa);
            foreach (IBioLinkReport report in list) {
                IBioLinkReport reportToExecute = report;
                reports.Items.Add(_builder.New(report.Name).Handler(() => { Explorer.RunReport(reportToExecute); }).MenuItem);
            }

            return reports;
        }

        private MenuItem BuildAddMenuItems() {

            MenuItem addMenu = _builder.New("TaxonExplorer.menu.Add").MenuItem;

            if (Taxon.AvailableName.GetValueOrDefault(false) || Taxon.LiteratureName.GetValueOrDefault(false)) {
                return null;
            }

            if (Taxon.TaxaParentID == -1) {
                TaxonRank rank = Explorer.Service.GetRankByOrder(1);
                if (rank != null) {
                    addMenu.Items.Add(_builder.New(rank.LongName).Handler(() => { Explorer.AddNewTaxon(Taxon, rank); }).MenuItem);
                    addMenu.Items.Add(_builder.New("TaxonExplorer.menu.Add.AllRanks").Handler(() => { Explorer.AddNewTaxonAllRanks(Taxon); }).MenuItem);
                }
            } else {
                switch (Taxon.ElemType) {
                    case "":
                        addMenu.Items.Add(_builder.New("Unranked Valid").Handler(() => { Explorer.AddUnrankedValid(Taxon); }).MenuItem);
                        break;
                    case TaxonRank.INCERTAE_SEDIS:
                    case TaxonRank.SPECIES_INQUIRENDA:
                        AddSpecialNameMenuItems(addMenu, true, false, false, false);
                        break;
                    default:

                        TaxonRank rank = Explorer.Service.GetTaxonRank(Taxon.ElemType);
                        if (rank != null) {
                            List<TaxonRank> validChildRanks = Explorer.Service.GetChildRanks(rank);
                            if (validChildRanks != null && validChildRanks.Count > 0) {
                                foreach (TaxonRank childRank in validChildRanks) {
                                    // The for loop variable is outside of the scope of the closure, so we need to create a local...
                                    TaxonRank closureRank = Explorer.Service.GetTaxonRank(childRank.Code);
                                    addMenu.Items.Add(_builder.New(childRank.LongName).Handler(() => {
                                        Explorer.AddNewTaxon(Taxon, closureRank);
                                    }).MenuItem);
                                }
                                addMenu.Items.Add(new Separator());
                            }

                            addMenu.Items.Add(_builder.New("Unranked Valid").Handler(() => { Explorer.AddUnrankedValid(Taxon); }).MenuItem);
                            addMenu.Items.Add(new Separator());
                            AddSpecialNameMenuItems(addMenu, rank.AvailableNameAllowed, rank.LituratueNameAllowed, rank.AvailableNameAllowed, rank.AvailableNameAllowed);
                            addMenu.Items.Add(new Separator());
                            foreach (TaxonRank childRank in validChildRanks) {
                                // The for loop variable is outside of the scope of the closure, so we need to create a local...
                                TaxonRank closureRank = Explorer.Service.GetTaxonRank(childRank.Code);
                                if (childRank.UnplacedAllowed.ValueOrFalse()) {
                                    addMenu.Items.Add(_builder.New("Unplaced " + childRank.LongName).Handler(() => { Explorer.AddNewTaxon(Taxon, closureRank, true); }).MenuItem);
                                }
                            }
                            
                        }
                        break;
                }
            }

            return addMenu;
        }

        private void AddSpecialNameMenuItems(MenuItem parentMenu, bool? availEnabled = true, bool? litEnabled = true, bool? ISEnabled = true, bool? SIEnabled = true) {            
            parentMenu.Items.Add(_builder.New("TaxonExplorer.menu.Add.AvailableName").Handler(() => { Explorer.AddAvailableName(Taxon); }).Enabled(availEnabled.ValueOrFalse()).MenuItem);
            parentMenu.Items.Add(_builder.New("TaxonExplorer.menu.Add.LiteratureName").Handler(() => { Explorer.AddLiteratureName(Taxon); }).Enabled(litEnabled.ValueOrFalse()).MenuItem);
            parentMenu.Items.Add(_builder.New("TaxonExplorer.menu.Add.IncertaeSedis").Handler(() => { Explorer.AddIncertaeSedis(Taxon); }).Enabled(ISEnabled.ValueOrFalse()).MenuItem);
            parentMenu.Items.Add(_builder.New("TaxonExplorer.menu.Add.SpeciesInquirenda").Handler(() => { Explorer.AddSpeciesInquirenda(Taxon); }).Enabled(SIEnabled.ValueOrFalse()).MenuItem);
        }


        private MenuItem BuildSortMenuItems() {

            MenuItem sort = _builder.New("Sort").MenuItem;
            sort.Items.Add(_builder.New("By Name").Handler(() => { Explorer.SetManualSortMode(false); }).Checked(!TaxonExplorer.IsManualSort).MenuItem);
            sort.Items.Add(_builder.New("Manual").Handler(() => { Explorer.SetManualSortMode(true); }).Checked(TaxonExplorer.IsManualSort).MenuItem);
            sort.Items.Add(new Separator());
            sort.Items.Add(_builder.New("Shift Up").Handler(() => { Explorer.ShiftTaxonUp(Taxon); }).Enabled(TaxonExplorer.IsManualSort && Explorer.IsUnlocked).MenuItem);
            sort.Items.Add(_builder.New("Shift Down").Handler(() => { Explorer.ShiftTaxonDown(Taxon); }).Enabled(TaxonExplorer.IsManualSort && Explorer.IsUnlocked).MenuItem);

            return sort;
        }

        internal ContextMenu BuildFindResultsMenu() {
            var builder = new ContextMenuBuilder(FormatterFunc);

            builder.New("TaxonExplorer.menu.ShowInContents").Handler(() => { Explorer.ShowInExplorer(Taxon.TaxaID); }).End();

            builder.Separator();

            if (Explorer.IsUnlocked) {                
                if (!Taxon.IsRootNode) {
                    builder.New("TaxonExplorer.menu.Delete", Taxon.DisplayLabel).Handler(() => { Explorer.DeleteTaxon(Taxon); });
                    builder.New("TaxonExplorer.menu.Rename", Taxon.DisplayLabel).Handler(() => { Explorer.RenameTaxon(Taxon); });
                    builder.Separator();
                    builder.New("TaxonExplorer.menu.ChangeRank", Taxon.DisplayLabel).Handler(() => { Explorer.ChangeRank(Taxon); });
                    if (Taxon.AvailableName == true) {
                        builder.New("TaxonExplorer.menu.ChangeToLiterature", Taxon.DisplayLabel).Handler(() => { Explorer.ChangeAvailable(Taxon); });
                    } else if (Taxon.LiteratureName == true) {
                        builder.New("TaxonExplorer.menu.ChangeToAvailable", Taxon.DisplayLabel).Handler(() => { Explorer.ChangeAvailable(Taxon); });
                    } 
                }

                MenuItem addMenu = BuildAddMenuItems();
                if (addMenu != null && addMenu.Items.Count > 0) {
                    builder.Separator();
                    builder.AddMenuItem(addMenu);
                }

            } else {
                builder.New("TaxonExplorer.menu.Unlock").Handler(() => { Explorer.btnLock.IsChecked = true; });
            }

            MenuItem reports = CreateReportMenuItems();            
            if (reports != null && reports.HasItems) {
                if (builder.HasItems) {
                    builder.Separator();
                }
                builder.New("Distribution _Map").Handler(() => { Explorer.DistributionMap(Taxon); }).End();
                builder.AddMenuItem(reports);
            }

            builder.Separator();
            builder.New("_Pin to pin board").Handler(() => { PluginManager.Instance.PinObject(new PinnableObject(TaxaPlugin.TAXA_PLUGIN_NAME, LookupType.Taxon, Taxon.TaxaID.Value)); }).End();
            builder.Separator();
            builder.New("_Permissions...").Handler(() => Explorer.EditBiotaPermissions(Taxon)).End();
            builder.Separator();
            builder.New("_Edit Name...").Handler(() => { Explorer.EditTaxonName(Taxon); }).End();
            builder.New("_Edit Details...").Handler(() => { Explorer.EditTaxonDetails(Taxon.TaxaID); }).End();

            return builder.ContextMenu;
        }

        internal ContextMenu BuildFavoritesMenu(HierarchicalViewModelBase node) {

            int? favoriteId = null;
            bool isFavoriteGroup = false;
            if (node is TaxonFavoriteViewModel) {
                var fav = node as TaxonFavoriteViewModel;
                favoriteId = fav.FavoriteID;
                isFavoriteGroup = fav.IsGroup; 
            }

            ContextMenuBuilder builder = new ContextMenuBuilder(FormatterFunc);
            builder.New("TaxonExplorer.menu.ShowInContents").Handler(() => { Explorer.ShowInExplorer(Taxon.TaxaID); });

            MenuItem reports = CreateReportMenuItems();            
            if (reports != null && reports.HasItems) {
                builder.Separator();
                builder.New("Distribution _Map").Handler(() => { Explorer.DistributionMap(Taxon); }).End();
                builder.AddMenuItem(reports);
            }            

            if (favoriteId != null && favoriteId.HasValue) {
                builder.Separator();
                if (isFavoriteGroup) {
                    builder.New("Rename group").Handler(() => { Explorer.RenameFavoriteGroup(node as TaxonFavoriteViewModel); });
                    builder.New("Add favorite group").Handler(() => { Explorer.AddFavoriteGroup(node); }).End();
                }
                
                builder.New("Remove from favorites").Handler(() => { Explorer.RemoveFromFavorites(favoriteId.Value); });
            }

            builder.Separator();
            builder.New("_Pin to pin board").Handler(() => { PluginManager.Instance.PinObject(new PinnableObject(TaxaPlugin.TAXA_PLUGIN_NAME, LookupType.Taxon, Taxon.TaxaID.Value)); });
            builder.Separator();
            builder.New("_Permissions...").Handler(() => Explorer.EditBiotaPermissions(Taxon)).End();
            builder.Separator();
            builder.New("_Edit Name...").Handler(() => { Explorer.EditTaxonName(Taxon); });
            builder.New("_Edit Details...").Handler(() => { Explorer.EditTaxonDetails(Taxon.TaxaID); }).End();

            return builder.ContextMenu;
        }

        #region properties

        protected TaxonViewModel Taxon { get; private set; }
        protected TaxonExplorer Explorer { get; private set; }
        protected MessageFormatterFunc FormatterFunc { get; private set; }

        #endregion

    }
}
