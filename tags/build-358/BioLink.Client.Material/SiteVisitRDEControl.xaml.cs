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
using BioLink.Data.Model;
using BioLink.Data;


namespace BioLink.Client.Material {
    /// <summary>
    /// Interaction logic for SiteVisitRDEControl.xaml
    /// </summary>
    public partial class SiteVisitRDEControl : UserControl {

        public SiteVisitRDEControl(User user) {
            InitializeComponent();
            txtCollector.BindUser(user);
            this.User = user;
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

        public User User { get; private set; }

    }
}
