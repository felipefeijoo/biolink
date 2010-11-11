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
using System.Collections.ObjectModel;

namespace BioLink.Client.Extensibility {
    /// <summary>
    /// Interaction logic for PinBoard.xaml
    /// </summary>
    public partial class PinBoard : UserControl {

        private ObservableCollection<ViewModelBase> _model = new ObservableCollection<ViewModelBase>();        

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

            // this.GiveFeedback += new GiveFeedbackEventHandler(PinBoard_GiveFeedback);
            this.PreviewDragOver += new DragEventHandler(PinBoard_PreviewDragEnter);
            this.PreviewDragEnter += new DragEventHandler(PinBoard_PreviewDragEnter);

            this.Drop += new DragEventHandler(PinBoard_Drop);
        }

        void PinBoard_Drop(object sender, DragEventArgs e) {
            var pinnable = e.Data.GetData(PinnableObject.DRAG_FORMAT_NAME) as PinnableObject;
            if (pinnable != null) {
                Pin(pinnable);
            }            
        }

        void PinBoard_PreviewDragEnter(object sender, DragEventArgs e) {

            var pinnable = e.Data.GetData(PinnableObject.DRAG_FORMAT_NAME) as PinnableObject;
            if (pinnable != null) {
                e.Effects = DragDropEffects.Link;
            } else {
                e.Effects = DragDropEffects.None;
            }
            e.Handled = true;
        }

        private void ShowPopupMenu() {

            ViewModelBase viewmodel = lvw.SelectedItem as ViewModelBase;

            if (viewmodel == null) {
                return;
            }

            ContextMenu menu = new ContextMenu();
            MenuItemBuilder builder = new MenuItemBuilder();

            menu.Items.Add(builder.New("_Unpin").Handler(() => { Unpin(viewmodel); }).MenuItem);

            var commands = PluginManager.Instance.SolicitCommandsForObject(viewmodel);

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

        public void Pin(PinnableObject pinnable) {            
            ViewModelBase model = GetViewModelForPinnable(pinnable);
            if (model != null) {
                model.Tag = pinnable;
                _model.Add(model);
            }
        }

        private ViewModelBase GetViewModelForPinnable(PinnableObject pinnable) {
            IBioLinkPlugin plugin = PluginManager.Instance.PluginByName(pinnable.PluginID);
            if (plugin != null) {
                return plugin.CreatePinnableViewModel(pinnable.State);
            } else {
                throw new Exception("Could not find a plugin with the name " + pinnable.PluginID);
            }
        }

        public IBioLinkPlugin Owner { get; private set; }

    }

    public class PinnableObject {

        public const string DRAG_FORMAT_NAME = "BioLinkPinnable";

        public PinnableObject(string pluginId, object state) {
            this.PluginID = pluginId;
            this.State = state;
        }

        public object State { get; set; } 

        public string PluginID { get; set; } 
    }

}
