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
using BioLink.Data;
using BioLink.Data.Model;
using BioLink.Client.Extensibility;
using System.Collections.ObjectModel;
using BioLink.Client.Utilities;

namespace BioLink.Client.Material {
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class MaterialExplorer : DatabaseActionControl {

        #region Designer constructor
        public MaterialExplorer() {
            InitializeComponent();
        }
        #endregion

        public MaterialExplorer(MaterialPlugin owner)
            : base(owner.User, "MaterialExplorer") {

            InitializeComponent();

            this.ChangeRegistered += new Action<IList<DatabaseAction>>((list) => {
                btnApply.IsEnabled = true;
                btnCancel.IsEnabled = true;
            });

            this.ChangesCommitted += new PendingChangesCommittedHandler((list) => {
                InitializeMaterialExplorer();
                btnApply.IsEnabled = false;
                btnCancel.IsEnabled = false;
            });

        }

        public void InitializeMaterialExplorer() {
            this.InvokeIfRequired(() => {
                var service = new MaterialService(User);
                var list = service.GetTopLevelExplorerItems();
                list.Sort((item1, item2) => {
                    int compare = item1.ElemType.CompareTo(item2.ElemType);
                    if (compare == 0) {
                        return item1.Name.CompareTo(item2.Name);
                    } 
                    return compare;
                });
                var regionsModel = BuildRegionsModel(list);
                regionsNode.ItemsSource = regionsModel;
                regionsNode.IsExpanded = true;
            });
        }

        private ObservableCollection<HierarchicalViewModelBase> BuildRegionsModel(List<SiteExplorerNode> list) {
            var regionsModel = new ObservableCollection<HierarchicalViewModelBase>(list.ConvertAll((model) => {
                var viewModel = new SiteExplorerNodeViewModel(model);
                if (model.NumChildren > 0) {
                    viewModel.Children.Add(new ViewModelPlaceholder("Loading..."));
                    viewModel.LazyLoadChildren += new HierarchicalViewModelAction(viewModel_LazyLoadChildren);
                }
                return viewModel;
            }));
            return regionsModel;
        }

        void viewModel_LazyLoadChildren(HierarchicalViewModelBase item) {

            var elem = item as SiteExplorerNodeViewModel;
            if (elem != null) {
                elem.Children.Clear();
                var service = new MaterialService(User);
                var list = service.GetExplorerElementsForParent(elem.ElemID, elem.ElemType);
                var viewModel = BuildRegionsModel(list);
                foreach (HierarchicalViewModelBase childViewModel in viewModel) {
                    elem.Children.Add(childViewModel);
                }                
            }
        }

        private void TreeViewItem_MouseRightButtonDown(object sender, MouseEventArgs e) {
        }

        private void EditableTextBlock_EditingComplete(object sender, string text) {
            var selected = tvwMaterial.SelectedItem as SiteExplorerNodeViewModel;
            if (selected != null) {
                selected.Name = text;
                switch (selected.NodeType) {
                    case SiteExplorerNodeType.Region:
                        RegisterPendingChange(new RenameRegionAction(selected));
                        break;
                    case SiteExplorerNodeType.Site:
                        RegisterPendingChange(new RenameSiteAction(selected));
                        break;
                    default:
                        throw new NotImplementedException(selected.NodeType.ToString());
                }                
            }
        }

        private void EditableTextBlock_EditingCancelled(object sender, string oldtext) {
        }

        private void tvwMaterial_MouseRightButtonUp(object sender, MouseButtonEventArgs e) {
            ShowContextMenu();
        }

        private void ShowContextMenu() {
            var selected = tvwMaterial.SelectedItem as SiteExplorerNodeViewModel;
            if (selected != null) {
                var menu  = SiteExplorerMenuBuilder.Build(selected, this);
                if (menu != null) {
                    tvwMaterial.ContextMenu = menu;
                }
            }
        }

        internal void AddRegion(SiteExplorerNodeViewModel parent) {

            parent.IsExpanded = true;

            var model = new SiteExplorerNode();
            model.Name = "<New Region>";
            model.ParentID = parent.ElemID;
            model.ElemType = SiteExplorerNodeType.Region.ToString();
            model.ElemID = -1;

            var viewModel = new SiteExplorerNodeViewModel(model);
            parent.Children.Add(viewModel);
            viewModel.IsSelected = true;
            viewModel.IsRenaming = true;

            RegisterPendingChange(new InsertRegionAction(viewModel));
                 
        }

        internal void EditRegion(int regionID) {
            if (regionID < 0) {
                ErrorMessage.Show("You must first apply the changes before editing the details of this item!");
                return;
            } else {
                var regionDetails = new RegionDetails(User, regionID);
                var caption = string.Format("Region Detail {0} ({1}) [{2}]", regionDetails.ViewModel.Name, regionDetails.ViewModel.Rank, regionDetails.ViewModel.PoliticalRegionID);
                PluginManager.Instance.AddNonDockableContent(Owner, regionDetails, caption, SizeToContent.Manual);
            }
        }

        internal void DeleteRegion(SiteExplorerNodeViewModel region) {
            region.IsDeleted = true;            
            RegisterPendingChange(new DeleteRegionAction(region.ElemID));
        }

        private void tvwMaterial_MouseRightButtonDown(object sender, MouseButtonEventArgs e) {
            TreeViewItem item = sender as TreeViewItem;
            if (item != null) {
                item.Focus();
                e.Handled = true;
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e) {
            InitializeMaterialExplorer();
        }

        private void btnApply_Click(object sender, RoutedEventArgs e) {
            ApplyChanges();
        }

        private void ApplyChanges() {
            CommitPendingChanges();
        }

        public MaterialPlugin Owner { get; private set; }

    }

    public class SiteExplorerNodeViewModel : GenericHierarchicalViewModelBase<SiteExplorerNode> {

        public SiteExplorerNodeViewModel(SiteExplorerNode model)
            : base(model) {
            this.DisplayLabel = model.Name;            
        }

        protected override string RelativeImagePath {
            get {
                var image = "Region";               
                switch (NodeType) {
                    case SiteExplorerNodeType.Region:
                        image = "Region";
                        break;
                    case SiteExplorerNodeType.Site:
                        image = "Site";
                        break;
                    case SiteExplorerNodeType.SiteVisit:
                        image = "SiteVisit";
                        break;
                    case SiteExplorerNodeType.Material:
                        image = "Material";
                        break;
                    case SiteExplorerNodeType.SiteGroup:
                        image = "SiteGroup";
                        break;

                }
                return String.Format(@"images\{0}.png", image); 
            }
        }

        public SiteExplorerNodeType NodeType {
            get {
                return (SiteExplorerNodeType)Enum.Parse(typeof(SiteExplorerNodeType), Model.ElemType);
            }
        }

        public int ElemID {
            get { return Model.ElemID; }
            set { SetProperty(() => Model.ElemID, value); }
        }

        public string ElemType {
            get { return Model.ElemType; }
            set { SetProperty(() => Model.ElemType, value); }
        }

        public string Name {
            get { return Model.Name; }
            set { SetProperty(() => Model.Name, value); }
        }

        public int ParentID {
            get { return Model.ParentID; }
            set { SetProperty(() => Model.ParentID, value); }
        }

        public int RegionID {
            get { return Model.RegionID; }
            set { SetProperty(() => Model.RegionID, value); }
        }

        public int NumChildren {
            get { return Model.NumChildren; }
            set { SetProperty(()=>Model.NumChildren, value); }
        }

    }
}
