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
using BioLink.Client.Extensibility;

namespace BioLink.Client.Tools {
    /// <summary>
    /// Interaction logic for PhraseManager.xaml
    /// </summary>
    public partial class PhraseManager : Window {

        private ObservableCollection<PhraseCategory> _model;

        private List<DatabaseAction<SupportService>> _changes = new List<DatabaseAction<SupportService>>();

        private Dictionary<PhraseCategory, ObservableCollection<PhraseViewModel>> _phraseCache = new Dictionary<PhraseCategory, ObservableCollection<PhraseViewModel>>();

        #region DesignerConstructor
        public PhraseManager() {
            InitializeComponent();
        }
        #endregion

        public PhraseManager(User user) {
            InitializeComponent();
            this.User = user;
            Service = new SupportService(User);
            RefreshCategories();
        }

        private void RefreshCategories() {
            
            _model = new ObservableCollection<PhraseCategory>(Service.GetPhraseCategories());
            lvwCategories.ItemsSource = _model;
            ClearFilter();
        }

        private void ClearFilter() {
            ListCollectionView dataView = CollectionViewSource.GetDefaultView(lvwCategories.ItemsSource) as ListCollectionView;
            dataView.Filter = null;
            dataView.Refresh();
            txtFilter.Text = "";
        }

        public void RegisterChangeAction(DatabaseAction<SupportService> action) {
            _changes.Add(action);
            btnApply.IsEnabled = true;
        }

        protected User User { get; private set; }

        protected SupportService Service { get; private set; }

        private void txtFilter_TypingPaused(string text) {

            if (String.IsNullOrEmpty(text)) {
                ClearFilter();
            }

            ListCollectionView dataView = CollectionViewSource.GetDefaultView(lvwCategories.ItemsSource) as ListCollectionView;
            text = text.ToLower();

            dataView.Filter = (obj) => {
                var category = obj as PhraseCategory;
                return category.Category.ToLower().Contains(text);
            };

            dataView.Refresh();

        }

        private void lvwCategories_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            ListPhrases(lvwCategories.SelectedItem as PhraseCategory);
        }

        private void ListPhrases(PhraseCategory category) {
            if (category != null) {
                
                if (!_phraseCache.ContainsKey(category)) {
                    var model = new ObservableCollection<PhraseViewModel>(Service.GetPhrases(category.CategoryID).ConvertAll<PhraseViewModel>((p) => new PhraseViewModel(p)));
                    _phraseCache[category] = model;
                }
                
                lvwPhrases.ItemsSource = _phraseCache[category];
            } else {
                lvwPhrases.ItemsSource = null;
            }
        }

        private void txtFilter_TextChanged(object sender, TextChangedEventArgs e) {
            if (String.IsNullOrEmpty(txtFilter.Text)) {
                ClearFilter();
            }
        }

        private void txtFilter_KeyUp(object sender, KeyEventArgs e) {
            if (e.Key == Key.Down) {
                lvwCategories.SelectedIndex = 0;
                if (lvwCategories.SelectedItem != null) {
                    ListBoxItem item = lvwCategories.ItemContainerGenerator.ContainerFromItem(lvwCategories.SelectedItem) as ListBoxItem;
                    item.Focus();
                }
            }
        }

        private void btnOK_Click(object sender, RoutedEventArgs e) {
            if (_changes.Count > 0) {
                if (this.Question("You have unsaved changes. Do you wish to save them before closing?", "Save changes?")) {
                    SaveChanges();
                }
            }
            this.Hide();
        }

        private void SaveChanges() {

            Service.BeginTransaction();
            try {
                foreach (DatabaseAction<SupportService> action in _changes) {
                    action.Process(Service);
                }
                Service.CommitTransaction();
                // Now reload the model
                RefreshCategories();

                _changes.Clear();
            } catch (Exception ex) {
                Service.RollbackTransaction();
                GlobalExceptionHandler.Handle(ex);
            }

        }

        private void btnCancel_Click(object sender, RoutedEventArgs e) {
            bool okToHide = true;
            if (_changes.Count > 0) {
                okToHide = this.Question("You have unsaved changes. Are you sure you wish to discard these changes?", "Discard changes?");
            } 
            
            if (okToHide) {
                _changes.Clear();
                _phraseCache.Clear();
                this.Hide();
            }

        }

        private void btnRenamePhrase_Click(object sender, RoutedEventArgs e) {
            RenameCurrentPhrase();
        }

        private void RenameCurrentPhrase() {
            var phrase = lvwPhrases.SelectedItem as PhraseViewModel;
            if (phrase == null) {
                return;
            }

            InputBox.Show(this, "Rename phrase", "Enter the new phrase value", phrase.PhraseText, (str) => {
                RegisterChangeAction(new RenamePhraseAction(phrase.Phrase, str));
                phrase.PhraseText = str;
            });
        }

    }

    public class PhraseViewModel : ViewModelBase {

        public PhraseViewModel(Phrase phrase) {
            this.Phrase = phrase;
        }

        public string PhraseText {
            get { return Phrase.PhraseText; }
            set { SetProperty(() => Phrase.PhraseText, Phrase, value); }
        }

        public int PhraseID {
            get { return Phrase.PhraseID; }
        }

        public int PhraseCatID {
            get { return Phrase.PhraseCatID; }
        }

        public Phrase Phrase { get; private set; }

        public override BitmapSource Icon { get; set; }
    }

    abstract class PhraseDatabaseAction : DatabaseAction<SupportService> {

        protected PhraseDatabaseAction(Phrase phrase) {
            this.Phrase = phrase;
        }

        public Phrase Phrase { get; private set; }
    }

    class RenamePhraseAction : PhraseDatabaseAction {

        public RenamePhraseAction(Phrase phrase, string newvalue) : base(phrase) {
            this.NewValue = newvalue;
        }

        protected override void ProcessImpl(SupportService service) {
            service.RenamePhrase(Phrase.PhraseID, NewValue);
        }

        public string NewValue { get; set; }
    }
}
