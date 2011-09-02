using BioLink.Client.Extensibility;
using SharpMap.Layers;

namespace BioLink.Client.Maps {

    public class LayerViewModel : GenericViewModelBase<ILayer> {

        public LayerViewModel(ILayer layer) : base(layer, null) { }

        public string LayerName {
            get { return Model.LayerName; }
            set { SetProperty(() => Model.LayerName, value); }
        }

    }

}
