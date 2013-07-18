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
using System.Windows.Threading;
using BioLink.Data;
using BioLink.Data.Model;
using System.Collections.ObjectModel;
using BioLink.Client.Utilities;
using BioLink.Client.Extensibility;

namespace BioLink.Client.Tools {
    /// <summary>
    /// Interaction logic for PhraseManager.xaml
    /// </summary>
    public partial class PhraseManager : DatabaseCommandControl {

        private ObservableCollection<PhraseCategoryViewModel> _model;

        private Dictionary<PhraseCategoryViewModel, ObservableCollection<PhraseViewModel>> _phraseCache = new Dictionary<PhraseCategoryViewModel, ObservableCollection<PhraseViewModel>>();

        #region DesignerConstructor
        public PhraseManager() {
            InitializeComponent();
        }
        #endregion

        public PhraseManager(User user) : base(user, "PhraseManager") {
            InitializeComponent();            
            ReloadModel();

            ChangesCommitted += new PendingChangesCommittedHandler((source) => {
                ReloadModel();                
            });

        }

        private void ReloadModel() {
            int oldCategoryId = CurrentCategory == null ? -1 : CurrentCategory.CategoryID;

            _model = new ObservableCollection<PhraseCategoryViewModel>(Service.GetPhraseCategories().ConvertAll((model) => {
                return new PhraseCategoryViewModel(model);
            }));

            lvwCategories.ItemsSource = _model;
            if (oldCategoryId >= 0) {
                var oldSelectedCategory = _model.FirstOrDefault((category) => {
                    return category.CategoryID == oldCategoryId;
                });

                if (oldSelectedCategory != null) {
                    oldSelectedCategory.IsSelected = true;
                }
            }
            // ClearFilter();
        }

        private void ClearFilter(ListView lvw, DelayedTriggerTextbox txtbox) {
            ListCollectionView dataView = CollectionViewSource.GetDefaultView(lvw.ItemsSource) as ListCollectionView;
            dataView.Filter = null;
            dataView.Refresh();
            txtbox.Text = "";
        }

