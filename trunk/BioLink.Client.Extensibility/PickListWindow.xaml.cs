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

        private Func<IEnumerable<string>> _itemsFunc;
        private Func<String, bool> _addItemFunc;

        private ObservableCollection<String> _model;

        public PickListWindow(User user, string caption, Func<IEnumerable<string>> itemsFunc, Func<String, bool> addItemFunc) {
            _itemsFunc = itemsFunc;
            _addItemFunc = addItemFunc;
            this.User = user;
            InitializeComponent();
            Config.RestoreWindowPosition(user, this);
            Title = caption;
            LoadModel();

            btnAddNew.Visibility = System.Windows.Visibility.Hidden;

            if (_addItemFunc != null) {
                btnAddNew.Visibility = Visibility.Visible;
                btnAddNew.Click += new RoutedEventHandler((source, e) => {
                    string prefill = txtFilter.Text;
                    InputBox.Show(this, "Add a new value", "Enter the new value, and click OK", prefill, (text) => {
                        if (_addItemFunc(text)) {
                            _model.Add(text);
                            lst.SelectedItem = text;
                            this.DialogResult = true;
                            this.Hide();
                        }
                    });
                });
            }
        }

        public void LoadModel() {
            var list = _itemsFunc();
                        
            _model = new ObservableCollection<String>();
            foreach (string item in list) {                
                if (!String.IsNullOrWhiteSpace(item)) {
                    _model.Add(item.Trim());
                }
            }
            lst.ItemsSource = _model;
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

        private void lst_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
            if (lst.SelectedItem != null) {
                this.DialogResult = true;
                this.Hide();
            }
        }

        protected User User { get; private set; }

    }
}
