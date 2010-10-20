using BioLink.Client.Extensibility;
using SharpMap.Layers;

namespace BioLink.Client.Maps {

    public class LayerViewModel : GenericViewModelBase<ILayer> {

        private string _filename;

        public LayerViewModel(ILayer layer, string filename)
            : base(layer) {
            _filename = filename;
        }

        public string LayerName {
            get { return Model.LayerName; }
            set { SetProperty(() => Model.LayerName, value); }
        }

        public string Filename {
            get { return _filename; }
            set { SetProperty("Filename", ref _filename, value); }
        }

    }

}
