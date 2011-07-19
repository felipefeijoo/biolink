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
using System.Collections.ObjectModel;
using BioLink.Client.Extensibility;
using BioLink.Client.Utilities;
using BioLink.Data;
using BioLink.Data.Model;


namespace BioLink.Client.Tools {
    /// <summary>
    /// Interaction logic for TraitTypesControl.xaml
    /// </summary>
    public partial class TraitTypesControl : AdministrationComponent {

        private List<TypeData> _typeData;
        private List<string> _categories;
        private ObservableCollection<TypeDataViewModel> _currentCategoryModel;
        private string _type;

        private KeyValuePair<string, LookupType>[] _typeMappings = new KeyValuePair<string, LookupType>[] {
            new KeyValuePair<string, LookupType>("MaterialID", LookupType.Material),
            new KeyValuePair<string, LookupType>("BiotaID", LookupType.Taxon),
            new KeyValuePair<string, LookupType>("SiteID", LookupType.Site),
            new KeyValuePair<string, LookupType>("SiteVisitID", LookupType.SiteVisit),
            new KeyValuePair<string, LookupType>("JournalID", LookupType.Journal),
            new KeyValuePair<string, LookupType>("ReferenceID", LookupType.Reference),
            new KeyValuePair<string, LookupType>("TrapID", LookupType.Trap)
        };

        public TraitTypesControl(User user, string type)  : base(user) {
            InitializeComponent();
            cmbCategory.SelectionChanged += new SelectionChangedEventHandler(cmbCategory_SelectionChanged);
            lstTypeData.SelectionChanged += new SelectionChangedEventHandler(lstTraitTypes_SelectionChanged);
            lstTypeData.MouseRightButtonUp += new MouseButtonEventHandler(lstTraitTypes_MouseRightButtonUp);
            lvwValues.SelectionChanged += new SelectionChangedEventHandler(lvwValues_SelectionChanged);
            lvwValues.MouseRightButtonUp += new MouseButtonEventHandler(lvwValues_MouseRightButtonUp);
            _type = type;
            if (_type == "note") {
                lblRTF.Content = "Note text:";
            }

        }

        public override void Populate() {
            _typeData = Service.GetTypeInfo(_type);
            var map = new Dictionary<string, string>();
            foreach (TypeData info in _typeData) {
                if (!map.ContainsKey(info.Category)) {
                    map[info.Category] = info.Category;
                }
            }
            _categories = new List<string>(map.Values);
            cmbCategory.ItemsSource = _categories;
            cmbCategory.SelectedItem = _categories[0];

            IsPopulated = true;
        }

        void lstTraitTypes_MouseRightButtonUp(object sender, MouseButtonEventArgs e) {
            var selected = lstTypeData.SelectedItem as TypeDataViewModel;
            if (selected != null) {
                ContextMenuBuilder builder = new ContextMenuBuilder(null);
                builder.New("Rename").Handler(() => { RenameTraitType(selected); }).End();
                builder.Separator();
                builder.New("Add new {0} type", _type).Handler(() => { AddNewTypeData(); }).End();
                builder.Separator();
                builder.New("Delete {0} type", _type).Handler(() => { DeleteTypeData(selected); }).End();                
                lstTypeData.ContextMenu = builder.ContextMenu;
            }
            
        }

        void lvwValues_MouseRightButtonUp(object sender, MouseButtonEventArgs e) {

            var selected = lvwValues.SelectedItem as TypeDataOwnerInfo;
            if (selected != null) {
                ContextMenuBuilder builder = new ContextMenuBuilder(null);
                builder.New("Edit owner").Handler(() => { EditOwner(selected); }).End();
                builder.New("Delete {0} from owner", _type).Handler(() => { DeleteTypeDataFromOwner(selected); }).End();

                lvwValues.ContextMenu = builder.ContextMenu;
            }
        }

