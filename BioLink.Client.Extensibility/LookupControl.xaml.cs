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
                default:
                    throw new Exception("Unhandled Lookup type: " + LookupType.ToString());
            }

            if (t != null) {
                PluginManager.Instance.StartSelect(t, (result) => {
                    txt.Text = result.Description;
                    this.ObjectID = result.ObjectID;
                });
            }
        }

        #region Properties

        public User User { get; private set; }

        public LookupType LookupType { get; private set; }

        public int? ObjectID { get; set; }

        public string Text { get; set; }

        #endregion

    }

    public enum LookupType {
        Taxon,
        Material,
        Site,
        SiteVisit,
        Trap,
        Reference
    }

}
