﻿using System;
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

namespace BioLink.Client.Tools {
    /// <summary>
    /// Interaction logic for LabelManagerControl.xaml
    /// </summary>
    public partial class LabelManagerControl : OneToManyDetailControl {

        private List<LabelSetItem> _allItems;
        private ObservableCollection<LabelSetItemViewModel> _itemModel;

        public LabelManagerControl(User user) : base(user, "LabelManager") {
            InitializeComponent();
            this.DataContextChanged += new DependencyPropertyChangedEventHandler(LabelManagerControl_DataContextChanged);

            lvw.MouseRightButtonUp += new MouseButtonEventHandler(lvw_MouseRightButtonUp);
            lvw.AllowDrop = true;
            lvw.PreviewDragOver += new DragEventHandler(lvw_PreviewDragOver);
            lvw.Drop += new DragEventHandler(lvw_Drop);

            var service = new SupportService(User);

            var fields = service.GetFieldMappings();
            lstFields.ItemsSource = fields;

            CollectionView myView = (CollectionView)CollectionViewSource.GetDefaultView(lstFields.ItemsSource);
            PropertyGroupDescription groupDescription = new PropertyGroupDescription("Category");
            myView.GroupDescriptions.Add(groupDescription);

        }

        private void ApplyFilter(string text) {

            ListCollectionView dataView = CollectionViewSource.GetDefaultView(lstFields.ItemsSource) as ListCollectionView;

            if (String.IsNullOrEmpty(text)) {
                dataView.Filter = null;
                return;
            }

            text = text.ToLower();
            dataView.Filter = (obj) => {
                var field = obj as FieldDescriptor;

                if (field != null) {
                    if (field.DisplayName.ToLower().Contains(text)) {
                        return true;
                    }

                    if (field.FieldName.ToLower().Contains(text)) {
                        return true;
                    }

                    if (field.TableName.ToLower().Contains(text)) {
                        return true;
                    }

                    return false;
                }
                return true;
            };

            dataView.Refresh();
        }


        void lvw_Drop(object sender, DragEventArgs e) {

            using (new OverrideCursor(Cursors.Wait)) {
                var pinnable = e.Data.GetData(PinnableObject.DRAG_FORMAT_NAME) as PinnableObject;
                if (pinnable != null) {
                    var maxOrder = _itemModel.Max((item) => {
                        return item.PrintOrder;
                    });

                    var newItem = new LabelSetItem { ItemID = pinnable.ObjectID, ItemType = pinnable.LookupType.ToString(), SetID = CurrentItemSetID, PrintOrder = maxOrder + 1, NumCopies = 1 };
                    var service = new MaterialService(User);
                    Site site = null;
                    SiteVisit visit = null;
                    Material material = null;
                    switch (pinnable.LookupType) {
                        case LookupType.Material:
                            material = service.GetMaterial(pinnable.ObjectID);
                            visit = service.GetSiteVisit(material.SiteVisitID);
                            site = service.GetSite(material.SiteID);
                            break;
                        case LookupType.SiteVisit:
                            visit = service.GetSiteVisit(pinnable.ObjectID);
                            site = service.GetSite(visit.SiteID);
                            break;
                        case LookupType.Site:
                            site = service.GetSite(pinnable.ObjectID);
                            break;
                    }

                    if (material != null) {
                        newItem.TaxaFullName = material.TaxaDesc;
                        newItem.AccessionNo = material.AccessionNumber;
                    }

                    if (visit != null) {
                        newItem.Collectors = visit.Collector;
                        newItem.DateType = visit.DateType;
                        newItem.CasualDate = visit.CasualTime;
                        newItem.StartDate = visit.DateStart;
                        newItem.EndDate = visit.DateEnd;
                    }

                    if (site != null) {
                        newItem.Region = site.PoliticalRegion;
                        newItem.Local = site.Locality;
                        newItem.LocalType = site.LocalityType;
                        newItem.Lat = site.PosY1;
                        newItem.Long = site.PosX1;
                        newItem.Lat2 = site.PosY2;
                        newItem.Long2 = site.PosX2;
                    }

                    var vm = new LabelSetItemViewModel(newItem);

                    vm.DataChanged += new DataChangedHandler((viewModel) => {
                        Host.RegisterUniquePendingChange(new UpdateLabelSetItemAction(newItem));
                    });

                    Host.RegisterUniquePendingChange(new InsertLabelSetItemAction(newItem));
                    _itemModel.Add(vm);
                }
            }
        }

