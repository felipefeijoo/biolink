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

        //private OneToManyDetailControl _defaultControl;
        private ObservableCollection<ViewModelBase> _model;
        private OneToManyDetailControl _defaultDetailControl;
        private OneToManyDetailControl _currentDetailControl;
        private bool _rdeMode;
        private Func<object, OneToManyDetailControl, OneToManyDetailControl> _controlFactory;

        #region Designer Constructor
        public OneToManyControl() {
            InitializeComponent();
        }
        #endregion

        public OneToManyControl(OneToManyDetailControl defaultDetailControl, Func<object, OneToManyDetailControl, OneToManyDetailControl> detailControlFactory = null, bool rdeMode = false)
            : base(PluginManager.Instance.User, "OneToManyControl" + Guid.NewGuid().ToString()) {

            InitializeComponent();
            _rdeMode = rdeMode;
            _defaultDetailControl = defaultDetailControl;
            _currentDetailControl = defaultDetailControl;

            _controlFactory = detailControlFactory;            
            detailsGrid.DataContextChanged += new DependencyPropertyChangedEventHandler(detailsGrid_DataContextChanged);
            lst.SelectionChanged += new SelectionChangedEventHandler(lst_SelectionChanged);
            
            detailsGrid.IsEnabled = false;

            ChangesCommitted += new PendingChangesCommittedHandler(OneToManyControl_ChangesCommitted);

            lst.PreviewDragEnter += new DragEventHandler(lst_PreviewDragOver);
            lst.PreviewDragOver += new DragEventHandler(lst_PreviewDragOver);
            lst.Drop += new DragEventHandler(lst_Drop);

        }

        void detailsGrid_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            if (_controlFactory != null) {
                _currentDetailControl = _controlFactory(detailsGrid.DataContext, _defaultDetailControl);
                detailsGrid.Children.Clear();
                if (_currentDetailControl != null) {
                    _currentDetailControl.Host = this;
                    if (_currentDetailControl.Owner == null) {
                        _currentDetailControl.Owner = Owner;
                    }
                    detailsGrid.Children.Add(_currentDetailControl);
                }
            }

        }

        void lst_Drop(object sender, DragEventArgs e) {
            var pinnable = e.Data.GetData(PinnableObject.DRAG_FORMAT_NAME) as PinnableObject;            
            if (pinnable != null) {
                var viewModel = AddNew();
                if (viewModel != null) {
                    CurrentDetailControl.PopulateFromPinnable(viewModel, pinnable);
                }
            }                            
            
        }

        void lst_PreviewDragOver(object sender, DragEventArgs e) {
            var pinnable = e.Data.GetData(PinnableObject.DRAG_FORMAT_NAME) as PinnableObject;
            e.Effects = DragDropEffects.None;
            if (pinnable != null) {
                if (CurrentDetailControl.AcceptDroppedPinnable(pinnable)) {
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

        public OneToManyDetailControl CurrentDetailControl {
            get { return _currentDetailControl == null ? _defaultDetailControl : _currentDetailControl; }
        }

        protected ViewModelBase Owner { get; set; }

        public void SetModel(ViewModelBase owner, ObservableCollection<ViewModelBase> model) {
            _model = model;
            lst.ItemsSource = _model;
            this.Owner = owner;
            _defaultDetailControl.Owner = owner;
            if (_currentDetailControl != null) {
                _currentDetailControl.Owner = owner;
            }
            
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
            if (selected != null && CurrentDetailControl != null) {
                _model.Remove(selected);
                var action = CurrentDetailControl.PrepareDeleteAction(selected);
                if (action != null) {
                    RegisterPendingChange(action);
                }
            }
        }

        public ViewModelBase AddNew() {
            if (CurrentDetailControl != null) {
                DatabaseCommand command = null;
                var viewModel = CurrentDetailControl.AddNewItem(out command);
                if (viewModel != null) {
                    _model.Add(viewModel);
                    if (command != null) {
                        RegisterPendingChange(command);
                    }
                    lst.SelectedItem = viewModel;

                    if (CurrentDetailControl.FirstControl != null) {
                        if (CurrentDetailControl.FirstControl is PickListControl) {
                            FocusHelper.Focus((CurrentDetailControl.FirstControl as PickListControl).txt);
                        } else {
                            FocusHelper.Focus(CurrentDetailControl.FirstControl);
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
            if (CurrentDetailControl != null && !_rdeMode) {
                detailsGrid.IsEnabled = false;

                var list = CurrentDetailControl.LoadModel();
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
            if (CurrentDetailControl != null) {
                var action = CurrentDetailControl.PrepareUpdateAction(viewmodel as ViewModelBase);
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
                control.CurrentDetailControl.IsEnabled = !val;
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
