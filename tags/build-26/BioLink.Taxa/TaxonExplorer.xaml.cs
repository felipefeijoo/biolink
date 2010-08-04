using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using BioLink.Data;
using BioLink.Data.Model;
using BioLink.Client.Utilities;
using BioLink.Client.Extensibility;
using System.Threading;

namespace BioLink.Client.Taxa {


    /// <summary>
    /// Interaction logic for TaxonExplorer.xaml
    /// </summary>
    public partial class TaxonExplorer : UserControl {

        private TaxaPlugin _owner;
        private ObservableCollection<HierarchicalViewModelBase> _explorerModel;
        private ObservableCollection<TaxonViewModel> _searchModel;
        private Point _startPoint;
        private bool _IsDragging = false;
        private TreeView _dragScope;
        private DragAdorner _adorner;
        private AdornerLayer _layer;
        private bool _dragHasLeftScope = false;
        private List<TaxonDatabaseAction> _pendingChanges = new List<TaxonDatabaseAction>();

        private int _nextNewTaxonID = -100;

        public TaxonExplorer() {
            InitializeComponent();
        }

        public TaxonExplorer(TaxaPlugin owner) {
            InitializeComponent();
            _owner = owner;

            lstResults.Margin = taxaBorder.Margin;
            lstResults.Visibility = Visibility.Hidden;
            _searchModel = new ObservableCollection<TaxonViewModel>();
            lstResults.ItemsSource = _searchModel;

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

            buttonBar.Visibility = Unlocked ? Visibility.Visible : Visibility.Hidden;
        }

        void btnLock_Checked(object sender, RoutedEventArgs e) {
            lblHeader.Visibility = Visibility.Hidden;
            buttonBar.Visibility = Unlocked ? Visibility.Visible : Visibility.Hidden;            
        }

        internal TaxaService Service { 
            get { return _owner.Service; } 
        }

        internal ObservableCollection<HierarchicalViewModelBase> ExplorerModel {
            get { return _explorerModel; }
            set {
                _explorerModel = value;
                tvwAllTaxa.Items.Clear();
                this.tvwAllTaxa.ItemsSource = _explorerModel;
            }
        }

        private void txtFind_TextChanged(object sender, TextChangedEventArgs e) {

            _searchModel.Clear();

            if (String.IsNullOrEmpty(txtFind.Text)) {
                tvwAllTaxa.Visibility = System.Windows.Visibility.Visible;
                lstResults.Visibility = Visibility.Hidden;
            } else {
                _searchModel.Clear();
                tvwAllTaxa.Visibility = Visibility.Hidden;
                lstResults.Visibility = Visibility.Visible;
            }
        }

        private string GenerateTaxonDisplayLabel(TaxonViewModel taxon) {

            if (taxon.TaxaParentID == -1) {
                return _R("TaxonExplorer.explorer.root");
            }

            string strAuthorYear = "";
            if (taxon.Unplaced.ValueOrFalse()) {
                return "Unplaced";
            } else if (taxon.ElemType == TaxonViewModel.SPECIES_INQUIRENDA_ELEM_TYPE) {
                return "Species Inquirenda";
            } else if (taxon.ElemType == TaxonViewModel.INCERTAE_SEDIS_ELEM_TYPE) {
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
                    TaxonRank rank = Service.GetTaxonRank(taxon.ElemType);
                    if (rank != null) {
                        String format = rank.ChecklistDisplayAs ?? "#";
                        strDisplay = format.Replace("#", taxon.Epithet) + " " + strAuthorYear;
                    }                                            
                    // ....
                }
                return strDisplay;
            }            
        }

