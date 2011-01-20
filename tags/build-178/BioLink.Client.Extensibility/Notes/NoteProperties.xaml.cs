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
using BioLink.Data.Model;
using BioLink.Client.Utilities;

namespace BioLink.Client.Extensibility {
    /// <summary>
    /// Interaction logic for NoteProperties.xaml
    /// </summary>
    public partial class NoteProperties : Window {

        #region Designer Constructor
        public NoteProperties() {
            InitializeComponent();
        }
        #endregion

        public NoteProperties(User user, NoteViewModel model) {
            InitializeComponent();
            this.User = user;
            this.SourceModel = model;

            txtReference.BindUser(user, LookupType.Reference);

            // Make a copy of the model so that changes are only applied when the ok button is clicked.
            var copy = new Note();
            ReflectionUtils.CopyProperties(model.Model, copy);
            Model = new NoteViewModel(copy);
            this.DataContext = Model;
        }

        #region Properties

        public User User { get; private set; }

        public NoteViewModel Model { get; private set; }

        protected NoteViewModel SourceModel { get; set; }

        #endregion

        private void btnCancel_Click(object sender, RoutedEventArgs e) {
            this.DialogResult = false;
            this.Hide();
        }

        private void btnOK_Click(object sender, RoutedEventArgs e) {
            // Copy over any changes to the source model...
            ReflectionUtils.CopyProperties(Model, SourceModel);

            this.DialogResult = true;
            this.Hide();
        }

    }
}
