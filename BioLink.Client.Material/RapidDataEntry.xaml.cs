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
                
                grpSites.DataContextChanged += new DependencyPropertyChangedEventHandler((s, e) => {
                    var site = e.NewValue as RDESiteViewModel;
                    if (site != null) {
                        grpSiteVisits.Items = site.SiteVisits;
                    }
                });

                grpSiteVisits.DataContextChanged += new DependencyPropertyChangedEventHandler((s, e) => {
                    var siteVisit = e.NewValue as RDESiteVisitViewModel;
                    if (siteVisit != null) {
                        grpMaterial.Items = siteVisit.Material;
                    }
                });

                grpSites.Items = siteModel;
                
            }            

            grpSites.Content = new SiteRDEControl(user);
            grpSiteVisits.Content = new SiteVisitRDEControl(user);
            grpMaterial.Content = new MaterialRDEControl(user);

        }

        private RDESiteViewModel BuildModel(int objectId, SiteExplorerNodeType objectType) {
            var service = new MaterialService(User);
            var supportService = new SupportService(User);

            RDESiteViewModel root = null;
            List<RDEMaterial> material = null;

            switch (objectType) {
                case SiteExplorerNodeType.Site:
                    var sites = service.GetRDESites(new int[] { objectId });
                    if (sites.Count > 0) {
                        root = CreateSiteViewModel(sites[0]);
                        var siteVisits = service.GetRDESiteVisits(new int[] {root.SiteID}, RDEObjectType.Site);
                        var idList = new List<Int32>(); // This collection will keep track of every site visit id for later use...
                        root.SiteVisits = new ObservableCollection<ViewModelBase>(siteVisits.ConvertAll((sv) => {
                            var vm = CreateSiteVisitViewModel(sv, root);
                            idList.Add(vm.SiteVisitID);
                            return vm;
                        }));

                        // This service call gets all the material for all site visits, saving multiple round trips to the database
                        material = service.GetRDEMaterial(idList.ToArray(), RDEObjectType.SiteVisit);

                        // But now we have to attach the material to the right visit...
                        foreach (RDESiteVisitViewModel sv in root.SiteVisits) {
                            // select out which material belongs to the current visit...
                            var selected = material.Where((m) => m.SiteVisitID == sv.SiteVisitID);
                            // create the material view models and attach to the visit.
                            sv.Material = CreateMaterialViewModels(selected, sv);
                        }
                        
                    }
                    break;
                case SiteExplorerNodeType.SiteVisit:
                    var visits = service.GetRDESiteVisits(new int[] { objectId });
                    if (visits.Count > 0) {
                        // get the site ...
                        sites = service.GetRDESites(new int[] { visits[0].SiteID });
                        if (sites.Count > 0) {
                            root = CreateSiteViewModel(sites[0]);
                            var visitModel = CreateSiteVisitViewModel(visits[0], root);
                            // get the material...
                            material = service.GetRDEMaterial(new int[] { visitModel.SiteVisitID }, RDEObjectType.SiteVisit);
                            CreateMaterialViewModels(material, visitModel);
                        }            
                    }
                    break;
                case SiteExplorerNodeType.Material:
                    material = service.GetRDEMaterial(new int[] { objectId });
                    if (material.Count > 0) {
                        var m = material[0];

                        // Get the Visit...
                        visits = service.GetRDESiteVisits(new int[] { m.SiteVisitID }, RDEObjectType.SiteVisit);
                        if (visits.Count > 0) {
                            // Get the site...
                            sites = service.GetRDESites(new int[] { visits[0].SiteID });
                            if (sites.Count > 0) {
                                root = CreateSiteViewModel(sites[0]);
                                var siteVisit = CreateSiteVisitViewModel(visits[0], root);
                                CreateMaterialViewModels(material, siteVisit);
                            }
                        }
                    }
                    break;
            }

            if (root != null) {
                // Get a single list of all the material view models loaded...
                var materialList = new List<RDEMaterialViewModel>();
                root.SiteVisits.ForEach((vm) => {
                    var sv = vm as RDESiteVisitViewModel;
                    sv.Material.ForEach((vm2) => {
                        var m = vm2 as RDEMaterialViewModel;
                        materialList.Add(m);
                    });
                });

                var materialIds = materialList.Select((m) => {
                    return m.MaterialID;
                }).ToArray();

                // for the material id list we can extract all the subparts in one round trip...
                var subparts = service.GetMaterialParts(materialIds);
                // But we have to hook them back up to the right material.
                foreach (RDEMaterialViewModel m in materialList) {
                    var selected = subparts.Where((part) => { return part.MaterialID == m.MaterialID; });
                    m.SubParts = new ObservableCollection<ViewModelBase>(selected.Select((part) => {
                        var vm = new MaterialPartViewModel(part);
                        vm.Locked = m.Locked;
                        vm.DataChanged += new DataChangedHandler(SubPart_DataChanged);
                        return vm;
                    }));
                }

            }

            return root;
        }

        void SubPart_DataChanged(ChangeableModelBase viewmodel) {
            var subpart = viewmodel as MaterialPartViewModel;
            if (subpart != null) {
                RegisterUniquePendingChange(new UpdateMaterialPartAction(subpart.Model));
            }
        }

        private RDESiteViewModel CreateSiteViewModel(RDESite site) {
            var supportService = new SupportService(User);
            var vm = new RDESiteViewModel(site);
            vm.Traits = supportService.GetTraits(TraitCategoryType.Site.ToString(), site.SiteID);
            vm.DataChanged += new DataChangedHandler(siteViewModel_DataChanged);
            return vm;
        }

        private RDESiteVisitViewModel CreateSiteVisitViewModel(RDESiteVisit visit, RDESiteViewModel site) {
            var vm = new RDESiteVisitViewModel(visit);
            vm.DataChanged += new DataChangedHandler(siteVisitViewModel_DataChanged);
            vm.Site = site;
            vm.SiteID = site.SiteID;
            site.SiteVisits.Add(vm);
            return vm;
        }

        private ObservableCollection<ViewModelBase> CreateMaterialViewModels(IEnumerable<RDEMaterial> material, RDESiteVisitViewModel siteVisit) {
            var service = new MaterialService(User);
            var supportService = new SupportService(User);
            siteVisit.Material.Clear();
            return new ObservableCollection<ViewModelBase>(material.Select((m) => {
                var vm = new RDEMaterialViewModel(m);
                vm.Traits = supportService.GetTraits(TraitCategoryType.Material.ToString(), vm.MaterialID);
                vm.DataChanged += new DataChangedHandler(materialViewModel_DataChanged);
                vm.SiteVisit = siteVisit;
                vm.SiteVisitID = siteVisit.SiteVisitID;
                siteVisit.Material.Add(vm);
                return (ViewModelBase)vm;
            }));
        }

        void materialViewModel_DataChanged(ChangeableModelBase viewmodel) {
            // TODO:!
        }

        void siteVisitViewModel_DataChanged(ChangeableModelBase viewmodel) {
            var siteVisit = viewmodel as RDESiteVisitViewModel;
            if (siteVisit != null) {
                RegisterUniquePendingChange(new UpdateRDESiteVisitAction(siteVisit.Model));
            }
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
                // First add the site
                var siteViewModel = new RDESiteViewModel(new RDESite());
                siteViewModel.DataChanged +=new DataChangedHandler(siteViewModel_DataChanged);

                // and a new visit
                var siteVisitViewModel = new RDESiteVisitViewModel(new RDESiteVisit());                
                siteVisitViewModel.DataChanged +=new DataChangedHandler(siteVisitViewModel_DataChanged);
                siteVisitViewModel.Site = siteViewModel;
                siteVisitViewModel.SiteID = siteVisitViewModel.SiteID;
                siteViewModel.SiteVisits.Add(siteVisitViewModel);

                // and some material...
                var materialViewModel = new RDEMaterialViewModel(new RDEMaterial());
                materialViewModel.DataChanged +=new DataChangedHandler(materialViewModel_DataChanged);
                materialViewModel.SiteVisit = siteVisitViewModel;
                materialViewModel.SiteVisitID = siteVisitViewModel.SiteVisitID;
                siteVisitViewModel.Material.Add(materialViewModel);

                // Add the new site to the group and select it...
                g.Items.Add(siteViewModel);
                g.SelectedItem = siteViewModel;

                RegisterPendingChange(new InsertRDESiteAction(siteViewModel.Model));
                RegisterPendingChange(new InsertRDESiteVisitAction(siteVisitViewModel.Model));
                RegisterPendingChange(new InsertRDEMaterialAction(materialViewModel.Model));
            }

        }

        private void grpSiteVisits_AddNewClicked(object sender, RoutedEventArgs e) {            
            var siteViewModel = grpSites.SelectedItem as RDESiteViewModel;
            if (siteViewModel != null) {

                // Add a new visit
                var siteVisitViewModel = new RDESiteVisitViewModel(new RDESiteVisit());
                siteVisitViewModel.DataChanged += new DataChangedHandler(siteVisitViewModel_DataChanged);
                siteVisitViewModel.Site = siteViewModel;
                siteVisitViewModel.SiteID = siteVisitViewModel.SiteID;
                siteViewModel.SiteVisits.Add(siteVisitViewModel);

                // and some material...
                var materialViewModel = new RDEMaterialViewModel(new RDEMaterial());
                materialViewModel.DataChanged += new DataChangedHandler(materialViewModel_DataChanged);
                materialViewModel.SiteVisit = siteVisitViewModel;
                materialViewModel.SiteVisitID = siteVisitViewModel.SiteVisitID;
                siteVisitViewModel.Material.Add(materialViewModel);

                grpSiteVisits.SelectedItem = siteVisitViewModel;
                
                RegisterPendingChange(new InsertRDESiteVisitAction(siteVisitViewModel.Model));
                RegisterPendingChange(new InsertRDEMaterialAction(materialViewModel.Model));
            }

        }

        private void grpMaterial_AddNewClicked(object sender, RoutedEventArgs e) {
            var siteVisitViewModel = grpSiteVisits.SelectedItem as RDESiteVisitViewModel;
            if (siteVisitViewModel != null) {

                // create the new material...
                var materialViewModel = new RDEMaterialViewModel(new RDEMaterial());
                materialViewModel.DataChanged += new DataChangedHandler(materialViewModel_DataChanged);
                materialViewModel.SiteVisit = siteVisitViewModel;
                materialViewModel.SiteVisitID = siteVisitViewModel.SiteVisitID;
                siteVisitViewModel.Material.Add(materialViewModel);

                grpMaterial.SelectedItem = materialViewModel;
                
                RegisterPendingChange(new InsertRDEMaterialAction(materialViewModel.Model));
            }
            
        }


    }
    
}
