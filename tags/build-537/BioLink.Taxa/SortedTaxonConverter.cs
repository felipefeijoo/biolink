/*******************************************************************************
 * Copyright (C) 2011 Atlas of Living Australia
 * All Rights Reserved.
 * 
 * The contents of this file are subject to the Mozilla Public
 * License Version 1.1 (the "License"); you may not use this file
 * except in compliance with the License. You may obtain a copy of
 * the License at http://www.mozilla.org/MPL/
 * 
 * Software distributed under the License is distributed on an "AS
 * IS" basis, WITHOUT WARRANTY OF ANY KIND, either express or
 * implied. See the License for the specific language governing
 * rights and limitations under the License.
 ******************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using BioLink.Client.Extensibility;
using BioLink.Client.Utilities;
using System.ComponentModel;

namespace BioLink.Client.Taxa {

    public class SortedTaxonConverter : IValueConverter {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            IEnumerable<HierarchicalViewModelBase> taxa = value as IEnumerable<HierarchicalViewModelBase>;
            ListCollectionView lcv = (ListCollectionView) CollectionViewSource.GetDefaultView(taxa);

            // When the sort mode is manual, the order will take precedence over the default sort order
            if (TaxonExplorer.IsManualSort) {
                lcv.SortDescriptions.Add(new SortDescription("Order", ListSortDirection.Ascending)); // int
            }

            // The default sort order is computed according to a set of rules too complicated for the SortDescription interface...
            lcv.SortDescriptions.Add(new SortDescription("DefaultSortOrder", ListSortDirection.Ascending));

            return lcv;
        }
    
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            throw new NotImplementedException();
        }
    }


    public class TaxonFavoriteSortingConverter : IValueConverter {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            IEnumerable<HierarchicalViewModelBase> favorites = value as IEnumerable<HierarchicalViewModelBase>;

            ListCollectionView lcv = (ListCollectionView)CollectionViewSource.GetDefaultView(favorites);

            // Favorite Groups (or folders) go first...
            lcv.SortDescriptions.Add(new SortDescription("IsGroup", ListSortDirection.Descending)); 

            // When the sort mode is manual, the order will take precedence over the name
            if (TaxonExplorer.IsManualSort) {
                lcv.SortDescriptions.Add(new SortDescription("Order", ListSortDirection.Ascending)); 
            }

            // Higher orders precede lower...
            lcv.SortDescriptions.Add(new SortDescription("ElemType", ListSortDirection.Ascending));

            // Finally the "default" sort order
            lcv.SortDescriptions.Add(new SortDescription("DefaultSortOrder", ListSortDirection.Ascending)); 

            return lcv;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            throw new NotImplementedException();
        }
    }

}
