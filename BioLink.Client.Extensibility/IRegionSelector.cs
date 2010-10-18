using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BioLink.Client.Extensibility {

    public interface IRegionSelector {

        void SelectRegions(List<string> preselectedRegions, Action<List<string>> updatefunc);

    }
}
