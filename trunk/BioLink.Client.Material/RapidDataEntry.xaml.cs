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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using BioLink.Client.Extensibility;
using BioLink.Client.Utilities;
using BioLink.Data;
using BioLink.Data.Model;
using System.Collections.ObjectModel;

namespace BioLink.Client.Material {
    /// <summary>
    /// Interaction logic for RapidDataEntry.xaml
    /// </summary>
    public partial class RapidDataEntry {

        private const string ConfigLatLongFormat = "RDE.LatLongFormat";
        private const string ConfigLockingMode = "RDE.LockAtStartMode";
        private const string ConfigAutofillMode = "RDE.AutoFillMode";

        private const string ConfigAutoNumber = "RDE.AutoNumber";

        private const string ConfigSiteTemplateId = "RDE.SiteTemplateID";
        private const string ConfigSitevisitTemplateId = "RDE.SiteVisitTemplateID";
        private const string ConfigMaterialTemplateId = "RDE.MaterialTemplateID";

        private readonly int _objectId;
        private readonly SiteExplorerNodeType _objectType;
        private bool _startLockMode;
        private AutoFillMode _autoFillMode = AutoFillMode.NoAutoFill;
        private bool _autoNumber;

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

            Loaded += RapidDataEntryLoaded;
            ChangesCommitted += RapidDataEntryChangesCommitted;

        }

