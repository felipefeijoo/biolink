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
    /// Interaction logic for JournalManager.xaml
    /// </summary>
    public partial class JournalManager : DatabaseActionControl {

        #region designer ctor
        public JournalManager() {
            InitializeComponent();
        }
        #endregion

        public JournalManager(User user, ToolsPlugin owner) : base(user, "JournalManager") {
            InitializeComponent();
            this.Owner = owner;
        }

        public ToolsPlugin Owner { get; private set; }

        private void txtFind_TypingPaused(string text) {

        }

        private void btnAddNew_Click(object sender, RoutedEventArgs e) {

        }

        private void btnProperties_Click(object sender, RoutedEventArgs e) {

        }

        private void btnDelete_Click(object sender, RoutedEventArgs e) {

        }

    }
}
