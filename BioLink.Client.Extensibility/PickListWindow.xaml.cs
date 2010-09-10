using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using BioLink.Client.Utilities;
using BioLink.Data;
using BioLink.Data.Model;
using System.Collections.Generic;


namespace BioLink.Client.Extensibility {
    /// <summary>
    /// Interaction logic for PickListWindow.xaml
    /// </summary>
    public partial class PickListWindow : Window {

        #region DesignerConstructor
        public PickListWindow() {
            InitializeComponent();
        }
        #endregion

        public PickListWindow(User user, PickListType pickListType, string categoryName, TraitCategoryType traitCategory) {
            this.User = user;
            this.CategoryName = categoryName;
            this.TraitCategory = traitCategory;
            this.Service = new SupportService(user);
            InitializeComponent();                        
            Config.RestoreWindowPosition(User, this);
            switch (pickListType) {
                case PickListType.Phrase:
                    LoadPhraseModel();
                    break;
                case PickListType.DistinctList:
                    LoadDistinctListModel();
                    break;
                case PickListType.Trait:
                    LoadTraitModel();
                    break;
                default:
                    throw new Exception("Unhandled pick list type: " + pickListType);
            }
            
        }

        private void LoadTraitModel() {
            Title = String.Format("Existing trait names for {0}", TraitCategory.ToString());
            ObservableCollection<String> model = new ObservableCollection<String>(Service.GetTraitNamesForCategory(TraitCategory.ToString()));
            lst.ItemsSource = model;
        }

        private void LoadDistinctListModel() {
            Title = String.Format("Values for '{0}'", CategoryName);
            ObservableCollection<String> model = new ObservableCollection<String>(Service.GetTraitDistinctValues(CategoryName, TraitCategory.ToString()));
            lst.ItemsSource = model;
        }

        private void LoadPhraseModel() {
            Title = String.Format("Values for '{0}'", CategoryName);
            int phraseCategoryID = Service.GetPhraseCategoryId(CategoryName, true);
            List<Phrase> list = Service.GetPhrases(phraseCategoryID);
            ObservableCollection<String> model = new ObservableCollection<String>(list.ConvertAll((phrase) => phrase.PhraseText));

            lst.ItemsSource = model;
            btnAddNew.Visibility = Visibility.Visible;
            btnAddNew.Click +=new RoutedEventHandler((source, e) => {
                AddNewPhrase(phraseCategoryID);
            });
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e) {
            this.Hide();
        }

        private void txtFilter_TypingPaused(string text) {
            FilterList(text);
        }

        private void FilterList(string text) {
            ListCollectionView dataView = CollectionViewSource.GetDefaultView(lst.ItemsSource) as ListCollectionView;

            if (String.IsNullOrEmpty(text)) {
                dataView.Filter = null;
                dataView.Refresh();
                return;
            }

            text = text.ToLower();
            
            dataView.Filter = (obj) => {
                string str = obj as string;
                return str.ToLower().Contains(text);
            };

            dataView.Refresh();
        }

        public string SelectedValue {
            get { return lst.SelectedItem as string; }
        }

        private void btnSelect_Click(object sender, RoutedEventArgs e) {
            if (lst.SelectedItem != null) {
                this.DialogResult = true;
                this.Hide();
            }
        }

        private void txtFilter_KeyUp(object sender, KeyEventArgs e) {
            if (e.Key == Key.Down) {
                lst.SelectedIndex = 0;
                if (lst.SelectedItem != null) {
                    ListBoxItem item = lst.ItemContainerGenerator.ContainerFromItem(lst.SelectedItem) as ListBoxItem;
                    item.Focus();
                }
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {            
            lst.Focus();
        }


        private void Window_Deactivated(object sender, EventArgs e) {
            Config.SaveWindowPosition(User, this);
        }

        private void AddNewPhrase(int phraseCategoryId) {
            InputBox.Show(this, "Add a new phrase value", "Enter the new phrase value, and click OK", (phrasetext) => {

                Phrase phrase = new Phrase();
                phrase.PhraseID = -1;
                phrase.PhraseCatID = phraseCategoryId;
                phrase.PhraseText = phrasetext;
                // Save the new phrase value...
                Service.AddPhrase(phrase);
                // reload the model...
                LoadPhraseModel();
            });
        }

        private void lst_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
            if (lst.SelectedItem != null) {
                this.DialogResult = true;
                this.Hide();
            }
        }

        #region Properties
         
        public User User { get; private set; }

        public PickListType PickListType { get; private set; }

        protected SupportService Service { get; private set; }

        protected string CategoryName { get; set; }

        protected TraitCategoryType TraitCategory { get; set; }

        #endregion


    }
}
