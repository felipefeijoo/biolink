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
    public partial class RapidDataEntry : DatabaseCommandControl {

        private static string CONFIG_LAT_LONG_FORMAT = "RDE.LatLongFormat";
        private static string CONFIG_LOCKING_MODE = "RDE.LockAtStartMode";
        private static string CONFIG_AUTOFILL_MODE = "RDE.AutoFillMode";

        private static string CONFIG_AUTO_NUMBER = "RDE.AutoNumber";

        private static string CONFIG_SITE_TEMPLATE_ID = "RDE.SiteTemplateID";
        private static string CONFIG_SITEVISIT_TEMPLATE_ID = "RDE.SiteVisitTemplateID"; 
        private static string CONFIG_MATERIAL_TEMPLATE_ID = "RDE.MaterialTemplateID";

        private int _objectId;
        private SiteExplorerNodeType _objectType;
        private bool _startLockMode;
        private AutoFillMode _autoFillMode = AutoFillMode.NoAutoFill;
        private bool _autoNumber = false;

        public RapidDataEntry(MaterialExplorer explorer, User user, int objectId, SiteExplorerNodeType objectType, SiteExplorerNodeViewModel parent) : base(user, "RDE:" + objectType.ToString() + ":" + objectId) {

            // Bind input gestures to the commands...
            AddNewSiteCmd.InputGestures.Add(new KeyGesture(Key.T, ModifierKeys.Control));
            AddNewSiteVisitCmd.InputGestures.Add(new KeyGesture(Key.I, ModifierKeys.Control));
            AddNewMaterialCmd.InputGestures.Add(new KeyGesture(Key.M, ModifierKeys.Control));
            MoveNextCmd.InputGestures.Add(new KeyGesture(Key.N, ModifierKeys.Control));
            MovePreviousCmd.InputGestures.Add(new KeyGesture(Key.P, ModifierKeys.Control));
            UnlockAllCmd.InputGestures.Add(new KeyGesture(Key.U, ModifierKeys.Control));

            InitializeComponent();

            _objectId = objectId;
            _objectType = objectType;
            ParentNode = parent;
            Explorer = explorer;

            this.Loaded += new RoutedEventHandler(RapidDataEntry_Loaded);
            this.ChangesCommitted += new PendingChangesCommittedHandler(RapidDataEntry_ChangesCommitted);

        }

        public override bool Validate(List<string> messages) {
            if (Config.GetGlobal("Material.CheckUniqueAccessionNumbers", true)) {                
                if (MaterialControl != null) {
                    var material = grpMaterial.Items.Select((vm) => {
                        return vm as RDEMaterialViewModel;
                    });
                    if (material.Count() > 0) {
                        var service = new MaterialService(User);
                        List<string> accessionNumbers = new List<String>();

                        foreach (RDEMaterialViewModel m in material) {                                   
                            // Check only new material
                            if (m.MaterialID < 0 && !string.IsNullOrWhiteSpace(m.AccessionNo)) {
                                if (accessionNumbers.Contains(m.AccessionNo)) {
                                    messages.Add("Some of the material you are adding share the same accession number (" + m.AccessionNo + ")");
                                }
                                var candidateIds = service.GetMaterialIdsByAccessionNo(m.AccessionNo);
                                if (candidateIds.Count > 0) {
                                    messages.Add("There is already material in the database with Accession number " + m.AccessionNo + " (Material ID " + candidateIds[0] + ")");
                                }

                                accessionNumbers.Add(m.AccessionNo);
                            }
                        }
                    }
                }
            }
            return messages.Count == 0;    
        }

        void RapidDataEntry_ChangesCommitted(object sender) {
            if (ParentNode != null && Explorer != null) {
                Explorer.RefreshNode(ParentNode);
            }
        }

        void RapidDataEntry_Loaded(object sender, RoutedEventArgs evt) {

            // Load templates, if any...
            int siteTemplateId = Config.GetProfile(User, CONFIG_SITE_TEMPLATE_ID, -1);
            if (siteTemplateId >= 0) {
                LoadSiteTemplate(siteTemplateId);
            }

            int siteVisitTemplateId = Config.GetProfile(User, CONFIG_SITEVISIT_TEMPLATE_ID, -1);
            if (siteVisitTemplateId >= 0) {
                LoadSiteVisitTemplate(siteVisitTemplateId);
            }

            int materialTemplateId = Config.GetProfile(User, CONFIG_MATERIAL_TEMPLATE_ID, -1);
            if (materialTemplateId >= 0) {
                LoadMaterialTemplate(materialTemplateId);
            }

            _startLockMode = Config.GetUser(User, CONFIG_LOCKING_MODE, true);
            SetStartingLockMode(_startLockMode);

            _autoFillMode = Config.GetUser(User, CONFIG_AUTOFILL_MODE, AutoFillMode.NoAutoFill);
            SetAutoFillMode(_autoFillMode);

            _autoNumber = Config.GetUser(User, CONFIG_AUTO_NUMBER, false);
            SetAutoNumber(_autoNumber);

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

            var latLongMode = Config.GetUser(User, CONFIG_LAT_LONG_FORMAT, LatLongMode.DegreesMinutesSeconds);
            SetLatLongFormat(latLongMode);

            // Command Bindings...            
            this.FindParentWindow().CommandBindings.Add(new CommandBinding(AddNewSiteCmd, ExecutedAddNewSite, CanExecuteAddNewSite));
            this.FindParentWindow().CommandBindings.Add(new CommandBinding(AddNewSiteVisitCmd, ExecutedAddNewSiteVisit, CanExecuteAddNewSiteVisit));
            this.FindParentWindow().CommandBindings.Add(new CommandBinding(AddNewMaterialCmd, ExecutedAddNewMaterial, CanExecuteAddNewMaterial));
            this.FindParentWindow().CommandBindings.Add(new CommandBinding(MoveNextCmd, ExecutedMoveNext, CanExecuteMoveNext));
            this.FindParentWindow().CommandBindings.Add(new CommandBinding(MovePreviousCmd, ExecutedMovePrevious, CanExecuteMovePrevious));
            this.FindParentWindow().CommandBindings.Add(new CommandBinding(UnlockAllCmd, ExecutedUnlockAll, CanExecuteUnlockAll));

        }

        protected SiteRDEControl SitesControl {
            get { return grpSites.Content as SiteRDEControl; }
        }

        protected SiteVisitRDEControl SiteVisitsControl {
            get { return grpSiteVisits.Content as SiteVisitRDEControl; }
        }

        protected MaterialRDEControl MaterialControl {
            get { return grpMaterial.Content as MaterialRDEControl; }
        }

        private RDESiteViewModel BuildModel(int objectId, SiteExplorerNodeType objectType) {
            var service = new MaterialService(User);
            var supportService = new SupportService(User);

            RDESiteViewModel root = null;
            List<RDEMaterial> material = null;

            if (objectId < 0) {
                root = CreateSiteViewModel(new RDESite { LocalType=0 });
                root.Locked = false;
                RegisterPendingChange(new InsertRDESiteCommand(root.Model));
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
                RegisterUniquePendingChange(new UpdateAssociateCommand(assoc.Model));
            }
        }

        void SubPart_DataChanged(ChangeableModelBase viewmodel) {
            var subpart = viewmodel as MaterialPartViewModel;
            if (subpart != null) {
                RegisterUniquePendingChange(new UpdateMaterialPartCommand(subpart.Model));
            }
        }

        private RDESiteViewModel CreateSiteViewModel(RDESite site, bool addChangedHandler = true) {
            var supportService = new SupportService(User);
            var vm = new RDESiteViewModel(site);

            if (User.HasPermission(PermissionCategory.SPARC_SITE, PERMISSION_MASK.UPDATE)) {
                vm.Locked = _startLockMode;
            } else {
                vm.Locked = true;
            }

            if (site.SiteID >= 0) {
                vm.Traits = supportService.GetTraits(TraitCategoryType.Site.ToString(), site.SiteID);
            }
            if (addChangedHandler) {
                vm.DataChanged += new DataChangedHandler(siteViewModel_DataChanged);
            }
            return vm;
        }

        private RDESiteVisitViewModel CreateSiteVisitViewModel(RDESiteVisit visit, RDESiteViewModel site) {
            var vm = new RDESiteVisitViewModel(visit);
            vm.DataChanged += new DataChangedHandler(siteVisitViewModel_DataChanged);
            vm.Site = site;
            vm.SiteID = site.SiteID;
            if (User.HasPermission(PermissionCategory.SPARC_SITEVISIT, PERMISSION_MASK.UPDATE)) {
                vm.Locked = _startLockMode;
            } else {
                vm.Locked = true;
            }
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
                if (User.HasPermission(PermissionCategory.SPARC_MATERIAL, PERMISSION_MASK.UPDATE)) {
                    vm.Locked = _startLockMode;
                } else {
                    vm.Locked = true;
                }
                return (ViewModelBase)vm;
            }));
        }

        void materialViewModel_DataChanged(ChangeableModelBase viewmodel) {
            var material = viewmodel as RDEMaterialViewModel;
            if (material != null) {
                RegisterUniquePendingChange(new UpdateRDEMaterialCommand(material.Model));
            }
        }

        void siteVisitViewModel_DataChanged(ChangeableModelBase viewmodel) {
            var siteVisit = viewmodel as RDESiteVisitViewModel;
            if (siteVisit != null) {
                RegisterUniquePendingChange(new UpdateRDESiteVisitCommand(siteVisit.Model));
            }
        }

        void siteViewModel_DataChanged(ChangeableModelBase viewmodel) {
            var site = viewmodel as RDESiteViewModel;
            if (site != null) {
                RegisterUniquePendingChange(new UpdateRDESiteCommand(site.Model));
            }
        }

        private void grpSites_AddNewClicked(object sender, RoutedEventArgs e) {
            AddNewSite();
        }

        private RDESite CreateNewSite(out List<Trait> traits) {

            traits = null;
            RDESiteViewModel copyFrom = null;
            switch (_autoFillMode) {
                case AutoFillMode.CopyCurrentData:
                    copyFrom = grpSites.SelectedItem as RDESiteViewModel;                    
                    break;
                case AutoFillMode.TemplateData:
                    copyFrom = _SiteTemplate;
                    break;                    
            }

            RDESite ret = null;
            if (copyFrom != null) {
                ret = ReflectionUtils.Clone(copyFrom.Model);
                ret.SiteID = -1;
                ret.SiteName = null;
                traits = new List<Trait>();
                var control = grpSites.Content as SiteRDEControl;
                if (control != null) {
                    foreach (Trait t in control.GetTraits()) {
                        var newTrait = ReflectionUtils.Clone(t);
                        newTrait.IntraCatID = -1;
                        traits.Add(newTrait);
                    }
                }
            } else {
                ret = new RDESite();
            }

            ret.LocalType = 0; // Locality

            ret.Locked = false;

            return ret;
        }

        private RDESiteVisit CreateNewSiteVisit() {
            
            RDESiteVisitViewModel copyFrom = null;
            switch (_autoFillMode) {
                case AutoFillMode.CopyCurrentData:
                    copyFrom = grpSiteVisits.SelectedItem as RDESiteVisitViewModel;
                    break;
                case AutoFillMode.TemplateData:
                    copyFrom = _SiteVisitTemplate;
                    break;
            }

            RDESiteVisit ret = null;
            if (copyFrom != null) {
                ret = ReflectionUtils.Clone(copyFrom.Model);
                ret.SiteVisitID = -1;
                ret.VisitName = null;
            } else {
                ret = new RDESiteVisit();
            }

            ret.Locked = false;

            return ret;
        }

        private RDEMaterial CreateNewMaterial(out List<Trait> traits, out List<Associate> associates, out List<MaterialPart> subparts) {

            
            RDEMaterialViewModel copyFrom = null;

            traits = null;
            associates = null;
            subparts = null;

            IEnumerable<Associate> copyFromAssociates = null;
            IEnumerable<MaterialPart> copyFromSubparts = null;

            switch (_autoFillMode) {
                case AutoFillMode.CopyCurrentData:
                    copyFrom = grpMaterial.SelectedItem as RDEMaterialViewModel;
                    if (copyFrom != null) {
                        var control = grpMaterial.Content as MaterialRDEControl;
                        copyFrom.Traits = control.GetTraits();
                        copyFromAssociates = control.GetAssociates();
                        copyFromSubparts = control.GetSubParts();
                    }
                    break;
                case AutoFillMode.TemplateData:
                    copyFrom = _MaterialTemplate;
                    if (_MaterialTemplate != null) {
                        copyFromAssociates = _MaterialTemplate.Associates.Select(vm => (vm as AssociateViewModel).Model);
                        copyFromSubparts = _MaterialTemplate.SubParts.Select(vm => (vm as MaterialPartViewModel).Model);
                    }
                    break;
            }

            RDEMaterial ret = null;
            if (copyFrom != null) {
                ret = ReflectionUtils.Clone(copyFrom.Model);
                ret.MaterialID = -1;
                ret.MaterialName = null;
                
                traits = new List<Trait>();
                associates = new List<Associate>();
                subparts = new List<MaterialPart>();

                foreach (Trait t in copyFrom.Traits) {
                    var newTrait = ReflectionUtils.Clone(t);
                    newTrait.IntraCatID = -1;
                    traits.Add(newTrait);
                }

                foreach (Associate a in copyFromAssociates) {
                    var newAssoc = ReflectionUtils.Clone(a);
                    newAssoc.AssociateID = -1;
                    associates.Add(newAssoc);
                }

                foreach (MaterialPart p in copyFromSubparts) {
                    var newSubpart = ReflectionUtils.Clone(p);
                    newSubpart.MaterialPartID = -1;
                    subparts.Add(newSubpart);
                }

            } else {
                ret = new RDEMaterial();
            }

            ret.Locked = false;

            return ret;
        }

        private void AddNewSite(bool createNewMaterial = true) {
            // First add the site
            var traits = new List<Trait>();
            var site = CreateNewSite(out traits);
            var siteViewModel = new RDESiteViewModel(site);            
            siteViewModel.DataChanged += new DataChangedHandler(siteViewModel_DataChanged);

            RegisterPendingChange(new InsertRDESiteCommand(site));
            RegisterUniquePendingChange(new UpdateRDESiteCommand(site));

            if (traits != null && traits.Count > 0) {
                foreach (Trait t in traits) {
                    siteViewModel.Traits.Add(t);
                    RegisterPendingChange(new UpdateTraitDatabaseCommand(t, siteViewModel));
                }
            }

            // and a new visit
            var siteVisitViewModel = AddNewSiteVisit(siteViewModel);

            // and some material...
            if (createNewMaterial) {
                AddNewMaterial(siteVisitViewModel);
            }

            // Add the new site to the group and select it...
            grpSites.Items.Add(siteViewModel);
            grpSites.SelectedItem = siteViewModel;            
        }

        private void grpSiteVisits_AddNewClicked(object sender, RoutedEventArgs e) {
            AddNewSiteVisit();
        }

        private void AddNewSiteVisit(bool addNewMaterial = true) {
            var siteViewModel = grpSites.SelectedItem as RDESiteViewModel;
            if (siteViewModel != null) {

                // Add a new visit
                var siteVisitViewModel = AddNewSiteVisit(siteViewModel);

                // and some material...
                if (addNewMaterial) {                    
                    AddNewMaterial(siteVisitViewModel);
                }

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

                    if (_autoNumber) {
                        var control = grpMaterial.Content as MaterialRDEControl;
                        if (control != null) {
                            control.GenerateAutoNumbers();
                        }
                    }

                }
            }
        }

        private RDESiteVisitViewModel AddNewSiteVisit(RDESiteViewModel site) {

            var sv = CreateNewSiteVisit();
            var siteVisit = new RDESiteVisitViewModel(sv);
            
            siteVisit.Site = site;
            siteVisit.SiteID = site.SiteID;
            site.SiteVisits.Add(siteVisit);

            siteVisit.DataChanged +=new DataChangedHandler(siteVisitViewModel_DataChanged);
            RegisterPendingChange(new InsertRDESiteVisitCommand(siteVisit.Model, site.Model));
            RegisterPendingChange(new UpdateRDESiteVisitCommand(siteVisit.Model));
            return siteVisit;
        }

        

        private RDEMaterialViewModel AddNewMaterial(RDESiteVisitViewModel siteVisit) {
            if (siteVisit != null) {

                // create the new material...
                List<Trait> traits = null;
                List<Associate> associates = null;
                List<MaterialPart> subparts = null;

                var material = CreateNewMaterial(out traits, out associates, out subparts);
                var materialViewModel = new RDEMaterialViewModel(material);

                RegisterPendingChange(new InsertRDEMaterialCommand(material, siteVisit.Model));
                RegisterUniquePendingChange(new UpdateRDEMaterialCommand(material));

                if (traits != null && traits.Count > 0) {
                    foreach (Trait t in traits) {
                        materialViewModel.Traits.Add(t);
                        RegisterPendingChange(new UpdateTraitDatabaseCommand(t, materialViewModel));
                    }
                }

                if (associates != null && associates.Count > 0) {
                    foreach (Associate a in associates) {
                        var vm = new AssociateViewModel(a);
                        vm.DataChanged += new DataChangedHandler(associate_DataChanged);
                        materialViewModel.Associates.Add(vm);
                        RegisterPendingChange(new InsertAssociateCommand(a, materialViewModel));
                    }
                }

                if (subparts != null && subparts.Count > 0) {
                    foreach (MaterialPart subpart in subparts) {
                        var vm = new MaterialPartViewModel(subpart);
                        vm.DataChanged +=new DataChangedHandler(SubPart_DataChanged);
                        materialViewModel.SubParts.Add(vm);
                        RegisterPendingChange(new InsertMaterialPartCommand(subpart, materialViewModel));
                    }
                } else {
                    // Add one subpart...
                    var subpart = new MaterialPartViewModel(new MaterialPart());
                    subpart.MaterialPartID = -1;
                    subpart.PartName = "<New>";
                    materialViewModel.SubParts.Add(subpart);
                    RegisterPendingChange(new InsertMaterialPartCommand(subpart.Model, materialViewModel));
                }
               
                materialViewModel.SiteVisit = siteVisit;
                materialViewModel.SiteVisitID = siteVisit.SiteVisitID;
                siteVisit.Material.Add(materialViewModel);
                materialViewModel.DataChanged +=new DataChangedHandler(materialViewModel_DataChanged);

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

        public static RoutedCommand UnlockAllCmd = new RoutedCommand();

        private void ExecutedUnlockAll(object sender, ExecutedRoutedEventArgs e) {
            RapidDataEntry rde = null;
            if (e.Source is RapidDataEntry) {
                rde = e.Source as RapidDataEntry;
            } else if (e.Source is ControlHostWindow) {
                var window = e.Source as ControlHostWindow;
                rde = window.Control as RapidDataEntry;
            }
            if (rde != null) {
                rde.UnlockAll();
            }
        }

        private void CanExecuteUnlockAll(object sender, CanExecuteRoutedEventArgs e) {
            Control target = e.Source as Control;

            if (target != null) {
                e.CanExecute = true;
            } else {
                e.CanExecute = false;
            }
        }

        private void UnlockAll() {
            grpSites.IsUnlocked = true;
            grpSiteVisits.IsUnlocked = true;
            grpMaterial.IsUnlocked = true;
        }

        private void mnuMoveToNewSite_Click(object sender, RoutedEventArgs e) {
            MoveMaterialToNewSite();
        }

        private void mnuMoveToNewSiteVisit_Click(object sender, RoutedEventArgs e) {
            MoveMaterialToNewSiteVisit();
        }

        private void MoveMaterialToNewSite() {
            var mat = grpMaterial.SelectedItem as RDEMaterialViewModel;
            if (mat == null) {
                return;
            }

            if (this.Question("Are you sure you want to remove this material (" + mat.TaxaDesc + ") from the current Site and Site Visit, and attach it to a new Site and Site Visit?", "Move material?")) {
                grpMaterial.Items.Remove(mat);
                AddNewSite(false); // Suppress the creation of a new piece of material...
                // attach the existing piece of material
                grpMaterial.Items.Add(mat);
                var newParent = grpSiteVisits.SelectedItem as RDESiteVisitViewModel;
                mat.SiteVisit = newParent;
                // mat.SiteVisitID = newParent.SiteVisitID;
                RegisterPendingChange(new MoveRDEMaterialCommand(mat.Model, newParent.Model));
            }
        }

        private void MoveMaterialToNewSiteVisit() {
            var mat = grpMaterial.SelectedItem as RDEMaterialViewModel;
            if (mat == null) {
                return;
            }

            if (this.Question("Are you sure you want to remove this material (" + mat.TaxaDesc + ") from the current Site Visit, and attach it to a new Site Visit?", "Move material?")) {
                grpMaterial.Items.Remove(mat);
                AddNewSiteVisit(false); // Suppress the creation of a new piece of material...
                // attach the existing piece of material
                grpMaterial.Items.Add(mat);
                var newParent = grpSiteVisits.SelectedItem as RDESiteVisitViewModel;
                mat.SiteVisit = newParent;
                // mat.SiteVisitID = newParent.SiteVisitID;
                RegisterPendingChange(new MoveRDEMaterialCommand(mat.Model, newParent.Model));
            }

        }

        private void SetLatLongFormat(LatLongMode mode) {
            var siteControl = grpSites.Content as SiteRDEControl;
            if (siteControl != null) {
                siteControl.SetLatLongFormat(mode);

                mnuLLDD.IsChecked = mode == LatLongMode.DecimalDegrees;
                mnuLLDMS.IsChecked = mode == LatLongMode.DegreesMinutesSeconds;
                mnuLLDDM.IsChecked = mode == LatLongMode.DegreesDecimalMinutes;

                Config.SetUser(User, CONFIG_LAT_LONG_FORMAT, mode);
            }
        }

        private void mnuLLDMS_Click(object sender, RoutedEventArgs e) {
            SetLatLongFormat(LatLongMode.DegreesMinutesSeconds);
        }

        private void mnuLLDD_Click(object sender, RoutedEventArgs e) {
            SetLatLongFormat(LatLongMode.DecimalDegrees);
        }

        private void mnuLLDDM_Click(object sender, RoutedEventArgs e) {
            SetLatLongFormat(LatLongMode.DegreesDecimalMinutes);
        }

        private void mnuLockAtStart_Click(object sender, RoutedEventArgs e) {
            SetStartingLockMode(true);
        }

        private void mnuUnlockAtStart_Click(object sender, RoutedEventArgs e) {
            SetStartingLockMode(false);
        }

        private void SetStartingLockMode(bool lockAtStart) {
            mnuLockAtStart.IsChecked = lockAtStart;
            mnuUnlockAtStart.IsChecked = !lockAtStart;
            Config.SetUser(User, CONFIG_LOCKING_MODE, lockAtStart);
        }

        private void SetAutoNumber(bool autonumber) {
            mnuAutoNumber.IsChecked = autonumber;
            _autoNumber = autonumber;
            Config.SetUser(User, CONFIG_AUTO_NUMBER, autonumber);
        }

        private void SetAutoFillMode(AutoFillMode mode) {

            mnuAutoFillNone.IsChecked = mode == AutoFillMode.NoAutoFill;
            mnuAutoFillCopyCurrent.IsChecked = mode == AutoFillMode.CopyCurrentData;
            mnuAutoFillTemplate.IsChecked = mode == AutoFillMode.TemplateData;

            _autoFillMode = mode;

            Config.SetUser(User, CONFIG_AUTOFILL_MODE, mode);
        }

        public enum AutoFillMode {
            NoAutoFill,
            CopyCurrentData,
            TemplateData
        }

        private void mnuAutoFillNone_Click(object sender, RoutedEventArgs e) {
            SetAutoFillMode(AutoFillMode.NoAutoFill);
        }

        private void mnuAutoFillCopyCurrent_Click(object sender, RoutedEventArgs e) {
            SetAutoFillMode(AutoFillMode.CopyCurrentData);
        }

        private void mnuAutoFillTemplate_Click(object sender, RoutedEventArgs e) {
            SetAutoFillMode(AutoFillMode.TemplateData);
        }

        private void mnuSetSiteTemplate_Click(object sender, RoutedEventArgs e) {
            ChooseSiteTemplate();
        }

        private void mnuSetSiteVisitTemplate_Click(object sender, RoutedEventArgs e) {
            ChooseSiteVisitTemplate();
        }

        private void mnuSetMaterialTemplate_Click(object sender, RoutedEventArgs e) {
            ChooseMaterialTemplate();
        }

        private RDESiteViewModel _SiteTemplate;
        private RDESiteVisitViewModel _SiteVisitTemplate;
        private RDEMaterialViewModel _MaterialTemplate;

        private int? ChooseTemplateID(Func<List<SiteExplorerNode>> func) {
            var pickList = new PickListWindow(User, "Select template", () => {                
                var templates = func();
                return templates.Select((m) => {
                    return new SiteExplorerNodeViewModel(m);
                });
            }, null);

            if (pickList.ShowDialog().ValueOrFalse()) {
                var selected = pickList.SelectedValue as SiteExplorerNodeViewModel;
                if (selected != null) {
                    return selected.ElemID;
                }
            }

            return null;
        }

        private void ChooseSiteTemplate() {
            var service = new MaterialService(User);
            var templateId = ChooseTemplateID(() => service.GetSiteTemplates());
            if (templateId != null && templateId.HasValue) {
                LoadSiteTemplate(templateId.Value);
            }
        }

        private void ChooseSiteVisitTemplate() {
            var service = new MaterialService(User);
            var templateId = ChooseTemplateID(() => service.GetSiteVisitTemplates());
            if (templateId != null && templateId.HasValue) {
                LoadSiteVisitTemplate(templateId.Value);
            }
        }

        private void ChooseMaterialTemplate() {
            var service = new MaterialService(User);
            var supportService = new SupportService(User);
            var templateId = ChooseTemplateID(() => service.GetMaterialTemplates());
            if (templateId != null && templateId.HasValue) {
                LoadMaterialTemplate(templateId.Value);
            }
        }

        private void LoadSiteTemplate(int templateId) {
            var service = new MaterialService(User);
            var list = service.GetRDESites(templateId);
            if (list != null && list.Count > 0) {
                _SiteTemplate = CreateSiteViewModel(list[0], false);
            } else {
                _SiteTemplate = null;
            }

            if (_SiteTemplate != null) {
                mnuSetSiteTemplate.Header = String.Format("Set _Site Template ({0}) ...", _SiteTemplate.SiteName);
                Config.SetProfile(User, CONFIG_SITE_TEMPLATE_ID, templateId);
            } else {
                mnuSetSiteTemplate.Header = "Set _Site Template...";
                Config.SetProfile(User, CONFIG_SITE_TEMPLATE_ID, -1);
            }

        }

        private void LoadSiteVisitTemplate(int templateId) {
            var service = new MaterialService(User);
            var list = service.GetRDESiteVisits(new int[] { templateId });
            if (list != null && list.Count > 0) {
                _SiteVisitTemplate = new RDESiteVisitViewModel(list[0]);
                
            }

            if (_SiteVisitTemplate != null) {
                mnuSetSiteVisitTemplate.Header = String.Format("Set Site _Visit Template ({0}) ...", _SiteVisitTemplate.VisitName);
                Config.SetProfile(User, CONFIG_SITEVISIT_TEMPLATE_ID, templateId);
            } else {
                mnuSetSiteVisitTemplate.Header = "Set Site _Visit Template...";
                Config.SetProfile(User, CONFIG_SITEVISIT_TEMPLATE_ID, -1);
            }
            
        }

        private void LoadMaterialTemplate(int templateId) {
            var supportService = new SupportService(User);
            var service = new MaterialService(User);

            var list = service.GetRDEMaterial(new int[] { templateId });
            if (list != null && list.Count > 0) {
                _MaterialTemplate = new RDEMaterialViewModel(list[0]);
                _MaterialTemplate.Traits = supportService.GetTraits(TraitCategoryType.Material.ToString(), _MaterialTemplate.MaterialID);
                // need to get associates and subparts...
                var subparts = service.GetMaterialParts(templateId);
                var associates = supportService.GetAssociates(TraitCategoryType.Material.ToString(), templateId);

                foreach (Associate assoc in associates) {
                    var vm = new AssociateViewModel(assoc);
                    _MaterialTemplate.Associates.Add(vm);
                }

                foreach (MaterialPart part in subparts) {
                    var vm = new MaterialPartViewModel(part);
                    _MaterialTemplate.SubParts.Add(vm);
                }

            }

            if (_MaterialTemplate != null) {
                mnuSetMaterialTemplate.Header = String.Format("Set _Material Template ({0}) ...", _MaterialTemplate.MaterialName);
                Config.SetProfile(User, CONFIG_MATERIAL_TEMPLATE_ID, templateId);
            } else {
                mnuSetMaterialTemplate.Header = "Set _Material Template...";
                Config.SetProfile(User, CONFIG_MATERIAL_TEMPLATE_ID, -1);
            }
            
        }

        private void mnuAutoNumber_Click(object sender, RoutedEventArgs e) {
            SetAutoNumber(mnuAutoNumber.IsChecked);
        }

        protected SiteExplorerNodeViewModel ParentNode { get; private set; }

        protected MaterialExplorer Explorer { get; private set; }

        private void mnuAddToLabelSet_Click(object sender, RoutedEventArgs e) {
            AddMaterialToLabelSet();
        }

        private void AddMaterialToLabelSet() {
            var m = grpMaterial.SelectedItem as RDEMaterialViewModel;
            if (m != null) {
                if (m.IsChanged || m.MaterialID <= 0) {
                    ErrorMessage.Show("This material has unsaved changes. Please apply the changes before trying again.");
                    return;
                }

                // make a pinnable...
                var pinnable = new PinnableObject(MaterialPlugin.MATERIAL_PLUGIN_NAME, Data.LookupType.Material, m.MaterialID);
                var target = PluginManager.Instance.FindAdaptorForPinnable<ILabelSetItemTarget>(pinnable);
                if (target != null) {
                    target.AddItemToLabelSet(pinnable);
                }
            }

        }

    }
    
}
