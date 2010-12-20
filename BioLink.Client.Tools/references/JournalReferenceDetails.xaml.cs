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
using BioLink.Data;
using BioLink.Data.Model;

namespace BioLink.Client.Tools {
    /// <summary>
    /// Interaction logic for JournalDetails.xaml
    /// </summary>
    public partial class JournalReferenceDetails : UserControl {

        public JournalReferenceDetails() {
            InitializeComponent();
        }

        public JournalReferenceDetails(User user) {
            InitializeComponent();
            txtJournal.BindUser(user, LookupType.Journal);
        }


    }
}
