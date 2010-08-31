using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using BioLink.Client.Utilities;
using BioLink.Data;
using BioLink.Data.Model;


namespace BioLink.Client.Extensibility {
    /// <summary>
    /// Interaction logic for PhraseSelectorWindow.xaml
    /// </summary>
    public partial class PhraseSelectorWindow : Window {

        #region DesignerConstructor
        public PhraseSelectorWindow() {
            InitializeComponent();
        }
        #endregion

        public PhraseSelectorWindow(User user, String categoryName, bool @fixed) {
            InitializeComponent();
            this.User = user;
            this.Service = new SupportService(user);
            Title = String.Format("Values for '{0}'", categoryName);
            this.CategoryId = Service.GetPhraseCategoryId(categoryName, @fixed);
            LoadModel();
        }

        private void LoadModel() {
            ObservableCollection<Phrase> model = new ObservableCollection<Phrase>(Service.GetPhrases(CategoryId));
            lst.ItemsSource = model;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e) {
            this.Hide();
        }

        private void txtFilter_TypingPaused(string text) {
            this.InvokeIfRequired(() => {
                FilterList(text);
            });
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
                var phrase = obj as Phrase;
                return phrase.PhraseText.ToLower().Contains(text);
            };

            dataView.Refresh();
        }

        public Phrase SelectedPhrase {
            get { return lst.SelectedItem as Phrase; }
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
            Config.RestoreWindowPosition(User, this);
            lst.Focus();
        }

        public User User { get; private set; }
        public int CategoryId { get; private set; }
        protected SupportService Service { get; private set; }

        private void Window_Deactivated(object sender, EventArgs e) {
            Config.SaveWindowPosition(User, this);
        }

        private void btnAddNew_Click(object sender, RoutedEventArgs e) {
            AddNewPhrase();
        }

        private void AddNewPhrase() {
            InputBox.Show(this, "Add a new phrase value", "Enter the new phrase value, and click OK", (phrase) => {
                // Save the new phrase value...
                Service.AddPhrase(CategoryId, phrase);
                // reload the model...
                LoadModel();
            });
        }
    }
}
