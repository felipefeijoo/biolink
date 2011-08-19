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

namespace BioLink.Client.Gazetteer {
    /// <summary>
    /// Interaction logic for GazetteerInfo.xaml
    /// </summary>
    public partial class GazetteerInfoForm : Window {

        public GazetteerInfoForm(GazetteerInfo info) {
            InitializeComponent();
            this.Info = info;

            this.DataContext = info;
        }

        protected GazetteerInfo Info { get; private set; }
    }
}
