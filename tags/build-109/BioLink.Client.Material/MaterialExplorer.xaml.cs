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
        }

        private void EditableTextBlock_EditingCancelled(object sender, string oldtext) {
        }
    }

    public class SiteExplorerNodeViewModel : GenericHierarchicalViewModelBase<SiteExplorerNode> {

        public SiteExplorerNodeViewModel(SiteExplorerNode model)
            : base(model) {
            this.DisplayLabel = model.Name;            
        }

        protected override string RelativeImagePath {
            get {
                var image = "Region";
                switch (Model.ElemType.ToLower()) {
                    case "region":
                        image = "Region";
                        break;
                    case "site":
                        image = "Site";
                        break;
                    case "sitevisit":
                        image = "SiteVisit";
                        break;
                    case "material":
                        image = "Material";
                        break;
                }
                return String.Format(@"images\{0}.png", image); 
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
