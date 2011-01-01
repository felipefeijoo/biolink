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
    /// Interaction logic for SiteVisitDetails.xaml
    /// </summary>
    public partial class SiteVisitDetails : DatabaseActionControl {

        #region Designer Constructor
        public SiteVisitDetails() {
            InitializeComponent();
        }
        #endregion

        public SiteVisitDetails(User user, int siteVisitId) :base(user, "SiteVisit:" + siteVisitId) {
            InitializeComponent();

            var service = new MaterialService(user);
            var model = service.GetSiteVisit(siteVisitId);
            var viewModel = new SiteVisitViewModel(model);
            this.DataContext = viewModel;

            viewModel.DataChanged += new DataChangedHandler(viewModel_DataChanged);

            tab.AddTabItem("Traits", new TraitControl(user, TraitCategoryType.SiteVisit, viewModel));
            tab.AddTabItem("Notes", new NotesControl(user, TraitCategoryType.SiteVisit, viewModel));
            tab.AddTabItem("Ownership", new OwnershipDetails(model));
        }

        void viewModel_DataChanged(ChangeableModelBase viewmodel) {
            RegisterUniquePendingChange(new UpdateSiteVisitAction((viewmodel as SiteVisitViewModel).Model));
        }

        private void txtCollector_Click(object sender, RoutedEventArgs e) {
            Func<IEnumerable<string>> itemsFunc = () => {
                var service = new MaterialService(User);
                return service.GetDistinctCollectors();
            };

            PickListWindow frm = new PickListWindow(User, "Select a collector", itemsFunc, null);
            if (frm.ShowDialog().ValueOrFalse()) {
                if (String.IsNullOrWhiteSpace(txtCollector.Text)) {
                    txtCollector.Text = frm.SelectedValue as string;
                } else {
                    txtCollector.Text += ", " + frm.SelectedValue;
                }
            }
        }

    }
}
