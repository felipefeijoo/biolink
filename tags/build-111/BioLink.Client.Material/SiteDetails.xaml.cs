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
using BioLink.Data;
using BioLink.Data.Model;
using BioLink.Client.Utilities;

namespace BioLink.Client.Material {
    /// <summary>
    /// Interaction logic for SiteDetails.xaml
    /// </summary>
    public partial class SiteDetails : DatabaseActionControl {

        #region Designer Constructor
        public SiteDetails() {
            InitializeComponent();
        }
        #endregion

        public SiteDetails(User user, int siteID) : base(user, "Site:" + siteID) {
            InitializeComponent();
            this.SiteID = siteID;

            var service = new MaterialService(user);
            var model = service.GetSite(siteID);

        }

        #region Properties

        public int SiteID { get; private set; }

        #endregion
    }
}
