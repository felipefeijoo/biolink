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
using BioLink.Client.Extensibility;
using BioLink.Client.Utilities;
using BioLink.Data.Model;
using GenericParsing;

namespace BioLink.Client.Taxa {
    /// <summary>
    /// Interaction logic for ImportDistributionRegionsControl.xaml
    /// </summary>
    public partial class ImportDistributionRegionsWindow : Window {

        public ImportDistributionRegionsWindow() {
            InitializeComponent();
        }

        public ImportDistributionRegionsWindow(User user) {
            InitializeComponent();
            this.User = user;
        }

        protected User User { get; private set; }

        private void btnCancel_Click(object sender, RoutedEventArgs e) {
            this.Hide();
        }

        private void btnOK_Click(object sender, RoutedEventArgs e) {

            this.BackgroundInvoke(() => {
                DoImport();
            });

        }

        private void DoImport() {
            if (String.IsNullOrEmpty(txtFilename.Text)) {
                ErrorMessage.Show("You must select a file before proceeding!");
                return;
            }

            int rowCount = 0;

            var service = new SupportService(User);

            using (var parser = new GenericParserAdapter(txtFilename.Text)) {
                parser.ColumnDelimiter = ',';
                parser.FirstRowHasHeader = chkFirstRowHeaders.IsChecked.GetValueOrDefault(false);
                parser.TextQualifier = '\"';
                parser.FirstRowSetsExpectedColumnCount = true;
                
                var columnNames = new List<String>();               
                var values = new List<string>();
                while (parser.Read()) {
                    if (rowCount == 0) {
                        for (int i = 0; i < parser.ColumnCount; ++i) {
                            if (parser.FirstRowHasHeader) {
                                columnNames.Add(parser.GetColumnName(i));
                            } else {
                                columnNames.Add("Column" + i);
                            }
                        }
                    }
                    values.Clear();
                    for (int i = 0; i < parser.ColumnCount; ++i) {
                        values.Add(parser[i]);
                    }

                    if (values.Count > 0) {
                        service.GetDistributionIdFromPath(values[0]);

                        lblProgress.InvokeIfRequired(() => {
                            lblProgress.Content = values[0];
                            lblProgress.UpdateLayout();
                        });

                        rowCount++;
                    }
                }
            }

            lblProgress.InvokeIfRequired(() => {
                lblProgress.Content = String.Format("{0} rows processed.", rowCount);
            });
            btnCancel.Content = "_Close";

        }

    }
}
