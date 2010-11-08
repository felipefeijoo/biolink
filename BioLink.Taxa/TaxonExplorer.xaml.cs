using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Controls;
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
    public partial class TaxonExplorer : DatabaseActionControl {
        
        private ObservableCollection<HierarchicalViewModelBase> _explorerModel;
        private ObservableCollection<TaxonViewModel> _searchModel;
        private Point _startPoint;
        private bool _IsDragging = false;
        private UIElement _dragScope;
        private DragAdorner _adorner;
        private AdornerLayer _layer;
        private bool _dragHasLeftScope = false;
        private int _nextNewTaxonID = -100;

        #region Designer Constructor
        public TaxonExplorer() {
            InitializeComponent();
        }
        #endregion

        public TaxonExplorer(TaxaPlugin owner)
            : base(owner.User, "TaxonExplorer") {

            InitializeComponent();

            Owner = owner;

            _searchModel = new ObservableCollection<TaxonViewModel>();
            lstResults.ItemsSource = _searchModel;

            ListCollectionView dataView = CollectionViewSource.GetDefaultView(lstResults.ItemsSource) as ListCollectionView;
            dataView.SortDescriptions.Clear();
            dataView.SortDescriptions.Add(new SortDescription("IsAvailableOrLiteratureName", ListSortDirection.Ascending));
            dataView.SortDescriptions.Add(new SortDescription("TaxaFullName", ListSortDirection.Ascending));
            dataView.Refresh();

            IsManualSort = Config.GetUser(Owner.User, "Taxa.ManualSort", false);

            if (IsManualSort) {
                btnDown.Visibility = System.Windows.Visibility.Visible;
                btnUp.Visibility = System.Windows.Visibility.Visible;
            }

            btnLock.Checked += new RoutedEventHandler(btnLock_Checked);

            btnLock.Unchecked += new RoutedEventHandler(btnLock_Unchecked);
        }

        void btnLock_Unchecked(object sender, RoutedEventArgs e) {
            lblHeader.Visibility = Visibility.Hidden;
            if (AnyChanges()) {
                if (this.Question(_R("TaxonExplorer.prompt.LockDiscardChanges"), _R("TaxonExplorer.prompt.ConfirmAction.Caption"))) {
                    ReloadModel();
                } else {
                    // Cancel the unlock
                    btnLock.IsChecked = true;
                }
            }
            gridContentsHeader.Background = SystemColors.ControlBrush;
            buttonBar.Visibility = IsUnlocked ? Visibility.Visible : Visibility.Hidden;
        }

        void btnLock_Checked(object sender, RoutedEventArgs e) {
            lblHeader.Visibility = Visibility.Hidden;
            buttonBar.Visibility = IsUnlocked ? Visibility.Visible : Visibility.Hidden;
            gridContentsHeader.Background = new LinearGradientBrush(Colors.DarkOrange, Colors.Orange, 90.0);
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

            try {
                lstResults.InvokeIfRequired(() => {
                    lstResults.Cursor = Cursors.Wait;
                });
                if (Owner == null) {
                    return;
                }
                List<TaxonSearchResult> results = new TaxaService(Owner.User).FindTaxa(searchTerm);
                _searchModel.Clear();
                lstResults.InvokeIfRequired(() => {                    
                    foreach (Taxon t in results) {
                        _searchModel.Add(new TaxonViewModel(null, t, GenerateTaxonDisplayLabel));
                    }

                });
            } finally {
                lstResults.InvokeIfRequired(() => {
                    lstResults.Cursor = Cursors.Arrow;
                });
            }
        }

        private void tabControl1_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (tabControl1.SelectedItem == tabFavorites) {
                if (favorites.User == null) {
                    favorites.BindUser(User);
                    favorites.LoadFavorites();
                }
            }
        }

        public void InitialiseTaxonExplorer() {
            // Load the model on the background thread
            ObservableCollection<HierarchicalViewModelBase> model = LoadTaxonViewModel();

            // Now see if we can auto-expand from the last session...
            if (model.Count > 0 && Config.GetGlobal<bool>("Taxa.RememberExpandedTaxa", true)) {
                var expanded = Config.GetProfile<List<String>>(Owner.User, "Taxa.Explorer.ExpandedTaxa", null);
                ExpandParentages(model[0], expanded);
            }

            // and set it on the the components own thread...
            this.InvokeIfRequired(() => {
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

        void item_LazyLoadChildren(HierarchicalViewModelBase item) {

            item.Children.Clear();

            if (item is TaxonViewModel) {
                TaxonViewModel tvm = item as TaxonViewModel;
                Debug.Assert(tvm.TaxaID.HasValue, "TaxonViewModel has no taxa id!");
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


        #region Drag and Drop stuff

        void tvwAllTaxa_PreviewMouseMove(object sender, MouseEventArgs e) {
            CommonPreviewMouseView(e, tvwAllTaxa);
        }

        private void CommonPreviewMouseView(MouseEventArgs e, TreeView treeView) {

            if (e.LeftButton == MouseButtonState.Pressed && !_IsDragging) {
                Point position = e.GetPosition(tvwAllTaxa);
                if (Math.Abs(position.X - _startPoint.X) > SystemParameters.MinimumHorizontalDragDistance || Math.Abs(position.Y - _startPoint.Y) > SystemParameters.MinimumVerticalDragDistance) {
                    if (treeView.SelectedItem != null) {
                        if (IsUnlocked) {
                            IInputElement hitelement = treeView.InputHitTest(_startPoint);
                            TreeViewItem item = GetTreeViewItemClicked((FrameworkElement)hitelement, treeView);
                            if (item != null) {
                                StartDrag(e, treeView, item);
                            }
                        } else {
                            lblHeader.Visibility = Visibility.Visible;
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

        void tvwAllTaxa_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            _startPoint = e.GetPosition(tvwAllTaxa);
        }

        private TreeViewItem GetHoveredTreeViewItem(DragEventArgs e) {
            TreeView tvw = _dragScope as TreeView;
            DependencyObject elem = tvw.InputHitTest(e.GetPosition(tvw)) as DependencyObject;
            while (elem != null && !(elem is TreeViewItem)) {
                elem = VisualTreeHelper.GetParent(elem);
            }
            return elem as TreeViewItem;
        }

        private void DragSource_GiveFeedback(object source, GiveFeedbackEventArgs e) {
            e.UseDefaultCursors = true;
            e.Handled = true;
        }

        private void DropSink_DragOver(object source, DragEventArgs e) {
            TaxonViewModel t = e.Data.GetData("Taxon") as TaxonViewModel;
            TreeView tvw = _dragScope as TreeView;

            if (t != null && tvw != null) {

                TreeViewItem destItem = GetHoveredTreeViewItem(e);
                if (destItem != null) {
                    TaxonViewModel destTaxon = destItem.Header as TaxonViewModel;
                    if (destTaxon != null) {
                        destItem.IsSelected = true;
                    }
                }

            }
            if (_adorner != null) {
                _adorner.LeftOffset = e.GetPosition(_dragScope).X;
                _adorner.TopOffset = e.GetPosition(_dragScope).Y;
            }

        }

        private void DragScope_DragLeave(object source, DragEventArgs e) {

            TreeViewItem destItem = GetHoveredTreeViewItem(e);
            if (destItem != null) {
                TaxonViewModel destTaxon = destItem.Header as TaxonViewModel;
                if (destTaxon != null) {
                }
            }

            if (e.OriginalSource == _dragScope) {
                Point p = e.GetPosition(_dragScope);
                Rect r = VisualTreeHelper.GetContentBounds(_dragScope);
                if (!r.Contains(p)) {
                    this._dragHasLeftScope = true;
                    e.Handled = true;
                }
            }
        }

        private void DragScope_QueryContinueDrag(object source, QueryContinueDragEventArgs e) {
            if (e.EscapePressed) {
                e.Action = DragAction.Cancel;
            }
            if (this._dragHasLeftScope) {
                e.Action = DragAction.Cancel;
                e.Handled = true;
            }
        }

        void _dragScope_Drop(object sender, DragEventArgs e) {
            TaxonViewModel src = e.Data.GetData("Taxon") as TaxonViewModel;
            TreeViewItem destItem = GetHoveredTreeViewItem(e);
            if (destItem != null) {
                TaxonViewModel dest = destItem.Header as TaxonViewModel;
                if (src != null && dest != null) {

                    if (src == dest) {
                        // if the source and the destination are the same, there is no logical operation that can be performed.
                        // We could irritate the user with a pop-up, but this situation is more than likely the result
                        // of an accidental drag, so just cancel the drop...
                        return;
                    }

                    ProcessTaxonDragDrop(src, dest);

                    e.Handled = true;
                }
            }

        }

        public void ProcessTaxonDragDrop(TaxonViewModel source, TaxonViewModel target) {

            try {
                // There are 4 possible outcomes from a taxon drop
                // 1) The drop is invalid, so do nothing (display an error message)
                // 2) The drop is a valid move, no conversion or merging required (simplest case)
                // 3) The drop is to that of a sibling rank, and so a decision is made to either merge or move as child (if possible)
                // 4) The drop is valid, but requires the source to be converted into a valid child of the target
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
                    List<DatabaseAction> dbActions = action.ProcessUI();
                    if (dbActions != null && dbActions.Count > 0) {
                        RegisterPendingChanges(dbActions);
                    }
                }
            } catch (IllegalTaxonMoveException ex) {
                MessageBox.Show(ex.Message, String.Format("Cannot move '{0}'", source.Epithet), MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }

            source.IsSelected = true;
        }

        private void StartDrag(MouseEventArgs e, TreeView treeView, TreeViewItem item) {
            _dragScope = treeView;
            bool previousDrop = _dragScope.AllowDrop;
            _dragScope.AllowDrop = true;

            GiveFeedbackEventHandler feedbackhandler = new GiveFeedbackEventHandler(DragSource_GiveFeedback);
            item.GiveFeedback += feedbackhandler;


            DragEventHandler drophandler = new DragEventHandler(_dragScope_Drop);
            _dragScope.Drop += drophandler;

            DragEventHandler draghandler = new DragEventHandler(DropSink_DragOver);
            _dragScope.PreviewDragOver += draghandler;

            DragEventHandler dragleavehandler = new DragEventHandler(DragScope_DragLeave);
            _dragScope.DragLeave += dragleavehandler;

            QueryContinueDragEventHandler queryhandler = new QueryContinueDragEventHandler(DragScope_QueryContinueDrag);
            _dragScope.QueryContinueDrag += queryhandler;

            _adorner = new DragAdorner(_dragScope, item, true, 0.5, e.GetPosition(item));
            _layer = AdornerLayer.GetAdornerLayer(_dragScope as Visual);
            _layer.Add(_adorner);

            _IsDragging = true;
            _dragHasLeftScope = false;
            TaxonViewModel taxon = treeView.SelectedItem as TaxonViewModel;
            if (taxon != null) {
                DataObject data = new DataObject("Taxon", taxon);
                DragDropEffects de = DragDrop.DoDragDrop(item, data, DragDropEffects.Move);
            }

            _dragScope.AllowDrop = previousDrop;
            AdornerLayer.GetAdornerLayer(_dragScope).Remove(_adorner);
            _adorner = null;

            item.GiveFeedback -= feedbackhandler;
            _dragScope.DragLeave -= dragleavehandler;
            _dragScope.QueryContinueDrag -= queryhandler;
            _dragScope.PreviewDragOver -= draghandler;
            _dragScope.Drop -= drophandler;

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
                // Can't drop on to an Available or Literature Name
                throw new IllegalTaxonMoveException(context.Source.Taxon, context.Target.Taxon, _R("TaxonExplorer.DropError.AvailableName", context.Source.Epithet, context.Target.Epithet));
            } else if (context.Source.AvailableName.GetValueOrDefault(false) || context.Source.LiteratureName.GetValueOrDefault(false)) {
                // if the source is an Available or Literature Name 
                if (context.Source.ElemType != context.Target.ElemType) {
                    // If the target is not of the same type as the source, confirm the conversion of available name type (e.g. species available name to Genus available name)
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
                    // Can't automatically move - check to see if a conversion is a) possible b) desired
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

        private void txtFind_TypingPaused(string text) {
            DoFind(text);
        }

        private void txtFind_KeyUp(object sender, KeyEventArgs e) {
            if (e.Key == Key.Down) {
                if (lstResults.IsVisible) {
                    lstResults.SelectedIndex = 0;
                    if (lstResults.SelectedItem != null) {
                        ListBoxItem item = lstResults.ItemContainerGenerator.ContainerFromItem(lstResults.SelectedItem) as ListBoxItem;
                        item.Focus();
                    }
                } else {
                    tvwAllTaxa.Focus();
                }
            }
        }

        private void tvwAllTaxa_MouseRightButtonUp(object sender, MouseButtonEventArgs e) {
            TaxonViewModel item = tvwAllTaxa.SelectedItem as TaxonViewModel;
            if (item != null) {
                ShowTaxonMenu(item, tvwAllTaxa);
            }

        }

        private void ShowTaxonMenu(TaxonViewModel taxon, FrameworkElement source) {

            TaxonMenuFactory factory = new TaxonMenuFactory(taxon, this, _R);

            ContextMenu menu = null;
            if (source is ListBox) {
                menu = factory.BuildFindResultsMenu();
            } else if (source is TreeView) {
                menu = factory.BuildExplorerMenu();
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
            return RegisterUniquePendingChange(new UpdateTaxonDatabaseAction(taxon.Taxon));
        }
       
        private void ShiftTaxon(TaxonViewModel taxon, System.Func<int, int> action) {
            if (IsManualSort && taxon.Parent != null) {
                ListCollectionView view = CollectionViewSource.GetDefaultView(taxon.Parent.Children) as ListCollectionView;                

                int oldIndex = view.IndexOf(taxon);
                int newIndex = action(oldIndex);
                
                // It's possible/probable that this set of taxa has never been ordered before, so we 
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

        internal void ShowTaxonDetails(int? taxonId) {
            Taxon fullDetails = Service.GetTaxon(taxonId.Value);
            TaxonViewModel model = new TaxonViewModel(null, fullDetails, null);
            TaxonDetails control = new TaxonDetails(model, User);
            TaxonRank taxonRank = Service.GetTaxonRank(model.ElemType);

            String title = String.Format("Taxon Detail: {0} ({1}) [{2}]", model.TaxaFullName, taxonRank.GetElementTypeLongName(model.Taxon), model.TaxaID);

            PluginManager.Instance.AddNonDockableContent(Owner, control, title, SizeToContent.Manual);
        }

        internal void Refresh() {
            if (AnyChanges()) {
                if (this.Question("You have unsaved changes. Refreshing will cause those changes to be discarded. Are you sure you want to discard unsaved changes?", "Discard unsaved changes?")) {
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
            // TODO: Permissions!
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
            viewModel.DisplayLabel = GetDefaultDisplayLabel(viewModel);
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

                RegisterPendingChange(new InsertTaxonDatabaseAction(viewModel));

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
                            t.DisplayLabel = GetDefaultDisplayLabel(t);
                        }, false, false);

                        foreach (HierarchicalViewModelBase vm in parent.Children) {
                            // Don't add the new child as a child of itself! Its already been added to children collection by the other AddNewTaxon method
                            if (vm != newUnplaced) {
                                TaxonViewModel child = vm as TaxonViewModel;
                                newUnplaced.Children.Add(child);
                                child.TaxaParentID = newUnplaced.TaxaID;
                                child.Parent = newUnplaced;
                                RegisterPendingChange(new MoveTaxonDatabaseAction(child, newUnplaced));
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

            // the alternate way, don't strikethrough - just remove it, and mark the current as changed
            // taxon.Parent.IsChanged = true;
            // taxon.Parent.Children.Remove(taxon);

            // Although we don't need to delete children explicitly from the database (the stored proc will take care of that for us),
            // we still need to mark each child as deleted in the UI
            foreach (HierarchicalViewModelBase child in taxon.Children) {
                MarkItemAsDeleted(child);
            }
        }

        internal void DeleteTaxon(TaxonViewModel taxon) {
            if (this.Question(_R("TaxonExplorer.prompt.DeleteTaxon", taxon.DisplayLabel), _R("TaxonExplorer.prompt.DeleteTaxon.Caption"))) {
                // First the UI bit...                
                MarkItemAsDeleted(taxon);
                // And schedule the database bit...                                
                RegisterPendingChange(new DeleteTaxonDatabaseAction(taxon));
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
                // Collapse the currently expanded nodes...
                // CollapseChildren(_explorerModel[0]);

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
                BringModelToView(tvwAllTaxa, child);
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

        private void lstResults_MouseRightButtonUp(object sender, MouseButtonEventArgs e) {
            TaxonViewModel item = lstResults.SelectedItem as TaxonViewModel;
            if (item != null) {
                ShowTaxonMenu(item, lstResults);
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
        }

        private string _R(string key, params object[] args) {
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

            return changed;
        }

        private void btnApplyChanges_Click(object sender, RoutedEventArgs e) {

            if (AnyChanges()) {
                CommitPendingChanges(() => {
                    ReloadModel();
                });
            }

        }

        private void btnCancelEditing_Click(object sender, RoutedEventArgs e) {
            btnLock.IsChecked = false;
        }

        private void EditableTextBlock_EditingComplete(object sender, string text) {
            TaxonViewModel tvm = (sender as EditableTextBlock).ViewModel as TaxonViewModel;
            ProcessRename(tvm, text);
        }

        private void EditableTextBlock_EditingCancelled(object sender, string oldtext) {
            TaxonViewModel tvm = (sender as EditableTextBlock).ViewModel as TaxonViewModel;
            tvm.DisplayLabel = null;
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
                taxon.DisplayLabel = null;
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
            ReportResults results = new ReportResults(report);
            PluginManager.Instance.AddDockableContent(this.Owner, results, report.Name);            
        }

        internal void EditTaxonName(int? taxonId) {
            TaxonNameDetails details = new TaxonNameDetails(taxonId, User);
            PluginManager.Instance.AddNonDockableContent(this.Owner, details, "Taxon name details",SizeToContent.WidthAndHeight);
        }

        public void BringModelToView(TreeView tvw, HierarchicalViewModelBase item) {
            ItemsControl itemsControl = tvw;

            // Get the stack of parentages...
            var stack = item.GetParentStack();

            // Descend through the levels until the desired TreeViewItem is found.
            while (stack.Count > 0) {
                HierarchicalViewModelBase model = stack.Pop();

                if (!model.IsExpanded) {
                    model.IsExpanded = true;
                }

                bool foundContainer = false;
                int index = (model.Parent == null ? 0 : model.Parent.Children.IndexOf(model));
                
                // Access the custom VSP that exposes BringIntoView
                MyVirtualizingStackPanel itemsHost = FindVisualChild<MyVirtualizingStackPanel>(itemsControl);
                if (itemsHost != null) {
                    // Due to virtualization, BringIntoView may not predict the offset correctly the first time.
                    ItemsControl nextItemsControl = null;
                    while (nextItemsControl == null) {
                        foundContainer = true;
                        itemsHost.BringIntoView(index);
                        Dispatcher.Invoke(DispatcherPriority.Background, (DispatcherOperationCallback)delegate(object unused) {
                            nextItemsControl = (ItemsControl)itemsControl.ItemContainerGenerator.ContainerFromIndex(index);
                            return null;
                        }, null);
                    }

                    itemsControl = nextItemsControl;
                }

                if (!foundContainer || (itemsControl == null)) {
                    // Abort the operation
                    return;
                }
            }
        }

        public TaxaService Service {
            get { return new TaxaService(User); }
        }

        private T FindVisualChild<T>(Visual visual) where T : Visual {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(visual); i++) {
                Visual child = (Visual)VisualTreeHelper.GetChild(visual, i);
                if (child != null) {
                    T correctlyTyped = child as T;
                    if (correctlyTyped != null) {
                        return correctlyTyped;
                    }

                    T descendent = FindVisualChild<T>(child);
                    if (descendent != null) {
                        return descendent;
                    }
                }
            }

            return null;
        }

        public TaxaPlugin Owner { get; private set; }

    }

    [StyleTypedProperty(Property = "ItemContainerStyle", StyleTargetType = typeof(MyTreeViewItem))]
    public class MyTreeView : TreeView {
        protected override DependencyObject GetContainerForItemOverride() {
            return new MyTreeViewItem();
        }

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item) {
            base.PrepareContainerForItemOverride(element, item);
            ((TreeViewItem)element).IsExpanded = true;
        }
    }

    [StyleTypedProperty(Property = "ItemContainerStyle", StyleTargetType = typeof(MyTreeViewItem))]
    public class MyTreeViewItem : TreeViewItem {
        protected override DependencyObject GetContainerForItemOverride() {
            return new MyTreeViewItem();
        }

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item) {
            base.PrepareContainerForItemOverride(element, item);
            // ((TreeViewItem)element).IsExpanded = true;
        }
    }

    public class MyVirtualizingStackPanel : VirtualizingStackPanel {
        public void BringIntoView(int index) {
            if (index < this.VisualChildrenCount) {
                this.BringIndexIntoView(index);
            }
        }
    }

}
