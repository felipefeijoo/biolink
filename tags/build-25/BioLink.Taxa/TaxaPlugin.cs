using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Client.Extensibility;
using BioLink.Client.Utilities;
using System.Resources;
using System.Windows;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using BioLink.Data;
using BioLink.Data.Model;

namespace BioLink.Client.Taxa {

    public class TaxaPlugin : BiolinkPluginBase {

        private ExplorerWorkspaceContribution<TaxonExplorer> _explorer;
        private TaxaService _taxaService;        

        public TaxaPlugin(User user, PluginManager pluginManager)
            : base(user, pluginManager) {
            Debug.Assert(user != null, "User is null!");
            _taxaService = new TaxaService(user);
        }

        public override string Name {
            get { return "Taxa"; }
        }

        public TaxaService Service {
            get { return _taxaService; }
        }

        public override List<IWorkspaceContribution> Contributions {
            get {

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
            }
        }

        public List<string> GetExpandedParentages<T>(ObservableCollection<T> model)  where T : HierarchicalViewModelBase {
            List<string> list = new List<string>();
            ProcessList(model, list);
            return list;
        }

        private void ProcessList<T>(ObservableCollection<T> model, List<string> list) where T : HierarchicalViewModelBase {
            foreach (HierarchicalViewModelBase m in model) {
                if (m.IsExpanded && m is TaxonViewModel) {
                    TaxonViewModel tvm = m as TaxonViewModel;
                    list.Add(tvm.GetParentage());
                    if (m.Children != null && m.Children.Count > 0) {
                        ProcessList(m.Children, list);
                    }
                }
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