        private void txtFilter_TypingPaused(string text) {

            if (String.IsNullOrEmpty(text)) {
                ClearFilter(lvwCategories, txtFilter);
            }

            ListCollectionView dataView = CollectionViewSource.GetDefaultView(lvwCategories.ItemsSource) as ListCollectionView;
            text = text.ToLower();

            dataView.Filter = (obj) => {
                var category = obj as PhraseCategoryViewModel;
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
                ClearFilter(lvwCategories, txtFilter);
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

        private void btnRenamePhrase_Click(object sender, RoutedEventArgs e) {
            var phrase = lvwPhrases.SelectedItem as PhraseViewModel;
            RenamePhrase(phrase);
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

            lvwPhrases.Dispatcher.BeginInvoke(new Action(() => {
                lvwPhrases.ScrollIntoView(viewModel);
                ListViewItem item = (ListViewItem)lvwPhrases.ItemContainerGenerator.ContainerFromItem(viewModel);
                if (item != null) {
                    item.Focus();
                }
            }));
            
            RegisterPendingChange(new InsertPhraseCommand(phrase));

            viewModel.IsRenaming = true;
        }

        private void DeletePhrase(PhraseViewModel phrase) {            
            if (phrase == null || phrase.IsDeleted) {
                return;
            }

            if (this.Question(String.Format("Are you sure you want to delete the phrase \"{0}\"?", phrase.PhraseText), "Delete phrase?")) {
                RegisterPendingChange(new DeletePhraseCommand(phrase.Model));
                phrase.IsDeleted = true;                
            }
        }

        private void RenamePhrase(PhraseViewModel phrase) {            
            if (phrase == null || phrase.IsDeleted) {
                return;
            }

            phrase.IsRenaming = true;
        }

        private void btnDeletePhrase_Click(object sender, RoutedEventArgs e) {
            var phrase = lvwPhrases.SelectedItem as PhraseViewModel;
            DeletePhrase(phrase);
        }

        private void btnAddPhrase_Click(object sender, RoutedEventArgs e) {
            AddNewPhraseValue();
        }

        private void PhraseText_EditingComplete(object sender, string text) {
            PhraseViewModel vm = (sender as EditableTextBlock).ViewModel as PhraseViewModel;
            RegisterPendingChange(new RenamePhraseCommand(vm.Model, text));
        }

        private void PhraseText_EditingCancelled(object sender, string oldtext) {
            PhraseViewModel vm = (sender as EditableTextBlock).ViewModel as PhraseViewModel;
            vm.PhraseText = oldtext;
        }

        private void lvwPhrases_MouseRightButtonUp(object sender, MouseButtonEventArgs e) {
            ShowPhraseContextMenu();
        }

        private void ShowPhraseContextMenu() {

            PhraseViewModel phrase = lvwPhrases.SelectedItem as PhraseViewModel;

            if (phrase == null) {
                return;
            }

            ContextMenu menu = new ContextMenu();

            MenuItemBuilder builder = new MenuItemBuilder();
            menu.Items.Add(builder.New("Delete").Handler(() => {
                DeletePhrase(phrase);
            }).MenuItem);

            menu.Items.Add(builder.New("Rename").Handler(() => {
                RenamePhrase(phrase);
            }).MenuItem);

            menu.Items.Add(new Separator());

            menu.Items.Add(builder.New("Add new phrase value").Handler(() => {
                AddNewPhraseValue();
            }).MenuItem);

            lvwPhrases.ContextMenu = menu;
        }

        private void ShowCategoryContextMenu() {
            PhraseCategoryViewModel category = CurrentCategory;
            if (category == null) {
                return;
            }

            ContextMenu menu = new ContextMenu();

            MenuItemBuilder builder = new MenuItemBuilder();
            menu.Items.Add(builder.New("Delete Category").Handler(() => { DeleteCategory(category); }).MenuItem);
            menu.Items.Add(builder.New("Add New Category").Handler(() => { AddPhraseCategory(); }).MenuItem);
            lvwCategories.ContextMenu = menu;
        }

        private void btnDeleteCategory_Click(object sender, RoutedEventArgs e) {
            DeleteCategory(CurrentCategory);
        }

        private void DeleteCategory(PhraseCategoryViewModel category) {
            if (category.Fixed) {
                ErrorMessage.Show("The phrase category '{0}' is required by the system, and cannot be deleted.", category.Category);
                return;
            }

            if (this.Question(String.Format("Are you sure you want to delete the phrase category \"{0}\"?", category.Category), "Delete category?")) {
                RegisterPendingChange(new DeletePhraseCategoryCommand(category.Model));
                category.IsDeleted = true;
            }

        }

        private void txtPhraseFilter_TypingPaused(string text) {
            if (String.IsNullOrEmpty(text)) {
                ClearFilter(lvwPhrases, txtPhraseFilter);
            }

            ListCollectionView dataView = CollectionViewSource.GetDefaultView(lvwPhrases.ItemsSource) as ListCollectionView;
            text = text.ToLower();

            dataView.Filter = (obj) => {
                var phrase = obj as PhraseViewModel;
                return phrase.PhraseText.ToLower().Contains(text);
            };

            dataView.Refresh();

        }

        private void lvwCategories_MouseRightButtonUp(object sender, MouseButtonEventArgs e) {
            ShowCategoryContextMenu();
        }

        private void txtPhraseFilter_TextChanged(object sender, TextChangedEventArgs e) {
            if (String.IsNullOrEmpty(txtPhraseFilter.Text)) {
                ClearFilter(lvwPhrases, txtPhraseFilter);
            }
        }

        protected SupportService Service { get { return new SupportService(User); } }

        private void btnAddCategory_Click(object sender, RoutedEventArgs e) {
            AddPhraseCategory();
        }

        private void AddPhraseCategory() {
            var frm = new AddPhraseCategory();
            frm.Owner = this.FindParentWindow();
            frm.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            if (frm.ShowDialog().ValueOrFalse()) {
                ReloadModel();                
            }
        }

    }

}
