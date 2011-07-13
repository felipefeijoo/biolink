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
        }

        void Children_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            this.IsChanged = true;
        }

        public bool IsAncestorOf(HierarchicalViewModelBase item) {
            if (item == null) {
                return false;
            }
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
                        IsChildrenLoaded = true;
                        LazyLoadChildren(this);
                    }
                }
                _expanded = value;
                RaisePropertyChanged("IsExpanded");                
            }
        }

        public bool IsChildrenLoaded { get; set; }

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

        public T FindFirst<T>(Predicate<T> predicate) where T : HierarchicalViewModelBase {

            if (predicate((T)this)) {
                return (T) this;
            }

            foreach (HierarchicalViewModelBase child in Children) {
                if (predicate((T) child)) {
                    return (T) child;
                }
            }

            return default(T);
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

        public virtual int NumChildren { get; set; }

        public HierarchicalViewModelBase Parent { get; set; }

        public ObservableCollection<HierarchicalViewModelBase> Children { get; private set; }

        public event HierarchicalViewModelAction LazyLoadChildren;

    }

    public delegate void HierarchicalViewModelAction(HierarchicalViewModelBase item);

    public class ViewModelPlaceholder : HierarchicalViewModelBase {

        private string _label;
        private string _imagePath;

        public ViewModelPlaceholder(string label, string imagePath = null) {
            _label = label;
            _imagePath = imagePath;            
        }

        protected override string RelativeImagePath {
            get {
                if (!String.IsNullOrEmpty(_imagePath)) {
                    return _imagePath;
                }
                return base.RelativeImagePath;
            }
        }

        public override string DisplayLabel {
            get { return _label; }
        }

        public override int? ObjectID {
            get { return null; }
        }

    }
}
