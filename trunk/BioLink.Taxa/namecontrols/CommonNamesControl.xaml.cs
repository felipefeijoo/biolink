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
using BioLink.Client.Extensibility;

namespace BioLink.Client.Taxa {
    /// <summary>
    /// Interaction logic for CommonNamesControl.xaml
    /// </summary>
    public partial class CommonNamesControl : DatabaseActionControl {

        #region Designer Constructor
        public CommonNamesControl() {
            InitializeComponent();
        }
        #endregion

        public CommonNamesControl(TaxonViewModel taxon, User user) 
            : base(user, "Taxon::CommonNames::" + taxon.TaxaID) {

            InitializeComponent();
        }
    }
}
