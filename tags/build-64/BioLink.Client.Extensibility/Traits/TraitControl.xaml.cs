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
using BioLink.Data;
using BioLink.Data.Model;
using System.Collections.ObjectModel;

namespace BioLink.Client.Extensibility {
    /// <summary>
    /// Interaction logic for TraitControl.xaml
    /// </summary>
    public partial class TraitControl : DatabaseActionControl<SupportService> {

        private List<TraitViewModel> _model;

        #region Designer Constructor
        public TraitControl() {
            InitializeComponent();
        }
        #endregion

        public TraitControl(User user, TraitCategoryType category, int? intraCatId) :base(new SupportService(user), "Traits:" + category.ToString() + ":" + intraCatId.Value) {

            this.User = user;
            this.TraitCategory = category;
            this.IntraCatID = intraCatId.Value;

            InitializeComponent();

            if (intraCatId.HasValue) {
                SupportService s = new SupportService(user);
                var list = s.GetTraits(category.ToString(), intraCatId.Value);
                var modellist = list.Select((t) => {
                    return new TraitViewModel(t);
                });
                _model = new List<TraitViewModel>(modellist);
            }

            ReloadTraitPanel();            
        }

        private void ReloadTraitPanel() {

            traitsPanel.Children.Clear();

            _model.Sort(new Comparison<TraitViewModel>((a , b) => {
                return a.Name.CompareTo(b.Name);
            }));

            foreach (TraitViewModel m in _model) {
                AddTraitEditor(m);
            }
        }

        private void AddTraitEditor(TraitViewModel model) {
            var itemControl = new TraitElementControl(User, model);
            itemControl.TraitChanged += new TraitElementControl.TraitEventHandler((source, trait) => {
                RegisterUniquePendingAction(new UpdateTraitDatabaseAction(trait.Model));
            });

            itemControl.TraitDeleted += new TraitElementControl.TraitEventHandler((source, trait) => {
                _model.Remove(trait);
                ReloadTraitPanel();
                RegisterPendingAction(new DeleteTraitDatabaseAction(trait.Model));                
            });
            traitsPanel.Children.Add(itemControl);
        }

        private void btnAddTrait_Click(object sender, RoutedEventArgs e) {
            AddNewTrait();
        }

        private void AddNewTrait() {
            var frm = new AddNewTraitWindow(User, TraitCategory);
            if (frm.ShowDialog().GetValueOrDefault(false)) {
                Trait t = new Trait();
                t.TraitID = -1;
                t.Value = "<New Trait Value>";
                t.Category = TraitCategory.ToString();
                t.IntraCatID = IntraCatID;
                t.Name = frm.TraitName;

                TraitViewModel viewModel = new TraitViewModel(t);
                _model.Add(viewModel);
                RegisterUniquePendingAction(new UpdateTraitDatabaseAction(t));
                ReloadTraitPanel();
            }
        }

        #region Properties

        public User User { get; private set; }

        public TraitCategoryType TraitCategory { get; private set; }

        public int IntraCatID { get; private set; }

        #endregion
    }

    public abstract class TraitDatabaseActionBase : DatabaseAction<SupportService> {

        public TraitDatabaseActionBase(Trait trait) {
            this.Trait = trait;
        }

        public Trait Trait { get; set; }
    }

    public class UpdateTraitDatabaseAction : TraitDatabaseActionBase {

        public UpdateTraitDatabaseAction(Trait trait)
            : base(trait) {
        }

        protected override void ProcessImpl(SupportService service) {
            Trait.TraitID = service.InsertOrUpdateTrait(Trait);
        }

        public override bool Equals(object obj) {
            var other = obj as UpdateTraitDatabaseAction;
            if (other != null) {
                return Trait == other.Trait;
            }
            return false;
        }

        public override int GetHashCode() {
            return base.GetHashCode();
        }

    }

    public class DeleteTraitDatabaseAction : TraitDatabaseActionBase {

        public DeleteTraitDatabaseAction(Trait trait)
            : base(trait) {
        }

        protected override void ProcessImpl(SupportService service) {
            service.DeleteTrait(Trait.TraitID);
        }
        
    }

    public class TraitViewModel : GenericViewModelBase<Trait> {

        public TraitViewModel(Trait t)
            : base(t) {            
        }

        public int TraitID {
            get { return Model.TraitID; }
            set { SetProperty(() => Model.TraitID, value); }
        }

        public string Name {
            get { return Model.Name; }
            set { SetProperty(() => Model.Name, value); }
        }

        public string DataType {
            get { return Model.DataType; }
            set { SetProperty(() => Model.DataType, value); }
        }

        public string Validation {
            get { return Model.Validation; }
            set { SetProperty(() => Model.Validation, value); }
        }

        public int IntraCatID {
            get { return Model.IntraCatID; }
            set { SetProperty(() => Model.IntraCatID, value); }
        }

        public string Value {
            get { return Model.Value; }
            set { SetProperty(() => Model.Value, value); }
        }

        public string Comment {
            get { return Model.Comment; }
            set { SetProperty(() => Model.Comment, value); }
        }

    }

    public enum TraitCategoryType {
        Material,
        Taxon,
        Site,
        Trap,
        SiteVisit,
    }
}
