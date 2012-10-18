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

namespace BioLink.Client.Material {
    /// <summary>
    /// Interaction logic for SiteRDEControl.xaml
    /// </summary>
    public partial class SiteRDEControl : UserControl, IItemsGroupBoxDetailControl {

        private TraitControl _traits;
        private RDESiteViewModel _currentSite;
       
        public SiteRDEControl(User user) {
            InitializeComponent();
            this.User = user;
            txtPolitical.BindUser(user, LookupType.Region);
            txtSource.BindUser(user, PickListType.Phrase, "Source", TraitCategoryType.Material);
            txtElevSource.BindUser(user, PickListType.Phrase, "Source", TraitCategoryType.Material);
            txtUnits.BindUser(user, PickListType.Phrase, "Units", TraitCategoryType.Material);
            _traits = new TraitControl(user, TraitCategoryType.Site, null, true);
            tabTraits.Content = _traits;

            this.DataContextChanged += new DependencyPropertyChangedEventHandler(SiteRDEControl_DataContextChanged);

            ctlPosition.LocationChanged += new LocationSelectedEvent(ctlPosition_LocationChanged);
            ctlPosition.BeforeLocationSelection += new BeforeNamedPlaceSelectionEvent(ctlPosition_BeforeLocationSelection);            
        }

        void ctlPosition_BeforeLocationSelection(NamedPlaceSelectionOptions options) {
            options.PlaceNameSeed = txtLocality.Text;
        }

        protected User User { get; private set; }

        void ctlPosition_LocationChanged(double latitude, double longitude, int? altitude, string altitudeUnits, string locality, string source) {

            if (!string.IsNullOrWhiteSpace(source)) {
                txtSource.Text = source;
            }

            if (!string.IsNullOrWhiteSpace(locality)) {
                var updateLocality = OptionalQuestions.UpdateLocalityQuestion.Ask(this.FindParentWindow(), txtLocality.Text, locality);

                if (updateLocality) {
                    txtLocality.Text = locality;
                }
            }

            if (altitude.HasValue) {
                if (OptionalQuestions.UpdateElevationQuestion.Ask(this.FindParentWindow(), altitude.Value)) {                    
                    txtElevUpper.Text = altitude.Value + "";
                    txtElevSource.Text = source;
                    txtUnits.Text = altitudeUnits;
                }
            }

        }

        void SiteRDEControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            var site = DataContext as RDESiteViewModel;
            if (site != null) {
                if (_currentSite != null) {
                    // although the database actions are registered for new/modified traits, we need to keep track of them so we can
                    // redisplay them as the user flips around the different sites.
                    _currentSite.Traits = _traits.GetModel();
                }
                _traits.BindModel(site.Traits, site);
                _currentSite = site;

                // There is a datacontext change cascade problem here.
                // When the RDE frame advances/retreats its first changes the data context on the various RDE Panels (this one included)
                // but our downstream dependents have yet to have their datacontexts changed (e.g. the position control, and it's descendants 
                // still point to the old site). This means when we clear the control (several lines down), it was actually clearing the old site as well as the control
                // text boxes. So the datacontext for the position control is explicitly set here so that it is correctly bound to the new site 
                // when it is cleared.
                // NOTE: have tried dispatching the call (kind of like invokeLater) and it didn't work.

                ctlPosition.DataContext = site;                
                if (!site.Latitude.HasValue || !site.Longitude.HasValue || (site.Latitude.Value == 0 && site.Longitude.Value == 0)) {
                    ctlPosition.Clear();
                }                
            }
        }

        public void SetLatLongFormat(LatLongMode mode) {
            this.ctlPosition.Mode = mode;
        }

        public List<Trait> GetTraits() {
            return _traits.GetModel();
        }

        public bool CanUnlock() {
            return User.HasPermission(PermissionCategory.SPARC_SITE, PERMISSION_MASK.UPDATE);
        }

        public bool CanAddNew() {
            return User.HasPermission(PermissionCategory.SPARC_SITE, PERMISSION_MASK.INSERT);
        }

    }
}
