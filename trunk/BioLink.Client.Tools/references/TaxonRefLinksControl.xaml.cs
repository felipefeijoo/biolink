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

namespace BioLink.Client.Tools {
    /// <summary>
    /// Interaction logic for TaxonRefLinksControl.xaml
    /// </summary>
    public partial class TaxonRefLinksControl : DatabaseActionControl {

        #region Designer ctor
        public TaxonRefLinksControl() {
            InitializeComponent();
        }
        #endregion

        public TaxonRefLinksControl(User user, int referenceID)
            : base(user, "TaxonRefLinks:" + referenceID) {
            InitializeComponent();

            this.ReferenceID = referenceID;


        }

        private void btnDelete_Click(object sender, RoutedEventArgs e) {

        }

        private void btnAdd_Click(object sender, RoutedEventArgs e) {

        }

        #region Properties

        public int ReferenceID { get; private set;  }

        #endregion

    }
}
