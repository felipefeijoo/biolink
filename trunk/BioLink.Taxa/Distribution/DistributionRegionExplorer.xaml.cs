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
using BioLink.Client.Utilities;
using BioLink.Client.Extensibility;
using BioLink.Data;
using BioLink.Data.Model;
using System.Collections.ObjectModel;

namespace BioLink.Client.Taxa {
    /// <summary>
    /// Interaction logic for DistributionRegionExplorer.xaml
    /// </summary>
    public partial class DistributionRegionExplorer : DatabaseCommandControl, ISelectionHostControl {

        public DistributionRegionExplorer(TaxaPlugin plugin, User user) : base(user, "Distribution Region Explorer") {

            InitializeComponent();

            this.Plugin = plugin;

            Loaded += new RoutedEventHandler(DistributionRegionExplorer_Loaded);
            this.Unloaded += new RoutedEventHandler(DistributionRegionExplorer_Unloaded);

            ChangesCommitted += new PendingChangesCommittedHandler(DistributionRegionExplorer_ChangesCommitted);

            TreeViewDragHelper.Bind(tvwRegions, TreeViewDragHelper.CreatePinnableGenerator(plugin.Name, LookupType.DistributionRegion));
            TreeViewDragHelper.Bind(tvwFind, TreeViewDragHelper.CreatePinnableGenerator(plugin.Name, LookupType.DistributionRegion));

            txtFind.PreviewKeyDown += new KeyEventHandler(txtFind_PreviewKeyDown);

        }

        void txtFind_PreviewKeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter) {
                DoFind();
                e.Handled = true;
            }

