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
                contrib.Add(new MenuWorkspaceContribution(this, "ShowExplorer", (obj, e) => { PluginManager.EnsureVisible(this, "TaxonExplorer"); },
                    String.Format("{{'Name':'View', 'Header':'{0}','InsertAfter':'File'}}", _R("Taxa.Menu.View")), 
                    String.Format("{{'Name':'ShowTaxaExplorer', 'Header':'{0}'}}", _R("Taxa.Menu.ShowExplorer"))
                ));

                _explorer = new ExplorerWorkspaceContribution<TaxonExplorer>(this, "TaxonExplorer", new TaxonExplorer(this), _R("TaxonExplorer.Title"),
                    (explorer) => {
                        // Load the model on the background thread
                        ObservableCollection<TaxonViewModel> model = LoadTaxonViewModel();
                        // and set it on the the components own thread...
                        explorer.InvokeIfRequired(() => {
                            explorer.ExplorerModel = model;
                        });
                    });

                contrib.Add(_explorer);

                return contrib;            
            }
        }

        public ObservableCollection<TaxonViewModel> LoadTaxonViewModel() {

            List<Taxon> taxa = new TaxaService(User).GetTopLevelTaxa();

            Taxon root = new Taxon();
            root.TaxaID = 0;
            root.TaxaParentID = 0;
            root.Epithet = _R("TaxonExplorer.explorer.root");

            TaxonViewModel rootNode = new TaxonViewModel(null, root);

            taxa.ForEach((taxon) => {
                TaxonViewModel item = new TaxonViewModel(rootNode, taxon);
                if (item.NumChildren > 0) {
                    item.LazyLoadChildren += new HierarchicalViewModelDelegate(item_LazyLoadChildren);
                    item.Children.Add(new ViewModelPlaceholder(_R("TaxonExplorer.explorer.loading", item.Epithet)));
                }                
                rootNode.Children.Add(item);
            });

            // Now see if we can auto-expand from the last session...
            var expanded = Preferences.GetProfile<List<String>>(User, "Taxa.Explorer.ExpandedTaxa", null);
            if (expanded != null && expanded.Count > 0) {
                var todo = new Stack<HierarchicalViewModelBase>(rootNode.Children);
                while (todo.Count > 0) {
                    var vm = todo.Pop();
                    if (vm is TaxonViewModel) {
                        var tvm = vm as TaxonViewModel;
                        string parentage = tvm.GetParentage();
                        if (expanded.Contains(parentage)) {
                            tvm.IsExpanded = true;
                            expanded.Remove(parentage);
                            tvm.Children.ForEach(child => todo.Push(child));
                        }
                    }                    
                }
            }

            ObservableCollection<TaxonViewModel> model = new ObservableCollection<TaxonViewModel>();
            model.Add(rootNode);
            rootNode.IsExpanded = true;
            return model;
        }

        void item_LazyLoadChildren(HierarchicalViewModelBase item) {

            item.Children.Clear();

            if (item is TaxonViewModel) {
                TaxonViewModel tvm = item as TaxonViewModel;
                Debug.Assert(tvm.TaxaID.HasValue, "TaxonViewModel has no taxa id!");
                List<Taxon> taxa = new TaxaService(User).GetTaxaForParent(tvm.TaxaID.Value);                
                foreach (Taxon taxon in taxa) {
                    TaxonViewModel child = new TaxonViewModel(tvm, taxon);                    
                    if (child.NumChildren > 0) {
                        child.LazyLoadChildren += new HierarchicalViewModelDelegate(item_LazyLoadChildren);
                        child.Children.Add(new ViewModelPlaceholder("Loading..."));
                    }
                    item.Children.Add(child);
                }
            }            
        }

        public void ProcessTaxonDragDrop(TaxonViewModel src, TaxonViewModel dest) {

            if (dest == src || dest == src.Parent) {
                throw new IllegalTaxonMoveException(src.Taxon, dest.Taxon, "The source and the destination are the same!");
            }

            if (src.IsAncestorOf(dest)) {
                throw new IllegalTaxonMoveException(src.Taxon, dest.Taxon, String.Format("'{0}' is an ancestor of '{1}'", src.Epithet, dest.Epithet));                
            }

            if (!dest.IsExpanded) {
                dest.IsExpanded = true;
            }

            ValidationResult result = new TaxaService(User).ValidateTaxonMove(src.Taxon, dest.Taxon);
            if (!result.Success) {
                throw new IllegalTaxonMoveException(src.Taxon, dest.Taxon, result.Message);
            }
            
            src.Parent.Children.Remove(src);
            dest.Children.Add(src);
            src.Parent = dest;
            src.TaxaParentID = dest.TaxaID;
            src.IsSelected = true;           
        }

        public override void Dispose() {
            base.Dispose();
            if (_explorer != null && _explorer.Content != null) {
                List<string> expandedElements = GetExpandedParentages(_explorer.ContentControl.ExplorerModel);
                if (expandedElements != null) {
                    Preferences.SetProfile(User, "Taxa.Explorer.ExpandedTaxa", expandedElements);
                }

            }
        }

        private List<string> GetExpandedParentages(ObservableCollection<TaxonViewModel> model) {
            List<string> list = new List<string>();
            ProcessList(model, list);
            return list;
        }

        private void ProcessList<T> (ObservableCollection<T> model, List<string> list) where T : HierarchicalViewModelBase {
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

    public class IllegalTaxonMoveException : Exception {        

        public IllegalTaxonMoveException(Taxon source, Taxon dest, string message) : base(message) {
            this.SourceTaxon = source;
            this.DestinationTaxon = dest;
        }

        public Taxon SourceTaxon { get; private set; }
        public Taxon DestinationTaxon { get; private set; }        
    }

    

}
