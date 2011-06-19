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
using BioLink.Data.Model;
using BioLink.Data;
using BioLink.Client.Utilities;
using System.Collections.ObjectModel;
using Newtonsoft.Json;

namespace BioLink.Client.Extensibility {
    /// <summary>
    /// Interaction logic for PinBoard.xaml
    /// </summary>
    public partial class PinBoard : UserControl {

        private ObservableCollection<ViewModelBase> _model = new ObservableCollection<ViewModelBase>();
        private bool _IsDragging;
        private Point _startPoint;

        #region Designer Constructor
        public PinBoard() {
            InitializeComponent();
        }
        #endregion

        public PinBoard(IBioLinkPlugin owner) {
            this.Owner = owner;
            InitializeComponent();
            this.DataContext = _model;
            lvw.ItemsSource = _model;
            lvw.MouseRightButtonUp +=new MouseButtonEventHandler((s,e) => { ShowPopupMenu(); } );
            this.AllowDrop = true;

            lvw.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(lvw_PreviewMouseLeftButtonDown);
            lvw.PreviewMouseMove += new MouseEventHandler(lvw_PreviewMouseMove);

            // this.GiveFeedback += new GiveFeedbackEventHandler(PinBoard_GiveFeedback);
            this.PreviewDragOver += new DragEventHandler(PinBoard_PreviewDragEnter);
            this.PreviewDragEnter += new DragEventHandler(PinBoard_PreviewDragEnter);

            this.Drop += new DragEventHandler(PinBoard_Drop);
        }

        void lvw_PreviewMouseMove(object sender, MouseEventArgs e) {
            CommonPreviewMouseMove(e, lvw);
        }

        void lvw_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            _startPoint = e.GetPosition(lvw);
        }

        private void CommonPreviewMouseMove(MouseEventArgs e, ListView listView) {

            if (_startPoint == null) {
                return;
            }

            if (e.LeftButton == MouseButtonState.Pressed && !_IsDragging) {
                Point position = e.GetPosition(listView);
                if (Math.Abs(position.X - _startPoint.X) > SystemParameters.MinimumHorizontalDragDistance || Math.Abs(position.Y - _startPoint.Y) > SystemParameters.MinimumVerticalDragDistance) {
                    if (listView.SelectedItem != null) {

                        ListViewItem item = lvw.ItemContainerGenerator.ContainerFromItem(listView.SelectedItem) as ListViewItem;
                        if (item != null) {
                            StartDrag(e, lvw, item);
                        }
                    }
                }
            }
        }

        private void StartDrag(MouseEventArgs mouseEventArgs, ListView listView, ListViewItem item) {

            var selected = listView.SelectedItem as ViewModelBase;
            if (selected != null) {
                var data = new DataObject("Pinnable", selected);
                var pinnable = selected.Tag as PinnableObject;
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

        void PinBoard_Drop(object sender, DragEventArgs e) {
            if (!_IsDragging) {
                var pinnable = e.Data.GetData(PinnableObject.DRAG_FORMAT_NAME) as PinnableObject;
                if (pinnable != null) {
                    Pin(pinnable);
                }
            }
        }

        void PinBoard_PreviewDragEnter(object sender, DragEventArgs e) {

            if (!_IsDragging) {
                var pinnable = e.Data.GetData(PinnableObject.DRAG_FORMAT_NAME) as PinnableObject;
                if (pinnable != null) {
                    e.Effects = DragDropEffects.Link;
                } else {
                    e.Effects = DragDropEffects.None;
                }                
            } else {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
            }
        }

        private void ShowPopupMenu() {

            ViewModelBase viewmodel = lvw.SelectedItem as ViewModelBase;

            if (viewmodel == null) {
                return;
            }

            ContextMenu menu = new ContextMenu();
            MenuItemBuilder builder = new MenuItemBuilder();

            menu.Items.Add(builder.New("_Unpin").Handler(() => { Unpin(viewmodel); }).MenuItem);

            var list = new List<ViewModelBase>();
            list.Add(viewmodel);
            var commands = PluginManager.Instance.SolicitCommandsForObjects(list);


            if (commands != null && commands.Count > 0) {
                menu.Items.Add(new Separator());
                foreach (Command loopvar in commands) {
                    Command cmd = loopvar; // include this in the closure scope, loopvar is outside, hence will always point to the last item...
                    if (cmd is CommandSeparator) {
                        menu.Items.Add(new Separator());
                    } else {
                        menu.Items.Add(builder.New(cmd.Caption).Handler(() => { cmd.CommandAction(viewmodel); }).MenuItem);
                    }
                }                
            }

            if (menu.HasItems) {
                lvw.ContextMenu = menu;
            }
        }

        internal void Unpin(ViewModelBase model) {
            _model.Remove(model);
        }

        public void PersistPinnedItems() {
            IEnumerable<PinnableObject> items = _model.Select((vm) => vm.Tag as PinnableObject);
            Config.SetProfile(Owner.User, "Pinboard.PinnedItems", items);
        }

        public void InitializePinBoard() {
            List<PinnableObject> items = Config.GetProfile(Owner.User, "Pinboard.PinnedItems", new List<PinnableObject>());
            foreach (PinnableObject pinnable in items) {
                Pin(pinnable);
            }
        }

        public void RefreshPinBoard() {
            this.InvokeIfRequired(() => {
                // First save the state...
                PersistPinnedItems();
                // Now clear the state...
                _model.Clear();
                // and reload the items...
                InitializePinBoard();
            });
        }

        public void Pin(PinnableObject pinnable) {
            this.InvokeIfRequired(() => {
                ViewModelBase model = GetViewModelForPinnable(pinnable);
                if (model != null) {
                    model.Tag = pinnable;
                    _model.Add(model);
                }
            });
        }

        private ViewModelBase GetViewModelForPinnable(PinnableObject pinnable) {
            IBioLinkPlugin plugin = PluginManager.Instance.PluginByName(pinnable.PluginID);
            if (plugin != null) {
                return plugin.CreatePinnableViewModel(pinnable);
            } else {
                throw new Exception("Could not find a plugin with the name " + pinnable.PluginID);
            }
        }

        public IBioLinkPlugin Owner { get; private set; }

    }

    public class PinnableObject {

        public const string DRAG_FORMAT_NAME = "BioLinkPinnable";

        public PinnableObject(string pluginId, LookupType lookupType, int objectID, object state = null) {
            this.PluginID = pluginId;
            this.ObjectID = objectID;
            this.LookupType = lookupType;
            if (state != null) {
                SetState(state);
            }
        }

        public int ObjectID { get; private set; }

        public string PluginID { get; private set; }

        public LookupType LookupType { get; private set; }

        public string StateData { get; set; }

        public void SetState(object state) {
            this.StateData = JsonConvert.SerializeObject(state);
        }

        public T GetState<T>() {
            if (StateData != null) {
                return JsonConvert.DeserializeObject<T>(StateData);
            }
            return default(T);
        }


    }

}
