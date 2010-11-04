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
using BioLink.Data.Model;

namespace BioLink.Client.Extensibility {
    /// <summary>
    /// Interaction logic for OwnershipDetails.xaml
    /// </summary>
    public partial class OwnershipDetails : UserControl {
        #region Designer Constructor
        public OwnershipDetails() {
            InitializeComponent();
        }
        #endregion

        public OwnershipDetails(OwnedDataObject dataObject) {
            InitializeComponent();
            DataObject = dataObject;
            this.DataContext = dataObject;
        }

        public OwnedDataObject DataObject { get; private set; }
    }
}
