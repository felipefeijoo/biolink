using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using System.Collections.Generic;
using System;
using BioLink.Client.Utilities;

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

        public List<string> GetExpandedParentages() {
            List<string> list = new List<string>();
            var model = ItemsSource as IEnumerable<HierarchicalViewModelBase>;            
            if (model != null) {
                CollectExpandedParentages(model, list);
            }
            return list;
        }

        private void CollectExpandedParentages(IEnumerable<HierarchicalViewModelBase> model, List<string> list) {
            foreach (HierarchicalViewModelBase tvm in model) {
                if (tvm.IsExpanded) {
                    list.Add(tvm.GetParentage());
                    if (tvm.Children != null && tvm.Children.Count > 0) {
                        CollectExpandedParentages(tvm.Children, list);
                    }
                }
            }
        }

        public void ExpandParentages(List<string> expanded) {
            var model = ItemsSource as IEnumerable<HierarchicalViewModelBase>;            
            if (model != null && expanded != null && expanded.Count > 0) {
                var todo = new Stack<HierarchicalViewModelBase>(model);
                while (todo.Count > 0) {
                    var vm = todo.Pop();
                    string parentage = vm.GetParentage();
                    if (expanded.Contains(parentage)) {
                        vm.IsExpanded = true;
                        expanded.Remove(parentage);
                        vm.Children.ForEach(child => todo.Push(child));
                    }
                }
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
            if (index < this.VisualChildrenCount && index >= 0) {
                this.BringIndexIntoView(index);
            }
        }
    }

    public static class TreeViewExtensions {

        public static void BringModelToView(this TreeView tvw, HierarchicalViewModelBase item) {
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

                int index = 0;
                if (model.Parent == null) {
                    var itemSource = tvw.ItemsSource as System.Collections.IList;
                    if (itemSource != null) {
                        index = itemSource.IndexOf(model);
                    }
                } else {
                    index = model.Parent.Children.IndexOf(model);
                }

                // Access the custom VSP that exposes BringIntoView
                if (index >= 0) {
                    BLVirtualizingStackPanel itemsHost = FindVisualChild<BLVirtualizingStackPanel>(itemsControl);
                    if (itemsHost != null) {
                        // Due to virtualization, BringIntoView may not predict the offset correctly the first time.
                        ItemsControl nextItemsControl = null;
                        while (nextItemsControl == null) {
                            foundContainer = true;
                            itemsHost.BringIntoView(index);
                            tvw.Dispatcher.Invoke(DispatcherPriority.Background, (DispatcherOperationCallback)delegate(object unused) {
                                nextItemsControl = (ItemsControl)itemsControl.ItemContainerGenerator.ContainerFromIndex(index);
                                return null;
                            }, null);
                        }

                        itemsControl = nextItemsControl;
                    }
                }

                if (!foundContainer || (itemsControl == null)) {
                    // Abort the operation
                    return;
                }
            }
        }

        private static T FindVisualChild<T>(Visual visual) where T : Visual {
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

    }


}
