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
    /// Interaction logic for NotesControl.xaml
    /// </summary>
    public partial class NotesControl : DatabaseCommandControl, ILazyPopulateControl {

        private ObservableCollection<NoteViewModel> _model;
        private NoteControl _currentNoteControl;        

        #region Designer Constructor
        public NotesControl() {
            InitializeComponent();
        }
        #endregion

        public NotesControl(User user, TraitCategoryType category, ViewModelBase owner) : base(user, "Notes:" + category.ToString() + ":" + owner.ObjectID.Value) {
            InitializeComponent();            
            TraitCategory = category;
            this.Owner = owner;
            btnColor.ColorSelected += new Action<Color>(btnColor_SelectedColorChanged);
        }

        void btnColor_SelectedColorChanged(Color color) {
            if (_currentNoteControl != null) {
                _currentNoteControl.txtNote.Selection.ApplyPropertyValue(TextElement.ForegroundProperty, new SolidColorBrush(color));
            }
        }

        private void LoadNotesPanel(NoteViewModel selected = null) {

            using (new OverrideCursor(Cursors.Wait)) {

                var service = new SupportService(User);
                var list = service.GetNotes(TraitCategory.ToString(), Owner.ObjectID.Value);
                _model = new ObservableCollection<NoteViewModel>(list.ConvertAll((model) => {
                    var viewModel = new NoteViewModel(model);
                    viewModel.DataChanged += new DataChangedHandler(viewModel_DataChanged);
                    return viewModel;
                }));

                RedrawNotes(selected);

                IsPopulated = true;
            }
        }

        private void RedrawNotes(NoteViewModel selected = null) {
            notesPanel.Children.Clear();

            foreach (NoteViewModel m in _model) {
                var control = new NoteControl(User, m) { IsReadOnly = IsReadOnly };
                control.NoteDeleted += new NoteControl.NoteEventHandler(control_NoteDeleted);
                control.TextSelectionChanged += new NoteControl.NoteEventHandler(control_TextSelectionChanged);

                if (selected != null && selected == m) {
                    control.IsExpanded = true;
                }
                notesPanel.Children.Add(control);
            }

        }

        private void control_TextSelectionChanged(object source, NoteViewModel note) {
            _currentNoteControl = source as NoteControl;
            if (_currentNoteControl != null) {
                var selectionBrush = _currentNoteControl.txtNote.Selection.GetPropertyValue(TextElement.ForegroundProperty) as SolidColorBrush;
                if (selectionBrush != null) {
                    btnColor.SelectedColor = selectionBrush.Color;
                } else {
                    btnColor.SelectedColor = Color.FromArgb(0, 0, 0, 0);
                }                
            }
        }

        private void control_NoteDeleted(object source, NoteViewModel note) {

            _model.Remove(note);
            if (note.NoteID >= 0) {
                RegisterPendingChange(new DeleteNoteCommand(note.Model, Owner));
            }
            RedrawNotes();
        }

        public void Populate() {
            if (!IsPopulated) {
                this.InvokeIfRequired(() => {
                    LoadNotesPanel();
                });
            }
        }

        void viewModel_DataChanged(ChangeableModelBase viewmodel) {
            var note = viewmodel as NoteViewModel;
            if (note != null) {
                if (note.NoteID >= 0) {
                    RegisterUniquePendingChange(new UpdateNoteCommand(note.Model, Owner));
                }
            }
        }

        public bool IsPopulated { get; private set; }

        public TraitCategoryType TraitCategory { get; private set; }

        public ViewModelBase Owner { get; private set; }

        private void btnAddNew_Click(object sender, RoutedEventArgs e) {
            AddNewNote();
        }

        private void AddNewNote() {
            var service = new SupportService(User);

            List<String> noteTypes = service.GetNoteTypesForCategory(TraitCategory.ToString());

            var picklist = new PickListWindow(User, "Choose a note type...", () => {
                return noteTypes;
            }, (text) => {
                noteTypes.Add(text);
                return true;
            });

            picklist.Owner = this.FindParentWindow();
            if (picklist.ShowDialog().ValueOrFalse()) {
                Note note = new Note();
                note.NoteID = -1;
                note.NoteType = picklist.SelectedValue as String;
                note.NoteCategory = TraitCategory.ToString();
                note.IntraCatID = Owner.ObjectID.Value;
                note.NoteRTF = "New Note";

                NoteViewModel viewModel = new NoteViewModel(note);
                _model.Add(viewModel);
                RegisterUniquePendingChange(new InsertNoteCommand(note, Owner));
                RedrawNotes(viewModel);
            }
        }

        private void ForEachNoteControl(Action<NoteControl> action) {
            if (action == null) {
                return;
            }
            foreach (NoteControl control in notesPanel.Children) {
                action(control);
            }
        }

        private void ExpandAll() {
            ForEachNoteControl((control) => {
                control.IsExpanded = true;
            });
        }

        private void CollapseAll() {
            ForEachNoteControl((control) => {
                control.IsExpanded = false;
            });
        }

        private void btnExpandALL_Click(object sender, RoutedEventArgs e) {
            ExpandAll();
        }

        private void btnCollapseAll_Click(object sender, RoutedEventArgs e) {
            CollapseAll();
        }

        private void btnFont_Click(object sender, RoutedEventArgs e) {
            if (_currentNoteControl != null) {

                var form = new FontChooser();
                form.Owner = this.FindParentWindow();
                form.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                if (form.ShowDialog().ValueOrFalse()) {
                    form.ApplyPropertiesToTextSelection(_currentNoteControl.txtNote.Selection);
                }
            }

        }

        public static readonly DependencyProperty IsReadOnlyProperty = DependencyProperty.Register("IsReadOnly", typeof(bool), typeof(NotesControl), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(OnIsReadOnlyChanged)));

        public bool IsReadOnly {
            get { return (bool)GetValue(IsReadOnlyProperty); }
            set { SetValue(IsReadOnlyProperty, value); }
        }

        private static void OnIsReadOnlyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args) {
            var control = (NotesControl)obj;
            if (control != null) {
                var readOnly = (bool)args.NewValue;                
                control.toolbar.IsEnabled = !readOnly;
            }
        }

    }

    public class NoteViewModel : GenericViewModelBase<Note> {

        public NoteViewModel(Note model) : base(model, ()=>model.NoteID) { }

        public override string DisplayLabel {
            get { return string.Format("Note: {0} - {1}", NoteID, NoteType); }
        }

        public string NoteCategory {
            get { return Model.NoteCategory; }
            set { SetProperty(() => Model.NoteCategory, value); }
        }

        public int NoteID {
            get { return Model.NoteID; }
            set { SetProperty(() => Model.NoteID, value); }
        }

        public string NoteType {
            get { return Model.NoteType; }
            set { SetProperty(() => Model.NoteType, value); }
        }

        public string NoteRTF {
            get { return Model.NoteRTF; }
            set { SetProperty(() => Model.NoteRTF, value); }
        }

        public string Author {
            get { return Model.Author; }
            set { SetProperty(() => Model.Author, value); }
        }

        public string Comments {
            get { return Model.Comments; }
            set { SetProperty(() => Model.Comments, value); }
        }

        public bool UseInReports {
            get { return Model.UseInReports; }
            set { SetProperty(() => Model.UseInReports, value); }
        }

        public int RefID {
            get { return Model.RefID; }
            set { SetProperty(() => Model.RefID, value); }
        }

        public string RefCode {
            get { return Model.RefCode; }
            set { SetProperty(() => Model.RefCode, value); }
        }

        public string RefPages {
            get { return Model.RefPages; }
            set { SetProperty(() => Model.RefPages, value); }
        }

    }
}
