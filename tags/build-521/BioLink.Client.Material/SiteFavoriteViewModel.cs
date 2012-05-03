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
using BioLink.Data;
using BioLink.Data.Model;
using BioLink.Client.Extensibility;
using BioLink.Client.Utilities;

namespace BioLink.Client.Material {

    public class SiteFavoriteViewModel : FavoriteViewModel<SiteFavorite> {

        public SiteFavoriteViewModel(SiteFavorite model)
            : base(model) {
        }

        public override string DisplayLabel {
            get {
                if (IsGroup) {
                    return GroupName;
                } else {
                    return Name;
                }
            }
        }

        public override System.Windows.FrameworkElement TooltipContent {
            get {
                return new SiteNodeTooltipContent(ElemID, this, ElemType, Name);
            }
        }

        protected override string RelativeImagePath {
            get {
                var image = "Region";
                SiteExplorerNodeType NodeType = (SiteExplorerNodeType)Enum.Parse(typeof(SiteExplorerNodeType), ElemType);
                switch (NodeType) {
                    case SiteExplorerNodeType.Region:
                        image = "Region";
                        break;
                    case SiteExplorerNodeType.Site:
                        image = "Site";
                        break;
                    case SiteExplorerNodeType.SiteVisit:
                        image = "SiteVisit";
                        break;
                    case SiteExplorerNodeType.Material:
                        image = "Material";
                        break;
                    case SiteExplorerNodeType.SiteGroup:
                        image = "SiteGroup";
                        break;
                    case SiteExplorerNodeType.Trap:
                        image = "Trap";
                        break;
                }
                return String.Format(@"images\{0}.png", image);
            }
        }

        public int ElemID {
            get { return Model.ElemID; }
            set { SetProperty(()=>Model.ElemID, value); }
        }

        public string Name {
            get { return Model.Name; }
            set { SetProperty(() => Model.Name, value); }
        }

        public string ElemType {
            get { return Model.ElemType; }
            set { SetProperty(() => Model.ElemType, value); }
        }

    }
}
