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
    public partial class SiteRDEControl : UserControl {

        private TraitControl _traits;
        private RDESiteViewModel _currentSite;
       
        public SiteRDEControl(User user) {
            InitializeComponent();
            txtPolitical.BindUser(user, LookupType.Region);
            txtSource.BindUser(user, PickListType.Phrase, "Source", TraitCategoryType.Material);
            txtElevSource.BindUser(user, PickListType.Phrase, "Source", TraitCategoryType.Material);
            txtUnits.BindUser(user, PickListType.Phrase, "Units", TraitCategoryType.Material);
            _traits = new TraitControl(user, TraitCategoryType.Site, null, true);
            tabTraits.Content = _traits;

            this.DataContextChanged += new DependencyPropertyChangedEventHandler(SiteRDEControl_DataContextChanged);

            ctlPosition.LocationChanged += new LocationSelectedEvent(ctlPosition_LocationChanged);
            
        }

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

                if (!site.Latitude.HasValue || !site.Longitude.HasValue || (site.Latitude.Value == 0 && site.Longitude.Value == 0) ) {
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

    }
}
