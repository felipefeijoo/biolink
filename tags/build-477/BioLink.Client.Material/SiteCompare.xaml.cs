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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using BioLink.Client.Extensibility;
using BioLink.Client.Utilities;
using BioLink.Data;
using BioLink.Data.Model;
using System.Collections.ObjectModel;

namespace BioLink.Client.Material {
    /// <summary>
    /// Interaction logic for SiteCompare.xaml
    /// </summary>
    /// 
    public partial class SiteCompare : Window {

        private ObservableCollection<SiteExplorerNodeViewModel> _compareModel;

        #region Designer Ctor
        public SiteCompare() {
            InitializeComponent();
        }
        #endregion

        public SiteCompare(Window owner, User user, SiteExplorerNodeViewModel dest, List<SiteExplorerNodeViewModel> otherSites) {
            this.Owner = owner;
            this.User = user;
            InitializeComponent();
            Destination = dest;
            _compareModel = new ObservableCollection<SiteExplorerNodeViewModel>(otherSites.ConvertAll((vm) => {
                vm.IsSelected = true;
                return vm;
            }));
            txtMergeInto.Text = dest.Name;
            lstRemove.ItemsSource = _compareModel;

            grpDiff.DataContextChanged += new DependencyPropertyChangedEventHandler(grpDiff_DataContextChanged);

            if (_compareModel.Count > 0) {
                lstRemove.SelectedItem = _compareModel[0];
            }

        }

        void grpDiff_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            var service = new MaterialService(User);
            var other = e.NewValue as SiteExplorerNodeViewModel;
            if (other != null) {
                var differences = service.CompareSites(Destination.ElemID, other.ElemID);
                lvw.ItemsSource = new ObservableCollection<SiteDifferenceViewModel>(differences.ConvertAll((m) => {
                    return new SiteDifferenceViewModel(m);
                }));
            }
        }

        private void btnMergeAll_Click(object sender, RoutedEventArgs e) {
            var list = new List<SiteExplorerNodeViewModel>();
            foreach (SiteExplorerNodeViewModel vm in _compareModel) {
                list.Add(vm);
            }
            SelectedSites = list;
            this.DialogResult = true;
            this.Hide();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e) {
            this.DialogResult = false;
            this.Hide();
        }

        private void btnMergeSelected_Click(object sender, RoutedEventArgs e) {
            var list = new List<SiteExplorerNodeViewModel>();
            foreach (SiteExplorerNodeViewModel vm in _compareModel) {
                if (vm.IsSelected) {
                    list.Add(vm);
                }
            }
            SelectedSites = list;
            this.DialogResult = true;
            this.Hide();            
        }

        public SiteExplorerNodeViewModel Destination { get; private set; }

        public User User { get; private set; }

        public List<SiteExplorerNodeViewModel> SelectedSites { get; private set; }

    }

    public class SiteDifferenceViewModel : ViewModelBase {
        public SiteDifferenceViewModel(SiteDifference model) {
            switch (model.Item.ToLower()) {
                case "tintlocaltype":
                    Item = "Local Type";
                    A = ConvertLocalType(model.A);
                    B = ConvertLocalType(model.B);
                    break;
                case "tintposcoordinates":
                    Item = "Coordinate Type";
                    A = ConvertCoordType(model.A);
                    B = ConvertCoordType(model.B);
                    break;
                case "tintposareatype":
                    Item = "Area Type";
                    A = ConvertAreaType(model.A);
                    B = ConvertAreaType(model.B);
                    break;
                case "tintelevtype":
                    Item = "Elevation Type";
                    A = ConvertElevType(model.A);
                    B = ConvertElevType(model.B);
                    break;
                default:
                    Item = StripPrefix(model.Item);
                    A = model.A;
                    B = model.B;
                    break;
            }
        }

        public string Item { get; set; }
        public string A { get; set; }
        public string B { get; set; }

        private string StripPrefix(string field) {
            int i = 0;
            while (char.IsLower(field[i])) {
                i++;
            };

            if (i > 0) {
                return field.Substring(i);
            }
            return field;
        }

        private static string[] _coordType = new string[] { "No coordinates specified", "Latitude/Longitude", "Eastings/Nortings" };
        private static string[] _localType = new string[] { "Nearest Place", "Locality"};
        private static string[] _areaType = new string[] { "Point", "Bounding Box", "Line" };
        private static string[] _elevType = new string[] { "Not Specified", "Elevation", "Depth" };

        private string ConvertType(string[] array, string type) {
            int itype;
            if (Int32.TryParse(type, out itype)) {
                if (itype < array.Length) {
                    return array[itype];
                }
            }
            return type;
        }

        private string ConvertLocalType(string type) {
            return ConvertType(_localType, type);
        }

        private string ConvertCoordType(string type) {
            return ConvertType(_coordType, type);
        }

        private string ConvertAreaType(string type) {
            return ConvertType(_areaType, type);
        }

        private string ConvertElevType(string type) {
            return ConvertType(_elevType, type);
        }

        public override int? ObjectID {
            get { return null; }
        }

    }

}
