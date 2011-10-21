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
using BioLink.Client.Extensibility;
using BioLink.Client.Utilities;
using BioLink.Data.Model;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace BioLink.Client.Material {

    public class SiteExplorerNodeViewModel : GenericHierarchicalViewModelBase<SiteExplorerNode> {

        public SiteExplorerNodeViewModel(SiteExplorerNode model, bool isFindViewModel = false) : base(model, ()=>model.ElemID) {
            IsFindViewModel = isFindViewModel;
        }

        public bool IsFindViewModel { get; private set; }

        public override string DisplayLabel {
            get {
                    if (IsFindViewModel && ElemType == "Site") {
                        var label = Name.Trim();
                        if (!label.EndsWith(";")) {
                            label += "; ";
                        } 
                        return label + Model.RegionName;                        
                    } 
                    return Name;
                }
        }

        //public string GetParentage() {
        //    String parentage = "";
        //    TraverseToTop((node) => {
        //        if (node is SiteExplorerNodeViewModel) {
        //            parentage = "/" + (node as SiteExplorerNodeViewModel).ElemID + parentage;
        //        } 
        //    });
        //    return parentage;
        //}

        public override System.Windows.FrameworkElement TooltipContent {
            get {
                return new SiteNodeTooltipContent(ObjectID.Value, this, ElemType, Name);
            }
        }

        protected override string RelativeImagePath {
            get {
                return String.Format(@"images\{0}.png", NodeType.ToString());
            }
        }

        private ImageSource _icon;

        public override System.Windows.Media.ImageSource Icon {
            get {
                if (_icon == null) {
                    _icon = base.Icon;
                }

                if (IsTemplate) {
                    _icon = ImageCache.ApplyOverlay(_icon, "pack://application:,,,/BioLink.Client.Extensibility;component/images/TemplateOverlay.png");
                }

                if (IsDeleted) {
                    _icon = ImageCache.ApplyOverlay(_icon, "pack://application:,,,/BioLink.Client.Extensibility;component/images/DeletedOverlay.png");
                }

                return _icon;
            }
            set { _icon = value; }
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

        public override int NumChildren {
            get { return Model.NumChildren; }
            set { SetProperty(() => Model.NumChildren, value); }
        }

        public override string ToString() {
            return String.Format("{0}: {1} [{2}]", NodeType, Name, ElemID);
        }

        public bool IsTemplate {
            get { return Model.IsTemplate; }
        }

    }
}
