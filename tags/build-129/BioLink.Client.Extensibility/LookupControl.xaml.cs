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

namespace BioLink.Client.Extensibility {
    /// <summary>
    /// Interaction logic for LookupControl.xaml
    /// </summary>
    public partial class LookupControl : UserControl {

        private bool _manualSet = false;

        public LookupControl() {
            InitializeComponent();

            this.GotFocus += new RoutedEventHandler((source, e) => {
                txt.Focus();
            });

            txt.IsReadOnly = true;

            txt.PreviewDragEnter += new DragEventHandler(txt_PreviewDragEnter);
            txt.PreviewDragOver += new DragEventHandler(txt_PreviewDragEnter);

            txt.Drop += new DragEventHandler(txt_Drop);
        }

        void txt_Drop(object sender, DragEventArgs e) {
            var pinnable = e.Data.GetData(PinnableObject.DRAG_FORMAT_NAME) as PinnableObject;
            if (pinnable != null) {
                var plugin = PluginManager.Instance.GetPluginByName(pinnable.PluginID);
                if (plugin != null) {                    
                    var viewModel = plugin.CreatePinnableViewModel(pinnable);
                    _manualSet = true;
                    this.ObjectID = pinnable.ObjectID;
                    this.Text = viewModel.DisplayLabel;
                    _manualSet = false;
                }
            }
        }

        void txt_PreviewDragEnter(object sender, DragEventArgs e) {
            var pinnable = e.Data.GetData(PinnableObject.DRAG_FORMAT_NAME) as PinnableObject;
            e.Effects = DragDropEffects.None;

            if (pinnable != null) {
                if (pinnable.LookupType == this.LookupType) {
                    e.Effects = DragDropEffects.Link;
                }
            } 

            e.Handled = true;            
        }

        public void BindUser(User user, LookupType lookupType) {
            User = user;
            LookupType = lookupType;
        }

        private void btnLookup_Click(object sender, RoutedEventArgs e) {
            LaunchLookup();
        }

        private void LaunchLookup() {
            Type t = null;
            switch (LookupType) {
                case LookupType.Reference:
                    t = typeof(ReferenceSearchResult);
                    break;
                case LookupType.Region:
                    t = typeof(Region);
                    break;
                case LookupType.Trap:
                    t = typeof(Trap);
                    break;
                case LookupType.Material:
                    t = typeof(Material);
                    break;
                case LookupType.Site:
                    t = typeof(Site);
                    break;
                case LookupType.SiteVisit:
                    t = typeof(SiteVisit);
                    break;
                case LookupType.Taxon:
                    t = typeof(Taxon);
                    break;
                default:
                    throw new Exception("Unhandled Lookup type: " + LookupType.ToString());
            }

            if (t != null) {
                PluginManager.Instance.StartSelect(t, (result) => {
                    _manualSet = true;
                    this.ObjectID = result.ObjectID;
                    this.Text = result.Description;                    
                    this.InvokeIfRequired(() => {
                        txt.Focus();
                    });
                    _manualSet = false;
                });
            }
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e) {
            EditObject();
        }

        private void EditObject() {
            if (ObjectID.GetValueOrDefault(-1) >= 0) {
                PluginManager.Instance.EditLookupObject(LookupType, ObjectID.Value);
            }
        }

        #region Dependency Properties

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(LookupControl), new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(OnTextChanged)));

        private static void OnTextChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args) {
            var control = (LookupControl)obj;
            control.txt.Text = args.NewValue as String;
            if (control.ValueChanged != null) {
                control.ValueChanged(control, control.txt.Text);
            }
        }

        public String Text {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public static readonly DependencyProperty ObjectIDProperty = DependencyProperty.Register("ObjectID", typeof(int?), typeof(LookupControl), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(OnObjectIDChanged)));

        private static void OnObjectIDChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args) {
            var control = (LookupControl) obj;
            if (control.ObjectIDChanged != null && control._manualSet) {
                control.ObjectIDChanged(control, control.ObjectID);
            }
        }

        public int? ObjectID {
            get { return (int?)GetValue(ObjectIDProperty); }
            set { SetValue(ObjectIDProperty, value); }
        }

        #endregion

        #region Properties

        public User User { get; private set; }

        public LookupType LookupType { get; private set; }

        #endregion

        #region Events

        public event TextChangedHandler ValueChanged;

        public event ObjectIDChangedHandler ObjectIDChanged;

        #endregion


    }

    public delegate void ObjectIDChangedHandler(object source, int? objectID);

    public enum LookupType {
        Unknown,
        Taxon,
        Region,
        Material,
        Site,
        SiteVisit,
        Trap,
        Reference
    }

}
