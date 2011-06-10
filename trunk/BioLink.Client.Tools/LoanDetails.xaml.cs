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

namespace BioLink.Client.Tools {
    /// <summary>
    /// Interaction logic for LoanDetails.xaml
    /// </summary>
    public partial class LoanDetails : DatabaseActionControl {

        public LoanDetails(User user, ToolsPlugin plugin, int loanID) : base(user, "Loan:" + loanID) {
            InitializeComponent();
            this.Plugin = plugin;
            this.LoanID = loanID;
        }

        public int LoanID { get; private set; }
        public ToolsPlugin Plugin { get; private set; }

    }
}
