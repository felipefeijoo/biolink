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

            lstNotes.ItemsSource = _model;
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

        private void Expander_Expanded(object sender, RoutedEventArgs e) {
            MessageBox.Show("jher");
        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            lstNotes.ItemsSource = _model;
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e) {
            Grid g = (Grid)sender;
            Binding b = new Binding("ActualWidth");
            b.Source = lstNotes;            
            g.SetBinding(Grid.WidthProperty, b);
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
