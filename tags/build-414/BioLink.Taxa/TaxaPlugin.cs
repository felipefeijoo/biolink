using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        public TaxaPlugin() {
        }

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
            List<IWorkspaceContribution> contrib = new List<IWorkspaceContribution>();

            contrib.Add(new MenuWorkspaceContribution(this, "ShowExplorer", (obj, e) => { PluginManager.EnsureVisible(this, "TaxonExplorer"); },
                String.Format("{{'Name':'View', 'Header':'{0}','InsertAfter':'File'}}", _R("Taxa.Menu.View")),
                String.Format("{{'Name':'ShowTaxaExplorer', 'Header':'{0}'}}", _R("Taxa.Menu.ShowExplorer"))
            ));

            contrib.Add(new MenuWorkspaceContribution(this, "ShowDistributionRegionExplorer", (obj, e) => { ShowRegionExplorer(); },
                String.Format("{{'Name':'View', 'Header':'{0}','InsertAfter':'File'}}", _R("Taxa.Menu.View")),
                String.Format("{{'Name':'ShowDistributionRegionExplorer', 'Header':'Show Distribution Region Explorer'}}")
            ));

            contrib.Add(new MenuWorkspaceContribution(this, "ShowXMLImport", (obj, e) => { ShowXMLImport(); },
                "{'Name':'Tools', 'Header':'Tools','InsertAfter':'View'}",
                "{'Name':'Import', 'Header':'Import'}",
                "{'Name':'ShowXMLImport', 'Header':'XML Import'}"));


            _explorer = new ExplorerWorkspaceContribution<TaxonExplorer>(this, "TaxonExplorer", new TaxonExplorer(this), _R("TaxonExplorer.Title"),
                (explorer) => {
                    explorer.InitialiseTaxonExplorer();
                });

            contrib.Add(_explorer);

            return contrib;            
        }

        private void ShowXMLImport() {
            if (_xmlImportOptions == null) {
                _xmlImportOptions = new XMLIOImportOptions();
                _xmlImportOptions.Owner = PluginManager.Instance.ParentWindow;
                _xmlImportOptions.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                _xmlImportOptions.Closed += new EventHandler((sender, args) => {
                    _xmlImportOptions = null;
                });
            }

            _xmlImportOptions.Show();
        }

        public override bool RequestShutdown() {
            if (_explorer != null && _explorer.Content != null) {
                TaxonExplorer explorer = _explorer.Content as TaxonExplorer;
                if (explorer.AnyChanges()) {
                    return explorer.Question(_R("TaxonExplorer.prompt.ShutdownDiscardChanges"), _R("TaxonExplorer.prompt.ConfirmAction.Caption"));
                }
            }
            return true;
        }

        public override void Dispose() {
            base.Dispose();
            if (_explorer != null && _explorer.Content != null) {

                if (Config.GetGlobal<bool>("Taxa.RememberExpandedTaxa", true)) {
                    List<string> expandedElements = GetExpandedParentages(_explorer.ContentControl.ExplorerModel);
                    if (expandedElements != null) {
                        Config.SetProfile(User, "Taxa.Explorer.ExpandedTaxa", expandedElements);
                    }
                }

                Config.SetUser(User, "Taxa.ManualSort", TaxonExplorer.IsManualSort);

            }
        }

        public List<string> GetExpandedParentages(ObservableCollection<HierarchicalViewModelBase> model) {
            List<string> list = new List<string>();
            ProcessList(model, list);
            return list;
        }

        private void ProcessList(ObservableCollection<HierarchicalViewModelBase> model, List<string> list) {
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
                    TaxonViewModel m = new TaxonViewModel(null, t, _explorer.ContentControl.GenerateTaxonDisplayLabel);
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

            var obj = selected[0] as TaxonViewModel;
            if (obj != null) {

                var taxon = obj as TaxonViewModel;

                
                list.Add(new Command("Show in explorer", (dataobj) => { _explorer.ContentControl.ShowInExplorer(taxon.TaxaID); }));

                var reports = GetReportsForTaxon(selected.ConvertAll((vm) => { return vm as TaxonViewModel; }));
                if (reports.Count > 0) {
                    list.Add(new CommandSeparator());
                    foreach (IBioLinkReport loopreport in reports) {
                        var report = loopreport;
                        list.Add(new Command(report.Name, (dataobj) => {
                            _explorer.ContentControl.RunReport(report);
                        }));
                    }
                }

                list.Add(new CommandSeparator());
                list.Add(new Command("Edit Name...", (dataobj) => { _explorer.ContentControl.EditTaxonName(taxon); }));
                list.Add(new Command("Edit Details...", (dataobj) => { _explorer.ContentControl.EditTaxonDetails(taxon.TaxaID); }));
            }

            if (selected[0] is DistributionRegionViewModel) {
                var region = selected[0] as DistributionRegionViewModel;
                list.Add(new Command("Taxa for Distribution Region Report", (dataobj) => {
                    PluginManager.Instance.RunReport(this, new TaxaForDistributionRegionReport(User, region.Model));
                }));
            }
            return list;
        }

        public List<IBioLinkReport> GetReportsForTaxon(List<TaxonViewModel> taxa) {
            List<IBioLinkReport> list = new List<IBioLinkReport>();

            
            list.Add(new MaterialForTaxonReport(User, taxa[0]));
            list.Add(new TypeListReport(User, taxa[0]));
            list.Add(new TaxaAssociatesReport(User, taxa));
            list.Add(new SiteForTaxaReport(User, taxa[0]));
            list.Add(new ChecklistReport(User, taxa[0]));
            list.Add(new TaxonStatisticsReport(User, taxa[0]));
            list.Add(new MultimediaReport(User, taxa[0], TraitCategoryType.Taxon));


            return list;
        }

        public PinnableObject CreatePinnableTaxon(int taxonID) {
            return new PinnableObject(TAXA_PLUGIN_NAME, LookupType.Taxon, taxonID);
        }

        public override bool CanSelect<T>() {
            var t = typeof(T);
            return typeof(Taxon).IsAssignableFrom(t) || typeof(DistributionRegion).IsAssignableFrom(t);
        }

        public override void Select<T>(LookupOptions options, Action<SelectionResult> success) {
            var t = typeof(T);
            IHierarchicalSelectorContentProvider selectionContent = null;
            if (typeof(Taxon).IsAssignableFrom(t)) {
                selectionContent = new TaxonSelectorContentProvider(User, _explorer.Content as TaxonExplorer, options);
            } else {
                throw new Exception("Unhandled Selection Type: " + t.Name);
            }

            if (selectionContent != null) {
                var frm = new HierarchicalSelector(User, selectionContent, success);
                frm.Owner = ParentWindow;
                frm.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
                frm.ShowDialog();
            }
        }

        public override bool CanEditObjectType(LookupType type) {
            return type == LookupType.Taxon;
        }

        public override void EditObject(LookupType type, int objectID) {
            if (type == LookupType.Taxon) {
                (_explorer.Content as TaxonExplorer).EditTaxonDetails(objectID);
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

                _regionExplorer.Closed += new EventHandler((object sender, EventArgs e) => {
                    _regionExplorer.Dispose();
                    _regionExplorer = null;
                });
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
            this.SourceTaxon = source;
            this.DestinationTaxon = dest;
        }

        public Taxon SourceTaxon { get; private set; }
        public Taxon DestinationTaxon { get; private set; }
    }

}
