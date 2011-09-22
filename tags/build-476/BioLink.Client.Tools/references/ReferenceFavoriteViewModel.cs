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

namespace BioLink.Client.Tools {

    public class ReferenceFavoriteViewModel : FavoriteViewModel<ReferenceFavorite> {

        public ReferenceFavoriteViewModel(ReferenceFavorite model)
            : base(model) {
        }

        protected override string RelativeImagePath {
            get { return @"images\Reference.png"; }
        }

        public override string DisplayLabel {
            get { return IsGroup ? GroupName : RefCode; }
        }

        public int RefID {
            get { return Model.RefID; }
            set { SetProperty(()=>Model.RefID, value); }
        }

        public string RefCode {
            get { return Model.RefCode; }
            set { SetProperty(() => Model.RefCode, value); }
        }

        public string FullRTF {
            get { return Model.FullRTF; }
            set { SetProperty(() => Model.FullRTF, value); }
        }        


    }
}
