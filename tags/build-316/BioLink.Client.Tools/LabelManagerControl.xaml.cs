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

namespace BioLink.Client.Tools {
    /// <summary>
    /// Interaction logic for LabelManagerControl.xaml
    /// </summary>
    public partial class LabelManagerControl : OneToManyDetailControl {

        private List<LabelSetItem> _items;

        public LabelManagerControl(User user) : base(user, "LabelManager") {
            InitializeComponent();

            this.DataContextChanged += new DependencyPropertyChangedEventHandler(LabelManagerControl_DataContextChanged);
        }

        void LabelManagerControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            var selected = e.NewValue as LabelSetViewModel;
            if (selected != null) {
                var itemlist = _items.FindAll((item) => {
                    return item.SetID == selected.ID;
                });
                var lvwModel = new ObservableCollection<LabelSetItemViewModel>(itemlist.Select((m) => {
                    return new LabelSetItemViewModel(m);
                }));

                lvw.ItemsSource = lvwModel;
            }
        }

        public override FrameworkElement FirstControl {
            get { return lvw;  }
        }

        public override List<ViewModelBase> LoadModel() {
            var service = new SupportService(User);
            var list = service.GetLabelSets();
            var sets = new List<ViewModelBase>(list.Select((model) => {
                return new LabelSetViewModel(model);
            }));

            _items = service.GetLabelSetItems();

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

    }

}