        private void EditOwner(TypeDataOwnerInfo ownerInfo) {
            var ep = EntryPoint.Parse(ownerInfo.EntryPoint);

            LookupType? type = null;
            int objectId = -1; 

            foreach (KeyValuePair<string, LookupType> mapping in _typeMappings) {
                if (ep.HasParameter(mapping.Key)) {
                    type = mapping.Value;                    
                    objectId = Int32.Parse(ep[mapping.Key]);
                    break;
                }
            }

            if (type.HasValue && objectId >= 0) {
                PluginManager.Instance.EditLookupObject(type.Value, objectId);
            } else {
                MessageBox.Show(String.Format("Can't edit object! Owner call: {0} Owner Name: {1}, OwnerID={2}, EntryPoint={3}", ownerInfo.OwnerCall, ownerInfo.OwnerName, ownerInfo.OwnerID, ownerInfo.EntryPoint));
            }
        }

        private void DeleteTypeDataFromOwner(TypeDataOwnerInfo ownerInfo) {
            DatabaseCommand action = null;
            switch (_type) {
                case "trait":
                    action = new DeleteTraitFromOwnerCommand(ownerInfo.ObjectID.Value);
                    break;
                case "note":
                    action = new DeleteNoteFromOwnerCommand(ownerInfo.ObjectID.Value);
                    break;
                default:
                    throw new NotImplementedException("Deleting type data for type: " + _type + " is currently unsupported!");
            }

            if (action != null) {
                var model = lvwValues.ItemsSource as ObservableCollection<TypeDataOwnerInfo>;
                if (model != null) {
                    model.Remove(ownerInfo);
                    RegisterPendingChange(action);
                }
            }
        }

        void lvwValues_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            txtComment.DataContext = lvwValues.SelectedItem;
        }

