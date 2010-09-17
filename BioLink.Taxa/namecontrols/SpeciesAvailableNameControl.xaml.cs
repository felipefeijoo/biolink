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
    /// Interaction logic for SpeciesAvailableNameControl.xaml
    /// </summary>
    public partial class SpeciesAvailableNameControl : DatabaseActionControl {

        #region Designer Constructor
        public SpeciesAvailableNameControl() {
            InitializeComponent();
        }
        #endregion

        public SpeciesAvailableNameControl(TaxonViewModel taxon, User user)
            : base(user, "Taxon::SpeciesAvailabeNames::" + taxon.TaxaID) {
                InitializeComponent();
        }

    }
}
