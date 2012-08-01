using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using BioLink.Client.Extensibility;

namespace BioLink.Client.Taxa {

    public class TaxonComparer : IComparer<TaxonViewModel>, IComparer, IComparer<HierarchicalViewModelBase> {

        public int Compare(TaxonViewModel x, TaxonViewModel y) {
            if (x == null || y == null) {
                return 0;
            }
            return StringComparer.Ordinal.Compare(y.DefaultSortOrder, x.DefaultSortOrder);
        }

        public int Compare(object x, object y) {
            return Compare(x as TaxonViewModel, y as TaxonViewModel);
        }

        public int Compare(HierarchicalViewModelBase x, HierarchicalViewModelBase y) {
            return Compare(x as TaxonViewModel, y as TaxonViewModel);
        }
    }
}
