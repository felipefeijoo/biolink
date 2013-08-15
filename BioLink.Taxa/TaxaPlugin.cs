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
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using BioLink.Client.Extensibility;
using BioLink.Client.Utilities;
using BioLink.Data;
using BioLink.Data.Model;

namespace BioLink.Client.Taxa {

    public class TaxaPlugin : BiolinkPluginBase {

        public const string TAXA_PLUGIN_NAME = "Taxa";

        private ExplorerWorkspaceContribution<TaxonExplorer> _explorer;
        private TaxaService _taxaService;
        private XMLIOImportOptions _xmlImportOptions;

        private ControlHostWindow _regionExplorer;

        public override void  InitializePlugin(User user, PluginManager pluginManager, Window parentWindow) {
 	        base.InitializePlugin(user, pluginManager, parentWindow);
            _taxaService = new TaxaService(user);            
        }

        public override string Name {
            get { return TAXA_PLUGIN_NAME; }
        }

        public TaxaService Service {
            get { return _taxaService; }
        }

        public override List<IWorkspaceContribution> GetContributions() {            
            var contrib = new List<IWorkspaceContribution> {
                                                               new MenuWorkspaceContribution(this, "ShowExplorer", (obj, e) => PluginManager.EnsureVisible(this, "TaxonExplorer"),
                                                                                             String.Format("{{'Name':'View', 'Header':'{0}','InsertAfter':'File'}}", _R("Taxa.Menu.View")),
                                                                                             String.Format("{{'Name':'ShowTaxaExplorer', 'Header':'{0}'}}", _R("Taxa.Menu.ShowExplorer"))
                                                                   ), new MenuWorkspaceContribution(this, "ShowDistributionRegionExplorer", (obj, e) => ShowRegionExplorer(),
                                                                                                    String.Format("{{'Name':'View', 'Header':'{0}','InsertAfter':'File'}}", _R("Taxa.Menu.View")),
                                                                                                    String.Format("{{'Name':'ShowDistributionRegionExplorer', 'Header':'Show Distribution Region Explorer'}}")
                                                                          ), new MenuWorkspaceContribution(this, "ShowXMLImport", (obj, e) => ShowXMLImport(),
                                                                                                           "{'Name':'Tools', 'Header':'Tools','InsertAfter':'View'}",
                                                                                                           "{'Name':'Import', 'Header':'Import'}",
                                                                                                           "{'Name':'ShowXMLImport', 'Header':'XML Import'}")
                                                           };


            _explorer = new ExplorerWorkspaceContribution<TaxonExplorer>(this, "TaxonExplorer", new TaxonExplorer(this), _R("TaxonExplorer.Title"), explorer => explorer.InitialiseTaxonExplorer());

            contrib.Add(_explorer);

            return contrib;            
        }

        private void ShowXMLImport() {
            User.CheckPermission(PermissionCategory.IMPORT_MATERIAL, PERMISSION_MASK.ALLOW, "You do not have sufficient privileges to import into BioLink!");

            if (_xmlImportOptions == null) {
                _xmlImportOptions = new XMLIOImportOptions {Owner = PluginManager.Instance.ParentWindow, WindowStartupLocation = WindowStartupLocation.CenterOwner};
                _xmlImportOptions.Closed += (sender, args) => { _xmlImportOptions = null; };
            }

            _xmlImportOptions.Show();
        }

        public override bool RequestShutdown() {
            if (_explorer != null && _explorer.Content != null) {
                var explorer = _explorer.Content as TaxonExplorer;
                if (explorer != null && explorer.AnyChanges()) {
                    return explorer.Question(_R("TaxonExplorer.prompt.ShutdownDiscardChanges"), _R("TaxonExplorer.prompt.ConfirmAction.Caption"));
                }
            }
            return true;
        }

        public override void Dispose() {
            base.Dispose();
            if (_explorer != null && _explorer.Content != null) {

                if (Config.GetGlobal("Taxa.RememberExpandedTaxa", true)) {
                    List<string> expandedElements = GetExpandedParentages(_explorer.ContentControl.ExplorerModel);
                    if (expandedElements != null) {
                        Config.SetProfile(User, "Taxa.Explorer.ExpandedTaxa", expandedElements);
                    }
                }

                Config.SetUser(User, "Taxa.ManualSort", TaxonExplorer.IsManualSort);

            }

            if (_xmlImportOptions != null) {
                _xmlImportOptions.Close();                
            }
        }

        public List<string> GetExpandedParentages(ObservableCollection<HierarchicalViewModelBase> model) {
            var list = new List<string>();
            ProcessList(model, list);
            return list;
        }

        private void ProcessList(IEnumerable<HierarchicalViewModelBase> model, List<string> list) {
            foreach (TaxonViewModel tvm in model) {
                if (tvm.IsExpanded) {
                    list.Add(tvm.GetParentage());
                    if (tvm.Children != null && tvm.Children.Count > 0) {
                        ProcessList(tvm.Children, list);
                    }
                }
            }
        }

        public override ViewModelBase CreatePinnableViewModel(PinnableObject pinnable) {
            if (pinnable != null && pinnable.LookupType == LookupType.Taxon) {
                Taxon t = Service.GetTaxon(pinnable.ObjectID);
                if (t != null) {
                    var m = new TaxonViewModel(null, t, tt => tt.TaxaFullName);
                    return m;
                }                
            }

            if (pinnable != null && pinnable.LookupType == LookupType.DistributionRegion) {
                var service = new SupportService(User);
                var region = service.GetDistributionRegion(pinnable.ObjectID);
                if (region != null) {
                    return new DistributionRegionViewModel(region);
                }
            }

            return null;            
        }

