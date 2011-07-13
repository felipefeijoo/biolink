using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using BioLink.Client.Utilities;
using BioLink.Data;

namespace BioLink.Client.Extensibility {

    public class TreeViewDragHelper : GenericDragHelper<TreeView, TreeViewItem> {

        protected TreeViewDragHelper(TreeView treeView, Func<ViewModelBase, DataObject> func) : base(treeView, func) { }

        protected override ViewModelBase GetSelectedItem(TreeView control) {
            return control.SelectedItem as ViewModelBase;
        }

        public static void Bind(TreeView treeView, Func<ViewModelBase, DataObject> dataObjFactory) {
            new TreeViewDragHelper(treeView, dataObjFactory);
        }
    }

    public class ListBoxDragHelper : GenericDragHelper<ListBox, ListBoxItem> {

        public ListBoxDragHelper(ListBox control, Func<ViewModelBase, DataObject> func) : base(control, func) { }

        protected override ViewModelBase GetSelectedItem(ListBox control) {
            return control.SelectedItem as ViewModelBase;
        }

        public static void Bind(ListBox control, Func<ViewModelBase, DataObject> dataObjFactory) {
            new ListBoxDragHelper(control, dataObjFactory);
        }

    }

    public class ListViewDragHelper : GenericDragHelper<ListView, ListViewItem> {

        public ListViewDragHelper(ListView listView, Func<ViewModelBase, DataObject> func) : base(listView, func) { }

        protected override ViewModelBase GetSelectedItem(ListView control) {
            return control.SelectedItem as ViewModelBase;
        }

        public static void Bind(ListView listView, Func<ViewModelBase, DataObject> dataObjFactory) {
            new ListViewDragHelper(listView, dataObjFactory);
        }

    }

    public abstract class GenericDragHelper<T, ItemType> where T : ItemsControl where ItemType : class {

        private static List<T> _BoundControls = new List<T>();

        protected GenericDragHelper(T control, Func<ViewModelBase, DataObject> dataObjFactory) {
            if (control != null) {
                this.Control = control;
                this.CreateDataObject = dataObjFactory;
                control.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(Control_PreviewMouseLeftButtonDown);
                control.PreviewMouseMove += new MouseEventHandler(Control_PreviewMouseMove);
                DragDropEffects = DragDropEffects.Copy | DragDropEffects.Move | DragDropEffects.Link;
            }
        }

        void Control_PreviewMouseMove(object sender, MouseEventArgs e) {
            if (StartPoint == null) {
                return;
            }

            if (e.LeftButton == MouseButtonState.Pressed && !IsDragging) {
                Point position = e.GetPosition(Control);
                if (Math.Abs(position.X - StartPoint.X) > SystemParameters.MinimumHorizontalDragDistance || Math.Abs(position.Y - StartPoint.Y) > SystemParameters.MinimumVerticalDragDistance) {
                    var selected = GetSelectedItem(Control);
                    if (selected != null) {
                        IInputElement hitelement = Control.InputHitTest(StartPoint);
                        ItemType item = Control.FindAncestorOfType<ItemType>((FrameworkElement)hitelement);
                        if (item != null) {
                            StartDrag(e, item);
                        }
                    }
                }
            }
        }

        public static Func<ViewModelBase, DataObject> CreatePinnableGenerator(string pluginName, LookupType lookupType) {

            return new Func<ViewModelBase, DataObject>((selected) => {
                var data = new DataObject("Pinnable", selected);
                var pinnable = new PinnableObject(pluginName, lookupType, selected.ObjectID.GetValueOrDefault(0));
                data.SetData(PinnableObject.DRAG_FORMAT_NAME, pinnable);
                data.SetData(DataFormats.Text, selected.DisplayLabel);
                data.SetData(DataFormats.UnicodeText, selected.DisplayLabel);
                data.SetData(DataFormats.StringFormat, selected.DisplayLabel);
                return data;
            });

        }

        void Control_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            StartPoint = e.GetPosition(Control);
        }

        private void StartDrag(MouseEventArgs mouseEventArgs, ItemType item) {

            var selected = GetSelectedItem(Control);
            if (selected != null && CreateDataObject != null) {
                var data = CreateDataObject(selected);
                if (data != null) {
                    try {
                        IsDragging = true;
                        DragDrop.DoDragDrop(item as DependencyObject, data, this.DragDropEffects);
                    } finally {
                        IsDragging = false;
                    }
                }
            }

            Control.InvalidateVisual();
        }

        protected Func<ViewModelBase, DataObject> CreateDataObject { get; private set; }

        protected abstract ViewModelBase GetSelectedItem(T control);

        protected T Control { get; private set; }

        public bool IsDragging { get; private set; }

        protected Point StartPoint { get; private set; }

        public DragDropEffects DragDropEffects { get; set; }


    }
}