        private void DoFind(string searchTerm) {

            try {
                lstResults.InvokeIfRequired(() => {
                    lstResults.Cursor = Cursors.Wait;
                });
                if (_owner == null) {
                    return;
                }
                List<TaxonSearchResult> results = new TaxaService(_owner.User).FindTaxa(searchTerm);
                lstResults.InvokeIfRequired(() => {
                    _searchModel.Clear();
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
                LoadFavorites();
            }
        }

        private void LoadFavorites() {
        }

        public void InitialiseTaxonExplorer() {
            // Load the model on the background thread
            ObservableCollection<HierarchicalViewModelBase> model = LoadTaxonViewModel();

            // Now see if we can auto-expand from the last session...
            if (model.Count > 0 && Config.GetGlobal<bool>("Taxa.RememberExpandedTaxa", true)) {
                var expanded = Config.GetProfile<List<String>>(_owner.User, "Taxa.Explorer.ExpandedTaxa", null);
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

            TaxonViewModel rootNode = new TaxonViewModel(null, root, GenerateTaxonDisplayLabel);

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

        void tvwAllTaxa_PreviewMouseMove(object sender, MouseEventArgs e) {
            CommonPreviewMouseView(e, tvwAllTaxa);
        }

        private void CommonPreviewMouseView(MouseEventArgs e, TreeView treeView) {

            if (e.LeftButton == MouseButtonState.Pressed && !_IsDragging) {
                Point position = e.GetPosition(tvwAllTaxa);
                if (Math.Abs(position.X - _startPoint.X) > SystemParameters.MinimumHorizontalDragDistance || Math.Abs(position.Y - _startPoint.Y) > SystemParameters.MinimumVerticalDragDistance) {
                    if (treeView.SelectedItem != null) {
                        if (Unlocked) {
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

        private void DragSource_GiveFeedback(object source, GiveFeedbackEventArgs e) {
            e.UseDefaultCursors = true;
            e.Handled = true;
        }

        private TreeViewItem GetHoveredTreeViewItem(DragEventArgs e) {
            TreeView tvw = _dragScope as TreeView;
            DependencyObject elem = tvw.InputHitTest(e.GetPosition(tvw)) as DependencyObject;
            while (elem != null && !(elem is TreeViewItem)) {
                elem = VisualTreeHelper.GetParent(elem);
            }
            return elem as TreeViewItem;
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


        internal bool Unlocked {
            get { return btnLock.IsChecked.GetValueOrDefault(false); }
        }

        private void ShowTaxonMenu(TaxonViewModel taxon, FrameworkElement source) {

            ContextMenu menu = new ContextMenu();

            MenuItemBuilder builder = new MenuItemBuilder(_R);

            if (source is ListBox) {
                menu.Items.Add(builder.New("TaxonExplorer.menu.ShowInContents").Handler(() => { ShowInExplorer(taxon); }).MenuItem);
                menu.Items.Add(new Separator());
            } else if (source is TreeView) {
                menu.Items.Add(builder.New("TaxonExplorer.menu.ExpandAll").Handler(() => {
                    JobExecutor.QueueJob(() => {
                        tvwAllTaxa.InvokeIfRequired(() => {
                            tvwAllTaxa.Cursor = Cursors.Wait;
                            ExpandChildren(taxon);
                            tvwAllTaxa.Cursor = Cursors.Arrow;
                        });
                    });
                }).MenuItem);

                menu.Items.Add(new Separator());
                if (Unlocked) {
                    menu.Items.Add(builder.New("TaxonExplorer.menu.Delete", taxon.TaxaFullName).Handler(() => { DeleteTaxon(taxon); }).MenuItem);

                    MenuItem addMenu = BuildAddMenuItems(taxon);
                    if (addMenu != null && addMenu.Items.Count > 0) {
                        menu.Items.Add(new Separator());
                        menu.Items.Add(addMenu);
                    }

                } else {
                    menu.Items.Add(builder.New("TaxonExplorer.menu.Unlock").Handler(() => { btnLock.IsChecked = true; }).MenuItem);
                }

                if (!Unlocked) {
                    menu.Items.Add(new Separator());
                    menu.Items.Add(builder.New("TaxonExplorer.menu.Refresh").Handler(() => Refresh()).MenuItem);
                }

            }


            source.ContextMenu = menu;
        }

        private void Refresh() {
            if (AnyChanges()) {
                if (this.Question("You have unsaved changes. Refreshing will cause those changes to be discarded. Are you sure you want to discard unsaved changes?", "Discard unsaved changes?")) {
                    ReloadModel();
                }
            } else {
                ReloadModel();
            }
        }

        private MenuItem BuildAddMenuItems(TaxonViewModel taxon) {

            MenuItemBuilder builder = new MenuItemBuilder(_R);

            MenuItem addMenu = builder.New("TaxonExplorer.menu.Add").MenuItem;

            if (taxon.AvailableName.GetValueOrDefault(false) || taxon.LiteratureName.GetValueOrDefault(false)) {
                return null;
            }

            if (taxon.TaxaParentID == -1) {
                TaxonRank rank = Service.GetRankByOrder(1);
                if (rank != null) {
                    addMenu.Items.Add(builder.New(rank.LongName).Handler(() => { AddNewTaxon(taxon, rank); }).MenuItem);
                    addMenu.Items.Add(builder.New("TaxonExplorer.menu.Add.AllRanks").Handler(() => { AddNewTaxonAllRanks(taxon); }).MenuItem);
                }
            } else {
                switch (taxon.ElemType) {
                    case "" :
                        break;
                    case TaxonViewModel.INCERTAE_SEDIS_ELEM_TYPE:
                    case TaxonViewModel.SPECIES_INQUIRENDA_ELEM_TYPE:
                        AddSpecialNameMenuItems(taxon, addMenu, true, false, false, false);
                        break;
                    default:

                        TaxonRank rank = Service.GetTaxonRank(taxon.ElemType);
                        if (rank != null) {
                            List<TaxonRank> validChildRanks = Service.GetChildRanks(rank);
                            if (validChildRanks != null && validChildRanks.Count > 0) {
                                foreach (TaxonRank childRank in validChildRanks) {
                                    addMenu.Items.Add(builder.New(childRank.LongName).Handler(() => {
                                        AddNewTaxon(taxon, childRank);
                                    }).MenuItem);
                                }
                                addMenu.Items.Add(new Separator());
                                addMenu.Items.Add(builder.New("Unranked Valid").Handler(() => { AddUnrankedValid(taxon); }).MenuItem);
                                addMenu.Items.Add(new Separator());
                                AddSpecialNameMenuItems(taxon, addMenu, rank.AvailableNameAllowed, rank.LituratueNameAllowed, rank.AvailableNameAllowed, rank.AvailableNameAllowed);
                                addMenu.Items.Add(new Separator());
                                bool atLeastOneUnplaced = false;
                                foreach (TaxonRank childRank in validChildRanks) {
                                    if (childRank.UnplacedAllowed.ValueOrFalse()) {
                                        addMenu.Items.Add(builder.New("Unplaced " + childRank.LongName).Handler(() => { AddNewTaxon(taxon, childRank, true); }).MenuItem);
                                        atLeastOneUnplaced = true;
                                    }
                                }
                                if (atLeastOneUnplaced) {
                                    addMenu.Items.Add(new Separator());
                                }
                            }
                            
                        }
                        break;
                }
            }

            return addMenu;
        }

        private void AddUnrankedValid(TaxonViewModel taxon) {
            throw new NotImplementedException();
        }

        private void AddSpecialNameMenuItems(TaxonViewModel parent, MenuItem parentMenu, bool? availEnabled = true, bool? litEnabled = true, bool? ISEnabled = true, bool? SIEnabled = true) {
            MenuItemBuilder builder = new MenuItemBuilder(_R);
            parentMenu.Items.Add(builder.New("TaxonExplorer.menu.Add.AvailableName").Handler(() => { AddAvailableName(parent); }).Enabled(availEnabled.ValueOrFalse()).MenuItem);
            parentMenu.Items.Add(builder.New("TaxonExplorer.menu.Add.LiteratureName").Handler(() => { AddLiteratureName(parent); }).Enabled(litEnabled.ValueOrFalse()).MenuItem);
            parentMenu.Items.Add(builder.New("TaxonExplorer.menu.Add.IncertaeSedis").Handler(() => { AddIncertaeSedis(parent); }).Enabled(ISEnabled.ValueOrFalse()).MenuItem);
            parentMenu.Items.Add(builder.New("TaxonExplorer.menu.Add.SpeciesInquirenda").Handler(() => { AddSpeciesInquirenda(parent); }).Enabled(SIEnabled.ValueOrFalse()).MenuItem);
        }

        private int GetNewTaxonID() {
            lock (this) {
                return _nextNewTaxonID--;
            }
        }

        private void AddNewTaxon(TaxonViewModel parent, TaxonViewModelAction action) {
            Taxon t = new Taxon();
            t.TaxaParentID = parent.TaxaID.Value;
            t.TaxaID = GetNewTaxonID();
            TaxonViewModel viewModel = new TaxonViewModel(parent, t, GenerateTaxonDisplayLabel);
            viewModel.IsChanged = true;
            parent.Children.Add(viewModel);
            if (action != null) {
                action(viewModel);
            }
            parent.IsExpanded = true;
            viewModel.IsSelected = true;
        }

        private void AddSpeciesInquirenda(TaxonViewModel parent) {
            AddNewTaxon(parent, (newChild) => {
                newChild.ElemType = TaxonViewModel.SPECIES_INQUIRENDA_ELEM_TYPE;
            });
        }

        private void AddIncertaeSedis(TaxonViewModel parent) {
            AddNewTaxon(parent, (newChild) => {
                newChild.ElemType = TaxonViewModel.INCERTAE_SEDIS_ELEM_TYPE;
            });
        }

        private void AddLiteratureName(TaxonViewModel parent) {
            AddNewTaxon(parent, (newChild) => {
                newChild.ElemType = parent.ElemType;
                newChild.LiteratureName = true;
            });
        }

        private void AddNewTaxonAllRanks(TaxonViewModel parent) {
            throw new NotImplementedException();
        }

        private void AddAvailableName(TaxonViewModel parent) {
            AddNewTaxon(parent, (newChild) => {
                String newElemType = parent.ElemType;
                switch (parent.ElemType) {
                    case TaxonViewModel.INCERTAE_SEDIS_ELEM_TYPE:
                    case TaxonViewModel.SPECIES_INQUIRENDA_ELEM_TYPE:
                        // Incerate sedis and species inq only have available species names.
                        newElemType = "SP";
                        break;
                    default:
                        break;
                }
                newChild.ElemType = newElemType;
                newChild.AvailableName = true;
            });
        }

        private void AddNewTaxon(TaxonViewModel parent, TaxonRank rank, bool unranked = false) {
            MessageBox.Show("Here in add taxon: " + rank);
        }

        internal void DeleteTaxon(TaxonViewModel taxon) {
            if (this.Question(_R("TaxonExplorer.prompt.DeleteTaxon", taxon.TaxaFullName), _R("TaxonExplorer.prompt.DeleteTaxon.Caption"))) {
                // First the UI bit...
                taxon.IsDeleted = true;
                taxon.Parent.IsChanged = true;
                taxon.Parent.Children.Remove(taxon);
                // And schedule the database bit...                
                if (taxon.TaxaID.Value >= 0) {
                    _pendingChanges.Add(new DeleteTaxonDatabaseAction(taxon.TaxaID.Value));
                }
            }
        }

        private void ExpandChildren(TaxonViewModel taxon, List<Taxon> remaining = null) {
            if (remaining == null) {
                remaining = Service.GetExpandFullTree(taxon.TaxaID.Value);
            }
            if (!taxon.IsExpanded) {
                taxon.BulkAddChildren(remaining.FindAll((elem) => { return elem.TaxaParentID == taxon.TaxaID; }), GenerateTaxonDisplayLabel);
                remaining.RemoveAll((elem) => { return elem.TaxaParentID == taxon.TaxaID; });
                taxon.IsExpanded = true;
            }

            foreach (HierarchicalViewModelBase child in taxon.Children) {
                ExpandChildren(child as TaxonViewModel, remaining);
            }
        }

        private void ShowInExplorer(TaxonViewModel taxon) {

            tabAllTaxa.IsSelected = true;
            tvwAllTaxa.Visibility = Visibility.Visible;
            lstResults.Visibility = Visibility.Hidden;
            txtFind.Text = "";

            JobExecutor.QueueJob(() => {
                // First make sure the explorer tree is visible...
                string parentage = Service.GetTaxonParentage(taxon.TaxaID.Value);
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
                expanded = _owner.GetExpandedParentages(_explorerModel);
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

        public void ProcessTaxonDragDrop(TaxonViewModel source, TaxonViewModel target) {

            try {
                // There are 4 possible outcomes from a taxon drop
                // 1) The drop is invalid, so do nothing (display an error message)
                // 2) The drop is a valid move, no conversion or merging required (simplest case)
                // 3) The drop is to that of a sibling rank, and so a decision is made to either merge or move as child (if possible)
                // 4) The drop is valid, but requires the source to be converted into a valid child of the target
                TaxonDropContext context = new TaxonDropContext(source, target, _owner);

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
                    List<TaxonDatabaseAction> dbActions = action.ProcessUI();
                    if (dbActions != null && dbActions.Count > 0) {
                        _pendingChanges.AddRange(dbActions);
                    }
                }
            } catch (IllegalTaxonMoveException ex) {
                MessageBox.Show(ex.Message, String.Format("Cannot move '{0}'", source.Epithet), MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }


            source.IsSelected = true;

        }

        internal void ClearPendingChanges() {
            _pendingChanges.Clear();
        }

        internal void CommitPendingChanges() {
            Service.BeginTransaction();
            try {
                foreach (TaxonDatabaseAction action in _pendingChanges) {
                    action.Process(Service);
                }
                Service.CommitTransaction();
                // Now reload the model
                ReloadModel();
                //
            } catch (Exception ex) {
                Service.RollbackTransaction();
                GlobalExceptionHandler.Handle(ex);
            }
        }

        private string _R(string key, params object[] args) {
            return _owner.GetCaption(key, args);
        }

        private DragDropAction PromptSourceTargetSame(TaxonDropContext context) {
            DragDropOptions form = new DragDropOptions(_owner);
            return form.ShowChooseMergeOrConvert(context);
        }

        private DragDropAction PromptConvert(TaxonDropContext context) {
            DragDropOptions form = new DragDropOptions(_owner);
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
                        if (this.Question(_R("TaxonExplorer.prompt.ConvertTaxon", context.Source.Epithet, context.TargetChildRank.LongName, context.Target.Epithet), _R("TaxonExplorer.prompt.ConfirmAction.Caption"))) {
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

        public bool AnyChanges() {

            bool changed = false;
            foreach (HierarchicalViewModelBase item in _explorerModel) {
                item.Traverse((node) => {
                    if (node.IsChanged) {
                        changed = true;
                    }
                });
            }

            return changed;
        }

        private void btnApplyChanges_Click(object sender, RoutedEventArgs e) {

            if (AnyChanges()) {
                CommitPendingChanges();
            }

        }

        private void btnCancelEditing_Click(object sender, RoutedEventArgs e) {
            btnLock.IsChecked = false;
        }

    }

    class DragAdorner : Adorner {

        protected UIElement _child;
        protected UIElement _owner;
        protected double XCenter;
        protected double YCenter;

        public DragAdorner(UIElement owner) : base(owner) { }

        public DragAdorner(UIElement owner, TreeViewItem adornElement, bool useVisualBrush, double opacity, Point offset)
            : base(owner) {
            System.Diagnostics.Debug.Assert(owner != null);
            System.Diagnostics.Debug.Assert(adornElement != null);
            _owner = owner;
            if (useVisualBrush) {

                VisualBrush _brush = new VisualBrush(adornElement);
                _brush.Opacity = opacity;
                _brush.Stretch = Stretch.None;
                _brush.AlignmentY = AlignmentY.Top;
                _brush.AlignmentX = AlignmentX.Left;
                Rectangle r = new Rectangle();
                r.Width = adornElement.ActualWidth;
                r.Height = adornElement.ActualHeight;
                XCenter = offset.X;
                YCenter = offset.Y;
                r.Fill = _brush;
                _child = r;
            } else {
                _child = adornElement;
            }
        }


        private double _leftOffset;
        public double LeftOffset {
            get { return _leftOffset; }
            set {
                _leftOffset = value - XCenter;
                UpdatePosition();
            }
        }

        private double _topOffset;
        public double TopOffset {
            get { return _topOffset; }
            set {
                _topOffset = value - YCenter;

                UpdatePosition();
            }
        }

        private void UpdatePosition() {
            AdornerLayer adorner = (AdornerLayer)this.Parent;
            if (adorner != null) {
                adorner.Update(this.AdornedElement);
            }
        }

        protected override Visual GetVisualChild(int index) {
            return _child;
        }

        protected override int VisualChildrenCount {
            get {
                return 1;
            }
        }


        protected override Size MeasureOverride(Size finalSize) {
            _child.Measure(finalSize);
            return _child.DesiredSize;
        }
        protected override Size ArrangeOverride(Size finalSize) {

            _child.Arrange(new Rect(_child.DesiredSize));
            return finalSize;
        }

        public override GeneralTransform GetDesiredTransform(GeneralTransform transform) {
            GeneralTransformGroup result = new GeneralTransformGroup();

            result.Children.Add(base.GetDesiredTransform(transform));
            result.Children.Add(new TranslateTransform(_leftOffset, _topOffset));
            return result;
        }
    }

}
