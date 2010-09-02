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
    public partial class PhraseManager : DatabaseActionControl<SupportService> {

        private ObservableCollection<PhraseCategoryViewModel> _model;

        private Dictionary<PhraseCategoryViewModel, ObservableCollection<PhraseViewModel>> _phraseCache = new Dictionary<PhraseCategoryViewModel, ObservableCollection<PhraseViewModel>>();

        #region DesignerConstructor
        public PhraseManager() {
            InitializeComponent();
        }
        #endregion

        public PhraseManager(User user) {
            InitializeComponent();
            this.User = user;
            Service = new SupportService(User);
            ReloadModel();

            PendingChangedRegistered +=new PendingChangedRegisteredHandler((source, action) => {
                btnApply.IsEnabled = true;
            });

        }

        private void ReloadModel() {
            int oldCategoryId = CurrentCategory == null ? -1 : CurrentCategory.CategoryID;

            _model = new ObservableCollection<PhraseCategoryViewModel>(Service.GetPhraseCategories().ConvertAll((model) => {
                return new PhraseCategoryViewModel(model);
            }));

            lvwCategories.ItemsSource = _model;
            if (oldCategoryId >= 0) {
                var oldSelectedCategory = _model.First((category) => {
                    return category.CategoryID == oldCategoryId;
                });

                if (oldSelectedCategory != null) {
                    oldSelectedCategory.IsSelected = true;
                }
            }
            // ClearFilter();
        }

        private void ClearFilter() {
            ListCollectionView dataView = CollectionViewSource.GetDefaultView(lvwCategories.ItemsSource) as ListCollectionView;
            dataView.Filter = null;
            dataView.Refresh();
            txtFilter.Text = "";
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

        protected PhraseCategoryViewModel CurrentCategory { get; private set; }

        private void lvwCategories_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            CurrentCategory = lvwCategories.SelectedItem as PhraseCategoryViewModel;
            ListPhrases(CurrentCategory);
        }

        private void ListPhrases(PhraseCategoryViewModel category) {
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
            if (HasPendingChanges) {
                if (this.Question("You have unsaved changes. Do you wish to save them before closing?", "Save changes?")) {
                    CommitPendingChanges(Service, () => {
                        ReloadModel();
                    });
                }
            }
            HideMe();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e) {
            bool okToHide = true;
            if (HasPendingChanges) {
                okToHide = this.Question("You have unsaved changes. Are you sure you wish to discard these changes?", "Discard changes?");
            } 
            
            if (okToHide) {
                HideMe();
            }

        }

        protected void HideMe() {
            _phraseCache = null;
            _model = null;
            ClearPendingChanges();
            this.FindParentWindow().Close();
        }


        private void btnRenamePhrase_Click(object sender, RoutedEventArgs e) {
            RenameCurrentPhrase();
        }

        private void AddNewPhraseValue() {

            if (CurrentCategory == null) {
                return;
            }

            Phrase phrase = new Phrase();
            phrase.PhraseText = "<New Phrase>";
            phrase.PhraseID = -1;
            phrase.PhraseCatID = CurrentCategory.CategoryID;

            PhraseViewModel viewModel = new PhraseViewModel(phrase);
            (lvwPhrases.ItemsSource as ObservableCollection<PhraseViewModel>).Add(viewModel);
            viewModel.IsSelected = true;
            
            RegisterPendingAction(new AddPhraseAction(phrase));

            viewModel.IsRenaming = true;
        }

        private void DeleteCurrentPhrase() {
            var phrase = lvwPhrases.SelectedItem as PhraseViewModel;
            if (phrase == null || phrase.IsDeleted) {
                return;
            }

            if (this.Question(String.Format("Are you sure you want to delete the phrase \"{0}\"?", phrase.PhraseText), "Delete phrase?")) {
                RegisterPendingAction(new DeletePhraseAction(phrase.Model));
                phrase.IsDeleted = true;                
            }

        }

        private void RenameCurrentPhrase() {
            var phrase = lvwPhrases.SelectedItem as PhraseViewModel;
            if (phrase == null || phrase.IsDeleted) {
                return;
            }

            phrase.IsRenaming = true;
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e) {

        }

        private void btnDeletePhrase_Click(object sender, RoutedEventArgs e) {
            DeleteCurrentPhrase();
        }

        private void btnAddPhrase_Click(object sender, RoutedEventArgs e) {
            AddNewPhraseValue();
        }

        private void btnApply_Click(object sender, RoutedEventArgs e) {

            CommitPendingChanges(Service, () => {
                ReloadModel();
                btnApply.IsEnabled = false;
            });

        }

        private void PhraseText_EditingComplete(object sender, string text) {
            PhraseViewModel vm = (sender as EditableTextBlock).ViewModel as PhraseViewModel;
            RegisterPendingAction(new RenamePhraseAction(vm.Model, text));
        }

        private void PhraseText_EditingCancelled(object sender, string oldtext) {
            PhraseViewModel vm = (sender as EditableTextBlock).ViewModel as PhraseViewModel;
            vm.PhraseText = oldtext;
        }

    }

}
