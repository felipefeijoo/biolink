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
        private IOneToManyDetailController _controller;
        private bool _rdeMode;

        #region Designer Constructor
        public OneToManyControl() {
            InitializeComponent();
        }
        #endregion

        public OneToManyControl(IOneToManyDetailController controller, bool rdeMode = false)
            : base(PluginManager.Instance.User, "OneToManyControl" + Guid.NewGuid().ToString()) {

            InitializeComponent();
            _rdeMode = rdeMode;
            _controller = controller;
            _controller.Host = this;

            detailsGrid.DataContextChanged += new DependencyPropertyChangedEventHandler(detailsGrid_DataContextChanged);
            lst.SelectionChanged += new SelectionChangedEventHandler(lst_SelectionChanged);

            detailsGrid.IsEnabled = false;

            ChangesCommitted += new PendingChangesCommittedHandler(OneToManyControl_ChangesCommitted);

            lst.PreviewDragEnter += new DragEventHandler(lst_PreviewDragOver);
            lst.PreviewDragOver += new DragEventHandler(lst_PreviewDragOver);
            lst.Drop += new DragEventHandler(lst_Drop);

        }

        void detailsGrid_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            if (_controller != null) {
                detailsGrid.Children.Clear();
                var editor = _controller.GetDetailEditor(detailsGrid.DataContext as ViewModelBase);
                if (editor != null) {
                    detailsGrid.Children.Add(editor);
                }
            }
        }

        void lst_Drop(object sender, DragEventArgs e) {
            var pinnable = e.Data.GetData(PinnableObject.DRAG_FORMAT_NAME) as PinnableObject;
            if (pinnable != null && _controller.AcceptDroppedPinnable(pinnable)) {
                var viewModel = AddNew();
                if (viewModel != null) {
                    _controller.PopulateFromPinnable(viewModel, pinnable);
                    viewModel.DataChanged += new DataChangedHandler(viewModel_DataChanged);
                }
                e.Handled = true;
            }
        }

        void lst_PreviewDragOver(object sender, DragEventArgs e) {
            var pinnable = e.Data.GetData(PinnableObject.DRAG_FORMAT_NAME) as PinnableObject;
            e.Effects = DragDropEffects.None;
            if (pinnable != null) {
                if (_controller.AcceptDroppedPinnable(pinnable)) {
                    e.Effects = DragDropEffects.Link;
                }
            }
            e.Handled = true;
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

        protected ViewModelBase Owner { get; set; }

        public void SetModel(ViewModelBase owner, ObservableCollection<ViewModelBase> model) {
            _model = model;
            lst.ItemsSource = _model;
            this.Owner = owner;
            _controller.Owner = owner;

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

        public UIElement DetailControl {
            get {
                if (detailsGrid.Children.Count > 0) {
                    return detailsGrid.Children[0];
                }
                return null;
            }
        }

        void lst_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            detailsGrid.IsEnabled = !IsReadOnly && lst.SelectedItem != null;
        }

        private void DeleteSelected() {
            var selected = lst.SelectedItem as ViewModelBase;
            if (selected != null && _controller != null) {
                _model.Remove(selected);
                var action = _controller.PrepareDeleteAction(selected);
                if (action != null) {
                    RegisterPendingChange(action);
                }
            }
        }

        public ViewModelBase AddNew() {
            if (_controller != null) {
                DatabaseCommand command = null;
                var viewModel = _controller.AddNewItem(out command);
                if (viewModel != null) {
                    _model.Add(viewModel);
                    viewModel.DataChanged +=new DataChangedHandler(viewModel_DataChanged);
                    if (command != null) {
                        RegisterPendingChange(command);
                    }
                    lst.SelectedItem = viewModel;

                    if (detailsGrid.Children.Count > 0) {
                        var editor = detailsGrid.Children[0] as IOneToManyDetailEditor;
                        if (editor != null) {
                            if (editor.FirstControl != null) {
                                if (editor.FirstControl is PickListControl) {
                                    FocusHelper.Focus((editor.FirstControl as PickListControl).txt);
                                } else {
                                    FocusHelper.Focus(editor.FirstControl);
                                }
                            }
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
            if (_controller != null && !_rdeMode) {
                detailsGrid.IsEnabled = false;

                var list = _controller.LoadModel();
                _model = new ObservableCollection<ViewModelBase>(list.ConvertAll((viewModel) => {
                    viewModel.DataChanged += new DataChangedHandler(viewModel_DataChanged);
                    return viewModel;
                }));

                _model.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(_model_CollectionChanged);

                lst.ItemsSource = _model;

                if (_model.Count > 0) {
                    lst.SelectedItem = _model[0];
                }

                IsPopulated = true;

                if (ModelChanged != null) {
                    ModelChanged(_model);
                }
            }
        }

        void _model_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            if (ModelChanged != null) {
                ModelChanged(_model);
            }
        }

        void viewModel_DataChanged(ChangeableModelBase viewmodel) {
            if (_controller != null) {

                var vm = viewmodel as ViewModelBase;
                if (vm == null || vm.ObjectID > 0) {
                    var action = _controller.PrepareUpdateAction(viewmodel as ViewModelBase);
                    if (action != null) {
                        RegisterUniquePendingChange(action);
                    }
                }
            }

            if (ModelChanged != null) {
                ModelChanged(_model);
            }
        }

        public event Action<Collection<ViewModelBase>> ModelChanged; 

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
                control.detailsGrid.IsEnabled = !val;
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
