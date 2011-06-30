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
using BioLink.Client.Extensibility;
using BioLink.Client.Utilities;
using BioLink.Data;
using BioLink.Data.Model;
using System.Collections.ObjectModel;

namespace BioLink.Client.Tools {
    /// <summary>
    /// Interaction logic for ContactBrowsePage.xaml
    /// </summary>
    public partial class ContactBrowsePage : UserControl {

        private ObservableCollection<ContactViewModel> _model = new ObservableCollection<ContactViewModel>();
        private Point _startPoint;
        private bool _IsDragging;

        #region Designer ctor
        public ContactBrowsePage() {
            InitializeComponent();            
        }
        #endregion

        public ContactBrowsePage(User user) {
            InitializeComponent();
            lvw.MouseRightButtonUp += new MouseButtonEventHandler(lvw_MouseRightButtonUp);

            lvw.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(lvw_PreviewMouseLeftButtonDown);
            lvw.PreviewMouseMove += new MouseEventHandler(lvw_PreviewMouseMove);

            this.User = user;
            lvw.ItemsSource = _model;
        }

        void lvw_PreviewMouseMove(object sender, MouseEventArgs e) {
            CommonPreviewMouseMove(e, sender as ListView);
        }

        private void CommonPreviewMouseMove(MouseEventArgs e, ListView listView) {

            if (_startPoint == null) {
                return;
            }

            if (e.LeftButton == MouseButtonState.Pressed && !_IsDragging) {
                Point position = e.GetPosition(listView);
                if (Math.Abs(position.X - _startPoint.X) > SystemParameters.MinimumHorizontalDragDistance || Math.Abs(position.Y - _startPoint.Y) > SystemParameters.MinimumVerticalDragDistance) {

                    var x = listView.InputHitTest(position) as FrameworkElement;
                    if (x != null && x.DataContext is ContactViewModel) {
                        if (listView.SelectedItem != null) {

                            ListViewItem item = listView.ItemContainerGenerator.ContainerFromItem(listView.SelectedItem) as ListViewItem;
                            if (item != null) {
                                StartDrag(e, listView, item);
                            }
                        }
                    }
                }
            }
        }

        private void StartDrag(MouseEventArgs mouseEventArgs, ListView listView, ListViewItem item) {

            var selected = listView.SelectedItem as ContactViewModel;
            if (selected != null) {
                var data = new DataObject("Pinnable", selected);

                var pinnable = new PinnableObject(ToolsPlugin.TOOLS_PLUGIN_NAME, LookupType.Contact, selected.ContactID);
                data.SetData(PinnableObject.DRAG_FORMAT_NAME, pinnable);
                data.SetData(DataFormats.Text, selected.DisplayLabel);

                try {
                    _IsDragging = true;
                    DragDrop.DoDragDrop(item, data, DragDropEffects.Copy | DragDropEffects.Move | DragDropEffects.Link);
                } finally {
                    _IsDragging = false;
                }
            }

            InvalidateVisual();
        }

        void lvw_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            _startPoint = e.GetPosition(lvw);
        }

        void lvw_MouseRightButtonUp(object sender, MouseButtonEventArgs e) {
            var selected = lvw.SelectedItem as ContactViewModel;
            if (ContextMenuRequested != null) {
                ContextMenuRequested(lvw, selected);
            }
        }

        public void LoadPage(string range) {

            lblPageHeader.Content = string.Format("Contacts - {0}", range);

            string[] bits = range.Split('-');
            _model.Clear();
            if (bits.Length == 2) {
                string from = bits[0];
                string to = bits[1];

                var service = new LoanService(User);
                var list = service.ListContactsRange(from, to);                
                foreach (Contact contact in list) {
                    _model.Add(new ContactViewModel(contact));
                }

            }            
        }

        public event Action<FrameworkElement, ContactViewModel> ContextMenuRequested;

        public void Clear() {
            _model.Clear();
        }

        public User User { get; private set; }

        public ContactViewModel SelectedItem {
            get { return lvw.SelectedItem as ContactViewModel; }
        }

        public ObservableCollection<ContactViewModel> Model { get { return _model; } }
    }
}
