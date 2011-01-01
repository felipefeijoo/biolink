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
using BioLink.Client.Utilities;

namespace BioLink.Client.Extensibility {
    /// <summary>
    /// Interaction logic for TraitControl.xaml
    /// </summary>
    public partial class TraitControl : DatabaseActionControl, ILazyPopulateControl {

        private List<TraitViewModel> _model;

        #region Designer Constructor
        public TraitControl() {
            InitializeComponent();
        }
        #endregion

        public TraitControl(User user, TraitCategoryType category, ViewModelBase owner) : base(user, "Traits:" + category.ToString() + ":" + owner.ObjectID.Value) {
            
            this.TraitCategory = category;
            this.Owner = owner;

            InitializeComponent();

        }

        private void ReloadTraitPanel() {

            traitsPanel.Children.Clear();

            _model.Sort(new Comparison<TraitViewModel>((a , b) => {
                if (a.Name != null) {
                    return a.Name.CompareTo(b.Name);
                } else {
                    return 0;
                }
            }));

            foreach (TraitViewModel m in _model) {
                AddTraitEditor(m);
            }
        }

        private void AddTraitEditor(TraitViewModel model) {
            var itemControl = new TraitElementControl(User, model);
            itemControl.TraitChanged += new TraitElementControl.TraitEventHandler((source, trait) => {
                RegisterUniquePendingChange(new UpdateTraitDatabaseAction(trait.Model, Owner));
            });

            itemControl.TraitDeleted += new TraitElementControl.TraitEventHandler((source, trait) => {
                _model.Remove(trait);
                ReloadTraitPanel();
                RegisterPendingChange(new DeleteTraitDatabaseAction(trait.Model, Owner));                
            });
            traitsPanel.Children.Add(itemControl);
        }

        private void btnAddTrait_Click(object sender, RoutedEventArgs e) {
            AddNewTrait();
        }

        private void AddNewTrait() {

            var service = new SupportService(User);

            List<String> traitTypes = service.GetTraitNamesForCategory(TraitCategory.ToString());

            var picklist = new PickListWindow(User, "Choose a trait type...", () => {
                return traitTypes;
            }, (text) => {
                traitTypes.Add(text);
                return true;
            });
            
            picklist.Owner = this.FindParentWindow();
            if (picklist.ShowDialog().ValueOrFalse()) {
                Trait t = new Trait();
                t.TraitID = -1;
                t.Value = "<New Trait Value>";
                t.Category = TraitCategory.ToString();
                t.IntraCatID = Owner.ObjectID.Value;
                t.Name = picklist.SelectedValue as string;

                TraitViewModel viewModel = new TraitViewModel(t);
                _model.Add(viewModel);
                RegisterUniquePendingChange(new UpdateTraitDatabaseAction(t, Owner));
                ReloadTraitPanel();
            }

        }

        public void Populate() {
            if (!IsPopulated) {
                if (Owner.ObjectID.HasValue && Owner.ObjectID.Value >= 0) {
                    SupportService service = new SupportService(User);
                    var list = service.GetTraits(TraitCategory.ToString(), Owner.ObjectID.Value);
                    var modellist = list.Select((t) => {
                        return new TraitViewModel(t);
                    });
                    _model = new List<TraitViewModel>(modellist);
                } else {
                    _model = new List<TraitViewModel>();
                }
                ReloadTraitPanel();
                IsPopulated = true;
            }
        }


        #region Properties

        public TraitCategoryType TraitCategory { get; private set; }

        public ViewModelBase Owner { get; private set; }

        public bool IsPopulated { get; private set; }

        #endregion

        

    }

}
