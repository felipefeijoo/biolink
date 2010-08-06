using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using BioLink.Client.Extensibility;
using System.ComponentModel;

namespace BioLink.Client.Taxa {

    public class SortedTaxonConverter : IValueConverter {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            IEnumerable<HierarchicalViewModelBase> taxa = value as IEnumerable<HierarchicalViewModelBase>;
            ListCollectionView lcv = (ListCollectionView) CollectionViewSource.GetDefaultView(taxa);            
            lcv.SortDescriptions.Add(new SortDescription("AvailableName", ListSortDirection.Descending)); // bool
            lcv.SortDescriptions.Add(new SortDescription("Epithet", ListSortDirection.Ascending));
            return lcv;
        }
    
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