        public override bool Validate(List<string> messages) {
            if (Preferences.UniqueAccessionNumbers.Value) {
                if (MaterialControl != null) {
                    var material = new List<RDEMaterialViewModel>(grpMaterial.Items.Select(vm => vm as RDEMaterialViewModel));
                    if (material.Count() > 0) {
                        var service = new MaterialService(User);
                        var accessionNumbers = new List<String>();

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

        void RapidDataEntryChangesCommitted(object sender) {
            if (ParentNode != null && Explorer != null) {
                Explorer.RefreshNode(ParentNode);
            }
        }

        void RapidDataEntryLoaded(object sender, RoutedEventArgs evt) {

            // Load templates, if any...
            int siteTemplateId = Config.GetProfile(User, ConfigSiteTemplateId, -1);
            if (siteTemplateId >= 0) {
                LoadSiteTemplate(siteTemplateId);
            }

            int siteVisitTemplateId = Config.GetProfile(User, ConfigSitevisitTemplateId, -1);
            if (siteVisitTemplateId >= 0) {
                LoadSiteVisitTemplate(siteVisitTemplateId);
            }

            int materialTemplateId = Config.GetProfile(User, ConfigMaterialTemplateId, -1);
            if (materialTemplateId >= 0) {
                LoadMaterialTemplate(materialTemplateId);
            }

            _startLockMode = Config.GetUser(User, ConfigLockingMode, true);
            SetStartingLockMode(_startLockMode);

            _autoFillMode = Config.GetUser(User, ConfigAutofillMode, AutoFillMode.NoAutoFill);
            SetAutoFillMode(_autoFillMode);

            _autoNumber = Config.GetUser(User, ConfigAutoNumber, false);
            SetAutoNumber(_autoNumber);

            var root = BuildModel(_objectId, _objectType);

            if (root != null) {
                var siteModel = new ObservableCollection<ViewModelBase> {root};

                grpSites.DataContextChanged += (s, e) => {
                    var site = e.NewValue as RDESiteViewModel;
                    if (site != null) {
                        grpSiteVisits.Items = site.SiteVisits;
                    }
                };

                grpSiteVisits.DataContextChanged += (s, e) => {
                    var siteVisit = e.NewValue as RDESiteVisitViewModel;
                    if (siteVisit != null) {
                        grpMaterial.Items = siteVisit.Material;
                    }
                };

                grpSites.Items = siteModel;

            }

            grpSites.Content = new SiteRDEControl(User);
            grpSiteVisits.Content = new SiteVisitRDEControl(User);
            grpMaterial.Content = new MaterialRDEControl(User);

            var latLongMode = Config.GetUser(User, ConfigLatLongFormat, LatLongMode.DegreesMinutesSeconds);
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
            List<RDEMaterial> material;

            if (objectId < 0) {
                root = CreateSiteViewModel(new RDESite { LocalType=0 });
                root.Locked = false;
                RegisterPendingChange(new InsertRDESiteCommand(root.Model));
                var siteVisit = AddNewSiteVisit(root);
                AddNewMaterial(siteVisit);
                return root;
            }

            switch (objectType) {
                case SiteExplorerNodeType.Site:
                    var sites = service.GetRDESites(new[] { objectId });
                    if (sites.Count > 0) {
                        root = CreateSiteViewModel(sites[0]);
                        var siteVisits = service.GetRDESiteVisits(new[] {root.SiteID}, RDEObjectType.Site);
                        var idList = new List<Int32>(); // This collection will keep track of every site visit id for later use...
                        root.SiteVisits = new ObservableCollection<ViewModelBase>(siteVisits.ConvertAll(sv => {
                            var vm = CreateSiteVisitViewModel(sv, root);
                            idList.Add(vm.SiteVisitID);                            
                            return vm;
                        }));

                        // This service call gets all the material for all site visits, saving multiple round trips to the database
                        material = service.GetRDEMaterial(idList.ToArray(), RDEObjectType.SiteVisit);

                        // But now we have to attach the material to the right visit...
                        foreach (RDESiteVisitViewModel sv in root.SiteVisits) {
                            // select out which material belongs to the current visit...
                            RDESiteVisitViewModel sv1 = sv;
                            var selected = material.Where(m => m.SiteVisitID == sv1.SiteVisitID);
                            // create the material view models and attach to the visit.
                            sv.Material = CreateMaterialViewModels(selected, sv);
                        }
                        
                    }
                    break;
                case SiteExplorerNodeType.SiteVisit:
                    var visits = service.GetRDESiteVisits(new[] { objectId });
                    if (visits.Count > 0) {
                        // get the site ...
                        sites = service.GetRDESites(new[] { visits[0].SiteID });
                        if (sites.Count > 0) {
                            root = CreateSiteViewModel(sites[0]);
                            var visitModel = CreateSiteVisitViewModel(visits[0], root);
                            // get the material...
                            material = service.GetRDEMaterial(new[] { visitModel.SiteVisitID }, RDEObjectType.SiteVisit);
                            CreateMaterialViewModels(material, visitModel);
                        }            
                    }
                    break;
                case SiteExplorerNodeType.Material:
                    material = service.GetRDEMaterial(new[] { objectId });
                    if (material.Count > 0) {
                        var m = material[0];

                        // Get the Visit...
                        visits = service.GetRDESiteVisits(new[] { m.SiteVisitID });
                        if (visits.Count > 0) {
                            // Get the site...
                            sites = service.GetRDESites(new[] { visits[0].SiteID });
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
                root.SiteVisits.ForEach(vm => {
                    var sv = vm as RDESiteVisitViewModel;
                    if (sv != null) {
                        sv.Material.ForEach(vm2 => {
                            var m = vm2 as RDEMaterialViewModel;
                            materialList.Add(m);
                        });
                    }
                });

                var materialIds = materialList.Select(m => m.MaterialID).ToArray();

                // for the material id list we can extract all the subparts in one round trip...
                var subparts = service.GetMaterialParts(materialIds);
                // and associates as well. This means we only need one pass through the material list in order to
                // hook up subordinate records
                var associates = supportService.GetAssociates(TraitCategoryType.Material.ToString(), materialIds);
                // But we have to hook them back up to the right material.
                foreach (RDEMaterialViewModel m in materialList) {
                    var mlocal = m;
                    var selectedSubParts = subparts.Where(part => part.MaterialID == mlocal.MaterialID);
                    m.SubParts = new ObservableCollection<ViewModelBase>(selectedSubParts.Select(part => {
                        var vm = new MaterialPartViewModel(part) { Locked = mlocal.Locked };
                        vm.DataChanged += SubPartDataChanged;
                        return vm;
                    }));

                    RDEMaterialViewModel m1 = m;
                    var selectedAssociates = associates.Where(assoc => assoc.FromIntraCatID == m1.MaterialID || assoc.ToIntraCatID == m1.MaterialID);
                    m.Associates = new ObservableCollection<ViewModelBase>(selectedAssociates.Select(assoc => {
                        var vm = new AssociateViewModel(assoc);                        
                        vm.DataChanged += AssociateDataChanged;
                        return vm;
                    }));
                }

            }

            return root;
        }

        void AssociateDataChanged(ChangeableModelBase viewmodel) {
            var assoc = viewmodel as AssociateViewModel;
            if (assoc != null) {
                RegisterUniquePendingChange(new UpdateAssociateCommand(assoc.Model));
            }
        }

        void SubPartDataChanged(ChangeableModelBase viewmodel) {
            var subpart = viewmodel as MaterialPartViewModel;
            if (subpart != null) {
                RegisterUniquePendingChange(new UpdateMaterialPartCommand(subpart.Model));
            }
        }

        private RDESiteViewModel CreateSiteViewModel(RDESite site, bool addChangedHandler = true) {
            var supportService = new SupportService(User);
            var vm = new RDESiteViewModel(site) {
                Locked = !User.HasPermission(PermissionCategory.SPARC_SITE, PERMISSION_MASK.UPDATE) || _startLockMode
            };

            if (site.SiteID >= 0) {
                vm.Traits = supportService.GetTraits(TraitCategoryType.Site.ToString(), site.SiteID);
            }
            if (addChangedHandler) {
                vm.DataChanged += SiteViewModelDataChanged;
            }
            return vm;
        }

        private RDESiteVisitViewModel CreateSiteVisitViewModel(RDESiteVisit visit, RDESiteViewModel site) {
            var vm = new RDESiteVisitViewModel(visit);
            vm.DataChanged += SiteVisitViewModelDataChanged;
            vm.Site = site;
            vm.SiteID = site.SiteID;
            vm.Locked = !User.HasPermission(PermissionCategory.SPARC_SITEVISIT, PERMISSION_MASK.UPDATE) || _startLockMode;
            site.SiteVisits.Add(vm);
            return vm;
        }

        private ObservableCollection<ViewModelBase> CreateMaterialViewModels(IEnumerable<RDEMaterial> material, RDESiteVisitViewModel siteVisit) {
            var supportService = new SupportService(User);
            siteVisit.Material.Clear();
            return new ObservableCollection<ViewModelBase>(material.Select(m => {
                var vm = new RDEMaterialViewModel(m);
                vm.Traits = supportService.GetTraits(TraitCategoryType.Material.ToString(), vm.MaterialID);
                vm.Multimedia = supportService.GetMultimediaItems(TraitCategoryType.Material.ToString(), vm.MaterialID);
                vm.DataChanged += MaterialViewModelDataChanged;
                vm.SiteVisit = siteVisit;
                vm.SiteVisitID = siteVisit.SiteVisitID;
                siteVisit.Material.Add(vm);
                vm.Locked = !User.HasPermission(PermissionCategory.SPARC_MATERIAL, PERMISSION_MASK.UPDATE) || _startLockMode;
                return (ViewModelBase)vm;
            }));
        }

        void MaterialViewModelDataChanged(ChangeableModelBase viewmodel) {
            var material = viewmodel as RDEMaterialViewModel;
            if (material != null) {
                RegisterUniquePendingChange(new UpdateRDEMaterialCommand(material.Model) { SuccessAction = () => material.Touch() });
            }
        }

        void SiteVisitViewModelDataChanged(ChangeableModelBase viewmodel) {
            var siteVisit = viewmodel as RDESiteVisitViewModel;
            if (siteVisit != null) {
                RegisterUniquePendingChange(new UpdateRDESiteVisitCommand(siteVisit.Model) { SuccessAction = () => siteVisit.Touch() });
            }
        }

        void SiteViewModelDataChanged(ChangeableModelBase viewmodel) {
            var site = viewmodel as RDESiteViewModel;
            if (site != null) {
                RegisterUniquePendingChange(new UpdateRDESiteCommand(site.Model));
            }
        }

        private void GrpSitesAddNewClicked(object sender, RoutedEventArgs e) {
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
                    copyFrom = _siteTemplate;
                    break;                    
            }

            RDESite ret;
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
                        newTrait.TraitID = -1;
                        traits.Add(newTrait);
                    }
                }
            } else {
                ret = new RDESite();
            }

            ret.PosAreaType = (int)AreaType.Point;  // RDE only shows one set of coords, so its always going to be a point
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
                    copyFrom = _siteVisitTemplate;
                    break;
            }

            RDESiteVisit ret;
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
                        if (control != null) {
                            copyFrom.Traits = control.GetTraits();
                            copyFromAssociates = control.GetAssociates();
                            copyFromSubparts = control.GetSubParts();
                        }
                    }
                    break;
                case AutoFillMode.TemplateData:
                    copyFrom = _materialTemplate;
                    if (_materialTemplate != null) {
                        copyFromAssociates = _materialTemplate.Associates.Select(vm => {
                                                                                     var associateViewModel = vm as AssociateViewModel;
                                                                                     return associateViewModel != null ? associateViewModel.Model : null;
                                                                                 });
                        copyFromSubparts = _materialTemplate.SubParts.Select(vm => {
                                                                                 var materialPartViewModel = vm as MaterialPartViewModel;
                                                                                 return materialPartViewModel != null ? materialPartViewModel.Model : null;
                                                                             });
                    }
                    break;
            }

