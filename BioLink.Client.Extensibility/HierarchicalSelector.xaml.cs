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
using BioLink.Client.Utilities;
using System.Collections.ObjectModel;

namespace BioLink.Client.Extensibility {

    /// <summary>
    /// Interaction logic for HierarchicalSelector.xaml
    /// </summary>
    public partial class HierarchicalSelector : Window {

        private IHierarchicalSelectorContentProvider _content;

        #region Designer Constructor

        public HierarchicalSelector() {
            InitializeComponent();
        }

        #endregion

        public HierarchicalSelector(User user, IHierarchicalSelectorContentProvider contentProvider, Action<SelectionResult> selectAction) {
            InitializeComponent();
            this.User = user;
            _content = contentProvider;
            if (_content != null) {
                this.Title = _content.Caption;
            }
            this.SelectedAction = selectAction;
            LoadTopLevel();
            Initialized += new EventHandler((s, e) => {
                txtFind.Focus();
            });

        }

        private void LoadTopLevel() {
            tvw.ItemsSource = LoadModel(null);
        }

        private ObservableCollection<HierarchicalViewModelBase> LoadModel(HierarchicalViewModelBase parent) {
            var model = new ObservableCollection<HierarchicalViewModelBase>();
            if (_content != null) {
                var list = _content.LoadModel(parent);
                foreach (HierarchicalViewModelBase vm in list) {
                    vm.Children.Add(new ViewModelPlaceholder("Loading..."));
                    vm.LazyLoadChildren += new HierarchicalViewModelAction((p) => {
                        p.Children.Clear();
                        var children = LoadModel(p);
                        foreach (HierarchicalViewModelBase child in children) {
                            p.Children.Add(child);
                        }
                    });
                }
                model = new ObservableCollection<HierarchicalViewModelBase>(list);
            }
            return model;
        }

        private void tvw_MouseRightButtonDown(object sender, MouseButtonEventArgs e) {
            TreeViewItem item = sender as TreeViewItem;
            if (item != null) {
                item.Focus();
                e.Handled = true;
            }
        }

        private void TreeViewItem_MouseRightButtonDown(object sender, MouseEventArgs e) {
        }

        private void EditableTextBlock_EditingComplete(object sender, string text) {
        }

        private void EditableTextBlock_EditingCancelled(object sender, string oldtext) {
        }

        private void txtFind_TypingPaused(string text) {
            if (_content == null) {
                return;
            }

            if (string.IsNullOrEmpty(txtFind.Text)) {
                tvw.Visibility = System.Windows.Visibility.Visible;
                lstSearchResults.Visibility = System.Windows.Visibility.Collapsed;
            } else {
                tvw.Visibility = System.Windows.Visibility.Collapsed;
                lstSearchResults.Visibility = System.Windows.Visibility.Visible;
            }

            var model = new ObservableCollection<HierarchicalViewModelBase>(_content.Search(txtFind.Text));
            lstSearchResults.ItemsSource = model;

        }

        private void txtFind_KeyUp(object sender, KeyEventArgs e) {
            if (e.Key == Key.Down) {
                if (lstSearchResults.IsVisible) {
                    lstSearchResults.SelectedIndex = 0;
                    if (lstSearchResults.SelectedItem != null) {
                        ListBoxItem item = lstSearchResults.ItemContainerGenerator.ContainerFromItem(lstSearchResults.SelectedItem) as ListBoxItem;
                        item.Focus();
                    }
                } else {
                    tvw.Focus();
                }
            }
        }

        private void btnSelect_Click(object sender, RoutedEventArgs e) {
            HierarchicalViewModelBase selected = null;
            if (tvw.IsVisible) {
                if (tvw.SelectedItem != null) {
                    selected = tvw.SelectedItem as HierarchicalViewModelBase;
                }
            } else if (lstSearchResults.IsVisible) {
                if (lstSearchResults.SelectedItem != null) {
                    selected = lstSearchResults.SelectedItem as HierarchicalViewModelBase;
                }
            }

            if (selected != null && Select(selected)) {
                this.DialogResult = true;
                this.Close();
            }

        }

        private bool Select(HierarchicalViewModelBase selected) {

            if (_content != null) {
                var result = _content.CreateSelectionResult(selected);
                if (SelectedAction != null) {
                    SelectedAction(result);
                    return true;
                }
            }

            return false;
        }

        #region Properties

        internal User User { get; private set; }

        internal Action<SelectionResult> SelectedAction { get; private set; }

        #endregion

    }

    public interface IHierarchicalSelectorContentProvider {

        string Caption { get; }

        List<HierarchicalViewModelBase> LoadModel(HierarchicalViewModelBase parent);

        List<HierarchicalViewModelBase> Search(string searchTerm);

        SelectionResult CreateSelectionResult(HierarchicalViewModelBase selectedItem);

    }
}
