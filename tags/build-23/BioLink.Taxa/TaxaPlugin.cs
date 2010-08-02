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
                        // Load the model on the background thread
                        ObservableCollection<HierarchicalViewModelBase> model = LoadTaxonViewModel();

                        // Now see if we can auto-expand from the last session...
                        if (model.Count > 0 && Config.GetGlobal<bool>("Taxa.RememberExpandedTaxa", true)) {
                            var expanded = Config.GetProfile<List<String>>(User, "Taxa.Explorer.ExpandedTaxa", null);
                            ExpandParentages(model[0], expanded);
                        }

                        // and set it on the the components own thread...
                        explorer.InvokeIfRequired(() => {
                            explorer.ExplorerModel = model;
                        });
                    });

                contrib.Add(_explorer);

                return contrib;
            }
        }

        public void ExpandParentages(HierarchicalViewModelBase rootNode, List<string> expanded) {
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
        }

        public ObservableCollection<HierarchicalViewModelBase> LoadTaxonViewModel() {

            List<Taxon> taxa = _taxaService.GetTopLevelTaxa();

            Taxon root = new Taxon();
            root.TaxaID = 0;
            root.TaxaParentID = -1;
            root.Epithet = _R("TaxonExplorer.explorer.root");

            TaxonViewModel rootNode = new TaxonViewModel(null, root);

            taxa.ForEach((taxon) => {
                TaxonViewModel item = new TaxonViewModel(rootNode, taxon);
                if (item.NumChildren > 0) {
                    item.LazyLoadChildren += new HierarchicalViewModelAction(item_LazyLoadChildren);
                    item.Children.Add(new ViewModelPlaceholder(_R("TaxonExplorer.explorer.loading", item.Epithet)));
                }
                rootNode.Children.Add(item);
            });

            ObservableCollection<HierarchicalViewModelBase> model = new ObservableCollection<HierarchicalViewModelBase>();
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
                        child.LazyLoadChildren += new HierarchicalViewModelAction(item_LazyLoadChildren);
                        child.Children.Add(new ViewModelPlaceholder("Loading..."));
                    }
                    item.Children.Add(child);
                }
            }
        }

        private DragDropAction PromptChangeAvailableName(TaxonDropContext context) {

            if (context.SourceRank.Category == context.TargetRank.Category) {
                return new ConvertDropAction(context.Source, context.Target, context.TargetRank);
            } else {
                MessageBoxResult result = MessageBox.Show(_R("TaxonExplorer.prompt.ConvertAvailableName", context.SourceRank.LongName, context.TargetRank.LongName), _R("TaxonExplorer.prompt.caption.ConvertAvailableName"), MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes) {
                    return new ConvertDropAction(context.Source, context.Target, context.TargetRank);
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

        private DragDropAction CreateAndValidateDropAction(TaxonDropContext context) {


            if (context.Target.AvailableName.GetValueOrDefault(false)) {
                // Can't drop on to an Available Name
                throw new IllegalTaxonMoveException(context.Source.Taxon, context.Target.Taxon, _R("TaxonExplorer.DropError.AvailableName", context.Source.Epithet, context.Target.Epithet));
            } else if (context.Source.AvailableName.GetValueOrDefault(false)) {
                // if the source is an Available Name
                if (context.Source.ElemType != context.Target.ElemType) {
                    // If the target is not of the same type as the source, confirm the conversion of available name type (e.g. species available name to Genus available name)
                    return PromptChangeAvailableName(context);
                } else {
                    return new MergeDropAction(context.Source, context.Target);
                }
            } else if (context.TargetRank == null || context.SourceRank == null || context.TargetRank.Order.GetValueOrDefault(-1) < context.SourceRank.Order.GetValueOrDefault(-1)) {
                // If the target element is a higher rank than the source, then the drop is acceptable as long as there are no children of the target, 
                // or they were of the same type.
                // Check the drag drop rules as defined in the database...
                DataValidationResult result = _taxaService.ValidateTaxonMove(context.Source.Taxon, context.Target.Taxon);
                if (!result.Success) {
                    // Can't automatically move - check to see if a conversion is a) possible b) desired
                    if (context.TargetChildRank == null) {
                        return PromptConvert(context);
                    } else {
                        return new ConvertDropAction(context.Source, context.Target, context.TargetChildRank);
                    }
                } else if (context.Source.ElemType == TaxaService.SPECIES_INQUIRENDA || context.Source.ElemType == TaxaService.INCERTAE_SEDIS) {
                    // Special 'unplaced' ranks - no real rules for drag and drop (TBA)...
                    return new MoveDropAction(context.Source, context.Target);
                } else if (context.TargetChildRank == null || context.TargetChildRank.Code == context.Source.ElemType) {
                    // Not sure what this means, the old BioLink did it...
                    return new MoveDropAction(context.Source, context.Target);
                } else {
                    throw new IllegalTaxonMoveException(context.Source.Taxon, context.Target.Taxon, _R("TaxonExplorer.DropError.CannotCoexist", context.TargetChildRank.LongName, context.SourceRank.LongName));
                }
            } else if (context.Source.ElemType == context.Target.ElemType) {
                // Determine what the user wishes to do when the drag/drop source and target are the same type.
                return PromptSourceTargetSame(context);
            }

            return null;
        }

        private DragDropAction PromptSourceTargetSame(TaxonDropContext context) {
            DragDropOptions form = new DragDropOptions(this);
            return form.ShowChooseMergeOrConvert(context);            
        }

        private DragDropAction PromptConvert(TaxonDropContext context) {
            DragDropOptions form = new DragDropOptions(this);
            List<TaxonRank> validChildren = _taxaService.GetChildRanks(context.TargetRank);
            TaxonRank choice = form.ShowChooseConversion(context.SourceRank, validChildren);
            if (choice != null) {
                return new ConvertDropAction(context.Source, context.Target, choice);
            }

            return null;
        }

        public void ProcessTaxonDragDrop(TaxonViewModel source, TaxonViewModel target) {

            // There are 4 possible outcomes from a taxon drop
            // 1) The drop is invalid, so do nothing (display an error message)
            // 2) The drop is a valid move, no conversion or merging required (simplest case)
            // 3) The drop is to that of a sibling rank, and so a merge is required
            // 4) The drop is valid, but requires the source to be converted into a valid child of the target
            TaxonDropContext context = new TaxonDropContext(source, target, _taxaService);

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

            DragDropAction action = CreateAndValidateDropAction(context);

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

    public class IllegalTaxonMoveException : Exception {

        public IllegalTaxonMoveException(Taxon source, Taxon dest, string message)
            : base(message) {
            this.SourceTaxon = source;
            this.DestinationTaxon = dest;
        }

        public Taxon SourceTaxon { get; private set; }
        public Taxon DestinationTaxon { get; private set; }
    }

    internal abstract class DragDropAction {

        public DragDropAction(TaxonViewModel source, TaxonViewModel target) {
            this.Source = source;
            this.Target = target;
        }
        public TaxonViewModel Source { get; private set; }
        public TaxonViewModel Target { get; private set; }

    }

    internal class MoveDropAction : DragDropAction {
        public MoveDropAction(TaxonViewModel source, TaxonViewModel target)
            : base(source, target) {
        }
    }

    internal class ConvertDropAction : DragDropAction {

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
