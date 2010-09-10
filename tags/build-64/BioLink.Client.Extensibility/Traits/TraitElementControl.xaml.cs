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
using System.IO;
using BioLink.Data;
using BioLink.Client.Utilities;

namespace BioLink.Client.Extensibility {
    /// <summary>
    /// Interaction logic for TraitElementControl.xaml
    /// </summary>
    public partial class TraitElementControl : UserControl {

        #region designer constructor
        public TraitElementControl() {
            InitializeComponent();
        }
        #endregion

        public TraitElementControl(User user, TraitViewModel model) {
            this.Model = model;
            DataContext = model;
            model.Comment = RTFUtils.StripMarkup(model.Comment);
            InitializeComponent();
            txtValue.BindUser(user, PickListType.DistinctList, model.Name, TraitCategoryType.Taxon);
            if (!String.IsNullOrEmpty(model.Comment)) {
                commentLink.Inlines.Clear();
                commentLink.Inlines.Add(new Run("Edit comment"));
            }

            txtValue.ValueChanged +=new PickListControl.ValueChangedHandler((source, txt) => {
                FireTraitChanged();
            });
            
        }

        protected void FireTraitChanged() {
            if (TraitChanged != null) {
                TraitChanged(this, Model);
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e) {
            if (TraitDeleted != null) {
                TraitDeleted(this, this.Model);
            }
        }

        private void commentLink_Click(object sender, RoutedEventArgs e) {            
        }

        public TraitViewModel Model { get; private set; }

        public event TraitEventHandler TraitChanged;

        public event TraitEventHandler TraitDeleted;

        public delegate void TraitEventHandler(object source, TraitViewModel model);

    }
}
