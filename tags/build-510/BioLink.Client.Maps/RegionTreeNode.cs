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
using SharpMap.Data;

namespace BioLink.Client.Maps {

    /// <summary>
    /// This class represents a node in a hierarchy of regions. A node with a null parent means its a top level (or root) node.
    /// Each node can have 0 or more children, each being RegionTreeNodes
    /// Leaf Name nodes have attached to them a Shape File geometry, but intermediate 'branch' nodes do not.
    /// </summary>
    public class RegionTreeNode {

        private bool _selected;

        public RegionTreeNode(RegionTreeNode parent, string name) {
            this.Parent = parent;
            this.Name = name;
            this.Children = new List<RegionTreeNode>();
            Path = CalculatePath();
        }

        internal string CalculatePath() {
            if (Parent == null) {
                return Name;
            } else {
                var parentPath = Parent.CalculatePath();
                if (parentPath.Length > 0) {
                    return parentPath + "\\" + Name;
                } else {
                    return Name;
                }
            }
        }

        public RegionTreeNode FindChildByName(string regionName) {
            var match = Children.Find((n) => { return n.Name.Equals(regionName); });
            return match;
        }

        internal RegionTreeNode AddChild(string regionName) {
            var newNode = new RegionTreeNode(this, regionName);
            Children.Add(newNode);
            return newNode;
        }

        public override string ToString() {
            return Name;
        }

        public RegionTreeNode FindByPath(string path) {
            string[] bits = path.Split('\\');
            var parent = this;
            foreach (string bit in bits) {
                parent = parent.FindChildByName(bit);
                if (parent == null) {
                    break;
                }
            }
            return parent;
        }

        public void Traverse(Action<RegionTreeNode> action) {
            if (action != null) {
                action(this);
            }
            foreach (RegionTreeNode child in Children) {
                child.Traverse(action);
            }
        }

        public List<RegionTreeNode> FindSelectedRegions() {
            var result = new List<RegionTreeNode>();
            Traverse((node) => {
                if (node.IsSelected) {
                    result.Add(node);
                }
            });
            return result;
        }

        public void DeselectAll() {
            this.Traverse((node) => {
                node.IsSelected = false;
            });
        }

        public bool IsAncestorSelected() {
            var p = this;
            while (p != null) {
                if (p.IsSelected) {
                    return true;
                }
                p = p.Parent;
            }
            return false;
        }

        public bool IsSelected {
            get {
                return _selected;
            }
            set {
                _selected = value;
                if (_selected) {
                    foreach (RegionTreeNode child in Children) {
                        child.IsSelected = true;
                    }
                } else {
                    if (!value && Parent != null) {
                        Parent.IsSelected = false;
                    }
                }
            }
        }

        #region Properties

        public RegionTreeNode Parent { get; set; }
        public string Name { get; set; }
        public List<RegionTreeNode> Children { get; private set; }
        public string Path { get; private set; }
        internal FeatureDataRow FeatureRow { get; set; }
        public bool IsThroughoutRegion { get; set; }

        #endregion

    }
}
