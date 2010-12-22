using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Data;
using BioLink.Data.Model;
using BioLink.Client.Extensibility;
using BioLink.Client.Utilities;
using System.Windows.Media;
using System.Windows.Media.Imaging;

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

        private BitmapSource _image;

        public override BitmapSource Icon {
            get {
                if (_image == null) {
                    if (IsGroup) {                        
                        _image = ImageCache.GetImage("pack://application:,,,/BioLink.Client.Extensibility;component/images/FavFolder.png");
                    } else {
                        _image = base.Icon;
                    }
                }
                return _image;
            }

            set {
                _image = value;
                RaisePropertyChanged("Icon");
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