        public override List<Command> GetCommandsForSelected(List<ViewModelBase> selected) {
            var list = new List<Command>();

            if (selected == null || selected.Count == 0) {
                return list;
            }

            var taxon = selected[0] as TaxonViewModel;
            if (taxon != null) {

                list.Add(new Command("Show in explorer", dataobj => {
                    PluginManager.EnsureVisible(this, "TaxonExplorer");
                    _explorer.ContentControl.ShowInExplorer(taxon.TaxaID); 
                }));

                var reports = GetReportsForTaxon(selected.ConvertAll(vm => vm as TaxonViewModel));
                if (reports.Count > 0) {
                    list.Add(new CommandSeparator());
                    list.AddRange(reports.Select(report => new Command(report.Name, dataobj => _explorer.ContentControl.RunReport(report))));
                }

                list.Add(new CommandSeparator());
                list.Add(new Command("Edit Name...", dataobj => _explorer.ContentControl.EditTaxonName(taxon)));
                list.Add(new Command("Edit Details...", dataobj => _explorer.ContentControl.EditTaxonDetails(taxon.TaxaID)) { IsDefaultCommand = true });
            }

            if (selected[0] is DistributionRegionViewModel) {
                var region = selected[0] as DistributionRegionViewModel;
                list.Add(new Command("Taxa for Distribution Region Report", dataobj => {
                    if (region != null) {
                        PluginManager.Instance.RunReport(this, new TaxaForDistributionRegionReport(User, region.Model));
                    }
                }));
            }
            return list;
        }

        public List<IBioLinkReport> GetReportsForTaxon(List<TaxonViewModel> taxa) {
            var list = new List<IBioLinkReport> {new MaterialForTaxonReport(User, taxa[0]), new TypeListReport(User, taxa[0]), new TaxaAssociatesReport(User, taxa), new SiteForTaxaReport(User, taxa[0]), new ChecklistReport(User, taxa[0]), new TaxonStatisticsReport(User, taxa[0]), new MultimediaReport(User, taxa[0], TraitCategoryType.Taxon), new TaxonReferencesReport(User, taxa[0].Taxon) };

            return list;
        }

        public PinnableObject CreatePinnableTaxon(int taxonID) {
            return new PinnableObject(TAXA_PLUGIN_NAME, LookupType.Taxon, taxonID);
        }

        public override bool CanSelect<T>() {
            var t = typeof(T);
            return typeof(Taxon).IsAssignableFrom(t) || typeof(DistributionRegion).IsAssignableFrom(t);
        }

        public override void Select<T>(LookupOptions options, Action<SelectionResult> success, SelectOptions selectOptions) {
            var t = typeof(T);
            IHierarchicalSelectorContentProvider selectionContent;
            if (typeof(Taxon).IsAssignableFrom(t)) {
                selectionContent = new TaxonSelectorContentProvider(User, _explorer.Content as TaxonExplorer, options);
            } else {
                throw new Exception("Unhandled Selection Type: " + t.Name);
            }


            var frm = new HierarchicalSelector(User, selectionContent, success) {Owner = ParentWindow, WindowStartupLocation = WindowStartupLocation.CenterOwner};
            frm.ShowDialog();
         
        }

        public override bool CanEditObjectType(LookupType type) {
            return type == LookupType.Taxon;
        }

        public override void EditObject(LookupType type, int objectID) {
            if (type == LookupType.Taxon) {
                var taxonExplorer = _explorer.Content as TaxonExplorer;
                if (taxonExplorer != null) {
                    taxonExplorer.EditTaxonDetails(objectID);
                }
            }
        }

        public override T GetAdaptorForPinnable<T>(PinnableObject pinnable) {
            if (pinnable.LookupType == LookupType.Taxon) {
                if (typeof(T) == typeof(IMapPointSetGenerator)) {
                    var taxon = Service.GetTaxon(pinnable.ObjectID);
                    object generator = new DelegatingPointSetGenerator<Taxon>(_explorer.ContentControl.GenerateSpecimenPoints, taxon);
                    return (T) generator;
                }
            }

            return base.GetAdaptorForPinnable<T>(pinnable);
        }

        public void ShowRegionExplorer(Action<SelectionResult> selectionFunc = null) {
            if (_regionExplorer == null) {
                var explorer = new DistributionRegionExplorer(this, User);
                _regionExplorer = PluginManager.Instance.AddNonDockableContent(this, explorer, "Distribution Region Explorer", SizeToContent.Manual);

                _regionExplorer.Closed += (sender, e) => {
                                              _regionExplorer.Dispose();
                                              _regionExplorer = null;
                                          };
            }

            if (_regionExplorer != null) {
                if (selectionFunc != null) {
                    _regionExplorer.BindSelectCallback(selectionFunc);
                }
                _regionExplorer.Show();
            }

        }

    }

    public delegate void TaxonViewModelAction(TaxonViewModel taxon);

    public class IllegalTaxonMoveException : Exception {

        public IllegalTaxonMoveException(Taxon source, Taxon dest, string message)
            : base(message) {
            SourceTaxon = source;
            DestinationTaxon = dest;
        }

        public Taxon SourceTaxon { get; private set; }
        public Taxon DestinationTaxon { get; private set; }
    }

}
