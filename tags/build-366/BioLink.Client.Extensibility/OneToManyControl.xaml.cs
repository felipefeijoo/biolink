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
    public partial class OneToManyControl : DatabaseCommandControl, ILazyPopulateControl {

        private OneToManyDetailControl _control;
        private ObservableCollection<ViewModelBase> _model;
        private bool _rdeMode;        

        #region Designer Constructor
        public OneToManyControl() {
            InitializeComponent();
        }
        #endregion

        public OneToManyControl(OneToManyDetailControl control, bool rdeMode = false) : base(control.User, control.ContentID) {

            InitializeComponent();
            _rdeMode = rdeMode;
            _control = control;
            detailsGrid.Children.Add(_control);
            lst.SelectionChanged += new SelectionChangedEventHandler(lst_SelectionChanged);

            control.Host = this;
            detailsGrid.IsEnabled = false;

            ChangesCommitted += new PendingChangesCommittedHandler(OneToManyControl_ChangesCommitted);

            lst.PreviewDragEnter += new DragEventHandler(lst_PreviewDragOver);
            lst.PreviewDragOver += new DragEventHandler(lst_PreviewDragOver);
            lst.Drop += new DragEventHandler(lst_Drop);

        }

        void lst_Drop(object sender, DragEventArgs e) {
            var pinnable = e.Data.GetData(PinnableObject.DRAG_FORMAT_NAME) as PinnableObject;            
            if (pinnable != null) {
                var viewModel = AddNew();
                if (viewModel != null) {
                    _control.PopulateFromPinnable(viewModel, pinnable);
                }
            }                            
            
        }

        void lst_PreviewDragOver(object sender, DragEventArgs e) {
            var pinnable = e.Data.GetData(PinnableObject.DRAG_FORMAT_NAME) as PinnableObject;
            e.Effects = DragDropEffects.None;
            if (pinnable != null) {
                if (_control.AcceptDroppedPinnable(pinnable)) {
                    e.Effects = DragDropEffects.Link;
                }
            }                            
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

        public OneToManyDetailControl DetailControl { get { return _control; } }

        protected ViewModelBase Owner { get; set; }

        public void SetModel(ViewModelBase owner, ObservableCollection<ViewModelBase> model) {
            _model = model;
            lst.ItemsSource = _model;
            _control.Owner = owner;
            if (_model.Count > 0) {
                lst.SelectedItem = _model[0];
            } else {
                detailsGrid.IsEnabled = false;
            }
        }

        public void SetSelectedItem(ViewModelBase selected) {
            if (selected != null) {
                lst.SelectedItem = selected;
            }
        }

        public ViewModelBase FindItem(Predicate<ViewModelBase> predicate) {
            if (_model != null) {
                foreach (ViewModelBase item in _model) {
                    if (predicate(item)) {
                        return item;
                    }
                }
            }
            return null;
        }

        public ObservableCollection<ViewModelBase> GetModel() {
            return _model;
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

        private ViewModelBase AddNew() {
            if (_control != null) {
                DatabaseCommand command = null;
                var viewModel = _control.AddNewItem(out command);
                if (viewModel != null) {
                    _model.Add(viewModel);
                    if (command != null) {
                        RegisterPendingChange(command);
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
                return viewModel;
            }

            return null;
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e) {
            DeleteSelected();
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e) {
            AddNew();
            e.Handled = true;
        }

        private void LoadModel() {
            if (_control != null && !_rdeMode) {
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

        public void AddButtonRHS(FrameworkElement control) {
            control.Margin = new Thickness(6, 0, 0, 0);
            pnlButtonsRHS.Children.Add(control);
        }

        public static readonly DependencyProperty IsReadOnlyProperty = DependencyProperty.Register("IsReadOnly", typeof(bool), typeof(OneToManyControl), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnIsReadOnlyChanged));

        public bool IsReadOnly {
            get { return (bool)GetValue(IsReadOnlyProperty); }
            set { SetValue(IsReadOnlyProperty, value); }
        }

        private static void OnIsReadOnlyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args) {

            var control = obj as OneToManyControl;
            if (control != null) {
                bool val = (bool)args.NewValue;
                control._control.IsEnabled = !val;
                control.btnAdd.IsEnabled = !val;
                control.btnDelete.IsEnabled = !val;
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
