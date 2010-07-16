using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Client.Extensibility;
using BioLink.Client.Utilities;
using System.Resources;
using System.Windows;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using BioLink.Data;
using BioLink.Data.Model;

namespace BioLink.Client.Taxa {

    public class TaxaPlugin : BiolinkPluginBase {

        private ExplorerWorkspaceContribution<TaxonExplorer> _explorer;

        public TaxaPlugin(User user, PluginManager pluginManager) : base(user, pluginManager) {
            Debug.Assert(user != null, "User is null!");
        }

        public override string Name {
            get { return "Taxa"; }
        }

        public override List<IWorkspaceContribution> Contributions {
            get {

                List<IWorkspaceContribution> contrib = new List<IWorkspaceContribution>();
                IBioLinkPlugin plugin = this;
                contrib.Add(new MenuWorkspaceContribution(this, "ShowExplorer", (obj, e) => { PluginManager.ContributeDockableContent(plugin, _explorer); },
                    String.Format("{{'Name':'View', 'Header':'{0}','InsertAfter':'File'}}", _R("Taxa.Menu.View")), 
                    String.Format("{{'Name':'ShowTaxaExplorer', 'Header':'{0}'}}", _R("Taxa.Menu.ShowExplorer"))
                ));

                _explorer = new ExplorerWorkspaceContribution<TaxonExplorer>(this, "TaxonExplorer", new TaxonExplorer(), _R("TaxonExplorer.Title"),
                    (explorer) => {
                        // Load the model on the background thread
                        List<Taxon> taxa = new TaxaService(User).GetTopLevelTaxa();

                        List<TaxonViewModel> viewModel = taxa.ConvertAll((taxon) => {
                            TaxonViewModel item = new TaxonViewModel(null, taxon);
                            if (item.NumChildren > 0) {
                                item.LazyLoadChildren += new ViewModelExpandedDelegate(item_LazyLoadChildren);
                                item.Children = new ObservableCollection<HierachicalViewModelBase>();
                                item.Children.Add(new ViewModelPlaceholder("Loading..."));
                            }
                            return item;
                        });

                        ObservableCollection<TaxonViewModel> model = new ObservableCollection<TaxonViewModel>(viewModel);
                        // and set it on the the components own thread...
                        explorer.InvokeIfRequired(() => {
                            explorer.SetModel(model);
                        });
                    });

                contrib.Add(_explorer);

                return contrib;            
            }
        }

        void item_LazyLoadChildren(HierachicalViewModelBase item) {

            item.Children.Clear();

            if (item is TaxonViewModel) {
                TaxonViewModel tvm = item as TaxonViewModel;
                Debug.Assert(tvm.TaxaID.HasValue, "TaxonViewModel has no taxa id!");
                List<Taxon> taxa = new TaxaService(User).GetTaxaForParent(tvm.TaxaID.Value);                
                foreach (Taxon taxon in taxa) {
                    TaxonViewModel child = new TaxonViewModel(null, taxon);
                    if (child.NumChildren > 0) {
                        child.LazyLoadChildren += new ViewModelExpandedDelegate(item_LazyLoadChildren);
                        child.Children = new ObservableCollection<HierachicalViewModelBase>();
                        child.Children.Add(new ViewModelPlaceholder("Loading..."));
                    }
                    item.Children.Add(child);
                }
            }

            
        }
    }

}
