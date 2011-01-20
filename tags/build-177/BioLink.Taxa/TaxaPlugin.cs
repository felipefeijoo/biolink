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

            _explorer = new ExplorerWorkspaceContribution<TaxonExplorer>(this, "TaxonExplorer", new TaxonExplorer(this), _R("TaxonExplorer.Title"),
                (explorer) => {
                    explorer.InitialiseTaxonExplorer();
                });

            contrib.Add(_explorer);

            return contrib;            
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

            return null;            
        }

        public override List<Command> GetCommandsForObject(ViewModelBase obj) {
            var list = new List<Command>();

            if (obj is TaxonViewModel) {

                var taxon = obj as TaxonViewModel;

                
                list.Add(new Command("Show in explorer", (dataobj) => { _explorer.ContentControl.ShowInExplorer(taxon.TaxaID); }));

                var reports = GetReportsForTaxon(taxon);
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
                list.Add(new Command("Edit Name...", (dataobj) => { _explorer.ContentControl.EditTaxonName(taxon.TaxaID); }));
                list.Add(new Command("Edit Details...", (dataobj) => { _explorer.ContentControl.ShowTaxonDetails(taxon.TaxaID); }));
            }
            return list;
        }

        public List<IBioLinkReport> GetReportsForTaxon(TaxonViewModel taxon) {
            List<IBioLinkReport> list = new List<IBioLinkReport>();

            list.Add(new TaxonStatisticsReport(User, taxon));
            list.Add(new MaterialForTaxonReport(User, taxon));

            return list;
        }

        public PinnableObject CreatePinnableTaxon(int taxonID) {
            return new PinnableObject(TAXA_PLUGIN_NAME, LookupType.Taxon, taxonID);
        }

        public override bool CanSelect(Type t) {
            return typeof(Taxon).IsAssignableFrom(t);
        }

        public override void Select(Type t, Action<SelectionResult> success) {
            IHierarchicalSelectorContentProvider selectionContent = null;
            if (typeof(Taxon).IsAssignableFrom(t)) {
                selectionContent = new TaxonSelectorContentProvider(User, _explorer.Content as TaxonExplorer);
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
                (_explorer.Content as TaxonExplorer).ShowTaxonDetails(objectID);
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
