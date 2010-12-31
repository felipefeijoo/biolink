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

namespace BioLink.Client.Material {
    /// <summary>
    /// Interaction logic for SiteRDEControl.xaml
    /// </summary>
    public partial class SiteRDEControl : UserControl {
       
        public SiteRDEControl(User user) {
            InitializeComponent();
            txtPolitical.BindUser(user, LookupType.Region);
            txtSource.BindUser(user, PickListType.Phrase, "Source", TraitCategoryType.Material);
            txtElevSource.BindUser(user, PickListType.Phrase, "Source", TraitCategoryType.Material);
            txtUnits.BindUser(user, PickListType.Phrase, "Units", TraitCategoryType.Material);
            var traits = new TraitControl(user, TraitCategoryType.Material, 0);
        }

    }
}
