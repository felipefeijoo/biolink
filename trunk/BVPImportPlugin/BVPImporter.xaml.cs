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
using BioLink.Data.Model;

namespace BioLink.Client.BVPImport {
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class BVPImporter : UserControl {

        public BVPImporter() {
            InitializeComponent();
        }

        public BVPImporter(User user) {
            InitializeComponent();
            this.User = user;
        }

        public User User { get; private set; }


    }
}
