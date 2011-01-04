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
    /// Interaction logic for RapidDataEntry.xaml
    /// </summary>
    public partial class RapidDataEntry : DatabaseActionControl {

        public RapidDataEntry(User user, int objectId, SiteExplorerNodeType objectType) : base(user, "RDE") {
            InitializeComponent();

            var root = BuildModel(objectId, objectType);

            if (root != null) {
                var siteModel = new ObservableCollection<ViewModelBase>();
                siteModel.Add(root);
                grpSites.Items = siteModel;

                grpSiteVisits.Items = root.SiteVisits;
            }            

            grpSites.Content = new SiteRDEControl(user);

            grpSiteVisits.Content = new SiteVisitRDEControl(user);

        }

        private RDESiteViewModel BuildModel(int objectId, SiteExplorerNodeType objectType) {
            var service = new MaterialService(User);
            var supportService = new SupportService(User);

            RDESiteViewModel root = null;

            switch (objectType) {
                case SiteExplorerNodeType.Site:
                    int[] ids = new int[] { objectId };
                    var sites = service.GetRDESites(ids);
                    if (sites.Count > 0) {
                        root = new RDESiteViewModel(sites[0]);
                        root.Traits = supportService.GetTraits(TraitCategoryType.Site.ToString(), root.SiteID);
                        root.DataChanged += new DataChangedHandler(siteViewModel_DataChanged);

                        var siteVisits = service.GetRDESiteVisits(root.SiteID);
                        root.SiteVisits = new ObservableCollection<ViewModelBase>(siteVisits.ConvertAll((sv) => {
                            var vm = new RDESiteVisitViewModel(sv);
                            vm.DataChanged += new DataChangedHandler(SiteVisitModel_DataChanged);
                            return vm;
                        }));
                        
                    }
                    break;
            }

            return root;
        }

        void SiteVisitModel_DataChanged(ChangeableModelBase viewmodel) {
            // TODO:!
        }

        void siteViewModel_DataChanged(ChangeableModelBase viewmodel) {
            var site = viewmodel as RDESiteViewModel;
            if (site != null) {
                RegisterUniquePendingChange(new UpdateRDESiteAction(site.Model));
            }
        }

        private void grpSites_AddNewClicked(object sender, RoutedEventArgs e) {
            var g = sender as ItemsGroupBox;
            if (g != null) {
                var model = new RDESite();
                model.SiteID = -1;
                model.Locality = "<New site>";

                var viewModel = new RDESiteViewModel(model);
                g.Items.Add(viewModel);
                g.SelectedItem = viewModel;

                viewModel.DataChanged +=new DataChangedHandler(siteViewModel_DataChanged);

                RegisterPendingChange(new InsertRDESiteAction(model));
            }

        }

    }
    
}
