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
using BioLink.Client.Utilities;
using BioLink.Client.Extensibility;
using BioLink.Data;

namespace BioLink.Client.Tools {
    /// <summary>
    /// Interaction logic for ImportMappingPage.xaml
    /// </summary>
    public partial class ImportMappingPage : WizardPage {

        public ImportMappingPage() {
            InitializeComponent();
        }

        public override string PageTitle {
            get { return "Field mapping"; }
        }
    }
}
