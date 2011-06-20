using System.Windows;
using System.Windows.Controls;

namespace BioLink.Client.Extensibility {

    [StyleTypedProperty(Property = "ItemContainerStyle", StyleTargetType = typeof(VTreeViewItem))]
    public class VTreeView : TreeView {

        protected override DependencyObject GetContainerForItemOverride() {            
            return new VTreeViewItem();
        }

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item) {
            base.PrepareContainerForItemOverride(element, item);
            if (AutoExpandTopLevel) {
                ((TreeViewItem)element).IsExpanded = true;
            }
        }

        public bool AutoExpandTopLevel { get; set; }
    }

    [StyleTypedProperty(Property = "ItemContainerStyle", StyleTargetType = typeof(VTreeViewItem))]
    public class VTreeViewItem : TreeViewItem {
        protected override DependencyObject GetContainerForItemOverride() {
            return new VTreeViewItem();
        }

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item) {
            base.PrepareContainerForItemOverride(element, item);
        }
    }

    public class BLVirtualizingStackPanel : VirtualizingStackPanel {
        public void BringIntoView(int index) {
            if (index < this.VisualChildrenCount) {
                this.BringIndexIntoView(index);
            }
        }
    }


}
