/*******************************************************************************
 * Copyright (C) 2011 Atlas of Living Australia
 * All Rights Reserved.
 * 
 * The contents of this file are subject to the Mozilla Public
 * License Version 1.1 (the "License"); you may not use this file
 * except in compliance with the License. You may obtain a copy of
 * the License at http://www.mozilla.org/MPL/
 * 
 * Software distributed under the License is distributed on an "AS
 * IS" basis, WITHOUT WARRANTY OF ANY KIND, either express or
 * implied. See the License for the specific language governing
 * rights and limitations under the License.
 ******************************************************************************/
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

        /// <summary>
        /// Traverses from this node to each successive parent until a node with parent is reached.
        /// </summary>
        /// <param name="func"></param>
        public void TraverseToTop(HierarchicalViewModelAction func) {
            HierarchicalViewModelBase p = this;
            while (p != null) {
                func(p);
                p = p.Parent;
            }
        }

        /// <summary>
        /// Visits this node, and all children recursively (i.e. It traverses the tree 'downwards' only, from parent to child).
        /// </summary>
        /// <param name="action"></param>
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

        /// <summary>
        /// Find the first node that matches the specified predicate by recursing downwards through children.
        /// This node is tested initially.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public T FindFirst<T>(Predicate<T> predicate) where T : HierarchicalViewModelBase {

            if (predicate((T)this)) {
                return (T)this;
            }

            foreach (HierarchicalViewModelBase child in Children) {
                if (predicate((T)child)) {
                    return (T)child;
                }
            }

            return default(T);
        }

        /// <summary>
        /// Returns a stack of nodes, each being an ancestor of this one (or this one), with the topmost node being the topmost parent, and the bottom of the stack being this node.
        /// </summary>
        /// <returns></returns>
        public Stack<HierarchicalViewModelBase> GetParentStack() {
            var p = this;
            var stack = new Stack<HierarchicalViewModelBase>();
            while (p != null) {
                stack.Push(p);
                p = p.Parent;
            }
            return stack;
        }

        /// <summary>
        /// Returns a delimited string or ancestor object ids, descending (from left to right) from the topmost node to this node.
        /// <para>
        /// E.g. /1231/32/23
        /// </para>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual string GetParentage() {
            String parentage = "";
            TraverseToTop((child) => {
                parentage = "/" + child.ObjectID + parentage;
            });
            return parentage;
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

        public override string ToString() {
            return _label;
        }

    }
}
