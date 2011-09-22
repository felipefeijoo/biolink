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
using System.Linq.Expressions;
using System.Text;
using BioLink.Data;
using BioLink.Data.Model;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using BioLink.Client.Utilities;
using System.Reflection;

namespace BioLink.Client.Extensibility {

    public class GenericHierarchicalViewModelBase<T> : HierarchicalViewModelBase {

        private Expression<Func<int>> _objectIDExpr = null;

        public GenericHierarchicalViewModelBase(T model, Expression<Func<int>> objectIDExpr) {
            this.Model = model;
            _objectIDExpr = objectIDExpr;
        }

        protected void SetProperty<K>(Expression<Func<K>> wrappedPropertyExpr, K value, Action doIfChanged = null, bool changeAgnostic = false) {
            SetProperty(wrappedPropertyExpr, Model, value, doIfChanged, changeAgnostic);
        }

        public override int NumChildren { get; set; }

        public T Model { get; private set; }

        public override int? ObjectID {
            get {
                if (_objectIDExpr == null) {
                    return null;
                } else {
                    var destProp = (PropertyInfo)((MemberExpression)_objectIDExpr.Body).Member;
                    return (int)destProp.GetValue(Model, null);
                }
            }
        }
    
    }

    public class FavoriteViewModel<T> : GenericHierarchicalViewModelBase<T> where T : Favorite {

        private ImageSource _image;

        public FavoriteViewModel(T model) : base(model, ()=>model.FavoriteID) { }

        public override ImageSource Icon {
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


        public string Username {
            get { return Model.Username; }
            set { SetProperty(() => Model.Username, value); }
        }

        public int FavoriteID {
            get { return Model.FavoriteID; }
            set { SetProperty(() => Model.FavoriteID, value); }
        }

        public int FavoriteParentID {
            get { return Model.FavoriteParentID; }
            set { SetProperty(() => Model.FavoriteParentID, value); }
        }

        public bool IsGroup {
            get { return Model.IsGroup; }
            set { SetProperty(() => Model.IsGroup, value); }
        }

        public string GroupName {
            get { return Model.GroupName; }
            set { SetProperty(() => Model.GroupName, value); }
        }

        public override int NumChildren {
            get { return Model.NumChildren; }
            set { SetProperty(() => Model.NumChildren, value); }
        }

        public bool IsGlobal {
            get { return Model.IsGlobal; }
            set { SetProperty(() => Model.IsGlobal, value); }
        }

    }

}
