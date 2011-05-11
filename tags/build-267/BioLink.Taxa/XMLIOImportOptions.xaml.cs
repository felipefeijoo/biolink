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
using System.IO;
using System.Collections.ObjectModel;

namespace BioLink.Client.Taxa {
    /// <summary>
    /// Interaction logic for XMLIOImportOptions.xaml
    /// </summary>
    public partial class XMLIOImportOptions : Window, IXMLImportProgressObserver {

        public XMLIOImportOptions() {
            InitializeComponent();
        }

        private void btnStart_Click(object sender, RoutedEventArgs e) {
            StartImport();
        }

        private void StartImport() {
            string filename = txtFilename.Text;
                
            if (string.IsNullOrWhiteSpace(filename)) {
                ErrorMessage.Show("You must supply the name of an XML file to import from");
                return;
            }

            if (!File.Exists(filename)) {
                ErrorMessage.Show("The specified file does not exist!");
                return;
            }



            JobExecutor.QueueJob(() => {
                this.InvokeIfRequired(() => {
                    btnStart.IsEnabled = false;
                    btnStop.IsEnabled = true;
                });

                try {

                    var importer = new XMLIOImporter(PluginManager.Instance.User, filename, this);
                    importer.Import();

                } finally {
                    this.InvokeIfRequired(() => {
                        btnStart.IsEnabled = true;
                        btnStop.IsEnabled = false;
                    });
                }
            });


        }

        public void ProgressMessage(string message) {
            lblMessage.InvokeIfRequired(() => {
                lblMessage.Content = message;
            });
        }

        public void ImportStarted(string message, List<XMLImportProgressItem> items) {

            lvw.InvokeIfRequired(() => {
                lvw.ItemsSource = new ObservableCollection<ProgressItemViewModel>(items.Select((m) => {
                    return new ProgressItemViewModel(m);
                }));
            });

        }

        public void ImportCompleted() {
            lblMessage.InvokeIfRequired(() => {
                lblMessage.Content = "Import complete.";
            });

        }

        public void ProgressTick(string itemName, int countCompleted) {
            var model = lvw.ItemsSource as ObservableCollection<ProgressItemViewModel>;
            if (model != null) {
                var item = model.FirstOrDefault((vm) => {
                    return vm.Name.Equals(itemName);
                });
                if (item != null) {
                    item.Completed = countCompleted;
                }
            }
        }
    }

    class ProgressItemViewModel : GenericViewModelBase<XMLImportProgressItem> {
        public ProgressItemViewModel(XMLImportProgressItem model)
            : base(model, null) {
        }

        public string Name {
            get { return Model.Name; }
            set { SetProperty(() => Model.Name, value); }
        }

        public int Total {
            get { return Model.Total; }
            set { SetProperty(() => Model.Total, value); }
        }

        public int Completed {
            get { return Model.Completed; }
            set { SetProperty(() => Model.Completed, value); }
        }

        public string PercentStr {
            get { return  string.Format("{0}%", Total == 0 ? 0 :  (int)((double)Completed / (double)Total) * 100.0); }
        }

    }
}