        void lvw_PreviewDragOver(object sender, DragEventArgs e) {
            var pinnable = e.Data.GetData(PinnableObject.DRAG_FORMAT_NAME) as PinnableObject;
            e.Effects = DragDropEffects.None;
            if (pinnable != null) {
                switch (pinnable.LookupType) {
                    case LookupType.Material:
                    case LookupType.SiteVisit:
                    case LookupType.Site:
                        e.Effects = DragDropEffects.Link;
                        break;
                }
            }
            e.Handled = true;

        }

        void lvw_MouseRightButtonUp(object sender, MouseButtonEventArgs e) {
            var selected = lvw.SelectedItem as LabelSetItemViewModel;
            if (selected != null) {
                var builder = new ContextMenuBuilder(null);

//                builder.New("Select _material...").Handler(() => { SelectMaterial(); }).End();
                builder.New("Load material by _User ID").Handler(() => { SelectMaterialByUser(); }).End();
                builder.Separator();
                builder.New("Remove item from list").Handler(() => DeleteSelected() ).End();
                builder.Separator();
                if (selected.ItemType.Equals("site", StringComparison.CurrentCultureIgnoreCase)) {
                    builder.New("Edit _Site").Handler(() => EditSite()).End();
                } else if (selected.ItemType.Equals("sitevisit", StringComparison.CurrentCultureIgnoreCase)) {
                    builder.New("Edit _Site").Handler(() => EditSite()).End();
                    builder.New("Edit Site _Visit").Handler(() => EditSiteVisit()).End();
                } else if (selected.ItemType.Equals("material", StringComparison.CurrentCultureIgnoreCase)) {
                    builder.New("Edit _Site").Handler(() => EditSite()).End();
                    builder.New("Edit Site _Visit").Handler(() => EditSiteVisit()).End();
                    builder.New("Edit _Material").Handler(() => EditMaterial()).End();
                }

                builder.Separator();
                builder.New("E_xport Label Set").Handler(() => ExportLabelSet()).End();

                lvw.ContextMenu = builder.ContextMenu;
            }
        
        }
        
        private void ExportLabelSet() {
            throw new NotImplementedException();
        }

        private void EditSite() {
            var selected = lvw.SelectedItem as LabelSetItemViewModel;
            if (selected != null) {
                PluginManager.Instance.EditLookupObject(LookupType.Site, selected.SiteID);
            }
        }

        private void EditSiteVisit() {
            var selected = lvw.SelectedItem as LabelSetItemViewModel;
            if (selected != null) {
                PluginManager.Instance.EditLookupObject(LookupType.SiteVisit, selected.VisitID);
            }
        }

        private void EditMaterial() {
            var selected = lvw.SelectedItem as LabelSetItemViewModel;
            if (selected != null) {
                PluginManager.Instance.EditLookupObject(LookupType.Material, selected.MaterialID);
            }
        }

        private void SelectMaterialByUser() {
            var frm = new MaterialByUserWindow((items) => {
                               
                foreach (LabelSetItem item in items) {
                    var vm = new LabelSetItemViewModel(item);
                    _itemModel.Add(vm);
                    item.SetID = CurrentItemSetID;
                    item.NumCopies = 1;
                    Host.RegisterUniquePendingChange(new InsertLabelSetItemAction(item));
                }

                ReorderItems();

            });
            frm.Owner = this.FindParentWindow();
            frm.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            frm.ShowDialog().ValueOrFalse();
        }

        void LabelManagerControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            var selected = e.NewValue as LabelSetViewModel;
            if (selected != null) {
                CurrentItemSetID = selected.ID;
                var itemlist = _allItems.FindAll((item) => {
                    return item.SetID == selected.ID;
                });
                var list = new List<LabelSetItemViewModel>(itemlist.Select((m) => {
                    return new LabelSetItemViewModel(m);
                }));

                list.Sort(new Comparison<LabelSetItemViewModel>((vm1, vm2) => {
                    return vm1.PrintOrder - vm2.PrintOrder;
                }));
                
                _itemModel = new ObservableCollection<LabelSetItemViewModel>(list);

                // ReorderItems();

                foreach (LabelSetItemViewModel item in _itemModel) {
                    item.DataChanged += new DataChangedHandler((vm) => {
                        Host.RegisterUniquePendingChange(new UpdateLabelSetItemAction((vm as LabelSetItemViewModel).Model));
                    });
                }

                lvw.ItemsSource = _itemModel;
            }
        }

        private int CurrentItemSetID { get; set; }

        public override FrameworkElement FirstControl {
            get { return lvw;  }
        }

        public override List<ViewModelBase> LoadModel() {
            var service = new SupportService(User);
            var list = service.GetLabelSets();
            var sets = new List<ViewModelBase>(list.Select((model) => {
                return new LabelSetViewModel(model);
            }));

            _allItems = service.GetLabelSetItems();

            return sets;
        }

