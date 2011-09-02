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
using BioLink.Client.Utilities;
using BioLink.Client.Extensibility;
using System.Linq.Expressions;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows;

namespace BioLink.Client.Taxa {

    public class TaxonFavoriteViewModel : FavoriteViewModel<TaxonFavorite> {

        public TaxonFavoriteViewModel(TaxonFavorite model) : base(model) {
            this.TaxonLabel = DisplayLabel;
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

        public override FrameworkElement TooltipContent {
            get {
                return new TaxonTooltipContent(PluginManager.Instance.User, Model.TaxaID);
            }
        }


        public override string ToString() {
            return "TaxonFavorite: " + DisplayLabel;
        }

        private ImageSource _image;

        public override ImageSource Icon {
            get {
                if (_image == null) {
                    if (IsGroup) {                        
                        _image = ImageCache.GetImage("pack://application:,,,/BioLink.Client.Extensibility;component/images/FavFolder.png");
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

        public string TaxonLabel {
            get { return (string)GetValue(TaxonLabelProperty); }
            set { SetValue(TaxonLabelProperty, value); }
        }

        public static readonly DependencyProperty TaxonLabelProperty = DependencyProperty.Register("TaxonLabel", typeof(string), typeof(TaxonFavoriteViewModel), new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.Journal | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(OnTaxonLabelChanged)));

        private static void OnTaxonLabelChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args) {
        }


    }

}
