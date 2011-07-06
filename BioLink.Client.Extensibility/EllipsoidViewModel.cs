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

        protected override void GetDetailText(Data.Model.OwnedDataObject model, TextTableBuilder builder) {
            var e = ViewModel as EllipsoidViewModel;
            if (e != null) {
                builder.Add("Equatorial Radius", e.EquatorialRadius + "");
                builder.Add("Eccentricity (squared)", e.EccentricitySquared + "");
            }
        }

        protected override Data.Model.OwnedDataObject GetModel() {
            return null;
        }
    }
}