        public override DatabaseAction PrepareDeleteAction(ViewModelBase viewModel) {
            return new DeleteLabelSetAction((viewModel as LabelSetViewModel).Model);
        }

        public override DatabaseAction PrepareUpdateAction(ViewModelBase viewModel) {
            return new UpdateLabelSetAction((viewModel as LabelSetViewModel).Model);
        }

        public override ViewModelBase AddNewItem(out DatabaseAction addAction) {
            var model = new LabelSet { Name = "New set" };
            addAction = new InsertLabelSetAction(model);
            return new LabelSetViewModel(model);
        }

        private void btnUp_Click(object sender, RoutedEventArgs e) {
            MoveSelectedUp();
        }

        private void btnDown_Click(object sender, RoutedEventArgs e) {
            MoveSelectedDown();
        }

        private void MoveSelectedUp() {
            var selected = lvw.SelectedItem as LabelSetItemViewModel;
            if (selected != null) {
                var index = _itemModel.IndexOf(selected);
                if (index > 0) {
                    _itemModel.Move(index, index - 1);
                    ReorderItems();
                }
            }
        }

        private void ReorderItems() {
            for (int i = 0; i < _itemModel.Count; ++i) {
                _itemModel[i].PrintOrder = i;
            }
        }

        private void MoveSelectedDown() {
            var selected = lvw.SelectedItem as LabelSetItemViewModel;
            if (selected != null) {
                var index = _itemModel.IndexOf(selected);
                if (index < _itemModel.Count - 1) {
                    _itemModel.Move(index, index + 1);
                    ReorderItems();
                }
            }

        }

        private void btnDelete_Click(object sender, RoutedEventArgs e) {
            DeleteSelected();
        }

        private void DeleteSelected() {
            var selected = lvw.SelectedItem as LabelSetItemViewModel;
            if (selected != null) {
                selected.IsDeleted = true;
                _itemModel.Remove(selected);
                Host.RegisterUniquePendingChange(new DeleteLabelSetItemAction(selected.Model));
            }
        }

        private void btnSelectField_Click(object sender, RoutedEventArgs e) {
            SelectField();
        }

        private void btnUnselectField_Click(object sender, RoutedEventArgs e) {
            UnselectField();
        }

        private void btnUnselectAll_Click(object sender, RoutedEventArgs e) {
            UnselectAll();
        }

        private void SelectField() {
        }

        private void UnselectField() {
        }

        private void UnselectAll() {
        }

