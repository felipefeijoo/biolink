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
using System.Threading;

namespace BioLink.Client.Extensibility {
    /// <summary>
    /// Interaction logic for OneToManyControl.xaml
    /// </summary>
    public partial class OneToManyControl : DatabaseActionControl, ILazyPopulateControl {

        private OneToManyDetailControl _control;
        private ObservableCollection<ViewModelBase> _model;

        #region Designer Constructor
        public OneToManyControl() {
            InitializeComponent();
        }
        #endregion

        public OneToManyControl(OneToManyDetailControl control)
            : base(control.User, control.ContentID) {

            InitializeComponent();

            _control = control;
            detailsGrid.Children.Add(_control);
            lst.SelectionChanged += new SelectionChangedEventHandler(lst_SelectionChanged);

            ChangesCommitted += new PendingChangesCommittedHandler(OneToManyControl_ChangesCommitted);
        }

        void OneToManyControl_ChangesCommitted(object sender) {
            int oldSelectedIndex = lst.SelectedIndex;
            LoadModel();
            if (oldSelectedIndex <= _model.Count && oldSelectedIndex >= 0) {
                lst.SelectedIndex = oldSelectedIndex;
            } else {
                if (_model.Count > 0) {
                    lst.SelectedIndex = 0;
                }
            }
            
        }

        void lst_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            detailsGrid.IsEnabled = lst.SelectedItem != null;
        }

        private void DeleteSelected() {
            var selected = lst.SelectedItem as ViewModelBase;
            if (selected != null && _control != null) {
                _model.Remove(selected);
                var action = _control.PrepareDeleteAction(selected);
                if (action != null) {
                    RegisterPendingChange(action);
                }
            }
        }

        private void AddNew() {
            if (_control != null) {
                DatabaseAction action = null;
                var viewModel = _control.AddNewItem(out action);
                if (viewModel != null) {
                    _model.Add(viewModel);
                    if (action != null) {
                        RegisterPendingChange(action);
                    }
                    lst.SelectedItem = viewModel;

                    if (_control.FirstControl != null) {
                        if (_control.FirstControl is PickListControl) {
                            FocusHelper.Focus((_control.FirstControl as PickListControl).txt);
                        } else {
                            FocusHelper.Focus(_control.FirstControl);
                        }
                    }
                }
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e) {
            DeleteSelected();
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e) {
            AddNew();
            e.Handled = true;
        }

        private void LoadModel() {
            if (_control != null) {
                detailsGrid.IsEnabled = false;

                var list = _control.LoadModel();
                _model = new ObservableCollection<ViewModelBase>(list.ConvertAll((viewModel) => {
                    viewModel.DataChanged += new DataChangedHandler(viewModel_DataChanged);
                    return viewModel;
                }));

                lst.ItemsSource = _model;

                if (_model.Count > 0) {
                    lst.SelectedItem = _model[0];
                }

                IsPopulated = true;
            }
        }

        void viewModel_DataChanged(ChangeableModelBase viewmodel) {
            if (_control != null) {
                var action = _control.PrepareUpdateAction(viewmodel as ViewModelBase);
                if (action != null) {
                    RegisterUniquePendingChange(action);
                }
            }
        }


        public bool IsPopulated { get; private set; }

        public void Populate() {

            if (!IsPopulated) {
                LoadModel();
            }

        }
    }

    static class FocusHelper {
        private delegate void MethodInvoker();

        public static void Focus(UIElement element) {
            //Focus in a callback to run on another thread, ensuring the main UI thread is initialized by the
            //time focus is set
            ThreadPool.QueueUserWorkItem(delegate(Object foo) {
                UIElement elem = (UIElement)foo;
                elem.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
                    (MethodInvoker)delegate() {
                    elem.Focus();
                    Keyboard.Focus(elem);
                });
            }, element);
        }
    }
}
