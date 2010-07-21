using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media.Imaging;
using BioLink.Client.Utilities;
using System.ComponentModel;

namespace BioLink.Client.Extensibility {

    public abstract class HierarchicalViewModelBase : ViewModelBase {

        private bool _expanded;

        public HierarchicalViewModelBase() {
            this.Children = new ObservableCollection<HierarchicalViewModelBase>();
            // this.Children.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(Children_CollectionChanged);
        }

        void Children_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            this.IsChanged = true;
        }

        public bool IsAncestorOf(HierarchicalViewModelBase item) {
            HierarchicalViewModelBase p = item.Parent;
            while (p != null) {
                if (p == this) {
                    return true;
                }
                p = p.Parent;
            }
            return false;
        }


        public bool IsExpanded {
            get { return _expanded; }
            set {
                if (value == true && !IsChildrenLoaded) {
                    if (LazyLoadChildren != null) {
                        LazyLoadChildren(this);
                    }
                }
                _expanded = value;
            }
        }

        public bool IsChildrenLoaded {
            get {
                if (Children == null) {
                    return false;
                }

                if (Children.Count == 1 && Children[0] is ViewModelPlaceholder) {
                    return false;
                }

                return true;
            }
        }

        public HierarchicalViewModelBase Parent { get; set; }

        public ObservableCollection<HierarchicalViewModelBase> Children { get; private set; }

        public event ViewModelExpandedDelegate LazyLoadChildren;

    }

    public delegate void ViewModelExpandedDelegate(HierarchicalViewModelBase item);


    public class ViewModelPlaceholder : HierarchicalViewModelBase {

        private string _label;
        private BitmapSource _icon;

        public ViewModelPlaceholder(string label) {
            _label = label;
        }

        public override string Label {
            get { return _label; }
        }


        public override BitmapSource Icon {
            get {
                return _icon;
            }
            set {
                _icon = value;
            }
        }
    }
}
