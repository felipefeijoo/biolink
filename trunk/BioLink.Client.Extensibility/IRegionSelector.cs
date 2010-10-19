using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BioLink.Client.Extensibility {

    public interface IRegionSelector {

        void SelectRegions(List<RegionDescriptor> preselectedRegions, Action<List<RegionDescriptor>> updatefunc);

    }

    public class RegionDescriptor {

        public RegionDescriptor() {}

        public RegionDescriptor(string path) {
            this.Path = path;
        }

        public RegionDescriptor(string path, bool throughoutRegion) {
            this.Path = path;
            this.IsThroughoutRegion = throughoutRegion;
        }

        public string Path { get; set; }

        public bool IsThroughoutRegion { get; set; }

    }
}
