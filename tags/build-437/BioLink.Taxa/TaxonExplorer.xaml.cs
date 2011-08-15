using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.ComponentModel;
using BioLink.Client.Extensibility;
using BioLink.Client.Utilities;
using BioLink.Data;
using BioLink.Data.Model;
using System.Windows.Threading;

namespace BioLink.Client.Taxa {


    /// <summary>
    /// Interaction logic for TaxonExplorer.xaml
    /// </summary>
    public partial class TaxonExplorer : ChangeContainerControl {
        
        private ObservableCollection<HierarchicalViewModelBase> _explorerModel;
        private ObservableCollection<TaxonViewModel> _searchModel;
        private Point _startPoint;
        private bool _IsDragging = false;
        private UIElement _dropScope;
        private DragAdorner _adorner;
        private AdornerLayer _layer;
        // private bool _dragHasLeftScope = false;
        private int _nextNewTaxonID = -100;

        #region Designer Constructor
        public TaxonExplorer() {
            InitializeComponent();
        }
        #endregion

        public TaxonExplorer(TaxaPlugin owner) : base(owner.User) {

            InitializeComponent();

            Owner = owner;

            _searchModel = new ObservableCollection<TaxonViewModel>();
            tvwResults.ItemsSource = _searchModel;


            ListCollectionView dataView = CollectionViewSource.GetDefaultView(tvwResults.ItemsSource) as ListCollectionView;
            dataView.SortDescriptions.Clear();
            dataView.SortDescriptions.Add(new SortDescription("IsAvailableOrLiteratureName", ListSortDirection.Ascending));
            dataView.SortDescriptions.Add(new SortDescription("TaxaFullName", ListSortDirection.Ascending));
            dataView.Refresh();

            IsManualSort = Config.GetUser(Owner.User, "Taxa.ManualSort", false);

            if (IsManualSort) {
                btnDown.Visibility = System.Windows.Visibility.Visible;
                btnUp.Visibility = System.Windows.Visibility.Visible;
            }


            tvwAllTaxa.PreviewKeyDown += new KeyEventHandler(TaxonExplorer_PreviewKeyDown);
            tvwResults.PreviewKeyDown += new KeyEventHandler(TaxonExplorer_PreviewKeyDown);
            
            btnLock.Checked += new RoutedEventHandler(btnLock_Checked);          
            btnLock.Unchecked += new RoutedEventHandler(btnLock_Unchecked);

            

            favorites.BindUser(User, this);
        }

