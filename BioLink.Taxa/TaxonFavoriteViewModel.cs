using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Data;
using BioLink.Data.Model;
using BioLink.Client.Utilities;
using BioLink.Client.Extensibility;
using System.Linq.Expressions;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace BioLink.Client.Taxa {

    public class TaxonFavoriteViewModel : FavoriteViewModel<TaxonFavorite> {

        public TaxonFavoriteViewModel(TaxonFavorite model)
            : base(model) {
        }

        public override string DisplayLabel {
            get {
                if (IsGroup) {
                    return GroupName;
                } else {
                    return TaxaFullName;
                }
            }
        }

        public override string ToString() {
            return "TaxonFavorite: " + DisplayLabel;
        }

        private BitmapSource _image;

        public override BitmapSource Icon {
            get {
                if (_image == null) {
                    if (IsGroup) {                        
                        string assemblyName = typeof(TaxonViewModel).Assembly.GetName().Name;
                        _image = ImageCache.GetImage(String.Format("pack://application:,,,/{0};component/images/FavFolder.png", assemblyName));
                    } else {
                        _image = TaxonViewModel.ConstructIcon(false, Model.ElemType, IsChanged);
                    }
                }
                return _image;
            }

            set {
                _image = value;
                RaisePropertyChanged("Icon");
            }
        }


        public int TaxaID {
            get { return Model.TaxaID; }
            set { SetProperty(() => Model.TaxaID, value); }
        }

        public int TaxaParentID {
            get { return Model.TaxaParentID; }
            set { SetProperty(() => Model.TaxaParentID, value); }
        }

        public string Epithet {
            get { return Model.Epithet; }
            set { SetProperty(() => Model.Epithet, value); }
        }

        public string TaxaFullName {
            get { return Model.TaxaFullName; }
            set { SetProperty(() => Model.TaxaFullName, value); }
        }

        public string YearOfPub {
            get { return Model.YearOfPub; }
            set { SetProperty(() => Model.YearOfPub, value); }
        }

        public string KingdomCode {
            get { return Model.KingdomCode; }
            set { SetProperty(() => Model.KingdomCode, value); }
        }

        public string ElemType {
            get { return Model.ElemType; }
            set { SetProperty(() => Model.ElemType, value); }
        }

        public bool Unverified {
            get { return Model.Unverified; }
            set { SetProperty(() => Model.Unverified, value); }
        }

        public bool Unplaced {
            get { return Model.Unplaced; }
            set { SetProperty(()=> Model.Unplaced, value); }
        }

        public int Order {
            get { return Model.Order; }
            set { SetProperty(() => Model.Order, value); }
        }

        public string Rank {
            get { return Model.Rank; }
            set { SetProperty(() => Model.Rank, value); }
        }

        public bool ChgComb {
            get { return Model.ChgComb; }
            set { SetProperty(() => Model.ChgComb, value); }
        }

        public string NameStatus { 
            get { return Model.NameStatus; }
            set { SetProperty(() => Model.NameStatus, value); }
        }

    }

}
