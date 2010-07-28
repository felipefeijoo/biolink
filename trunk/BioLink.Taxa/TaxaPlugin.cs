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
        private TaxaService _taxaService;

        public TaxaPlugin(User user, PluginManager pluginManager)
            : base(user, pluginManager) {
            Debug.Assert(user != null, "User is null!");
            _taxaService = new TaxaService(user);
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

            List<Taxon> taxa = _taxaService.GetTopLevelTaxa();

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
                List<Taxon> taxa = _taxaService.GetTaxaForParent(tvm.TaxaID.Value);
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

        private DragDropAction PromptChangeAvailableName(TaxonViewModel source, TaxonViewModel target) {

            TaxonRank sourceRank = _taxaService.GetTaxonRank(source.ElemType);
            TaxonRank targetRank = _taxaService.GetTaxonRank(target.ElemType);

            if (sourceRank.Category == targetRank.Category) {
                return new ConvertDropAction(source, target, targetRank);
            } else {
                MessageBoxResult result = MessageBox.Show(_R("TaxonExplorer.prompt.ConvertAvailableName", sourceRank.LongName, targetRank.LongName), _R("TaxonExplorer.prompt.caption.ConvertAvailableName"), MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes) {
                    return new ConvertDropAction(source, target, targetRank);
                }
            }

            return null;
        }

        /// <summary>
        /// Return the elemType of the first child that is not "unplaced", including available names, species inquirenda and incertae sedis
        /// </summary>
        /// <param name="parent"></param>
        /// <returns></returns>
        private string GetChildElementType(TaxonViewModel parent) {
            if (!parent.IsExpanded) {
                parent.IsExpanded = true; // This will load the children, if they are not already loaded...
            }

            foreach (TaxonViewModel child in parent.Children) {

                if (!String.IsNullOrEmpty(child.ElemType)) {
                    continue;
                }

                if (child.ElemType == TaxaService.SPECIES_INQUIRENDA || child.ElemType == TaxaService.INCERTAE_SEDIS) {
                    continue;
                }

                return child.ElemType;
            }

            return "";
        }

        private DragDropAction CreateAndValidateDropAction(TaxonViewModel source, TaxonViewModel target) {

            // More comprehensive Drag and Drop rules (ported from original BioLink)

            TaxaService service = new TaxaService(User);

            TaxonRank sourceRank = service.GetTaxonRank(source.ElemType);
            TaxonRank targetRank = service.GetTaxonRank(target.ElemType);

            // Work out what kind of elements exist under the target already...
            string strChildType = GetChildElementType(target);
            TaxonRank targetChildRank = null;
            if (!String.IsNullOrEmpty(strChildType)) {            
                targetChildRank =_taxaService.GetTaxonRank(strChildType);
            }

            if (target.AvailableName.GetValueOrDefault(false)) {
                // Can't drop on to an Available Name
                throw new IllegalTaxonMoveException(source.Taxon, target.Taxon, _R("TaxonExplorer.DropError.AvailableName", source.Epithet, target.Epithet));
            } else if (source.AvailableName.GetValueOrDefault(false)) {
                // if the source is an Available Name
                if (source.ElemType != target.ElemType) {
                    // If the target is not of the same type as the source, confirm the conversion of available name type (e.g. species available name to Genus available name)
                    return PromptChangeAvailableName(source, target);
                } else {
                    return new MergeDropAction(source, target);
                }
            } else if (targetRank == null || sourceRank == null || targetRank.Order.GetValueOrDefault(-1) < sourceRank.Order.GetValueOrDefault(-1)) {
                // If the target element is a higher rank than the source, then the drop is acceptable as long as there are no children of the target, 
                // or they were of the same type.
                // Check the drag drop rules as defined in the database...
                ValidationResult result = service.ValidateTaxonMove(source.Taxon, target.Taxon);
                if (!result.Success) {
                    // Can't automatically move - check to see if a conversion is a) possible b) desired
                    if (targetChildRank == null) {
                        return PromptConvert(source, sourceRank, target, targetRank);
                    } else {
                        return new ConvertDropAction(source, target, targetChildRank);
                    }
                } else if (source.ElemType == TaxaService.SPECIES_INQUIRENDA || source.ElemType == TaxaService.INCERTAE_SEDIS) {
                    // Special 'unplaced' ranks - no real rules for drag and drop (TBA)...
                    return new MoveDropAction(source, target);
                } else if (targetChildRank == null || targetChildRank.Code == source.ElemType) {
                    // Not sure what this means, the old BioLink did it...
                    return new MoveDropAction(source, target);
                } else {
                    throw new IllegalTaxonMoveException(source.Taxon, target.Taxon, _R("TaxonExplorer.DropError.CannotCoexist", targetChildRank.LongName, sourceRank.LongName));
                }
            } else if (source.ElemType == target.ElemType) {
                // Determine what the user wishes to do when the drag/drop source and target are the same type.
                return PromptSourceTargetSame(source, sourceRank, target, targetRank, targetChildRank);
            }


            return null;
        }

        private DragDropAction PromptSourceTargetSame(TaxonViewModel source, TaxonRank sourceRank, TaxonViewModel target, TaxonRank targetRank, TaxonRank targetChildRank) {
            List<TaxonRank> conversionOptions = new List<TaxonRank>();
            if (targetChildRank != null) {
                conversionOptions.Add(targetChildRank);
            } else {
                conversionOptions.AddRange(_taxaService.GetChildRanks(targetRank));
            }

            // TODO: show the dialog which allows you to choose between merging and creating a new child element.
            return new MoveDropAction(source, target);
        }

        private DragDropAction PromptConvert(TaxonViewModel source, TaxonRank sourceRank, TaxonViewModel target, TaxonRank targetRank) {
            DragDropOptions form = new DragDropOptions(this);
            List<TaxonRank> validChildren = _taxaService.GetChildRanks(targetRank);
            TaxonRank choice = form.ShowChooseConversion(sourceRank, validChildren);
            if (choice != null) {
                return new ConvertDropAction(source, target, choice);
            }

            return null;
        }

        public void ProcessTaxonDragDrop(TaxonViewModel source, TaxonViewModel target) {

            // There are 4 possible outcomes from a taxon drop
            // 1) The drop is invalid, so do nothing (display an error message)
            // 2) The drop is a valid move, no conversion or merging required (simplest case)
            // 3) The drop is to that of a sibling rank, and so a merge is required
            // 4) The drop is valid, but requires the source to be converted into a valid child of the target

            //Basic sanity checks first....
            if (target == source.Parent) {
                throw new IllegalTaxonMoveException(source.Taxon, target.Taxon, _R("TaxonExplorer.DropError.AlreadyAChild", source.Epithet, target.Epithet));
            }

            if (source.IsAncestorOf(target)) {
                throw new IllegalTaxonMoveException(source.Taxon, target.Taxon, _R("TaxonExplorer.DropError.SourceAncestorOfDest", source.Epithet, target.Epithet));
            }

            if (!target.IsExpanded) {
                target.IsExpanded = true;
            }

            DragDropAction action = CreateAndValidateDropAction(source, target);

            if (action != null) {
                // process the action...
                // TODO - the action is going to be more comprehensive than just this...
                source.Parent.Children.Remove(source);
                target.Children.Add(source);
                source.Parent = target;
                source.TaxaParentID = target.TaxaID;
                source.IsSelected = true;
            }
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

    public class IllegalTaxonMoveException : Exception {

        public IllegalTaxonMoveException(Taxon source, Taxon dest, string message)
            : base(message) {
            this.SourceTaxon = source;
            this.DestinationTaxon = dest;
        }

        public Taxon SourceTaxon { get; private set; }
        public Taxon DestinationTaxon { get; private set; }
    }

    abstract class DragDropAction {

        public DragDropAction(TaxonViewModel source, TaxonViewModel target) {
            this.Source = source;
            this.Target = target;
        }
        public TaxonViewModel Source { get; private set; }
        public TaxonViewModel Target { get; private set; }

    }

    class MoveDropAction : DragDropAction {
        public MoveDropAction(TaxonViewModel source, TaxonViewModel target)
            : base(source, target) {
        }
    }

    class ConvertDropAction : DragDropAction {

        public ConvertDropAction(TaxonViewModel source, TaxonViewModel target, TaxonRank convertRank)
            : base(source, target) {
            this.ConvertRank = convertRank;
        }
        public TaxonRank ConvertRank { get; private set; }
    }

    class MergeDropAction : DragDropAction {

        public MergeDropAction(TaxonViewModel source, TaxonViewModel target)
            : base(source, target) {
        }

    }

}