        void TaxonExplorer_PreviewKeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Delete && IsUnlocked) {
                var tvw = sender as TreeView;
                if (tvw != null) {
                    var selected = tvw.SelectedItem as TaxonViewModel;
                    if (selected != null) {
                        DeleteTaxon(selected);
                    }
                }
            }
        }

        void btnLock_Unchecked(object sender, RoutedEventArgs e) {
            
            if (AnyChanges()) {
                if (this.DiscardChangesQuestion()) {
                    ReloadModel();
                } else {
                    // Cancel the unlock
                    btnLock.IsChecked = true;
                    return;
                }
            }

            lblHeader.Visibility = Visibility.Hidden;
            gridContentsHeader.Background = SystemColors.ControlBrush;
            buttonBar.Visibility = IsUnlocked ? Visibility.Visible : Visibility.Hidden;
        }

        void btnLock_Checked(object sender, RoutedEventArgs e) {
            lblHeader.Visibility = Visibility.Hidden;
            buttonBar.Visibility = IsUnlocked ? Visibility.Visible : Visibility.Hidden;
            gridContentsHeader.Background = new LinearGradientBrush(Colors.DarkOrange, Colors.Orange, 90.0);

            if (!User.HasPermission(PermissionCategory.SPIN_TAXON, PERMISSION_MASK.UPDATE)) {
                btnLock.IsChecked = false;
                ErrorMessage.Show("You do not have sufficient priviledges to edit the taxon tree!");
                return;
            }

        }

        public string GenerateTaxonDisplayLabel(TaxonViewModel taxon) {

            if (taxon.TaxaParentID == -1) {
                return _R("TaxonExplorer.explorer.root");
            }

            string strAuthorYear = "";
            if (taxon.Unplaced.ValueOrFalse()) {
                return "Unplaced";
            } else if (taxon.ElemType == TaxonRank.SPECIES_INQUIRENDA) {
                return "Species Inquirenda";
            } else if (taxon.ElemType == TaxonRank.INCERTAE_SEDIS) {
                return "Incertae Sedis";
            } else {
                if (!String.IsNullOrEmpty(taxon.Author)) {
                    strAuthorYear = taxon.Author;
                }
                if (!String.IsNullOrEmpty(taxon.YearOfPub)) {
                    if (strAuthorYear.Length > 0) {
                        strAuthorYear = strAuthorYear + ", " + taxon.YearOfPub;
                    } else {
                        strAuthorYear = taxon.YearOfPub;
                    }
                }
                if (taxon.ChgComb.ValueOrFalse()) {
                    strAuthorYear = "(" + strAuthorYear + ")";
                }
                string strDisplay = null;
                if (taxon.AvailableName.ValueOrFalse() || taxon.LiteratureName.ValueOrFalse()) {
                    strDisplay = taxon.Epithet + " " + strAuthorYear;
                } else {
                    string format = "#";
                    TaxonRank rank = Service.GetTaxonRank(taxon.ElemType);
                    if (rank != null) {
                        format = rank.ChecklistDisplayAs ?? "#";
                    }
                    strDisplay = format.Replace("#", taxon.Epithet) + " " + strAuthorYear;
                }
                return strDisplay;
            }
        }

        private void DoFind(string searchTerm) {

            if (String.IsNullOrEmpty(searchTerm)) {
                return;
            }

            _searchModel.Clear();

            using (new OverrideCursor(Cursors.Wait)) {
                if (Owner == null) {
                    return;
                }
                List<TaxonSearchResult> results = new TaxaService(Owner.User).FindTaxa(searchTerm);

                if (!PluginManager.Instance.CheckSearchResults(results)) {
                    return;
                }

                tvwResults.InvokeIfRequired(() => {                    
                    foreach (Taxon t in results) {
                        var item = new TaxonViewModel(null, t, GenerateTaxonDisplayLabel);
                        if (item.NumChildren > 0) {
                            item.LazyLoadChildren += new HierarchicalViewModelAction(item_LazyLoadChildren);
                            item.Children.Add(new ViewModelPlaceholder(_R("TaxonExplorer.explorer.loading", item.Epithet)));
                        }

                        _searchModel.Add(item);
                    }

                });
            } 
        }

        private void tabControl1_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (taxonTabControl.SelectedItem == tabFavorites) {
                favorites.LoadFavorites();
            } else {
                var parent = gridContentsHeader.Parent as Grid;
                if (parent != null) {
                    parent.Children.Remove(gridContentsHeader);
                    if (taxonTabControl.SelectedItem == tabAllTaxa) {
                        gridAllTaxaContent.Children.Add(gridContentsHeader);
                    } else {
                        gridFindTaxaContent.Children.Add(gridContentsHeader);
                    }
                }
            }
        }

        public void InitialiseTaxonExplorer() {
            this.InvokeIfRequired(() => {
                // Load the model on the background thread
                ObservableCollection<HierarchicalViewModelBase> model = LoadTaxonViewModel();

                // Now see if we can auto-expand from the last session...
                if (model.Count > 0 && Config.GetGlobal<bool>("Taxa.RememberExpandedTaxa", true)) {
                    var expanded = Config.GetProfile<List<String>>(Owner.User, "Taxa.Explorer.ExpandedTaxa", null);
                    ExpandParentages(model[0], expanded);
                }
                // and set it on the the components own thread...            
                this.ExplorerModel = model;
            });

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
            using (new OverrideCursor(Cursors.Wait)) {
                List<Taxon> taxa = Service.GetTopLevelTaxa();

                Taxon root = new Taxon();
                root.TaxaID = 0;
                root.TaxaParentID = -1;
                root.Epithet = _R("TaxonExplorer.explorer.root");

                TaxonViewModel rootNode = new TaxonViewModel(null, root, GenerateTaxonDisplayLabel, true);

                taxa.ForEach((taxon) => {
                    TaxonViewModel item = new TaxonViewModel(rootNode, taxon, GenerateTaxonDisplayLabel);
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
        }

        void item_LazyLoadChildren(HierarchicalViewModelBase item) {

            using (new OverrideCursor(Cursors.Wait)) {
                item.Children.Clear();

                if (item is TaxonViewModel) {
                    TaxonViewModel tvm = item as TaxonViewModel;
                    Debug.Assert(tvm.TaxaID.HasValue, "TaxonViewModel has no favorites id!");
                    List<Taxon> taxa = Service.GetTaxaForParent(tvm.TaxaID.Value);
                    foreach (Taxon taxon in taxa) {
                        TaxonViewModel child = new TaxonViewModel(tvm, taxon, GenerateTaxonDisplayLabel);
                        if (child.NumChildren > 0) {
                            child.LazyLoadChildren += new HierarchicalViewModelAction(item_LazyLoadChildren);
                            child.Children.Add(new ViewModelPlaceholder("Loading..."));
                        }
                        item.Children.Add(child);
                    }
                }
            }
        }


        #region Drag and Drop stuff

        void TreeView_PreviewMouseMove(object sender, MouseEventArgs e) {

            var tvw = sender as TreeView;

            if (e.LeftButton == MouseButtonState.Pressed && !_IsDragging) {
                Point position = e.GetPosition(tvw);
                if (Math.Abs(position.X - _startPoint.X) > SystemParameters.MinimumHorizontalDragDistance || Math.Abs(position.Y - _startPoint.Y) > SystemParameters.MinimumVerticalDragDistance) {
                    if (tvw.SelectedItem != null) {
                        IInputElement hitelement = tvw.InputHitTest(_startPoint);
                        TreeViewItem item = GetTreeViewItemClicked((FrameworkElement)hitelement, tvw);
                        if (item != null) {
                            StartDrag(e, tvw.SelectedItem as TaxonViewModel, item, tvw);
                        }
                    }
                }
            }
        }

        private TreeViewItem GetTreeViewItemClicked(FrameworkElement sender, TreeView treeView) {
            Point p = sender.TranslatePoint(new Point(1, 1), treeView);
            DependencyObject obj = treeView.InputHitTest(p) as DependencyObject;
            while (obj != null && !(obj is TreeViewItem)) {
                obj = VisualTreeHelper.GetParent(obj);
            }
            return obj as TreeViewItem;
        }

        void TreeView_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            var tvw = sender as TreeView;
            _startPoint = e.GetPosition(tvw);
        }

        private DependencyObject GetCurrentHoverItem(DragEventArgs e) {
            FrameworkElement scope = _dropScope as FrameworkElement;
            DependencyObject elem = scope.InputHitTest(e.GetPosition(scope)) as DependencyObject;
            while (elem != null && !(elem is TreeViewItem)) {
                elem = VisualTreeHelper.GetParent(elem);
            }
            return elem;
        }

        private void DropSink_DragOver(object source, DragEventArgs e) {
            TaxonViewModel t = e.Data.GetData("Taxon") as TaxonViewModel;            

            if (t != null) {
                DependencyObject destItem = GetCurrentHoverItem(e);

                if (destItem is TreeViewItem) {
                    var tvwItem = destItem as TreeViewItem;                    
                    tvwItem.IsSelected = true;
                } else if (destItem is ListBoxItem) {
                    var lstItem = destItem as ListBoxItem;
                    lstItem.IsSelected = true;
                }

            }
            if (_adorner != null) {
                _adorner.LeftOffset = e.GetPosition(_dropScope).X;
                _adorner.TopOffset = e.GetPosition(_dropScope).Y;
            }

        }

        private void DragScope_DragLeave(object source, DragEventArgs e) {

            if (e.OriginalSource == _dropScope) {
                Point p = e.GetPosition(_dropScope);
                Rect r = VisualTreeHelper.GetContentBounds(_dropScope);
                if (!r.Contains(p)) {
                    // this._dragHasLeftScope = true;
                    e.Handled = true;
                }
            }
        }

        private void DragScope_QueryContinueDrag(object source, QueryContinueDragEventArgs e) {
            if (e.EscapePressed) {
                e.Action = DragAction.Cancel;
            }
            //if (this._dragHasLeftScope) {
            //    mouseEvent.Action = DragAction.Cancel;
            //    mouseEvent.Handled = true;
            //}
        }

        void _dragScope_Drop(object sender, DragEventArgs e) {
            TaxonViewModel src = e.Data.GetData("Taxon") as TaxonViewModel;
            DependencyObject destItem = GetCurrentHoverItem(e);
            if (destItem != null) {
                TaxonViewModel dest = null;
                if (destItem is TreeViewItem) {
                    dest = (destItem as TreeViewItem).Header as TaxonViewModel;
                } else if (destItem is ListBoxItem) {
                    dest = (destItem as ListBoxItem).Content as TaxonViewModel;
                }

                if (src != null && dest != null) {

                    if (src == dest) {
                        // if the source and the destination are the same, there is no logical operation that can be performed.
                        // We could irritate the user with a pop-up, but this situation is more than likely the result
                        // of an accidental drag, so just cancel the drop...
                        return;
                    }

                    try {
                        ProcessTaxonDragDrop(src, dest);
                    } finally {
                        e.Handled = true;
                    }
                }
            }

        }

        private bool CheckPermission(PERMISSION_MASK mask, TaxonViewModel target = null) {

            // system administrator has full rights to all.
            if (User.IsSysAdmin) {
                return true;
            }

            // Ensure the permissions set at the user group level take precendence to the indivual taxon based permissions.
            try {

                if (mask != PERMISSION_MASK.OWNER) {
                    if (!User.HasPermission(PermissionCategory.SPIN_TAXON, mask)) {
                        return false;
                    }
                }

                if (target != null) {
                    if (target.TaxaID.Value < 0) {
                        // new items are automatically approved!
                        return true;
                    }
                    var service = new SupportService(User);
                    if (!service.HasBiotaPermission(target.TaxaID.Value, mask)) {
                        throw new NoPermissionException(PermissionCategory.SPIN_TAXON, mask, "You do not have permission to move this item!");
                    }
                }

                return true;

            } catch (NoPermissionException npex) {
                string txt = npex.Message;
                if (!string.IsNullOrEmpty(npex.DeniedMessage)) {
                    txt = npex.DeniedMessage;
                }
                string caption = string.Format("Permission Error [{0} {1}]", npex.PermissionCategory, npex.RequestedMask);
                MessageBox.Show(txt, caption, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return false;
            }
        }

        public void ProcessTaxonDragDrop(TaxonViewModel source, TaxonViewModel target) {

            try {
                // There are 4 possible outcomes from a taxon drop
                // 1) The drop is invalid, so do nothing (display an error message)
                // 2) The drop is a valid move, no conversion or merging required (simplest case)
                // 3) The drop is to that of a sibling rank, and so a decision is made to either merge or move as child (if possible)
                // 4) The drop is valid, but requires the source to be converted into a valid child of the target

                if (!IsUnlocked) {
                    return;
                }

                if (!CheckPermission(PERMISSION_MASK.UPDATE, target)) {
                    return;
                }
                
                TaxonDropContext context = new TaxonDropContext(source, target, Owner);

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
                    List<DatabaseCommand> dbActions = action.ProcessUI();
                    if (dbActions != null && dbActions.Count > 0) {
                        RegisterPendingChanges(dbActions, this);
                    }
                }
            } catch (IllegalTaxonMoveException ex) {
                MessageBox.Show(ex.Message, String.Format("Cannot move '{0}'", source.Epithet), MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }

            source.IsSelected = true;
        }

        private void StartDrag(MouseEventArgs mouseEvent, TaxonViewModel taxon, FrameworkElement item, FrameworkElement dropScope) {
            _dropScope = dropScope;
            bool previousDrop = _dropScope.AllowDrop;
            _dropScope.AllowDrop = true;


            DragEventHandler drophandler = new DragEventHandler(_dragScope_Drop);
            _dropScope.Drop += drophandler;

            DragEventHandler draghandler = new DragEventHandler(DropSink_DragOver);
            _dropScope.PreviewDragOver += draghandler;

            DragEventHandler dragleavehandler = new DragEventHandler(DragScope_DragLeave);
            _dropScope.DragLeave += dragleavehandler;

            QueryContinueDragEventHandler queryhandler = new QueryContinueDragEventHandler(DragScope_QueryContinueDrag);
            _dropScope.QueryContinueDrag += queryhandler;

            var previewHandler = new DragEventHandler((s, evt) => {
                if (!IsUnlocked) {
                    evt.Effects = DragDropEffects.None;
                } else {
                    evt.Effects = DragDropEffects.Move;
                }
                evt.Handled = true;
            });

            _dropScope.PreviewDragEnter += previewHandler;
            _dropScope.PreviewDragOver += previewHandler;

            _adorner = new DragAdorner(_dropScope, item, true, 0.5, mouseEvent.GetPosition(item));
            _layer = AdornerLayer.GetAdornerLayer(_dropScope as Visual);
            _layer.Add(_adorner);
            _IsDragging = true;
            
            if (taxon != null && !string.IsNullOrWhiteSpace(taxon.TaxaFullName)) {
                DataObject data = new DataObject("Taxon", taxon);
                data.SetData(PinnableObject.DRAG_FORMAT_NAME, Owner.CreatePinnableTaxon(taxon.TaxaID.Value));
                data.SetData(DataFormats.Text, taxon.TaxaFullName);
                data.SetData(DataFormats.UnicodeText, taxon.TaxaFullName);
                data.SetData(DataFormats.StringFormat, taxon.TaxaFullName);

                if (!IsUnlocked) {
                    lblHeader.Visibility = Visibility.Visible;
                }

                // Actually kick off the drag drop here!
                DragDropEffects de = DragDrop.DoDragDrop(item, data, DragDropEffects.Move | DragDropEffects.Copy | DragDropEffects.Link );
            }

            _dropScope.AllowDrop = previousDrop;
            AdornerLayer.GetAdornerLayer(_dropScope).Remove(_adorner);
            _adorner = null;
            
            _dropScope.DragLeave -= dragleavehandler;
            _dropScope.QueryContinueDrag -= queryhandler;
            _dropScope.PreviewDragOver -= draghandler;
            _dropScope.Drop -= drophandler;
            _dropScope.PreviewDragEnter -= previewHandler;
            _dropScope.PreviewDragOver -= previewHandler;

            _IsDragging = false;

            InvalidateVisual();
        }

        private DragDropAction PromptSourceTargetSame(TaxonDropContext context) {
            DragDropOptions form = new DragDropOptions(Owner);
            return form.ShowChooseMergeOrConvert(context);
        }

        private DragDropAction PromptConvert(TaxonDropContext context) {
            DragDropOptions form = new DragDropOptions(Owner);
            List<TaxonRank> validChildren = Service.GetChildRanks(context.TargetRank);
            TaxonRank choice = form.ShowChooseConversion(context.SourceRank, validChildren);
            if (choice != null) {
                return new ConvertingMoveDropAction(context, choice);
            }

            return null;
        }

        private DragDropAction CreateAndValidateDropAction(TaxonDropContext context) {


            if (context.Target.AvailableName.GetValueOrDefault(false) || context.Target.LiteratureName.GetValueOrDefault(false)) {
                // Can'note drop on to an Available or Literature Name
                throw new IllegalTaxonMoveException(context.Source.Taxon, context.Target.Taxon, _R("TaxonExplorer.DropError.AvailableName", context.Source.Epithet, context.Target.Epithet));
            } else if (context.Source.AvailableName.GetValueOrDefault(false) || context.Source.LiteratureName.GetValueOrDefault(false)) {
                // if the source is an Available or Literature Name 
                if (context.Source.ElemType != context.Target.ElemType) {
                    // If the target is not of the same type as the source, confirm the conversion of available name type (mouseEventArgs.g. species available name to Genus available name)
                    return PromptChangeAvailableName(context);
                } else {
                    return new MoveDropAction(context);
                }
            } else if (context.TargetRank == null || context.SourceRank == null || context.TargetRank.Order.GetValueOrDefault(-1) < context.SourceRank.Order.GetValueOrDefault(-1)) {
                // If the target element is a higher rank than the source, then the drop is acceptable as long as there are no children of the target, 
                // or they were of the same type.
                // Check the drag drop rules as defined in the database...
                DataValidationResult result = Service.ValidateTaxonMove(context.Source.Taxon, context.Target.Taxon);
                if (!result.Success) {
                    // Can'note automatically move - check to see if a conversion is a) possible b) desired
                    if (context.TargetChildRank == null) {
                        return PromptConvert(context);
                    } else {
                        if (this.Question(_R("TaxonExplorer.prompt.ConvertTaxon", context.Source.DisplayLabel, context.TargetChildRank.LongName, context.Target.DisplayLabel), _R("TaxonExplorer.prompt.ConfirmAction.Caption"))) {
                            return new ConvertingMoveDropAction(context, context.TargetChildRank);
                        } else {
                            return null;
                        }
                    }
                } else if (context.Source.ElemType == TaxaService.SPECIES_INQUIRENDA || context.Source.ElemType == TaxaService.INCERTAE_SEDIS) {
                    // Special 'unplaced' ranks - no real rules for drag and drop (TBA)...
                    return new MoveDropAction(context);
                } else if (context.TargetChildRank == null || context.TargetChildRank.Code == context.Source.ElemType) {
                    // Not sure what this means, the old BioLink did it...
                    return new MoveDropAction(context);
                } else {
                    throw new IllegalTaxonMoveException(context.Source.Taxon, context.Target.Taxon, _R("TaxonExplorer.DropError.CannotCoexist", context.TargetChildRank.LongName, context.SourceRank.LongName));
                }
            } else if (context.Source.ElemType == context.Target.ElemType) {
                // Determine what the user wishes to do when the drag/drop source and target are the same type.
                return PromptSourceTargetSame(context);
            }

            return null;
        }

        private DragDropAction PromptChangeAvailableName(TaxonDropContext context) {

            if (context.TargetRank != null) {
                if (context.SourceRank.Category == context.TargetRank.Category) {
                    return new ConvertingMoveDropAction(context, context.TargetChildRank);
                } else {
                    if (this.Question(_R("TaxonExplorer.prompt.ConvertAvailableName", context.SourceRank.LongName, context.TargetRank.LongName), _R("TaxonExplorer.prompt.ConvertAvailableName.Caption"))) {
                        return new ConvertingMoveDropAction(context, context.TargetRank);
                    }
                }
            }

            return null;
        }

        #endregion

        internal MapPointSet GenerateSpecimenPoints(bool showOptions, Taxon taxon) {
            if (showOptions) {
                var frm = new PointSetOptionsWindow(taxon.TaxaFullName, TaxonPointFunctionFactory(taxon));
                frm.Owner = this.FindParentWindow();
                if (frm.ShowDialog() == true) {
                    return frm.Points;
                }
            } else {
                return ExtractSpecimenPointSet(taxon);
            }
            return null;
        }

        protected Func<MapPointSet> TaxonPointFunctionFactory(Taxon t) {
            return () => {
                return ExtractSpecimenPointSet(t);
            };
        }

        protected MapPointSet ExtractSpecimenPointSet(Taxon t) {
            var data = Service.GetMaterialForTaxon(t.TaxaID.Value);
            var set = new MatrixMapPointSet(t.TaxaFullName, data, null);            
            return set;
        }

        public void XMLIOExport(params int[] taxaIds) {
            var list = new List<int>(taxaIds);
            var frm = new XMLExportOptions(User, list);
            frm.Owner = this.FindParentWindow();
            frm.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            if (frm.ShowDialog() == true) {
                // ???
            }
        }

        public void DistributionMap(TaxonViewModel taxon) {
            var map = PluginManager.Instance.GetMap();
            if (map != null) {
                var data = GenerateSpecimenPoints(true, taxon.Taxon);
                if (data != null) {
                    map.Show();
                    map.PlotPoints(data);
                }
            }
        }

        private void txtFind_TypingPaused(string text) {
            if (text.Length > 4) {
                DoFind(text);
            }
        }

        private void txtFind_KeyUp(object sender, KeyEventArgs e) {
            if (e.Key == Key.Down) {
                if (tvwResults.IsVisible) {                    
                    if (_searchModel != null && _searchModel.Count > 0) {
                        _searchModel[0].IsSelected = true;
                        TreeViewItem item = tvwResults.ItemContainerGenerator.ContainerFromItem(tvwResults.SelectedItem) as TreeViewItem;
                        item.Focus();
                    }                    
                } else {
                    tvwAllTaxa.Focus();
                }
            }

            if (e.Key == Key.Enter) {
                DoFind(txtFind.Text);
            }
        }

        private void TreeView_MouseRightButtonUp(object sender, MouseButtonEventArgs e) {
            var tvw = sender as TreeView;
            TaxonViewModel item = tvw.SelectedItem as TaxonViewModel;
            if (item != null) {
                ShowTaxonMenu(item, tvw);
            }
        }

        private void ShowTaxonMenu(TaxonViewModel taxon, FrameworkElement source) {

            TaxonMenuFactory factory = new TaxonMenuFactory(taxon, this, _R);

            ContextMenu menu = null;
            if (source is TreeView) {
                var tvw = source as TreeView;
                if (tvw == tvwAllTaxa) {
                    menu = factory.BuildExplorerMenu();
                } else if (tvw == tvwResults) {
                    menu = factory.BuildFindResultsMenu();
                } 
            }

            if (menu != null && menu.HasItems) {
                source.ContextMenu = menu;
            }
        }

        internal void SetManualSortMode(bool value) {
            IsManualSort = value;
            if (IsManualSort) {
                btnUp.Visibility = System.Windows.Visibility.Visible;
                btnDown.Visibility = System.Windows.Visibility.Visible;
            } else {
                btnUp.Visibility = System.Windows.Visibility.Hidden;
                btnDown.Visibility = System.Windows.Visibility.Hidden;
            }

            ReloadModel(true);
        }

        private bool InsertUniquePendingUpdate(TaxonViewModel taxon) {
            return RegisterUniquePendingChange(new UpdateTaxonCommand(taxon.Taxon), this);
        }
       
        private void ShiftTaxon(TaxonViewModel taxon, System.Func<int, int> action) {
            if (IsManualSort && taxon.Parent != null) {
                ListCollectionView view = CollectionViewSource.GetDefaultView(taxon.Parent.Children) as ListCollectionView;                

                int oldIndex = view.IndexOf(taxon);
                int newIndex = action(oldIndex);
                
                // It's possible/probable that this set of favorites has never been ordered before, so we 
                // assign an order based on the current view.
                foreach (TaxonViewModel child in taxon.Parent.Children) {                    
                    child.Order = view.IndexOf(child);
                    InsertUniquePendingUpdate(child);
                }

                if (newIndex >= 0 && newIndex < taxon.Parent.Children.Count) {
                    TaxonViewModel swapee = view.GetItemAt(newIndex) as TaxonViewModel;
                    int tmp = taxon.Order ?? 0;
                    taxon.Order = swapee.Order;
                    swapee.Order = tmp;
                    InsertUniquePendingUpdate(taxon);
                    InsertUniquePendingUpdate(swapee);
                }

                view.Refresh();
            }
            taxon.IsSelected = true;
        }

        internal void ShiftTaxonUp(TaxonViewModel taxon) {
            ShiftTaxon(taxon, (oldindex) => { return oldindex - 1; });
        }

        internal void ShiftTaxonDown(TaxonViewModel taxon) {
            ShiftTaxon(taxon, (oldindex) => { return oldindex + 1; });
        }

        private void RegenerateLabel(TaxonViewModel changedModel) {
            FindInModels(vm => {
                    if (vm is TaxonViewModel) {
                        return (vm as TaxonViewModel).TaxaID.Value == changedModel.TaxaID.Value;
                    } else if (vm is TaxonFavoriteViewModel) {
                        return (vm as TaxonFavoriteViewModel).TaxaID == changedModel.TaxaID.Value;
                    } else {
                        return false;
                    }                    
                }, 
                vm => {
                    if (vm is TaxonViewModel) {
                        var viewModel = vm as TaxonViewModel;
                        viewModel.SuspendChangeMonitoring = true;
                        viewModel.Unverified = changedModel.Unverified;
                        viewModel.Epithet = changedModel.Epithet;
                        viewModel.Author = changedModel.Author;
                        viewModel.YearOfPub = changedModel.YearOfPub;
                        viewModel.SuspendChangeMonitoring = false;
                        viewModel.RegenerateLabel();
                    } else if (vm is TaxonFavoriteViewModel) {
                        var viewModel = vm as TaxonFavoriteViewModel;
                        viewModel.SuspendChangeMonitoring = true;
                        viewModel.Unverified = changedModel.Unverified.Value;
                        viewModel.Epithet = changedModel.Epithet;                        
                        viewModel.YearOfPub = changedModel.YearOfPub;
                        viewModel.SuspendChangeMonitoring = false;
                        viewModel.TaxonLabel = changedModel.TaxonLabel;
                    }
                }
            );
        }

        internal void EditTaxonDetails(int? taxonId) {
            if (taxonId.HasValue) {

                if (taxonId.Value < 0) {
                    ErrorMessage.Show("You must save your changes before proceeding");
                    return;
                }

                Taxon fullDetails = Service.GetTaxon(taxonId.Value);
                TaxonViewModel model = new TaxonViewModel(null, fullDetails, null);
                bool readOnly = !User.HasBiotaPermission(taxonId.Value, PERMISSION_MASK.UPDATE);
                TaxonDetails control = new TaxonDetails(Owner, model, User, (changedModel) => {
                    RegenerateLabel(changedModel);
                }, readOnly);

                TaxonRank taxonRank = Service.GetTaxonRank(model.ElemType);

                String title = String.Format("Taxon Detail: {0} ({1}) [{2}] {3}", model.TaxaFullName, taxonRank == null ? "Unranked" : taxonRank.GetElementTypeLongName(model.Taxon), model.TaxaID, readOnly ? "(Read Only)" : "");

                PluginManager.Instance.AddNonDockableContent(Owner, control, title, SizeToContent.Manual);
            }
        }

        internal void Refresh() {
            if (AnyChanges()) {
                if (this.DiscardChangesQuestion("You have unsaved changes. Refreshing will cause those changes to be discarded. Are you sure you want to discard unsaved changes?")) {
                    ReloadModel();
                }
            } else {
                ReloadModel();
            }
        }

        private int GetNewTaxonID() {
            lock (this) {
                return _nextNewTaxonID--;
            }
        }

        internal void RenameTaxon(TaxonViewModel taxon) {

            if (!User.HasBiotaPermission(taxon.TaxaID.Value, PERMISSION_MASK.UPDATE)) {
                ErrorMessage.Show("You do not have permission to rename '" + taxon.TaxaFullName + "'!");
                return;
            }

            taxon.IsSelected = true;
            taxon.IsRenaming = true;
        }

        private string GetDefaultDisplayLabel(TaxonViewModel taxon) {

            if (taxon.Unplaced.ValueOrFalse()) {
                return "Unplaced";
            }

            if (taxon.ElemType.Equals(TaxonRank.INCERTAE_SEDIS)) {
                return "Incertae Sedis";
            }

            if (taxon.ElemType.Equals(TaxonRank.SPECIES_INQUIRENDA)) {
                return "Species Inquirenda";
            }

            if (taxon.AvailableName.ValueOrFalse() || taxon.AvailableName.ValueOrFalse()) {
                return "Name Author, Year";
            }

            TaxonRank rank = Service.GetTaxonRank(taxon.ElemType);
            if (rank != null) {
                if (rank.JoinToParentInFullName.ValueOrFalse()) {
                    return "OnlyEpithet Author, Year";
                }
            }

            return "Name Author, Year";
        }

        #region Add New Taxon

        internal TaxonViewModel AddNewTaxon(TaxonViewModel parent, string elemType, TaxonViewModelAction action, bool startRename = true, bool select = true) {

            // TODO: check permissions...

            Taxon t = new Taxon();
            t.TaxaParentID = parent.TaxaID.Value;
            t.TaxaID = GetNewTaxonID();
            TaxonViewModel viewModel = new TaxonViewModel(parent, t, GenerateTaxonDisplayLabel);

            viewModel.ElemType = elemType;
            viewModel.KingdomCode = parent.KingdomCode;
            viewModel.Author = "";
            viewModel.YearOfPub = "";
            viewModel.Epithet = "";            
            parent.IsExpanded = true;

            try {
                if (action != null) {
                    action(viewModel);
                }

                parent.Children.Add(viewModel);
                // Move to the top of the list, until it is saved...
                parent.Children.Move(parent.Children.IndexOf(viewModel), 0);
                if (select) {
                    viewModel.IsSelected = true;
                }

                if (startRename) {
                    RenameTaxon(viewModel);
                }

                RegisterPendingChange(new InsertTaxonDatabaseCommand(viewModel), this);

            } catch (Exception ex) {
                ErrorMessage.Show(ex.Message);
            }
            return viewModel;
        }

        internal TaxonViewModel AddNewTaxon(TaxonViewModel parent, TaxonRank rank, bool unplaced = false) {

            return AddNewTaxon(parent, rank.Code, (taxon) => {
                taxon.Unplaced = unplaced;
                string parentChildElemType = GetChildElementType(parent);
                if (!String.IsNullOrEmpty(parentChildElemType) && taxon.ElemType != parentChildElemType) {
                    TaxonRank parentChildRank = Service.GetTaxonRank(parentChildElemType);
                    if (!Service.IsValidChild(parentChildRank, rank)) {
                        throw new Exception("Cannot insert an " + rank.LongName + " entry because this entry cannot be a valid current for the current children.");
                    } else {
                        // Create a new unplaced to hold the existing children
                        TaxonViewModel newUnplaced = AddNewTaxon(parent, rank.Code, (t) => {
                            t.Unplaced = true;                            
                        }, false, false);

                        foreach (HierarchicalViewModelBase vm in parent.Children) {
                            // Don'note add the new child as a child of itself! Its already been added to children collection by the other AddNewTaxon method
                            if (vm != newUnplaced) {
                                TaxonViewModel child = vm as TaxonViewModel;
                                newUnplaced.Children.Add(child);
                                child.TaxaParentID = newUnplaced.TaxaID;
                                child.Parent = newUnplaced;
                                RegisterPendingChange(new MoveTaxonDatabaseCommand(child, newUnplaced), this);
                            }
                        }
                        parent.Children.Clear();
                        parent.Children.Add(newUnplaced);
                        newUnplaced.IsExpanded = true;
                    }
                }
            }, true);

        }

        internal void AddUnrankedValid(TaxonViewModel taxon) {
            AddNewTaxon(taxon, "", (child) => {

            });
        }

        internal void AddSpeciesInquirenda(TaxonViewModel parent) {
            AddNewTaxon(parent, TaxonRank.SPECIES_INQUIRENDA, (newChild) => {
            }, false);
        }

        internal void AddIncertaeSedis(TaxonViewModel parent) {
            AddNewTaxon(parent, TaxonRank.INCERTAE_SEDIS, (newChild) => {
            }, false);
        }

        internal void AddLiteratureName(TaxonViewModel parent) {
            AddNewTaxon(parent, parent.ElemType, (newChild) => {
                newChild.LiteratureName = true;
            });
        }

        internal void AddNewTaxonAllRanks(TaxonViewModel parent) {
            throw new NotImplementedException();
        }

        internal void AddAvailableName(TaxonViewModel parent) {
            AddNewTaxon(parent, parent.ElemType, (newChild) => {
                switch (parent.ElemType) {
                    case TaxonRank.INCERTAE_SEDIS:
                    case TaxonRank.SPECIES_INQUIRENDA:
                        // Incerate sedis and species inq only have available species names.
                        newChild.ElemType = TaxonRank.SPECIES;
                        break;
                    default:
                        break;
                }
                newChild.AvailableName = true;
            });
        }

        #endregion

        private void MarkItemAsDeleted(HierarchicalViewModelBase taxon) {
            taxon.IsDeleted = true;

            // the alternate way, don'note strikethrough - just remove it, and mark the current as changed
            // taxon.Parent.IsChanged = true;
            // taxon.Parent.Children.Remove(taxon);

            // Although we don'note need to delete children explicitly from the database (the stored proc will take care of that for us),
            // we still need to mark each child as deleted in the UI
            foreach (HierarchicalViewModelBase child in taxon.Children) {
                MarkItemAsDeleted(child);
            }
        }

        internal void DeleteTaxon(TaxonViewModel taxon) {
            if (this.Question(_R("TaxonExplorer.prompt.DeleteTaxon", taxon.DisplayLabel), _R("TaxonExplorer.prompt.DeleteTaxon.Caption"))) {
                // schedule the database bit...                                
                RegisterPendingChange(new DeleteTaxonDatabaseCommand(taxon), this);
                // and the update the UI
                MarkItemAsDeleted(taxon);
            }
        }

        internal void ExpandChildren(TaxonViewModel taxon, List<Taxon> remaining = null) {
            if (remaining == null) {
                remaining = Service.GetExpandFullTree(taxon.TaxaID.Value);
            }
            if (!taxon.IsExpanded) {
                // BringModelToView(tvwAllTaxa, taxon);
                taxon.BulkAddChildren(remaining.FindAll((elem) => { return elem.TaxaParentID == taxon.TaxaID; }), GenerateTaxonDisplayLabel);
                remaining.RemoveAll((elem) => { return elem.TaxaParentID == taxon.TaxaID; });
                taxon.IsExpanded = true;
                
            }

            foreach (HierarchicalViewModelBase child in taxon.Children) {
                ExpandChildren(child as TaxonViewModel, remaining);
            }
        }

        internal void CollapseChildren(HierarchicalViewModelBase taxon) {
            foreach (HierarchicalViewModelBase m in taxon.Children) {
                if (m.IsExpanded) {
                    CollapseChildren(m);
                    m.IsExpanded = false;
                }
            }
        }

        internal void ShowInExplorer(int? taxonId) {

            if (!taxonId.HasValue) {
                return;
            }

            tabAllTaxa.IsSelected = true;
            txtFind.Text = "";

            JobExecutor.QueueJob(() => {
                // make sure the explorer tree is visible...
                string parentage = Service.GetTaxonParentage(taxonId.Value);
                if (!String.IsNullOrEmpty(parentage)) {
                    this.InvokeIfRequired(() => {
                        ExpandFromParentage(parentage);
                    });
                }

            });
        }

        public void ExpandFromParentage(string parentage) {
            string[] bits = ("0" + parentage).Split('\\');
            // Start at the top...
            ObservableCollection<HierarchicalViewModelBase> col = _explorerModel;
            TaxonViewModel child = null;

            foreach (string taxonId in bits) {
                if (!String.IsNullOrEmpty(taxonId)) {
                    int intTaxonId = Int32.Parse(taxonId);
                    child = col.FirstOrDefault((item) => {
                        return (item as TaxonViewModel).TaxaID == intTaxonId;
                    }) as TaxonViewModel;
                    if (child != null) {                        
                        child.IsExpanded = true;
                        col = child.Children;
                    }
                }
            }

            if (child != null) {
                tvwAllTaxa.BringModelToView(child);
                tvwAllTaxa.Focus();
                child.IsSelected = true;
            }

        }

        private void TreeViewItem_MouseRightButtonDown(object sender, MouseEventArgs e) {
            TreeViewItem item = sender as TreeViewItem;
            if (item != null) {
                item.Focus();
                e.Handled = true;
            }
        }

        public void ReloadModel(bool rememberExpanded = true) {
            // Keep track of which nodes are expanded.
            List<string> expanded = null;
            if (rememberExpanded) {
                expanded = Owner.GetExpandedParentages(_explorerModel);
            }

            // Reload the model
            _explorerModel = LoadTaxonViewModel();
            if (expanded != null && expanded.Count > 0) {
                // expand out to what it was before the tree was re-locked                        
                if (_explorerModel != null && _explorerModel.Count > 0) {
                    ExpandParentages(_explorerModel[0], expanded);
                }
            }
            ClearPendingChanges();
            tvwAllTaxa.ItemsSource = _explorerModel;
            // Favorites
            if (favorites.IsFavoritesLoaded) {
                favorites.ReloadFavorites();
            }

        }

        public string _R(string key, params object[] args) {
            return Owner.GetCaption(key, args);
        }

        /// <summary>
        /// Return the elemType of the first child that is not "unplaced", including available names, species inquirenda and incertae sedis
        /// </summary>
        /// <param name="current"></param>
        /// <returns></returns>
        private string GetChildElementType(TaxonViewModel parent) {
            if (!parent.IsExpanded) {
                parent.IsExpanded = true; // This will load the children, if they are not already loaded...
            }

            foreach (TaxonViewModel child in parent.Children) {

                if (String.IsNullOrEmpty(child.ElemType)) {
                    continue;
                }

                // Ignore available and literature names
                if (child.AvailableName.ValueOrFalse() || child.LiteratureName.ValueOrFalse()) {
                    continue;
                }

                // Also special consideration for species inquirenda and incertae sedis
                if (child.ElemType == TaxaService.SPECIES_INQUIRENDA || child.ElemType == TaxaService.INCERTAE_SEDIS) {
                    continue;
                }

                return child.ElemType;
            }

            return "";
        }

        public bool AnyChanges() {

            bool changed = false;
            if (_explorerModel != null) {
                foreach (HierarchicalViewModelBase item in _explorerModel) {
                    item.Traverse((node) => {
                        if (node.IsChanged) {
                            changed = true;
                        }
                    });
                }
            }

            if (!changed) {
                changed = favorites.HasPendingChanges;
            }

            return changed;
        }

        private void btnApplyChanges_Click(object sender, RoutedEventArgs e) {

            if (AnyChanges()) {
                CommitPendingChanges(() => {
                    ReloadModel();
                });
                btnLock.IsChecked = false;
            }

        }

        private void btnCancelEditing_Click(object sender, RoutedEventArgs e) {
            btnLock.IsChecked = false;
        }

        private void EditableTextBlock_EditingComplete(object sender, string text) {
            TaxonViewModel tvm = (sender as EditableTextBlock).ViewModel as TaxonViewModel;
            ProcessRename(tvm, text);
        }

        /// <summary>
        /// Attempt to extract epithet author year and change combination status from the text
        /// </summary>
        /// <param name="taxon"></param>
        /// <param name="label"></param>
        private void ProcessRename(TaxonViewModel taxon, string label) {

            TaxonName name = TaxonNameParser.ParseName(taxon, label);
            if (name != null) {
                taxon.Author = name.Author;
                taxon.Epithet = name.Epithet;
                taxon.YearOfPub = name.Year;
                taxon.ChgComb = name.ChangeCombination;                
                InsertUniquePendingUpdate(taxon);
            } else {
                ErrorMessage.Show("Please enter at least the epithet, with author and year where appropriate.");
                RenameTaxon(taxon);
            }

        }

        private void btnDown_Click(object sender, RoutedEventArgs e) {
            var taxon = tvwAllTaxa.SelectedItem as TaxonViewModel;
            if (taxon != null) {
                ShiftTaxonDown(taxon);
            }
        }

        private void btnUp_Click(object sender, RoutedEventArgs e) {
            var taxon = tvwAllTaxa.SelectedItem as TaxonViewModel;
            if (taxon != null) {
                ShiftTaxonUp(taxon);
            }
        }

        #region Properties

        internal ObservableCollection<HierarchicalViewModelBase> ExplorerModel {
            get { return _explorerModel; }
            set {
                _explorerModel = value;
                tvwAllTaxa.Items.Clear();
                this.tvwAllTaxa.ItemsSource = _explorerModel;
            }
        }

        internal bool IsUnlocked {
            get { return btnLock.IsChecked.GetValueOrDefault(false); }
        }

        public static bool IsManualSort { get; set; }

        #endregion

        internal void RunReport(IBioLinkReport report) {
            PluginManager.Instance.RunReport(Owner, report);
        }

        internal void EditTaxonName(TaxonViewModel viewModel) {

            if (viewModel.TaxaID.Value < 0) {
                ErrorMessage.Show("You must save your changes before proceeding");
                return;
            }

            var readOnly = !User.HasBiotaPermission(viewModel.TaxaID.Value, PERMISSION_MASK.UPDATE);

            TaxonNameDetails details = new TaxonNameDetails(viewModel.TaxaID, User, (nameDetails) => {
                RegenerateLabel(nameDetails);
            }) { IsReadOnly = readOnly };
            PluginManager.Instance.AddNonDockableContent(this.Owner, details, "Taxon name details",SizeToContent.WidthAndHeight);
        }

        private List<HierarchicalViewModelBase> FindInModels(Predicate<HierarchicalViewModelBase> predicate, Action<HierarchicalViewModelBase> action = null) {

            var candidates = new List<HierarchicalViewModelBase>();

            var models = new System.Collections.IList[] { _explorerModel, _searchModel, favorites.Model };

            foreach (System.Collections.IList model in models) {

                if (model == null) {
                    continue;
                }

                foreach (HierarchicalViewModelBase vm in model) {
                    vm.Traverse((node) => {
                        if (predicate(node)) {
                            candidates.Add(node);
                            if (action != null) {
                                action(node);
                            }
                        }
                    });
                }

            }

            return candidates;
        }

        //public void BringModelToView(TreeView tvw, HierarchicalViewModelBase item) {
        //    ItemsControl itemsControl = tvw;

        //    // Get the stack of parentages...
        //    var stack = item.GetParentStack();

        //    // Descend through the levels until the desired TreeViewItem is found.
        //    while (stack.Count > 0) {
        //        HierarchicalViewModelBase model = stack.Pop();

        //        if (!model.IsExpanded) {
        //            model.IsExpanded = true;
        //        }

        //        bool foundContainer = false;
        //        int index = (model.Parent == null ? 0 : model.Parent.Children.IndexOf(model));
                
        //        // Access the custom VSP that exposes BringIntoView
        //        BLVirtualizingStackPanel itemsHost = FindVisualChild<BLVirtualizingStackPanel>(itemsControl);
        //        if (itemsHost != null) {
        //            // Due to virtualization, BringIntoView may not predict the offset correctly the first time.
        //            ItemsControl nextItemsControl = null;
        //            while (nextItemsControl == null) {
        //                foundContainer = true;
        //                itemsHost.BringIntoView(index);
        //                Dispatcher.Invoke(DispatcherPriority.Background, (DispatcherOperationCallback)delegate(object unused) {
        //                    nextItemsControl = (ItemsControl)itemsControl.ItemContainerGenerator.ContainerFromIndex(index);
        //                    return null;
        //                }, null);
        //            }

        //            itemsControl = nextItemsControl;
        //        }

        //        if (!foundContainer || (itemsControl == null)) {
        //            // Abort the operation
        //            return;
        //        }
        //    }
        //}

        public TaxaService Service {
            get { return new TaxaService(User); }
        }

        //private T FindVisualChild<T>(Visual visual) where T : Visual {
        //    for (int i = 0; i < VisualTreeHelper.GetChildrenCount(visual); i++) {
        //        Visual child = (Visual)VisualTreeHelper.GetChild(visual, i);
        //        if (child != null) {
        //            T correctlyTyped = child as T;
        //            if (correctlyTyped != null) {
        //                return correctlyTyped;
        //            }

        //            T descendent = FindVisualChild<T>(child);
        //            if (descendent != null) {
        //                return descendent;
        //            }
        //        }
        //    }

        //    return null;
        //}

        public TaxaPlugin Owner { get; private set; }

        internal void RemoveFromFavorites(int favoriteId) {
            favorites.DeleteFavorite(favoriteId);
        }

        internal void AddToFavorites(TaxonViewModel Taxon, bool global) {
            tabFavorites.IsSelected = true;
            favorites.AddToFavorites(Taxon, global);
            favorites.Focus();
        }

        internal void AddFavoriteGroup(HierarchicalViewModelBase parentNode) {
            favorites.AddFavoriteGroup(parentNode);
        }

        internal void RenameFavoriteGroup(TaxonFavoriteViewModel taxonFavoriteViewModel) {
            favorites.RenameFavoriteGroup(taxonFavoriteViewModel);
        }

        internal void EditBiotaPermissions(TaxonViewModel taxon) {
            if (!taxon.TaxaID.HasValue || taxon.TaxaID.Value < 0) {
                ErrorMessage.Show("You must save apply changes before you can proceed.");
                return;
            }

            bool readOnly = !User.HasBiotaPermission(taxon.TaxaID.Value, PERMISSION_MASK.OWNER);

            PluginManager.Instance.AddNonDockableContent(Owner, new BiotaPermissions(User, taxon, readOnly), "Taxon Permissions for '" + taxon.TaxaFullName + (readOnly ? "' (Read Only)" : "'"), SizeToContent.Manual);
        }

        internal void ChangeRank(TaxonViewModel taxon) {
            if (!User.HasBiotaPermission(taxon.TaxaID.Value, PERMISSION_MASK.UPDATE)) {
                ErrorMessage.Show("You do not have sufficient priviledges to perform this operation!");
                return;
            }

            var validTypes = GetValidChildren(taxon);

            var frm = new ChangeRankWindow(User, taxon, validTypes);
            frm.Owner = this.FindParentWindow();
            if (frm.ShowDialog() == true) {
                var selectedRank = frm.SelectedRank == null ? "" : frm.SelectedRank.Code;
                if (selectedRank != taxon.ElemType) {
                    taxon.ElemType = selectedRank; // should trigger a change!
                    InsertUniquePendingUpdate(taxon);
                }                
            }

        }

        private List<TaxonRank> GetValidChildren(TaxonViewModel taxon) {

            // Look at the valid ranks for taxon's children...

            string siblingRank = null;

            bool parentIsRoot = false;

            var parent = taxon.Parent as TaxonViewModel;
            if (parent == null) {
                if (tabFind.IsSelected == true) {
                    ErrorMessage.Show("Could not find the rank of the current items parent. Please use 'Find in explorer' and try again");
                    return null;
                } else {
                    parentIsRoot = true;
                }
            } else {
                siblingRank = GetChildElementType(parent);
            }

            var currentChildRank = GetChildElementType(taxon);
            var list = new List<TaxonRank>();
            if (parentIsRoot) {
                // Get the element type of the first taxon in tree...
                var toplevel = _explorerModel[0].Children[0] as TaxonViewModel;
                string elemType = Service.GetTaxonRanks()[0].Code;
                if (toplevel != null) {
                    elemType = toplevel.ElemType;
                }
                list.Add(Service.GetTaxonRank(elemType));
            } else if (!string.IsNullOrWhiteSpace(siblingRank)) {
                // sibling has a rank, use it...
                list.Add(Service.GetTaxonRank(siblingRank));                
            } else if (string.IsNullOrWhiteSpace(parent.ElemType)) {
                // parent is unranked, so allow all options...                
                list.AddRange(Service.GetDefaultChildRanks("", taxon.KingdomCode));
            } else if (string.IsNullOrWhiteSpace(currentChildRank)) {
                // Current item has no children or children of no rank so the current item can become any of the valid children of the parent.
                list.AddRange(Service.GetDefaultChildRanks(parent.ElemType, taxon.KingdomCode));
            } else {
                // For each of the possible child types associated with the parent, see which ones will co-exist with the 'to be ranked' items children.
                var candidates = Service.GetDefaultChildRanks(parent.ElemType, taxon.KingdomCode);
                foreach (TaxonRank candidate in candidates) {
                    var validChildTypes = Service.GetDefaultChildRanks(currentChildRank, taxon.KingdomCode);
                    bool match = false;
                    foreach (TaxonRank child in validChildTypes) {
                        if (candidate.Code.Equals(child.Code)) {
                            match = true;
                            break;
                        }
                    }
                    if (match) {
                        list.Add(candidate);
                    }
                }
            }

            return list;
        }


        internal void ChangeAvailable(TaxonViewModel taxon) {

            if (taxon == null) {
                return;
            }

            if (!User.HasBiotaPermission(taxon.TaxaID.Value, PERMISSION_MASK.UPDATE)) {
                ErrorMessage.Show("You do not have sufficient priviledges to perform this operation!");
                return;
            }

            var rankCategory = Service.GetTaxonRank(taxon.ElemType).Category ?? "";

            string auxMsg = "";

            if (taxon.AvailableName == true) {
                
                switch (rankCategory.ToUpper()) {
                    case "G":
                        auxMsg = "WARNING: Changing to Literature Name will result in the permanent loss of any Type Species Designation information.\n\n";
                        break;
                    case "S":
                        auxMsg = "WARNING: Changing to Literature Name will result in the permanent loss of any Type Data information.\n\n";
                        break;
                }

                if (this.Question(auxMsg + "Are you sure you wish to change this Available Name to a Literature Name?", "Change to Literature Name?")) {
                    taxon.LiteratureName = true;
                    taxon.AvailableName = false;
                    InsertUniquePendingUpdate(taxon);
                }
            } else if (taxon.LiteratureName == true) {

                if (this.Question("Are you sure you wish to change this Literature Name to an Available Name?", "Change to Available Name?")) {
                    taxon.LiteratureName = false;
                    taxon.AvailableName = true;
                    InsertUniquePendingUpdate(taxon);
                }

            }

        }

        internal void DeleteLevel(TaxonViewModel taxon) {
            if (taxon == null) {
                return;
            }

            if (!User.HasBiotaPermission(taxon.TaxaID.Value, PERMISSION_MASK.DELETE)) {
                ErrorMessage.Show("You do not have sufficient priviledges to perform this operation!");
                return;
            }

            var parent = taxon.Parent as TaxonViewModel;
            if (parent == null) {
                ErrorMessage.Show("You cannot remove the top level!");
                return;
            }

            // Prompt the user to continue.
            if (!this.Question("Are you sure you would like to remove all the valid names in the level of which '" + taxon.TaxaFullName + "' is a member.", "Delete Level?")) {
                return;
            }

            // Create a list of children that need to be moved to the parent....
            taxon.IsExpanded = true;
            var parentRank = Service.GetTaxonRank(parent.ElemType);

            var deleteList = new List<TaxonViewModel>();
            foreach (TaxonViewModel sibling in parent.Children) {
                if (!sibling.ElemType.Equals(TaxonRank.INCERTAE_SEDIS) && !sibling.ElemType.Equals(TaxonRank.SPECIES_INQUIRENDA) && !sibling.IsAvailableOrLiteratureName) {
                    deleteList.Add(sibling);
                }
            }

            var childList = new List<TaxonViewModel>();
            string childType = "";
            foreach (TaxonViewModel sibling in deleteList) {
                // Determine from the examination of the current child if the process can continue.                
                sibling.IsExpanded = true;

                string strCurrentSiblingElemType = GetChildElementType(sibling);
                var siblingChildRank = Service.GetTaxonRank(strCurrentSiblingElemType);

                if (string.IsNullOrEmpty(childType)) {
                    childType = strCurrentSiblingElemType;
                    if (!string.IsNullOrEmpty(strCurrentSiblingElemType)) {
                        // If the child type cannot exist under the new parent type, then exit
                        if (!Service.IsValidChild(siblingChildRank, parentRank)) {
                            ErrorMessage.Show("Delete Level Failed: Items of type '" + siblingChildRank.LongName + "' cannot exist as children of a '" + parentRank.LongName + "' item!");
                            return;
                        }
                    }
                } else {
                    if (!string.IsNullOrEmpty(strCurrentSiblingElemType) && !sibling.ElemType.Equals(childType)) {
                        // tell the user that the children of the level being removed are mixed.
                        ErrorMessage.Show("Delete Level Failed: The subitems of the level being removed must be of the same type!");
                        return;
                    }
                }

                foreach (TaxonViewModel child in sibling.Children) {
                    childList.Add(child);
                }
            }

            // Go through the collection and convert the available names if required, and move each element...
            foreach (TaxonViewModel child in childList) {
                if (child.IsAvailableOrLiteratureName) {
                    child.ElemType = parent.ElemType;
                }
                Move(child, parent);
                InsertUniquePendingUpdate(child);
            }

            foreach (TaxonViewModel sibling in deleteList) {
                // schedule the database bit...                                
                RegisterPendingChange(new DeleteTaxonDatabaseCommand(sibling), this);
                // and the update the UI
                MarkItemAsDeleted(sibling);
            }

        }

        protected void Move(TaxonViewModel source, TaxonViewModel target) {
            source.Parent.Children.Remove(source);
            target.Children.Add(source);
            source.Parent = target;
            source.TaxaParentID = target.TaxaID;
        }

    }

}