        void lstTraitTypes_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (IsPopulated) {
                var info = lstTypeData.SelectedItem as TypeDataViewModel;
                if (info != null) {
                    lvwValues.ItemsSource = null;

                    JobExecutor.QueueJob(() => {
                        lock (lvwValues) {
                            try {
                                this.InvokeIfRequired(() => {
                                    lvwValues.Cursor = Cursors.Wait;
                                });

                                List<TypeDataOwnerInfo> list = null;
                                if (_type == "trait") {
                                    list = new List<TypeDataOwnerInfo>(Service.GetTraitOwnerInfo(info.ID));
                                } else if (_type == "note") {
                                    list = new List<TypeDataOwnerInfo>(Service.GetNoteOwnerInfo(info.ID));
                                } else {
                                    throw new NotImplementedException();
                                }

                                if (list != null) {
                                    this.InvokeIfRequired(() => {
                                        lvwValues.ItemsSource = new ObservableCollection<TypeDataOwnerInfo>(list);
                                    });
                                }
                            } finally {
                                this.InvokeIfRequired(() => {
                                    lvwValues.Cursor = Cursors.Arrow;
                                });
                            }
                        }
                    });
                }
            }
        }

        void cmbCategory_SelectionChanged(object sender, SelectionChangedEventArgs e) {            
            string category = cmbCategory.SelectedItem as string;
            if (category != null) {
                var list = _typeData.FindAll((info) => info.Category.Equals(category));
                _currentCategoryModel = new ObservableCollection<TypeDataViewModel>(list.Select((m) => {
                    var vm = new TypeDataViewModel(m);
                    return vm;
                }));
                lstTypeData.ItemsSource = _currentCategoryModel;
            }            
        }

        private void DeleteTypeData(TypeDataViewModel selected) {

            if (selected != null) {
                _currentCategoryModel.Remove(selected);
                _typeData.Remove(selected.Model);
                RegisterPendingChange(new DeleteTypeDataCommand(selected.Model, _type));
            }

        }

        private void RenameTraitType(TypeDataViewModel selected) {
            selected.IsRenaming = true;
        }

        private void AddNewTypeData() {
            var model = new TypeData();
            model.Category = cmbCategory.SelectedItem as string;
            model.Description = string.Format("<New {0} type>", _type);
            model.ID = -1;
            var viewModel = new TypeDataViewModel(model);
            _currentCategoryModel.Add(viewModel);
            lstTypeData.SelectedItem = viewModel;

            lstTypeData.ScrollIntoView(viewModel);

            viewModel.IsRenaming = true;
            _typeData.Add(model);
            RegisterPendingChange(new InsertTypeDataCommand(model, _type));            
        }

        private void btnAddNew_Click(object sender, RoutedEventArgs e) {
            AddNewTypeData();
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e) {
            var selected = lstTypeData.SelectedItem as TypeDataViewModel;
            DeleteTypeData(selected);
        }

        private void txtTraitType_EditingComplete(object sender, string text) {
            var selected = lstTypeData.SelectedItem as TypeDataViewModel;
            if (selected != null) {
                selected.Description = text;
                if (selected.ID >= 0) {
                    RegisterUniquePendingChange(new UpdateTypeDataCommand(selected.Model, _type));
                }
            }
        }
    }

    

    public class TypeDataViewModel : GenericViewModelBase<TypeData> {

        public TypeDataViewModel(TypeData model) : base(model, ()=>model.ID) { }

        public int ID {
            get { return Model.ID; }
            set { SetProperty(() => Model.ID, value); }
        }

        public string Description {
            get { return Model.Description; }
            set { SetProperty(() => Model.Description, value); }
        }

        public string Category {
            get { return Model.Category; }
            set { SetProperty(() => Model.Category, value); }
        }

    }

    public abstract class TypeDataCommand : GenericDatabaseCommand<TypeData> {
        public TypeDataCommand(TypeData model, string type) : base(model) {
            this.Type = type;
        }

        protected string Type { get; private set; }

    }

    public class UpdateTypeDataCommand : TypeDataCommand {

        public UpdateTypeDataCommand(TypeData model, string type) : base(model, type) { }

        protected override void ProcessImpl(User user) {
            var service = new SupportService(user);
            service.UpdateTypeData(Type, Model.ID, Model.Description);
        }


        protected override void BindPermissions(PermissionBuilder required) {
            required.None();
        }
    }

    public class InsertTypeDataCommand : TypeDataCommand {

        public InsertTypeDataCommand(TypeData model, string type) : base(model, type) { }

        protected override void ProcessImpl(User user) {
            var service = new SupportService(user);
            Model.ID = service.InsertTypeData(Type, Model.Category, Model.Description);
        }


        protected override void BindPermissions(PermissionBuilder required) {
            required.None();
        }
    }

    public class DeleteTypeDataCommand : TypeDataCommand {

        public DeleteTypeDataCommand(TypeData model, string type) : base(model, type) { }

        protected override void ProcessImpl(User user) {
            var service = new SupportService(user);
            if (Model.ID >= 0) {
                service.DeleteTypeData(Type, Model.ID);
            }
        }

        protected override void BindPermissions(PermissionBuilder required) {
            required.None();
        }
    }

    public class DeleteTraitFromOwnerCommand : DatabaseCommand {

        public DeleteTraitFromOwnerCommand(int traitID) {
            this.TraitID = traitID;
        }

        protected override void ProcessImpl(User user) {
            var service = new SupportService(user);
            service.DeleteTrait(TraitID);
        }

        protected int TraitID { get; private set; }

        public override string ToString() {
            return string.Format("Delete Trait (TraitID={0})", TraitID);
        }

        protected override void BindPermissions(PermissionBuilder required) {
            required.None();
        }
    }

    public class DeleteNoteFromOwnerCommand : DatabaseCommand {

        public DeleteNoteFromOwnerCommand(int noteID) {
            this.NoteID = noteID;
        }

        protected override void ProcessImpl(User user) {
            var service = new SupportService(user);
            service.DeleteNote(NoteID);
        }

        protected int NoteID { get; private set; }

        public override string ToString() {
            return string.Format("Delete Note (NoteID={0})", NoteID);
        }

        protected override void BindPermissions(PermissionBuilder required) {
            required.None();
        }
    }


}
