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

        private int _objectId;
        private SiteExplorerNodeType _objectType;

        public RapidDataEntry(User user, int objectId, SiteExplorerNodeType objectType) : base(user, "RDE") {
            InitializeComponent();

            _objectId = objectId;
            _objectType = objectType;

            this.Loaded += new RoutedEventHandler(RapidDataEntry_Loaded);

        }

        void RapidDataEntry_Loaded(object sender, RoutedEventArgs evt) {

            var root = BuildModel(_objectId, _objectType);

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

            grpSites.Content = new SiteRDEControl(User);
            grpSiteVisits.Content = new SiteVisitRDEControl(User);
            grpMaterial.Content = new MaterialRDEControl(User);

            // Bind input gestures to the commands...
            AddNewSiteCmd.InputGestures.Add(new KeyGesture(Key.T, ModifierKeys.Control));
            AddNewSiteVisitCmd.InputGestures.Add(new KeyGesture(Key.I, ModifierKeys.Control));
            AddNewMaterialCmd.InputGestures.Add(new KeyGesture(Key.M, ModifierKeys.Control));
            MoveNextCmd.InputGestures.Add(new KeyGesture(Key.N, ModifierKeys.Control));
            MovePreviousCmd.InputGestures.Add(new KeyGesture(Key.P, ModifierKeys.Control));

            // Command Bindings...            
            this.FindParentWindow().CommandBindings.Add(new CommandBinding(AddNewSiteCmd, ExecutedAddNewSite, CanExecuteAddNewSite));
            this.FindParentWindow().CommandBindings.Add(new CommandBinding(AddNewSiteVisitCmd, ExecutedAddNewSiteVisit, CanExecuteAddNewSiteVisit));
            this.FindParentWindow().CommandBindings.Add(new CommandBinding(AddNewMaterialCmd, ExecutedAddNewMaterial, CanExecuteAddNewMaterial));

            this.FindParentWindow().CommandBindings.Add(new CommandBinding(MoveNextCmd, ExecutedMoveNext, CanExecuteMoveNext));
            this.FindParentWindow().CommandBindings.Add(new CommandBinding(MovePreviousCmd, ExecutedMovePrevious, CanExecuteMovePrevious));

        }

        private RDESiteViewModel BuildModel(int objectId, SiteExplorerNodeType objectType) {
            var service = new MaterialService(User);
            var supportService = new SupportService(User);

            RDESiteViewModel root = null;
            List<RDEMaterial> material = null;

            if (objectId < 0) {                
                root = CreateSiteViewModel(new RDESite());
                RegisterPendingChange(new InsertRDESiteAction(root.Model));
                var siteVisit = AddNewSiteVisit(root);
                var newMaterial = AddNewMaterial(siteVisit);
                return root;
            }

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
                // and associates as well. This means we only need one pass through the material list in order to
                // hook up subordinate records
                var associates = supportService.GetAssociates(TraitCategoryType.Material.ToString(), materialIds);
                // But we have to hook them back up to the right material.
                foreach (RDEMaterialViewModel m in materialList) {
                    var selectedSubParts = subparts.Where((part) => { return part.MaterialID == m.MaterialID; });
                    m.SubParts = new ObservableCollection<ViewModelBase>(selectedSubParts.Select((part) => {
                        var vm = new MaterialPartViewModel(part);
                        vm.Locked = m.Locked;
                        vm.DataChanged += new DataChangedHandler(SubPart_DataChanged);
                        return vm;
                    }));

                    var selectedAssociates = associates.Where((assoc) => { return assoc.FromIntraCatID == m.MaterialID || assoc.ToIntraCatID == m.MaterialID; });
                    m.Associates = new ObservableCollection<ViewModelBase>(selectedAssociates.Select((assoc) => {
                        var vm = new AssociateViewModel(assoc);                        
                        vm.DataChanged += new DataChangedHandler(associate_DataChanged);
                        return vm;
                    }));
                }

            }

            return root;
        }

        void associate_DataChanged(ChangeableModelBase viewmodel) {
            var assoc = viewmodel as AssociateViewModel;
            if (assoc != null) {
                RegisterUniquePendingChange(new UpdateAssociateAction(assoc.Model));
            }
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
            var material = viewmodel as RDEMaterialViewModel;
            if (material != null) {
                RegisterUniquePendingChange(new UpdateRDEMaterialAction(material.Model));
            }
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
            AddNewSite();
        }

        private void AddNewSite() {
            // First add the site
            var siteViewModel = CreateSiteViewModel(new RDESite());

            // and a new visit
            var siteVisitViewModel = AddNewSiteVisit(siteViewModel);

            // and some material...
            var materialViewModel = AddNewMaterial(siteVisitViewModel);

            // Add the new site to the group and select it...
            grpSites.Items.Add(siteViewModel);
            grpSites.SelectedItem = siteViewModel;

            RegisterPendingChange(new InsertRDESiteAction(siteViewModel.Model));
        }

        private void grpSiteVisits_AddNewClicked(object sender, RoutedEventArgs e) {
            AddNewSiteVisit();
        }

        private void AddNewSiteVisit() {
            var siteViewModel = grpSites.SelectedItem as RDESiteViewModel;
            if (siteViewModel != null) {

                // Add a new visit
                var siteVisitViewModel = AddNewSiteVisit(siteViewModel);

                // and some material...
                var materialViewModel = AddNewMaterial(siteVisitViewModel);

                grpSiteVisits.SelectedItem = siteVisitViewModel;
                
            }
        }

        private void grpMaterial_AddNewClicked(object sender, RoutedEventArgs e) {
            AddNewMaterial();
        }

        private void AddNewMaterial() {
            var siteVisitViewModel = grpSiteVisits.SelectedItem as RDESiteVisitViewModel;
            if (siteVisitViewModel != null) {
                // create the new material...
                var materialViewModel = AddNewMaterial(siteVisitViewModel);
                // and select it
                if (materialViewModel != null) {
                    grpMaterial.SelectedItem = materialViewModel;
                }
            }
        }

        private RDESiteVisitViewModel AddNewSiteVisit(RDESiteViewModel site) {
            var siteVisit = new RDESiteVisitViewModel(new RDESiteVisit());
            
            siteVisit.Site = site;
            siteVisit.SiteID = site.SiteID;
            site.SiteVisits.Add(siteVisit);

            siteVisit.DataChanged +=new DataChangedHandler(siteVisitViewModel_DataChanged);
            RegisterPendingChange(new InsertRDESiteVisitAction(siteVisit.Model, site.Model));
            return siteVisit;
        }

        private RDEMaterialViewModel AddNewMaterial(RDESiteVisitViewModel siteVisit) {
            if (siteVisit != null) {

                // create the new material...
                var materialViewModel = new RDEMaterialViewModel(new RDEMaterial());
                
                materialViewModel.SiteVisit = siteVisit;
                materialViewModel.SiteVisitID = siteVisit.SiteVisitID;

                siteVisit.Material.Add(materialViewModel);

                // Add one subpart...
                var subpart = new MaterialPartViewModel(new MaterialPart());
                subpart.MaterialPartID = -1;
                subpart.PartName = "<New>";

                materialViewModel.SubParts.Add(subpart);

                materialViewModel.DataChanged +=new DataChangedHandler(materialViewModel_DataChanged);

                RegisterPendingChange(new InsertRDEMaterialAction(materialViewModel.Model, siteVisit.Model));
                RegisterPendingChange(new InsertMaterialPartAction(subpart.Model, materialViewModel));

                return materialViewModel;
            }

            return null;

        }

        private void EditDetails(ViewModelBase selected, LookupType objectType) {

            if (selected == null || !selected.ObjectID.HasValue) {
                return;
            }

            if (HasPendingChanges) {
                if (this.Question("There are unsaved changes. Press 'Yes' to save all changes and continue, or 'No' to cancel the edit", "Edit unsaved data")) {
                    CommitPendingChanges();
                } else {
                    // abort!
                    return;
                }
            }

            PluginManager.Instance.EditLookupObject(objectType, selected.ObjectID.Value);
        }

        private void mnuEditSite_Click(object sender, RoutedEventArgs e) {
            EditDetails(grpSites.SelectedItem, LookupType.Site);
        }

        private void mnuEditSiteVisit_Click(object sender, RoutedEventArgs e) {
            EditDetails(grpSiteVisits.SelectedItem, LookupType.SiteVisit);
        }

        private void mnuEditMaterial_Click(object sender, RoutedEventArgs e) {
            EditDetails(grpMaterial.SelectedItem, LookupType.Material);
        }

        private ItemsGroupBox GetSelectedGroup() {
            var element = FocusManager.GetFocusedElement(this);

            if (element != null && element is FrameworkElement) {
                var fw = element as FrameworkElement;
                var group = fw.FindParent<ItemsGroupBox>();
                return group;
            }
            return null;
        }

        private void MoveNext(RoutedEventArgs e) {
            var group = GetSelectedGroup();
            if (group != null) {
                group.MoveNext(e);
            }
        }

        private void MovePrevious(RoutedEventArgs e) {
            var group = GetSelectedGroup();
            if (group != null) {
                group.MovePrevious(e);
            }
        }

        public static RoutedCommand AddNewSiteCmd = new RoutedCommand();

        private void ExecutedAddNewSite(object sender, ExecutedRoutedEventArgs e) {
            RapidDataEntry rde = null;
            if (e.Source is RapidDataEntry) {
                rde = e.Source as RapidDataEntry;
            } else if (e.Source is ControlHostWindow) {
                var window = e.Source as ControlHostWindow;
                rde = window.Control as RapidDataEntry;
            }
            if (rde != null) {
                rde.AddNewSite();
            }
        }

        private void CanExecuteAddNewSite(object sender, CanExecuteRoutedEventArgs e) {
            Control target = e.Source as Control;

            if (target != null) {
                e.CanExecute = true;
            } else {
                e.CanExecute = false;
            }
        }

        public static RoutedCommand AddNewSiteVisitCmd = new RoutedCommand();

        private void ExecutedAddNewSiteVisit(object sender, ExecutedRoutedEventArgs e) {
            RapidDataEntry rde = null;
            if (e.Source is RapidDataEntry) {
                rde = e.Source as RapidDataEntry;
            } else if (e.Source is ControlHostWindow) {
                var window = e.Source as ControlHostWindow;
                rde = window.Control as RapidDataEntry;
            }
            if (rde != null) {
                rde.AddNewSiteVisit();
            }
        }

        private void CanExecuteAddNewSiteVisit(object sender, CanExecuteRoutedEventArgs e) {
            Control target = e.Source as Control;

            if (target != null) {
                e.CanExecute = true;
            } else {
                e.CanExecute = false;
            }
        }

        public static RoutedCommand AddNewMaterialCmd = new RoutedCommand();

        private void ExecutedAddNewMaterial(object sender, ExecutedRoutedEventArgs e) {
            RapidDataEntry rde = null;
            if (e.Source is RapidDataEntry) {
                rde = e.Source as RapidDataEntry;
            } else if (e.Source is ControlHostWindow) {
                var window = e.Source as ControlHostWindow;
                rde = window.Control as RapidDataEntry;
            }
            if (rde != null) {
                rde.AddNewMaterial();
            }
        }

        private void CanExecuteAddNewMaterial(object sender, CanExecuteRoutedEventArgs e) {
            Control target = e.Source as Control;

            if (target != null) {
                e.CanExecute = true;
            } else {
                e.CanExecute = false;
            }
        }

        public static RoutedCommand MoveNextCmd = new RoutedCommand();

        private void ExecutedMoveNext(object sender, ExecutedRoutedEventArgs e) {
            RapidDataEntry rde = null;
            if (e.Source is RapidDataEntry) {
                rde = e.Source as RapidDataEntry;
            } else if (e.Source is ControlHostWindow) {
                var window = e.Source as ControlHostWindow;
                rde = window.Control as RapidDataEntry;
            }
            if (rde != null) {
                rde.MoveNext(e);
            }
        }

        private void CanExecuteMoveNext(object sender, CanExecuteRoutedEventArgs e) {
            Control target = e.Source as Control;

            if (target != null) {
                e.CanExecute = true;
            } else {
                e.CanExecute = false;
            }
        }

        public static RoutedCommand MovePreviousCmd = new RoutedCommand();

        private void ExecutedMovePrevious(object sender, ExecutedRoutedEventArgs e) {
            RapidDataEntry rde = null;
            if (e.Source is RapidDataEntry) {
                rde = e.Source as RapidDataEntry;
            } else if (e.Source is ControlHostWindow) {
                var window = e.Source as ControlHostWindow;
                rde = window.Control as RapidDataEntry;
            }
            if (rde != null) {
                rde.MovePrevious(e);
            }
        }

        private void CanExecuteMovePrevious(object sender, CanExecuteRoutedEventArgs e) {
            Control target = e.Source as Control;

            if (target != null) {
                e.CanExecute = true;
            } else {
                e.CanExecute = false;
            }
        }


    }
    
}
