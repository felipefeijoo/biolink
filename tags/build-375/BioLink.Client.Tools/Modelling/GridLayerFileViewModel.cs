using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Client.Extensibility;
using BioLink.Client.Utilities;
using BioLink.Data;
using BioLink.Data.Model;

namespace BioLink.Client.Tools {

    public class GridLayerFileViewModel : ViewModelBase {

        public GridLayerFileViewModel(string filename) {
            this.Name = filename;
        }

        protected override string RelativeImagePath {
            get { return @"images\GridLayer.png"; }
        }

        public string Name { get; private set; }


        public override int? ObjectID {
            get { return null; }
        }
    }
}
