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

            txt.PreviewDrop += new DragEventHandler(txt_PreviewDrop);

            txt.PreviewLostKeyboardFocus += new KeyboardFocusChangedEventHandler(txt_PreviewLostKeyboardFocus);

            txt.TextChanged += new TextChangedEventHandler((source, e) => {
                this.Text = txt.Text;
            });

            txt.PreviewKeyDown += new KeyEventHandler(txt_PreviewKeyDown);

            txtWatermark.DataContext = this;

            EnforceLookup = true;
        }

        void txt_PreviewDrop(object sender, DragEventArgs e) {
            var pinnable = e.Data.GetData(PinnableObject.DRAG_FORMAT_NAME) as PinnableObject;
            if (pinnable != null) {
                var plugin = PluginManager.Instance.GetPluginByName(pinnable.PluginID);
                if (plugin != null) {
                    var viewModel = plugin.CreatePinnableViewModel(pinnable);
                    _manualSet = true;
                    this.Text = viewModel.DisplayLabel;
                    this.ObjectID = pinnable.ObjectID;
                    _manualSet = false;
                    e.Handled = true;
                }
            }            
        }

        private bool _validate = true;

        void txt_PreviewKeyDown(object sender, KeyEventArgs e) {

            if (e.Key == Key.Return) {
                DoFind(txt.Text);
                e.Handled = true;
            }

            if (e.Key == Key.Space && (Keyboard.Modifiers & ModifierKeys.Control) > 0) {
                e.Handled = true;
                DoFind(txt.Text);
            }

        }

        private bool DoFind(string filter) {

            if (string.IsNullOrEmpty(filter)) {
                return false;
            }

            _validate = false;
            var service = new SupportService(User);
            var lookupResults = service.LookupSearch(filter, LookupType);
            _validate = true;
            if (lookupResults != null) {
                if (lookupResults.Count > 1) {

                    if (!PluginManager.Instance.CheckSearchResults(lookupResults)) {
                        return false;
                    }

                    GeneralTransform transform = txt.TransformToAncestor(this);
                    var rootPoint = txt.PointToScreen(transform.Transform(new Point(0, 0)));

                    var frm = new LookupResults(lookupResults);
                    frm.Owner = this.FindParentWindow();

                    frm.Top = rootPoint.Y + txt.ActualHeight;
                    frm.Left = rootPoint.X;
                    frm.Width = txt.ActualWidth;
                    frm.Height = 250;

                    if (frm.ShowDialog().GetValueOrDefault(false)) {
                        _manualSet = true;
                        Text = frm.SelectedItem.Label;
                        ObjectID = frm.SelectedItem.ObjectID;
                        SelectedObject = frm.SelectedItem;
                        _manualSet = false;
                        return true;
                    }
                } else if (lookupResults.Count == 1) {
                    _manualSet = true;
                    Text = lookupResults[0].Label;
                    ObjectID = lookupResults[0].ObjectID;
                    SelectedObject = lookupResults[0];
                    _manualSet = false;
                    return true;
                }
            }
            return false;
        }

        void txt_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) {
            if (_validate) {
                if (!txt.IsReadOnly) {
                    if (EnforceLookup && !ValidateLookup()) {
                        if (!DoFind(txt.Text)) {
                            e.Handled = true;
                        }
                    }
                }
            }
        }

        private bool ValidateLookup() {

            if (string.IsNullOrWhiteSpace(txt.Text)) {
                ObjectID = 0;
                return true;
            }

            if (LookupType == LookupType.Contact && ObjectID.HasValue && ObjectID.Value > 0) {
                var loanService = new LoanService(User);
                var contact = loanService.GetContact(ObjectID.Value);
                if (contact != null) {
                    if (LoanService.FormatName(contact).Equals(txt.Text, StringComparison.CurrentCultureIgnoreCase)) {
                        return true;
                    }
                }
            }

            var service = new SupportService(User);
            var lookupResults = service.LookupSearch(txt.Text, LookupType);
            if (lookupResults != null && lookupResults.Count >= 1) {
                foreach (LookupResult result in lookupResults) {
                    if (result.Label.Equals(txt.Text, StringComparison.CurrentCultureIgnoreCase) && this.ObjectID == result.ObjectID) {
                        return true;
                    }
                }
            }

            return false;
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

        public void BindUser(User user, LookupType lookupType, LookupOptions options = LookupOptions.None) {
            User = user;
            LookupType = lookupType;
            LookupOptions = options;
            btnEdit.IsEnabled = true;
            switch (LookupType) {
                case LookupType.Material:
                    btnEdit.ToolTip = "Edit Material details...";
                    btnLookup.ToolTip = "Select Material from the Site explorer...";
                    break;
                case LookupType.SiteVisit:
                    btnEdit.ToolTip = "Edit Site Visit details...";
                    btnLookup.ToolTip = "Select a Site Visit from the Site explorer...";
                    break;
                case LookupType.Site:
                    btnEdit.ToolTip = "Edit Site details...";
                    btnLookup.ToolTip = "Select a Site from the Site explorer...";
                    break;
                case LookupType.Trap:
                    btnEdit.ToolTip = "Edit Trap details...";
                    btnLookup.ToolTip = "Select a Trap from the Site explorer...";
                    break;
                case LookupType.Region:
                    btnEdit.ToolTip = "";
                    btnEdit.IsEnabled = false;
                    btnLookup.ToolTip = "Select a Region from the Site explorer...";
                    break;
                //case LookupType.SiteGroup:
                //    btnEdit.ToolTip = "";
                //    btnEdit.IsEnabled = false;
                //    btnLookup.ToolTip = "Select a Site Group from the Material explorer...";
                //    break;
                case LookupType.Contact:
                    btnEdit.ToolTip = "Edit Contact details...";
                    btnLookup.ToolTip = "Select a Contact from the Contact explorer...";
                    break;
                case LookupType.Taxon:
                    btnEdit.ToolTip = "Edit taxon details...";
                    btnLookup.ToolTip = "Select a Taxon from the Taxon explorer...";
                    break;
                case LookupType.Reference:
                    btnEdit.ToolTip = "Edit Reference details...";
                    btnLookup.ToolTip = "Select a Reference from the Reference Manager...";
                    break;
                case LookupType.Journal:
                    btnEdit.ToolTip = "Edit Journal details...";
                    btnLookup.ToolTip = "Select a Journal from the Journal Manager...";
                    break;
            }
        }

        private void btnLookup_Click(object sender, RoutedEventArgs e) {
            LaunchLookup();
        }

        public LookupResult SelectedObject { get; private set; }

        private void GenericLookup<T>() {
            PluginManager.Instance.StartSelect<T>((result) => {
                _manualSet = true;
                this.Text = result.Description;
                this.ObjectID = result.ObjectID;
                if (ObjectID.HasValue) {
                    this.SelectedObject = new LookupResult { LookupObjectID = result.ObjectID.Value, Label = result.Description, LookupType = result.LookupType };
                }
                this.InvokeIfRequired(() => {
                    txt.Focus();
                });
                if (ObjectSelected != null) {
                    ObjectSelected(this, result);
                }
                _manualSet = false;
            }, LookupOptions);

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
                    GenericLookup<Site>();
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
                case LookupType.SiteOrRegion:
                    GenericLookup<SiteExplorerNode>();
                    break;
                case LookupType.Contact:
                    GenericLookup<Contact>();
                    break;
                default:
                    throw new Exception("Unhandled Lookup type: " + LookupType.ToString());
            }

        }

        private void btnEdit_Click(object sender, RoutedEventArgs e) {
            EditObject();
        }

        private void EditObject() {
            if (ObjectID.GetValueOrDefault(-1) > 0) {
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

        public static readonly DependencyProperty WatermarkTextProperty = DependencyProperty.Register("WatermarkText", typeof(string), typeof(LookupControl), new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(OnWatermarkTextChanged)));

        private static void OnWatermarkTextChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args) {
        }

        public String WatermarkText {
            get { return (string)GetValue(WatermarkTextProperty); }
            set { SetValue(WatermarkTextProperty, value); }
        }


        public static readonly DependencyProperty ObjectIDProperty = DependencyProperty.Register("ObjectID", typeof(int?), typeof(LookupControl), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(OnObjectIDChanged)));

        private static void OnObjectIDChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args) {
            var control = (LookupControl)obj;
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
                bool readOnly = (bool) args.NewValue;
                control.btnLookup.IsEnabled = !readOnly;
                control.txt.IsReadOnly = readOnly;
            }
        }

        #endregion

        #region Properties

        public User User { get; private set; }

        public LookupType LookupType { get; private set; }

        public LookupOptions LookupOptions { get; private set; }

        public bool EnforceLookup { get; set; }

        #endregion

        #region Events

        public event TextChangedHandler ValueChanged;

        public event ObjectIDChangedHandler ObjectIDChanged;

        public event ObjectSelectedHandler ObjectSelected;

        #endregion

        public void PreSelect(int? objectID, string text, Data.LookupType lookupType) {
            this.ObjectID = objectID;
            this.Text = text;
            if (objectID.HasValue) {
                this.SelectedObject = new LookupResult { Label = text, LookupObjectID = objectID.Value, LookupType = lookupType };
            } else {
                this.SelectedObject = null;
            }
        }

    }

    public delegate void ObjectIDChangedHandler(object source, int? objectID);

    public delegate void ObjectSelectedHandler(object source, SelectionResult result);

}
