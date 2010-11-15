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
    public partial class NotesControl : DatabaseActionControl {

        private ObservableCollection<NoteViewModel> _model;

        #region Designer Constructor
        public NotesControl() {
            InitializeComponent();
        }
        #endregion

        public NotesControl(User user, TraitCategoryType category, int? intraCatId) : base(user, "Notes:" + category.ToString() + ":" + intraCatId.Value) {
            InitializeComponent();
            Debug.Assert(intraCatId.HasValue);
            TraitCategory = category;
            this.IntraCatID = intraCatId.Value;

            var service = new SupportService(User);
            var list = service.GetNotes(TraitCategory.ToString(), IntraCatID);
            _model = new ObservableCollection<NoteViewModel>(list.ConvertAll((model) => {
                var viewModel = new NoteViewModel(model);
                viewModel.DataChanged += new DataChangedHandler(viewModel_DataChanged);
                return viewModel;
            }));

            LoadNotesPanel();
        }

        private void LoadNotesPanel() {
            notesPanel.Children.Clear();
            foreach (NoteViewModel m in _model) {
                var control = new NoteControl(m);
                notesPanel.Children.Add(control);
            }
        }

        public void PopulateControl() {
            this.InvokeIfRequired(() => {
            });            
        }

        void viewModel_DataChanged(ChangeableModelBase viewmodel) {
            // to do, create an update action
        }

        public bool IsPopulated { get; private set; }

        public TraitCategoryType TraitCategory { get; private set; }

        public int IntraCatID { get; private set; }

        private void btnAddNew_Click(object sender, RoutedEventArgs e) {
            AddNewNote();
        }

        private void AddNewNote() {
            var service = new TaxaService(User);

            List<String> noteTypes = service.GetNoteTypesForCategory(TraitCategory.ToString());

            var picklist = new PickListWindow(User, "Choose a trait type...", () => {
                return noteTypes;
            }, (text) => {
                noteTypes.Add(text);
                return true;
            });

            picklist.Owner = this.FindParentWindow();
            if (picklist.ShowDialog().ValueOrFalse()) {
                Note t = new Note();
                t.NoteID = -1;
                t.NoteType = picklist.SelectedValue;
                t.NoteCategory = TraitCategory.ToString();
                t.IntraCatID = IntraCatID;
                t.NoteRTF = "New Note";

                NoteViewModel viewModel = new NoteViewModel(t);
                _model.Add(viewModel);
//                RegisterUniquePendingChange(new UpdateTraitDatabaseAction(t));
                LoadNotesPanel();
            }
        }

    }

    public class NoteViewModel : GenericViewModelBase<Note> {

        public NoteViewModel(Note model)
            : base(model) {
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
