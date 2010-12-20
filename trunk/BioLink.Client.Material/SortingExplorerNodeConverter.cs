using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using BioLink.Client.Extensibility;
using BioLink.Client.Utilities;
using System.ComponentModel;


namespace BioLink.Client.Material {

    public class SortingExplorerNodeConverter : IValueConverter {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {

            IEnumerable<HierarchicalViewModelBase> element = value as IEnumerable<HierarchicalViewModelBase>;
            ListCollectionView lcv = (ListCollectionView)CollectionViewSource.GetDefaultView(element);
            
            lcv.SortDescriptions.Add(new SortDescription("NodeType", ListSortDirection.Ascending));
            lcv.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));

            lcv.Refresh();

            return lcv;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
