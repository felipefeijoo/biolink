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
    /// Interaction logic for GenusAvailableNameControl.xaml
    /// </summary>
    public partial class GenusAvailableNameControl : DatabaseActionControl {

        #region designer constructor
        public GenusAvailableNameControl() {
            InitializeComponent();
        }
        #endregion

        public GenusAvailableNameControl(TaxonViewModel taxon, User user)
            : base(user, "Taxon::GenusAvailableNames::" + taxon.TaxaID) {
                InitializeComponent();
        }
    }
}
