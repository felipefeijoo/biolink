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
using BioLink.Client.Utilities;

namespace BioLink.Client.Extensibility {

    public class EllipsoidViewModel : GenericViewModelBase<Ellipsoid> {

        public EllipsoidViewModel(Ellipsoid model) : base(model, () => model.ID) { }

        public override string DisplayLabel {
            get { return Model.Name; }
        }

        public override System.Windows.FrameworkElement TooltipContent {
            get { return new EllipsoidTooltipContent(this); }
        }

        public int ID {
            get { return Model.ID; }
        }

        public string Name {
            get { return Model.Name; }
        }

        public double EquatorialRadius {
            get { return Model.EquatorialRadius; }
        }

        public double EccentricitySquared {
            get { return Model.EccentricitySquared; }
        }

    }

    public class EllipsoidTooltipContent : TooltipContentBase {

        public EllipsoidTooltipContent(EllipsoidViewModel vm) : base(vm.ID, vm) { }

        protected override void GetDetailText(Data.Model.BioLinkDataObject model, TextTableBuilder builder) {
            var e = ViewModel as EllipsoidViewModel;
            if (e != null) {
                builder.Add("Equatorial Radius", e.EquatorialRadius + "");
                builder.Add("Eccentricity (squared)", e.EccentricitySquared + "");
            }
        }

        protected override Data.Model.BioLinkDataObject GetModel() {
            return null;
        }
    }
}
