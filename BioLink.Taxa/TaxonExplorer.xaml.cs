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


        private Timer _timer;
        private IBioLinkPlugin _owner;

        private ObservableCollection<TaxonViewModel> _searchModel;

        public TaxonExplorer() {
            InitializeComponent();
        }

        public TaxonExplorer(IBioLinkPlugin owner) {
            InitializeComponent();
            _owner = owner;
            
            lstResults.Margin = tvwAllTaxa.Margin;
            lstResults.Visibility = Visibility.Hidden;
            _searchModel = new ObservableCollection<TaxonViewModel>();
            lstResults.ItemsSource = _searchModel;
            _timer = new Timer(new TimerCallback((obj) => {
                DoFind();
            }),null, Timeout.Infinite, Timeout.Infinite);

        }

        internal void SetModel(ObservableCollection<TaxonViewModel> model) {
            tvwAllTaxa.Items.Clear();
            this.tvwAllTaxa.ItemsSource = model;
        }

        private void txtFind_TextChanged(object sender, TextChangedEventArgs e) {

            if (String.IsNullOrEmpty(txtFind.Text)) {
                tvwAllTaxa.Visibility = System.Windows.Visibility.Visible;
                lstResults.Visibility = Visibility.Hidden;
                _searchModel.Clear();
            } else {
                _timer.Change(Timeout.Infinite, Timeout.Infinite);
                _timer.Change(300, 300);
                _searchModel.Clear();
                tvwAllTaxa.Visibility = Visibility.Hidden;
                lstResults.Visibility = Visibility.Visible;                
            }
        }

        private void DoFind() {
            if (_owner == null) {
                return;
            }
            _timer.Change(Timeout.Infinite, Timeout.Infinite);
            string searchTerm = null;
            txtFind.InvokeIfRequired(() => { searchTerm = txtFind.Text; });            
            List<Taxon> results = new TaxaService(_owner.User).FindTaxa(searchTerm);
            lstResults.InvokeIfRequired(() => {
                _searchModel.Clear();
                foreach (Taxon t in results) {
                    _searchModel.Add(new TaxonViewModel(null, t));
                }
            });
        }

        private void tabControl1_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (tabControl1.SelectedItem == tabFavorites) {
                LoadFavorites();
            }
        }

        private void LoadFavorites() {
        }

        private Point _startPoint;
        private bool _IsDragging = false;
        private TreeView _dragScope;
        private DragAdorner _adorner;
        private AdornerLayer _layer;
        private bool _dragHasLeftScope = false;

        void tvwAllTaxa_PreviewMouseMove(object sender, MouseEventArgs e) {
            CommonPreviewMouseView(e, tvwAllTaxa);
        }

        private void CommonPreviewMouseView(MouseEventArgs e, TreeView treeView) {
            if (e.LeftButton == MouseButtonState.Pressed && !_IsDragging) {
                Point position = e.GetPosition(null);
                if (Math.Abs(position.X - _startPoint.X) > SystemParameters.MinimumHorizontalDragDistance || Math.Abs(position.Y - _startPoint.Y) > SystemParameters.MinimumVerticalDragDistance) {
                    if (treeView.SelectedItem != null) {
                        TreeViewItem item = GetTreeViewItemClicked((FrameworkElement) e.OriginalSource, treeView);
                        if (item != null) {
                            StartDrag(e, treeView, item);
                        }
                    }
                }
            }

        }

        private TreeViewItem GetTreeViewItemClicked(FrameworkElement sender, TreeView treeView) {
            Point p = sender.TranslatePoint(new Point(0, 0), treeView);
            DependencyObject obj = treeView.InputHitTest(p) as DependencyObject;
            while (obj != null && !(obj is TreeViewItem)) {
                obj = VisualTreeHelper.GetParent(obj);
            }
            return obj as TreeViewItem;
        }

        void tvwAllTaxa_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            _startPoint = e.GetPosition(null);
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
                    if (dest == src || dest == src.Parent) {
                        // Error ?                        
                        return;
                    }

                    if (src.IsAncestorOf(dest)) {
                        return;
                    }

                    if (!dest.IsExpanded) {
                        dest.IsExpanded = true;
                    }

                    src.Parent.Children.Remove(src);
                    dest.Children.Add(src);
                    src.Parent = dest;
                    src.TaxaParentID = dest.TaxaID;
                    src.IsSelected = true;           
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

    }

    class DragAdorner : Adorner {

        protected UIElement _child;        
        protected UIElement _owner;
        protected double XCenter;
        protected double YCenter;

        public DragAdorner(UIElement owner) : base(owner) { }

        public DragAdorner(UIElement owner, TreeViewItem adornElement, bool useVisualBrush, double opacity, Point offset) : base(owner) {
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
