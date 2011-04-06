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
    /// Interaction logic for LookupControl.xaml
    /// </summary>
    public partial class LookupControl : UserControl {

        private bool _manualSet = false;        

        public LookupControl() {
            InitializeComponent();

            this.GotFocus += new RoutedEventHandler((source, e) => {
                txt.Focus();
            });

            txt.PreviewDragEnter += new DragEventHandler(txt_PreviewDragEnter);
            txt.PreviewDragOver += new DragEventHandler(txt_PreviewDragEnter);

            txt.Drop += new DragEventHandler(txt_Drop);

            txt.PreviewLostKeyboardFocus += new KeyboardFocusChangedEventHandler(txt_PreviewLostKeyboardFocus);

            txt.TextChanged += new TextChangedEventHandler((source, e) => {
                this.Text = txt.Text;
            });

            txt.PreviewKeyDown += new KeyEventHandler(txt_PreviewKeyDown);
        }

        private bool _validate = true;

        void txt_PreviewKeyDown(object sender, KeyEventArgs e) {

            if (e.Key == Key.Return ) {
                DoFind(txt.Text);
                e.Handled = true;
            }

            if (e.Key == Key.Space && (Keyboard.Modifiers & ModifierKeys.Control) > 0) {
                e.Handled = true;
                DoFind(txt.Text + "%");
            }

        }

        private void DoFind(string filter) {
            _validate = false;
            var service = new SupportService(User);
            var lookupResults = service.LookupSearch(filter, LookupType);
            if (lookupResults != null && lookupResults.Count >= 1) {

                GeneralTransform transform = txt.TransformToAncestor(this);
                var rootPoint = txt.PointToScreen(transform.Transform(new Point(0, 0)));

                var frm = new LookupResults(lookupResults);
                frm.Owner = this.FindParentWindow();

                    
                frm.Top =  rootPoint.Y + txt.ActualHeight;
                frm.Left = rootPoint.X;
                frm.Width = txt.ActualWidth;
                frm.Height = 250;

                if (frm.ShowDialog().GetValueOrDefault(false)) {
                    _manualSet = true;
                    Text = frm.SelectedItem.Label;
                    ObjectID = frm.SelectedItem.ObjectID;
                    _manualSet = false;
                }

            }
            _validate = true;            
        }

        void txt_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) {
            if (_validate) {
                if (!txt.IsReadOnly) {
                    if (!ValidateLookup()) {
                        e.Handled = true;
                    }
                }
            }
        }

        private bool ValidateLookup() {
            
            if (string.IsNullOrWhiteSpace(txt.Text)) {
                ObjectID = 0;
                return true;
            }
            
            var service = new SupportService(User);
            var lookupResults = service.LookupSearch(txt.Text, LookupType);
            if (lookupResults != null && lookupResults.Count >= 1) {
                if (lookupResults.Count == 1) {
                    var result = lookupResults[0];
                    this.ObjectID = result.ObjectID;
                    return result.Label.Equals(txt.Text);
                } 
            }
            
            return false;            
        }

        void txt_Drop(object sender, DragEventArgs e) {
            var pinnable = e.Data.GetData(PinnableObject.DRAG_FORMAT_NAME) as PinnableObject;
            if (pinnable != null) {
                var plugin = PluginManager.Instance.GetPluginByName(pinnable.PluginID);
                if (plugin != null) {                    
                    var viewModel = plugin.CreatePinnableViewModel(pinnable);
                    _manualSet = true;
                    this.Text = viewModel.DisplayLabel;
                    this.ObjectID = pinnable.ObjectID;                    
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

        private void GenericLookup<T>() {
            PluginManager.Instance.StartSelect<T>((result) => {
                _manualSet = true;
                this.Text = result.Description;
                this.ObjectID = result.ObjectID;
                this.InvokeIfRequired(() => {
                    txt.Focus();
                });
                if (ObjectSelected != null) {
                    ObjectSelected(this, result);
                }
                _manualSet = false;
            });

        }

        private void LaunchLookup() {
            
            switch (LookupType) {
                case LookupType.Reference:
                    GenericLookup<ReferenceSearchResult>();
                    break;
                case LookupType.Region:
                    GenericLookup<Region>();
                    break;
                case LookupType.Trap:
                    GenericLookup<Trap>();
                    break;
                case LookupType.Material:
                    GenericLookup<Material>();
                    break;
                case LookupType.Site:
                    GenericLookup <Site>();
                    break;
                case LookupType.SiteVisit:
                    GenericLookup<SiteVisit>();
                    break;
                case LookupType.Taxon:
                    GenericLookup<Taxon>();
                    break;
                case LookupType.Journal:
                    GenericLookup<Journal>();
                    break;
                default:
                    throw new Exception("Unhandled Lookup type: " + LookupType.ToString());
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

        public static readonly DependencyProperty IsReadOnlyProperty = DependencyProperty.Register("IsReadOnly", typeof(bool), typeof(LookupControl), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(OnIsReadOnlyChanged)));

        public bool IsReadOnly {
            get { return (bool)GetValue(IsReadOnlyProperty); }
            set { SetValue(IsReadOnlyProperty, value); }
        }

        private static void OnIsReadOnlyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args) {
            var control = (LookupControl)obj;
            if (control != null) {
                control.btnLookup.IsEnabled = !(bool)args.NewValue;
                // TODO: when filtering in the textbox is enabled, need to disable it when read only...
                // control.txt.IsEnabled = !(bool)args.NewValue;
            }
        }


        #endregion

        #region Properties

        public User User { get; private set; }

        public LookupType LookupType { get; private set; }

        #endregion

        #region Events

        public event TextChangedHandler ValueChanged;

        public event ObjectIDChangedHandler ObjectIDChanged;

        public event ObjectSelectedHandler ObjectSelected;

        #endregion

        private void btnAccept_Click(object sender, RoutedEventArgs e) {

        }

        private void btnCanel_Click(object sender, RoutedEventArgs e) {

        }

    }

    public delegate void ObjectIDChangedHandler(object source, int? objectID);

    public delegate void ObjectSelectedHandler(object source, SelectionResult result);

}
