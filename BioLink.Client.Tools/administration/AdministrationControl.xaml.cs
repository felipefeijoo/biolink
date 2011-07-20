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
    /// Interaction logic for AdministrationWindow.xaml
    /// </summary>
    public partial class AdministrationControl : DatabaseCommandControl {

        public AdministrationControl(User user) : base(user, "AdminControl") {
            InitializeComponent();
            this.User = user;            
            tabControl.AddTabItem("Trait types", new TraitTypesControl(user, "trait"));
            tabControl.AddTabItem("Note types", new TraitTypesControl(user, "note"));
            tabControl.AddTabItem("Ref Link types", new NameListAdminControl(user, "ref"));
            tabControl.AddTabItem("Multimedia Link types", new NameListAdminControl(user, "mm"));
        }

    }
}
