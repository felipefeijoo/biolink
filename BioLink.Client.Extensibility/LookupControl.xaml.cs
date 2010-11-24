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

        #region Designer Constructor
        public LookupControl() {
            InitializeComponent();

            this.GotFocus += new RoutedEventHandler((source, e) => {
                txt.Focus();
            });
        }
        #endregion

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
                default:
                    throw new Exception("Unhandled Lookup type: " + LookupType.ToString());
            }

            if (t != null) {
                PluginManager.Instance.StartSelect(t, (result) => {
                    this.Text = result.Description;
                    this.ObjectID = result.ObjectID;
                    this.InvokeIfRequired(() => {
                        txt.Focus();
                    });
                });
            }
        }

        #region Dependency Properties

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(LookupControl), new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(OnTextChanged)));

        private static void OnTextChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args) {
            var control = (LookupControl)obj;
            control.txt.Text = args.NewValue as String;
            control.FireValueChanged(control.txt.Text);
        }

        protected void FireValueChanged(string text) {
            if (this.ValueChanged != null) {
                ValueChanged(this, text);
            }
        }

        public String Text {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public static readonly DependencyProperty ObjectIDProperty = DependencyProperty.Register("ObjectID", typeof(int?), typeof(LookupControl), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(OnObjectIDChanged)));

        private static void OnObjectIDChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args) {
            //var control = (LookupControl) color;
            //control.txt.Text = args.NewValue as String;
            //control.FireValueChanged(control.txt.Text);
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

        #endregion

    }

    public enum LookupType {
        Taxon,
        Region,
        Material,
        Site,
        SiteVisit,
        Trap,
        Reference
    }

}
