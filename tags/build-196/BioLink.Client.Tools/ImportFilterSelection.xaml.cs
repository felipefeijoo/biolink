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
    /// Interaction logic for ImportFilterSelection.xaml
    /// </summary>
    public partial class ImportFilterSelection : WizardPage {

        private List<TabularDataImporter> _model;

        public ImportFilterSelection() {
            InitializeComponent();

            _model = PluginManager.Instance.GetExtensionsOfType<TabularDataImporter>();
            listBox.ItemsSource = _model;
            listBox.SelectedIndex = 0;
        }

        public override string PageTitle {
            get { return "Select data source"; }
        }

        public override bool CanMoveNext() {
            return listBox.SelectedItem != null;
        }
        
        public override bool OnPageExit(WizardDirection todirection) {
            var selected = listBox.SelectedItem as TabularDataImporter;
            if (selected != null) {
                var options = selected.GetOptions(this.FindParentWindow());
                if (options != null) {
                    var c = WizardContext as ImportWizardContext;
                    if (c != null) {
                        c.Importer = selected;
                        c.ImporterOptions = options;
                        return true;
                    }
                }
            }
            return false;
        }

        private void listBox_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
            if (listBox.SelectedItem != null) {
                RaiseRequestNextPage();
            }
        }

    }
}