            RDEMaterial ret;
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
                    newTrait.TraitID = -1;
                    traits.Add(newTrait);
                }

                if (copyFromAssociates != null) {
                    foreach (Associate a in copyFromAssociates) {
                        var newAssoc = ReflectionUtils.Clone(a);
                        newAssoc.AssociateID = -1;
                        newAssoc.AssociateID = -1;
                        associates.Add(newAssoc);
                    }
                }

                if (copyFromSubparts != null) {
                    foreach (MaterialPart p in copyFromSubparts) {
                        var newSubpart = ReflectionUtils.Clone(p);
                        newSubpart.MaterialPartID = -1;
                        newSubpart.MaterialID = -1;
                        subparts.Add(newSubpart);
                    }
                }
            } else {
                ret = new RDEMaterial();
            }

            ret.Locked = false;

            return ret;
        }

        private void AddNewSite(bool createNewMaterial = true) {
            // First add the site
            List<Trait> traits;
            var site = CreateNewSite(out traits);
            var siteViewModel = new RDESiteViewModel(site);            
            siteViewModel.DataChanged += SiteViewModelDataChanged;

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

        private void GrpSiteVisitsAddNewClicked(object sender, RoutedEventArgs e) {
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

        private void GrpMaterialAddNewClicked(object sender, RoutedEventArgs e) {
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
            var siteVisit = new RDESiteVisitViewModel(sv) {Site = site, SiteID = site.SiteID};

            site.SiteVisits.Add(siteVisit);

            siteVisit.DataChanged +=SiteVisitViewModelDataChanged;
            RegisterPendingChange(new InsertRDESiteVisitCommand(siteVisit.Model, site.Model));
            RegisterPendingChange(new UpdateRDESiteVisitCommand(siteVisit.Model) {
                SuccessAction = () => siteVisit.Touch()
            });
            return siteVisit;
        }

        

        private RDEMaterialViewModel AddNewMaterial(RDESiteVisitViewModel siteVisit) {
            if (siteVisit != null) {

                // create the new material...
                List<Trait> traits;
                List<Associate> associates;
                List<MaterialPart> subparts;

                var material = CreateNewMaterial(out traits, out associates, out subparts);
                var materialViewModel = new RDEMaterialViewModel(material);

                RegisterPendingChange(new InsertRDEMaterialCommand(material, siteVisit.Model));
                RegisterUniquePendingChange(new UpdateRDEMaterialCommand(material) {
                    SuccessAction = () => {
                        materialViewModel.MaterialName = material.MaterialName;
                    }
                });

                if (traits != null && traits.Count > 0) {
                    foreach (Trait t in traits) {
                        materialViewModel.Traits.Add(t);
                        RegisterPendingChange(new UpdateTraitDatabaseCommand(t, materialViewModel));
                    }
                }

                if (associates != null && associates.Count > 0) {
                    foreach (Associate a in associates) {
                        var vm = new AssociateViewModel(a);
                        vm.DataChanged += AssociateDataChanged;
                        materialViewModel.Associates.Add(vm);
                        RegisterPendingChange(new InsertAssociateCommand(a, materialViewModel));
                    }
                }

                if (subparts != null && subparts.Count > 0) {
                    foreach (MaterialPart subpart in subparts) {
                        var vm = new MaterialPartViewModel(subpart);
                        vm.DataChanged +=SubPartDataChanged;
                        materialViewModel.SubParts.Add(vm);
                        RegisterPendingChange(new InsertMaterialPartCommand(subpart, materialViewModel));
                    }
                } else {
                    // Add one subpart...
                    var subpart = new MaterialPartViewModel(new MaterialPart()) {MaterialPartID = -1, PartName = "<New>"};
                    materialViewModel.SubParts.Add(subpart);
                    RegisterPendingChange(new InsertMaterialPartCommand(subpart.Model, materialViewModel));
                }
               
                materialViewModel.SiteVisit = siteVisit;
                materialViewModel.SiteVisitID = siteVisit.SiteVisitID;
                siteVisit.Material.Add(materialViewModel);
                materialViewModel.DataChanged +=MaterialViewModelDataChanged;

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

        private void MnuEditSiteClick(object sender, RoutedEventArgs e) {
            EditDetails(grpSites.SelectedItem, LookupType.Site);
        }

        private void MnuEditSiteVisitClick(object sender, RoutedEventArgs e) {
            EditDetails(grpSiteVisits.SelectedItem, LookupType.SiteVisit);
        }

        private void MnuEditMaterialClick(object sender, RoutedEventArgs e) {
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
            var target = e.Source as Control;

            e.CanExecute = target != null;
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
            var target = e.Source as Control;
            e.CanExecute = target != null;
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
            var target = e.Source as Control;

            e.CanExecute = target != null;
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
            var target = e.Source as Control;

            e.CanExecute = target != null;
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
            var target = e.Source as Control;

            e.CanExecute = target != null;
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
            var target = e.Source as Control;

            e.CanExecute = target != null;
        }

        private void UnlockAll() {
            grpSites.IsUnlocked = true;
            grpSiteVisits.IsUnlocked = true;
            grpMaterial.IsUnlocked = true;
        }

        private void MnuMoveToNewSiteClick(object sender, RoutedEventArgs e) {
            MoveMaterialToNewSite();
        }

        private void MnuMoveToNewSiteVisitClick(object sender, RoutedEventArgs e) {
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
                if (newParent != null) {
                    RegisterPendingChange(new MoveRDEMaterialCommand(mat.Model, newParent.Model));
                }
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
                if (newParent != null) {
                    RegisterPendingChange(new MoveRDEMaterialCommand(mat.Model, newParent.Model));
                }
            }

        }

        private void SetLatLongFormat(LatLongMode mode) {
            var siteControl = grpSites.Content as SiteRDEControl;
            if (siteControl != null) {
                siteControl.SetLatLongFormat(mode);

                mnuLLDD.IsChecked = mode == LatLongMode.DecimalDegrees;
                mnuLLDMS.IsChecked = mode == LatLongMode.DegreesMinutesSeconds;
                mnuLLDDM.IsChecked = mode == LatLongMode.DegreesDecimalMinutes;

                Config.SetUser(User, ConfigLatLongFormat, mode);
            }
        }

        private void MnuLldmsClick(object sender, RoutedEventArgs e) {
            SetLatLongFormat(LatLongMode.DegreesMinutesSeconds);
        }

        private void MnuLlddClick(object sender, RoutedEventArgs e) {
            SetLatLongFormat(LatLongMode.DecimalDegrees);
        }

        private void MnuLlddmClick(object sender, RoutedEventArgs e) {
            SetLatLongFormat(LatLongMode.DegreesDecimalMinutes);
        }

        private void MnuLockAtStartClick(object sender, RoutedEventArgs e) {
            SetStartingLockMode(true);
        }

        private void MnuUnlockAtStartClick(object sender, RoutedEventArgs e) {
            SetStartingLockMode(false);
        }

        private void SetStartingLockMode(bool lockAtStart) {
            mnuLockAtStart.IsChecked = lockAtStart;
            mnuUnlockAtStart.IsChecked = !lockAtStart;
            Config.SetUser(User, ConfigLockingMode, lockAtStart);
        }

        private void SetAutoNumber(bool autonumber) {
            mnuAutoNumber.IsChecked = autonumber;
            _autoNumber = autonumber;
            Config.SetUser(User, ConfigAutoNumber, autonumber);
        }

        private void SetAutoFillMode(AutoFillMode mode) {

            mnuAutoFillNone.IsChecked = mode == AutoFillMode.NoAutoFill;
            mnuAutoFillCopyCurrent.IsChecked = mode == AutoFillMode.CopyCurrentData;
            mnuAutoFillTemplate.IsChecked = mode == AutoFillMode.TemplateData;

            _autoFillMode = mode;

            Config.SetUser(User, ConfigAutofillMode, mode);
        }

        public enum AutoFillMode {
            NoAutoFill,
            CopyCurrentData,
            TemplateData
        }

        private void MnuAutoFillNoneClick(object sender, RoutedEventArgs e) {
            SetAutoFillMode(AutoFillMode.NoAutoFill);
        }

        private void MnuAutoFillCopyCurrentClick(object sender, RoutedEventArgs e) {
            SetAutoFillMode(AutoFillMode.CopyCurrentData);
        }

        private void MnuAutoFillTemplateClick(object sender, RoutedEventArgs e) {
            SetAutoFillMode(AutoFillMode.TemplateData);
        }

        private void MnuSetSiteTemplateClick(object sender, RoutedEventArgs e) {
            ChooseSiteTemplate();
        }

        private void MnuSetSiteVisitTemplateClick(object sender, RoutedEventArgs e) {
            ChooseSiteVisitTemplate();
        }

        private void MnuSetMaterialTemplateClick(object sender, RoutedEventArgs e) {
            ChooseMaterialTemplate();
        }

        private RDESiteViewModel _siteTemplate;
        private RDESiteVisitViewModel _siteVisitTemplate;
        private RDEMaterialViewModel _materialTemplate;

        private int? ChooseTemplateId(Func<List<SiteExplorerNode>> func) {
            var pickList = new PickListWindow(User, "Select template", () => {                
                var templates = func();
                return templates.Select(m => new SiteExplorerNodeViewModel(m));
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
            var templateId = ChooseTemplateId(service.GetSiteTemplates);
            if (templateId != null) {
                LoadSiteTemplate(templateId.Value);
            }
        }

        private void ChooseSiteVisitTemplate() {
            var service = new MaterialService(User);
            var templateId = ChooseTemplateId(service.GetSiteVisitTemplates);
            if (templateId != null) {
                LoadSiteVisitTemplate(templateId.Value);
            }
        }

        private void ChooseMaterialTemplate() {            
            var templateId = ChooseTemplateId(new MaterialService(User).GetMaterialTemplates);
            if (templateId != null) {
                LoadMaterialTemplate(templateId.Value);
            }
        }

        private void LoadSiteTemplate(int templateId) {
            var service = new MaterialService(User);
            var list = service.GetRDESites(templateId);
            if (list != null && list.Count > 0) {
                _siteTemplate = CreateSiteViewModel(list[0], false);
            } else {
                _siteTemplate = null;
            }

            if (_siteTemplate != null) {
                mnuSetSiteTemplate.Header = String.Format("Set _Site Template ({0}) ...", _siteTemplate.SiteName);
                Config.SetProfile(User, ConfigSiteTemplateId, templateId);
            } else {
                mnuSetSiteTemplate.Header = "Set _Site Template...";
                Config.SetProfile(User, ConfigSiteTemplateId, -1);
            }

        }

        private void LoadSiteVisitTemplate(int templateId) {
            var service = new MaterialService(User);
            var list = service.GetRDESiteVisits(new[] { templateId });
            if (list != null && list.Count > 0) {
                _siteVisitTemplate = new RDESiteVisitViewModel(list[0]);
                
            }

            if (_siteVisitTemplate != null) {
                mnuSetSiteVisitTemplate.Header = String.Format("Set Site _Visit Template ({0}) ...", _siteVisitTemplate.VisitName);
                Config.SetProfile(User, ConfigSitevisitTemplateId, templateId);
            } else {
                mnuSetSiteVisitTemplate.Header = "Set Site _Visit Template...";
                Config.SetProfile(User, ConfigSitevisitTemplateId, -1);
            }
            
        }

        private void LoadMaterialTemplate(int templateId) {
            var supportService = new SupportService(User);
            var service = new MaterialService(User);

            var list = service.GetRDEMaterial(new[] { templateId });
            if (list != null && list.Count > 0) {
                _materialTemplate = new RDEMaterialViewModel(list[0]);
                _materialTemplate.Traits = supportService.GetTraits(TraitCategoryType.Material.ToString(), _materialTemplate.MaterialID);
                // need to get associates and subparts...
                var subparts = service.GetMaterialParts(templateId);
                var associates = supportService.GetAssociates(TraitCategoryType.Material.ToString(), templateId);

                foreach (Associate assoc in associates) {
                    var vm = new AssociateViewModel(assoc);
                    _materialTemplate.Associates.Add(vm);
                }

                foreach (MaterialPart part in subparts) {
                    var vm = new MaterialPartViewModel(part);
                    _materialTemplate.SubParts.Add(vm);
                }

            }

            if (_materialTemplate != null) {
                mnuSetMaterialTemplate.Header = String.Format("Set _Material Template ({0}) ...", _materialTemplate.MaterialName);
                Config.SetProfile(User, ConfigMaterialTemplateId, templateId);
            } else {
                mnuSetMaterialTemplate.Header = "Set _Material Template...";
                Config.SetProfile(User, ConfigMaterialTemplateId, -1);
            }
            
        }

        private void MnuAutoNumberClick(object sender, RoutedEventArgs e) {
            SetAutoNumber(mnuAutoNumber.IsChecked);
        }

        protected SiteExplorerNodeViewModel ParentNode { get; private set; }

        protected MaterialExplorer Explorer { get; private set; }

        private void MnuAddToLabelSetClick(object sender, RoutedEventArgs e) {
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
                var pinnable = new PinnableObject(MaterialPlugin.MATERIAL_PLUGIN_NAME, LookupType.Material, m.MaterialID);
                var target = PluginManager.Instance.FindAdaptorForPinnable<ILabelSetItemTarget>(pinnable);
                if (target != null) {
                    target.AddItemToLabelSet(pinnable);
                }
            }

        }

    }
    
}