            if (e.Key == Key.Down) {
                tvwFind.Focus();
                var model = tvwFind.ItemsSource as ObservableCollection<DistributionRegionViewModel>;
                if (model != null && model.Count > 0) {
                    model[0].IsSelected = true;
                }
                e.Handled = true;
            }
        }

        void DistributionRegionExplorer_Unloaded(object sender, RoutedEventArgs e) {
            var expanded = tvwRegions.GetExpandedParentages();
            Config.SetUser(User, "DistributionRegion.LastExpanded", expanded);
        }

        void DistributionRegionExplorer_ChangesCommitted(object sender) {
            ReloadModel();            
        }

        private void ReloadModel() {
            var expanded = tvwRegions.GetExpandedParentages();
            LoadModel();
            tvwRegions.ExpandParentages(expanded);
        }

        private void LoadModel() {
            using (new OverrideCursor(Cursors.Wait)) {
                var service = new SupportService(User);
                var rootList = service.GetDistributionRegions(0);
                var model = new ObservableCollection<DistributionRegionViewModel>(rootList.Select((m) => {
                    var vm = new DistributionRegionViewModel(m);
                    vm.LazyLoadChildren += new HierarchicalViewModelAction(vm_LazyLoadChildren);
                    vm.Children.Add(new ViewModelPlaceholder("Loading..."));
                    return vm;
                }));
                
                tvwRegions.ItemsSource = model;

                var expanded = Config.GetUser<List<String>>(User, "DistributionRegion.LastExpanded", null);
                if (expanded != null) {
                    tvwRegions.ExpandParentages(expanded);
                }
            }

        }

        void DistributionRegionExplorer_Loaded(object sender, RoutedEventArgs e) {
            tvwRegions.Items.Clear();
            LoadModel();
        }

        void vm_LazyLoadChildren(HierarchicalViewModelBase item) {
            var parent = item as DistributionRegionViewModel;
            if (parent != null) {
                using (new OverrideCursor(Cursors.Wait)) {
                    var service = new SupportService(User);
                    var list = service.GetDistributionRegions(parent.DistRegionID);
                    parent.Children.Clear();
                    foreach (DistributionRegion region in list) {
                        var childVm = new DistributionRegionViewModel(region);
                        childVm.Parent = parent;
                        childVm.LazyLoadChildren +=new HierarchicalViewModelAction(vm_LazyLoadChildren);
                        childVm.Children.Add(new ViewModelPlaceholder("Loading..."));
                        parent.Children.Add(childVm);
                    }                    
                }
            }
        }

        private TreeView CurrentTreeView {
            get {
                if (tvwRegions.IsVisible) {
                    return tvwRegions;
                }

                return tvwFind;
            }
        }

        private void RegionName_EditingComplete(object sender, string text) {
            var selected = CurrentTreeView.SelectedItem as DistributionRegionViewModel;
            if (selected != null) {
                RegisterUniquePendingChange(new UpdateDistributionRegionCommand(selected.Model));
            }
        }

        private void TreeViewItem_MouseRightButtonDown(object sender, MouseEventArgs e) {
            TreeViewItem item = sender as TreeViewItem;
            if (item != null) {
                item.Focus();
                e.Handled = true;
            }
        }

        private void TreeView_MouseRightButtonUp(object sender, MouseButtonEventArgs e) {
            var tvw = sender as TreeView;
            var item = tvw.SelectedItem as DistributionRegionViewModel;
            if (item != null) {

                var builder = new ContextMenuBuilder(null);

                builder.New("Add region").Handler(() => { AddRegion(item);}).End();
                builder.Separator();
                builder.New("Delete region").Handler(() => { DeleteRegion(item); }).End();
                builder.New("Rename region").Handler(() => { RenameRegion(item); }).End();
                builder.Separator();
                builder.New("Expand all").Handler(() => { ExpandAll(item); }).End();

                tvw.ContextMenu = builder.ContextMenu;

            }
        }

        private void ExpandAll(DistributionRegionViewModel selected) {
            using (new OverrideCursor(Cursors.Wait)) {
                if (selected != null) {
                    selected.Traverse((child) => {
                        child.IsExpanded = true;
                    });
                }
            }
        }

        private void DeleteRegion(DistributionRegionViewModel selected) {
            if (selected != null) {
                selected.IsDeleted = true;

                selected.Traverse((child) => {
                    child.IsDeleted = true;
                });
                
                RegisterUniquePendingChange(new DeleteDistributionRegion(selected.Model));
            }
        }

        private void RenameRegion(DistributionRegionViewModel selected) {
            if (selected != null) {
                selected.IsRenaming = true;
            }
        }

        private void AddRegion(DistributionRegionViewModel parent) {
            if (parent != null) {

                if (!parent.IsExpanded) {
                    parent.IsExpanded = true;
                }

                var newRegion = new DistributionRegion { DistRegionName = "<New Region", DistRegionParentID = parent.DistRegionID, DistRegionID = -1 };
                var newVm = new DistributionRegionViewModel(newRegion);
                newVm.Parent = parent;
                parent.Children.Add(newVm);

                newVm.IsSelected = true;
                newVm.IsRenaming = true;

                RegisterPendingChange(new InsertDistributionRegionCommand(newRegion, parent.Model));
            }
        }

        private void btnFind_Click(object sender, RoutedEventArgs e) {
            DoFind();
        }

        private void DoFind() {
            var searchTerm = txtFind.Text;
            tvwFind.ItemsSource = null;
            if (string.IsNullOrWhiteSpace(searchTerm)) {
                ErrorMessage.Show("Please supply a search term");
                return;
            }

            var service= new SupportService(User);
            var results = service.FindDistributionRegions(searchTerm);
            if (results.Count == 0) {
                InfoBox.Show("No matching regions found.", "No results", this);
                return;
            }

            var model = new ObservableCollection<DistributionRegionViewModel>(results.Select((region) => {
                var vm = new DistributionRegionViewModel(region);
                vm.LazyLoadChildren += new HierarchicalViewModelAction(vm_LazyLoadChildren);
                vm.Children.Add(new ViewModelPlaceholder("Loading..."));
                return vm;
            }));

            tvwFind.ItemsSource = model;
        }


        public SelectionResult Select() {
            var selected = CurrentTreeView.SelectedItem as DistributionRegionViewModel;
            if (selected != null) {
                return new SelectionResult { DataObject = selected, Description = selected.DistRegionName, LookupType = LookupType.DistributionRegion, ObjectID = selected.DistRegionID };
            }
            return null;
        }

        protected TaxaPlugin Plugin { get; private set; }

    }

    public class DeleteDistributionRegion : GenericDatabaseCommand<DistributionRegion> {
        public DeleteDistributionRegion(DistributionRegion model) : base(model) { }

        protected override void BindPermissions(PermissionBuilder required) {
            required.None();
        }

        protected override void ProcessImpl(User user) {
            var service = new SupportService(user);
            service.DeleteDistributionRegion(Model.DistRegionID);
        }
    }

    public class InsertDistributionRegionCommand : GenericDatabaseCommand<DistributionRegion> {

        public InsertDistributionRegionCommand(DistributionRegion region, DistributionRegion parentRegion) : base(region) {
            ParentRegion = parentRegion;
        }

        protected override void BindPermissions(PermissionBuilder required) {
            required.None();
        }

        protected override void ProcessImpl(User user) {
            var service = new SupportService(user);
            if (ParentRegion != null) {
                Model.DistRegionParentID = ParentRegion.DistRegionID;
            }

            Model.DistRegionID = service.InsertDistributionRegion(Model);
        }

        protected DistributionRegion ParentRegion { get; private set; }
    }

    public class UpdateDistributionRegionCommand : GenericDatabaseCommand<DistributionRegion> {

        public UpdateDistributionRegionCommand(DistributionRegion model) : base(model) { }

        protected override void BindPermissions(PermissionBuilder required) {
            required.None();
        }

        protected override void ProcessImpl(User user) {
            var service = new SupportService(user);
            service.UpdateDistributionRegion(Model);
        }
    }

    public class DistributionRegionViewModel : GenericHierarchicalViewModelBase<DistributionRegion> {

        public DistributionRegionViewModel(DistributionRegion model) : base(model, () => model.DistRegionID) { }

        public override ImageSource Icon {
            get {
                return ImageCache.GetImage("pack://application:,,,/BioLink.Client.Extensibility;component/images/DistributionRegion.png");
            }
            set { base.Icon = value; }
        }

        public override FrameworkElement TooltipContent {
            get { return new DistributionRegionTooltipContent(PluginManager.Instance.User, Model.DistRegionID); }
        }

        public override string DisplayLabel {
            get { return DistRegionName; }
        }

        public int DistRegionID {
            get { return Model.DistRegionID; }
            set { SetProperty(() => Model.DistRegionID, value); }
        }

        public int DistRegionParentID {
            get { return Model.DistRegionParentID; }
            set { SetProperty(() => Model.DistRegionParentID, value); }
        }

        public string DistRegionName {
            get { return Model.DistRegionName; }
            set { SetProperty(() => Model.DistRegionName, value); }
        }

        public override int NumChildren {
            get { return Model.NumChildren; }
            set { SetProperty(() => Model.NumChildren, value); }
        }


        internal string GetFullPath() {
            var stack = GetParentStack();            
            var sb = new StringBuilder();
            while (stack.Count > 0) {
                var region = stack.Pop() as DistributionRegionViewModel;
                if (region != null) {
                    if (sb.Length > 0) {
                        sb.Append("\\");
                    }
                    sb.Append(region.DistRegionName);
                }
            }
            return sb.ToString();
        }
    }
}

