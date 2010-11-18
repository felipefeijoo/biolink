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
        }

        public void InitializeMaterialExplorer() {
            this.InvokeIfRequired(() => {
                var service = new MaterialService(User);                
                var regionsModel = BuildRegionsModel(service.GetTopLevelRegions());
                regionsNode.ItemsSource = regionsModel;
                regionsNode.IsExpanded = true;
            });

        }

        private ObservableCollection<HierarchicalViewModelBase> BuildRegionsModel(List<RegionTreeNode> list) {
            var regionsModel = new ObservableCollection<HierarchicalViewModelBase>(list.ConvertAll((model) => {
                var viewModel = new RegionTreeNodeViewModel(model);

                if (model.NumChildren > 0) {
                    viewModel.Children.Add(new ViewModelPlaceholder("Loading..."));
                    viewModel.LazyLoadChildren += new HierarchicalViewModelAction(viewModel_LazyLoadChildren);
                }
                return viewModel;
            }));
            return regionsModel;
        }

        void viewModel_LazyLoadChildren(HierarchicalViewModelBase item) {

            var region = item as RegionTreeNodeViewModel;
            if (region != null) {
                region.Children.Clear();
                var service = new MaterialService(User);
                var list = service.GetRegionsForParent(region.RegionID);
                var viewModel = BuildRegionsModel(list);
                foreach (HierarchicalViewModelBase childViewModel in viewModel) {
                    region.Children.Add(childViewModel);
                }                
            }
        }

        private void TreeViewItem_MouseRightButtonDown(object sender, MouseEventArgs e) {
        }

        private void EditableTextBlock_EditingComplete(object sender, string text) {
        }

        private void EditableTextBlock_EditingCancelled(object sender, string oldtext) {
        }
    }

    public class RegionTreeNodeViewModel : GenericHierarchicalViewModelBase<RegionTreeNode> {

        public RegionTreeNodeViewModel(RegionTreeNode model)
            : base(model) {
            this.DisplayLabel = model.Region;
        }

        public int RegionID {
            get { return Model.RegionID; }
            set { SetProperty(() => Model.RegionID, value); }
        }

        public string Region {
            get { return Model.Region; }
            set { SetProperty(() => Model.Region, value); }
        }

        public int ParentID {
            get { return Model.ParentID; }
            set { SetProperty(() => Model.ParentID, value); }
        }

        public int NumChildren {
            get { return Model.NumChildren; }
            set { SetProperty(()=>Model.NumChildren, value); }
        }

    }
}
