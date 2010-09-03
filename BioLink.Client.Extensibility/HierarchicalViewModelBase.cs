using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media.Imaging;
using BioLink.Client.Utilities;
using System.ComponentModel;
using BioLink.Data.Model;

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
                RaisePropertyChanged("IsExpanded");                
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

        public void TraverseToTop(HierarchicalViewModelAction func) {
            HierarchicalViewModelBase p = this;
            while (p != null) {
                func(p);
                p = p.Parent;
            }
        }

        public void Traverse(HierarchicalViewModelAction action) {
            if (action == null) { 
                return; 
            }

            // Firstly visit me...
            action(this);
            // then each of my children
            foreach (HierarchicalViewModelBase child in Children) {
                child.Traverse(action);
            }
            
        }

        public Stack<HierarchicalViewModelBase> GetParentStack() {
            var p = this;
            var stack = new Stack<HierarchicalViewModelBase>();
            while (p != null) {
                stack.Push(p);
                p = p.Parent;
            }
            return stack;
        }

        public HierarchicalViewModelBase Parent { get; set; }

        public ObservableCollection<HierarchicalViewModelBase> Children { get; private set; }

        public event HierarchicalViewModelAction LazyLoadChildren;

    }

    public delegate void HierarchicalViewModelAction(HierarchicalViewModelBase item);

    public class ViewModelPlaceholder : HierarchicalViewModelBase {

        public ViewModelPlaceholder(string label) {
            this.DisplayLabel = label;
        }

    }
}
