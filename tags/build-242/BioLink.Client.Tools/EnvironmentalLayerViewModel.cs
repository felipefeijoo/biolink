using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Client.Extensibility;
using BioLink.Client.Utilities;
using BioLink.Data;
using BioLink.Data.Model;

namespace BioLink.Client.Tools {

    public class EnvironmentalLayerViewModel : GenericViewModelBase<IEnvironmentalLayer> {

        public EnvironmentalLayerViewModel(IEnvironmentalLayer layer) : base(layer, ()=> 0) { }

        protected override string RelativeImagePath {
            get { return @"images\GridLayer.png"; }
        }

        public string Name {
            get { return Model.Name; }
        }

    }
}
