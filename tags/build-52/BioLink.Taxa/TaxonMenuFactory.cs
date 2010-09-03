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
            ContextMenu menu = new ContextMenu();

            if (Explorer.IsUnlocked) {
                if (!Taxon.IsRootNode) {                    
                    menu.Items.Add(_builder.New("TaxonExplorer.menu.Delete", Taxon.DisplayLabel).Handler(() => { Explorer.DeleteTaxon(Taxon); }).MenuItem);
                    menu.Items.Add(_builder.New("TaxonExplorer.menu.Rename", Taxon.DisplayLabel).Handler(() => { Explorer.RenameTaxon(Taxon); }).MenuItem);
                }

                MenuItem addMenu = BuildAddMenuItems();
                if (addMenu != null && addMenu.Items.Count > 0) {
                    menu.Items.Add(new Separator());
                    menu.Items.Add(addMenu);
                }

            } else {
                menu.Items.Add(_builder.New("TaxonExplorer.menu.Unlock").Handler(() => { Explorer.btnLock.IsChecked = true; }).MenuItem);
            }

            if (menu.HasItems) {
                menu.Items.Add(new Separator());
            }

            menu.Items.Add(_builder.New("TaxonExplorer.menu.ExpandAll").Handler(() => {
                JobExecutor.QueueJob(() => {
                    Explorer.tvwAllTaxa.InvokeIfRequired(() => {
                        Explorer.tvwAllTaxa.Cursor = Cursors.Wait;
                        Explorer.ExpandChildren(Taxon);
                        Explorer.tvwAllTaxa.Cursor = Cursors.Arrow;
                    });
                });
            }).MenuItem);
            

            MenuItem sortMenu = BuildSortMenuItems();
            if (sortMenu != null && sortMenu.HasItems) {
                menu.Items.Add(new Separator());
                menu.Items.Add(sortMenu);
            }

            if (!Explorer.IsUnlocked) {
                menu.Items.Add(new Separator());
                menu.Items.Add(_builder.New("TaxonExplorer.menu.Refresh").Handler(() => Explorer.Refresh()).MenuItem);
            }

            if (!Taxon.IsRootNode) {
                MenuItem reports = CreateReportMenuItems();
                if (reports != null && reports.HasItems) {
                    if (menu.HasItems) {
                        menu.Items.Add(new Separator());
                    }
                    menu.Items.Add(reports);
                }

                if (menu.HasItems) {
                    menu.Items.Add(new Separator());
                }
                menu.Items.Add(_builder.New("_Edit Name...").Handler(() => { Explorer.EditTaxonName(Taxon); }).MenuItem);
                menu.Items.Add(_builder.New("_Edit Details...").Handler(() => { Explorer.ShowTaxonDetails(Taxon); }).MenuItem);
            }

            return menu;
        
        }

        private MenuItem CreateReportMenuItems() {
            MenuItem reports = _builder.New("Reports").MenuItem;

            List<IBioLinkReport> list = new List<IBioLinkReport>();

            list.Add(new TaxonStatisticsReport(Explorer.User, Taxon));
            list.Add(new MaterialForTaxonReport(Explorer.User, Taxon));

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
            ContextMenu menu = new ContextMenu();

            menu.Items.Add(_builder.New("TaxonExplorer.menu.ShowInContents").Handler(() => { Explorer.ShowInExplorer(Taxon); }).MenuItem);            
            MenuItem reports = CreateReportMenuItems();
            if (reports != null && reports.HasItems) {
                if (menu.HasItems) {
                    menu.Items.Add(new Separator());
                }
                menu.Items.Add(reports);
            }

            menu.Items.Add(new Separator());
            menu.Items.Add(_builder.New("_Edit Name...").Handler(() => { Explorer.EditTaxonName(Taxon); }).MenuItem);
            menu.Items.Add(_builder.New("_Edit Details...").Handler(() => { Explorer.ShowTaxonDetails(Taxon); }).MenuItem);

            return menu;
        }

        #region properties

        protected TaxonViewModel Taxon { get; private set; }
        protected TaxonExplorer Explorer { get; private set; }
        protected MessageFormatterFunc FormatterFunc { get; private set; }

        #endregion

    }
}
