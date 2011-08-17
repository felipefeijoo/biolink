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
    public partial class TraitControl : DatabaseCommandControl, ILazyPopulateControl {

        private List<TraitViewModel> _model;
        private bool _rdeMode = false;

        #region Designer Constructor
        public TraitControl() {
            InitializeComponent();
        }
        #endregion

        public TraitControl(User user, TraitCategoryType category, ViewModelBase owner, bool RDEMode = false) : base(user, "Traits:" + category.ToString() + ":" + (owner == null ? -1 : owner.ObjectID.Value)) {            
            this.TraitCategory = category;
            this.Owner = owner;
            _rdeMode = RDEMode;

            InitializeComponent();

        }

        public void BindModel(List<Trait> traits, ViewModelBase owner) {
            this.Owner = owner;

            if (traits == null) {
                traits = new List<Trait>();
            }

            _model = traits.ConvertAll((model) => {
                return new TraitViewModel(model);
            });
            ReloadTraitPanel();
        }

        public List<Trait> GetModel() {
            return _model.ConvertAll((vm) => { return vm.Model; });
        }

        private void ReloadTraitPanel() {

            traitsPanel.Children.Clear();

            if (_model != null) {

                _model.Sort(new Comparison<TraitViewModel>((a, b) => {
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
        }

        private void AddTraitEditor(TraitViewModel model) {
            var itemControl = new TraitElementControl(User, model, TraitCategory);
            itemControl.TraitChanged += new TraitElementControl.TraitEventHandler((source, trait) => {
                RegisterUniquePendingChange(new UpdateTraitDatabaseCommand(trait.Model, Owner));
            });

            itemControl.TraitDeleted += new TraitElementControl.TraitEventHandler((source, trait) => {
                _model.Remove(trait);
                ReloadTraitPanel();
                RegisterPendingChange(new DeleteTraitDatabaseCommand(trait.Model, Owner));                
            });

            itemControl.IsReadOnly = this.IsReadOnly;

            traitsPanel.Children.Add(itemControl);
        }

        private void btnAddTrait_Click(object sender, RoutedEventArgs e) {
            AddNewTrait();
            e.Handled = true;
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

                var existing = FindTraitByName(picklist.SelectedValue as string);
                if (existing == null) {
                    Trait t = new Trait();
                    t.TraitID = -1;
                    t.Value = "<New Trait Value>";
                    t.Category = TraitCategory.ToString();
                    t.IntraCatID = Owner.ObjectID.Value;
                    t.Name = picklist.SelectedValue as string;

                    TraitViewModel viewModel = new TraitViewModel(t);
                    _model.Add(viewModel);
                    RegisterUniquePendingChange(new UpdateTraitDatabaseCommand(t, Owner));
                    ReloadTraitPanel();

                    SelectTrait(viewModel);
                } else {
                    SelectTrait(existing);
                }
            }
            
        }

        private TraitViewModel FindTraitByName(string name) {
            var existing = _model.FirstOrDefault((tm) => {
                return tm.Name.Equals(name);
            });
            return existing;
        }

        private void SelectTrait(TraitViewModel vm) {
            // find the trait editor for the selected trait...
            foreach (TraitElementControl ctl in traitsPanel.Children) {
                if (ctl.Model == vm) {
                    if (!ctl.txtValue.IsLoaded) {
                        ctl.txtValue.txt.Loaded += new RoutedEventHandler((source, e) => {
                            Keyboard.Focus(ctl.txtValue.txt);
                        });
                    } else {
                        Keyboard.Focus(ctl.txtValue.txt);
                    }
                    return;
                }
            }
        }

        public void Populate() {
            if (!IsPopulated && !_rdeMode) {
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

        public static readonly DependencyProperty IsReadOnlyProperty = DependencyProperty.Register("IsReadOnly", typeof(bool), typeof(TraitControl), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnIsReadOnlyChanged));

        public bool IsReadOnly {
            get { return (bool)GetValue(IsReadOnlyProperty); }
            set { SetValue(IsReadOnlyProperty, value); }
        }

        private static void OnIsReadOnlyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args) {

            var control = obj as TraitControl;
            if (control != null) {
                control.ReloadTraitPanel();
                control.btnAddTrait.IsEnabled = !(bool) args.NewValue;
            }
        }



        #region Properties

        public TraitCategoryType TraitCategory { get; private set; }

        public ViewModelBase Owner { get; private set; }

        public bool IsPopulated { get; private set; }

        #endregion

        

    }

}