        private void txtFilter_TypingPaused(string text) {
            ApplyFilter(text);
        }

    }

    public class LabelSetViewModel : GenericViewModelBase<LabelSet> {

        public LabelSetViewModel(LabelSet model) : base(model, () => model.ID) { }

        public override string DisplayLabel {
            get { return string.Format("{0}", Name); }
        }

        public int ID {
            get { return Model.ID; }
            set { SetProperty(() => Model.ID, value); }
        }

        public string Name {
            get { return Model.Name; }
            set { SetProperty(() => Model.Name, value); }
        }

        public string Delimited {
            get { return Model.Delimited; }
            set { SetProperty(() => Model.Delimited, value); }
        }

    }

    public class InsertLabelSetItemAction : GenericDatabaseAction<LabelSetItem> {

        public InsertLabelSetItemAction(LabelSetItem model) : base(model) { }

        protected override void ProcessImpl(User user) {
            var service = new SupportService(user);
            Model.LabelItemID = service.InsertLabelSetItem(Model);
        }
    }

    public class UpdateLabelSetItemAction : GenericDatabaseAction<LabelSetItem> {

        public UpdateLabelSetItemAction(LabelSetItem model) : base(model) { }

        protected override void ProcessImpl(User user) {
            var service = new SupportService(user);
            service.UpdateLabelSetItem(Model);
        }
    }

    public class DeleteLabelSetItemAction : GenericDatabaseAction<LabelSetItem> {

        public DeleteLabelSetItemAction(LabelSetItem model) : base(model) { }

        protected override void ProcessImpl(User user) {
            var service = new SupportService(user);
            service.DeleteLabelSetItem(Model.LabelItemID);
        }
    }

    public class DeleteLabelSetAction: GenericDatabaseAction<LabelSet> {

        public DeleteLabelSetAction(LabelSet model) : base(model) { }

        protected override void ProcessImpl(User user) {
            var service = new SupportService(user);
            service.DeleteLabelSet(Model.ID);
        }
    }

    public class InsertLabelSetAction : GenericDatabaseAction<LabelSet> {

        public InsertLabelSetAction(LabelSet model) : base(model) { }

        protected override void ProcessImpl(User user) {
            var service = new SupportService(user);
            Model.ID = service.InsertLabelSet(Model);
        }
    }

    public class UpdateLabelSetAction : GenericDatabaseAction<LabelSet> {
        public UpdateLabelSetAction(LabelSet model) : base(model) { }

        protected override void ProcessImpl(User user) {
            var service = new SupportService(user);
            service.UpdateLabelSet(Model);
        }
    }

    public class LabelSetItemViewModel : GenericViewModelBase<LabelSetItem> {

        public LabelSetItemViewModel(LabelSetItem model) : base(model, () => model.LabelItemID) { }

        public int SetID {
            get { return Model.SetID; }
            set { SetProperty(() => Model.SetID, value); }
        }

        public int LabelItemID {
            get { return Model.LabelItemID; }
            set { SetProperty(() => Model.LabelItemID, value); }
        }

        public int ItemID {
            get { return Model.ItemID; }
            set { SetProperty(() => Model.ItemID, value); }
        }

        public string ItemType {
            get { return Model.ItemType; }
            set { SetProperty(() => Model.ItemType, value); }
        }

        public int SiteID {
            get { return Model.SiteID; }
            set { SetProperty(() => Model.SiteID, value); }
        }

        public int VisitID {
            get { return Model.VisitID; }
            set { SetProperty(() => Model.VisitID, value); }
        }

        public int MaterialID {
            get { return Model.MaterialID; }
            set { SetProperty(() => Model.MaterialID, value); }
        }

        public string Region {
            get { return Model.Region; }
            set { SetProperty(() => Model.Region, value); }
        }

        public int LocalType {
            get { return Model.LocalType; }
            set { SetProperty(() => Model.LocalType, value); }
        }

        public string Local {
            get { return Model.Local; }
            set { SetProperty(() => Model.Local, value); }
        }

        public string DistanceFromPlace {
            get { return Model.DistanceFromPlace; }
            set { SetProperty(() => Model.DistanceFromPlace, value); }
        }

        public string DirFromPlace {
            get { return Model.DirFromPlace; }
            set { SetProperty(() => Model.DirFromPlace, value); }
        }

        public int AreaType {
            get { return Model.AreaType; }
            set { SetProperty(() => Model.AreaType, value); }
        }

        public double? Long {
            get { return Model.Long; }
            set { SetProperty(() => Model.Long, value); }
        }

        public double? Lat {
            get { return Model.Lat; }
            set { SetProperty(() => Model.Lat, value); }
        }

        public double? Long2 {
            get { return Model.Long2; }
            set { SetProperty(() => Model.Long2, value); }
        }

        public double? Lat2 {
            get { return Model.Lat2; }
            set { SetProperty(() => Model.Lat2, value); }
        }

        public string Collectors {
            get { return Model.Collectors; }
            set { SetProperty(() => Model.Collectors, value); }
        }

        public int DateType {
            get { return Model.DateType; }
            set { SetProperty(() => Model.DateType, value); }
        }

        public int? StartDate {
            get { return Model.StartDate; }
            set { SetProperty(() => Model.StartDate, value); }
        }

        public int? EndDate {
            get { return Model.EndDate; }
            set { SetProperty(() => Model.EndDate, value); }
        }

        public string CasualDate {
            get { return Model.CasualDate; }
            set { SetProperty(() => Model.CasualDate, value); }
        }

        public string AccessionNo {
            get { return Model.AccessionNo; }
            set { SetProperty(() => Model.AccessionNo, value); }
        }

        public string TaxaFullName {
            get { return Model.TaxaFullName; }
            set { SetProperty(() => Model.TaxaFullName, value); }
        }

        public int PrintOrder {
            get { return Model.PrintOrder; }
            set { SetProperty(() => Model.PrintOrder, value); }
        }

        public int NumCopies {
            get { return Model.NumCopies; }
            set { SetProperty(() => Model.NumCopies, value); }
        }

        public string Position {
            get {
                var sb = new StringBuilder();
                if (Lat.HasValue && Long.HasValue) {
                    sb.AppendFormat("{0}, {1}", Lat.Value, Long.Value);
                    if (Lat2.HasValue && Long2.HasValue) {
                        sb.AppendFormat(" - {0}, {1}", Lat2.Value, Long2.Value);
                    }
                }

                if (sb.Length == 0) {
                    sb.Append("No position");
                }


                return sb.ToString();

            }
        }

        public string DateStr {
            get { return DateUtils.FormatDates(DateType, StartDate, EndDate, CasualDate); }
        }

    }

}
