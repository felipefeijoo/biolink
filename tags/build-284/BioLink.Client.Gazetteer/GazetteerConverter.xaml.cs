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
using System.Data.OleDb;

namespace BioLink.Client.Gazetteer {

    /// <summary>
    /// Interaction logic for GazetteerConverter.xaml
    /// </summary>
    public partial class GazetteerConverter : Window {
        public GazetteerConverter() {
            InitializeComponent();
            txtSource.FileSelected += new Action<string>(txtSource_FileSelected);
        }

        void txtSource_FileSelected(string filename) {
            PrefillDestination(filename);
        }

        private void PrefillDestination(string filename) {
            string dest = null;
            var f = new FileInfo(filename);
            string destName = f.Name;

            if (f.Name.Contains(".")) {
                destName = destName.Substring(0, destName.LastIndexOf('.'));
            }

            if (string.IsNullOrWhiteSpace(txtDest.Text)) {                
                dest = string.Format("{0}\\{1}.gaz", f.DirectoryName, destName);                
            } else {
                var existing = new FileInfo(txtDest.Text);
                dest = string.Format("{0}\\{1}.gaz", existing.DirectoryName, destName);
            }

            if (!string.IsNullOrWhiteSpace(dest)) {
                txtDest.Text = dest;
            }

        }

        private void btnConvert_Click(object sender, RoutedEventArgs e) {
            DoConvert();
        }

        private void DoConvert() {
            if (string.IsNullOrWhiteSpace(txtSource.Text)) {
                ErrorMessage.Show("You must select a source gazetteer file to convert!");
                return;
            }

            if (string.IsNullOrWhiteSpace(txtDest.Text)) {
                ErrorMessage.Show("You must enter a destination filename!");
                return;
            }

            if (!File.Exists(txtSource.Text)) {
                ErrorMessage.Show("Source file does not exist, or is not readable!");
                return;
            }

            if (File.Exists(txtDest.Text)) {
                if (!this.Question("The destination file already exists. Do you wish to overwrite it?", "Overwrite file?")) {
                    return;
                }                   
            }

            var source = txtSource.Text;
            var dest = txtDest.Text;

            JobExecutor.QueueJob(() => {
                try {
                    this.InvokeIfRequired(() => {
                        btnCancel.IsEnabled = false;
                        btnConvert.IsEnabled = false;
                    });

                    ConvertFile(source, dest);
                } finally {
                    this.InvokeIfRequired(() => {
                        btnCancel.IsEnabled = true;
                        btnConvert.IsEnabled = true;
                    });
                }
            });
            
        }

        private void StatusMessage(string message) {
            lblProgress.InvokeIfRequired(() => {
                lblProgress.Content = message;
            });
        }

        private void ConvertFile(string sourceFile, string destFile) {
            // First try and connect to the source file...
            try {
                int settingsCount = 0;
                int divisionCount = 0;
                int placeCount = 0;
                int errors = 0;
                using (var con = new OleDbConnection(@"Provider=Microsoft.JET.OLEDB.4.0;data source=" + sourceFile + ";Jet OLEDB:Database Password=123456")) {
                    con.Open();
                    OleDbCommand cmd;
                    StatusMessage("Checking source file...");
                    using (cmd = new OleDbCommand("SELECT SettingValue from tblSettings WHERE SettingKey = 'DatabaseVersion'", con)) {
                        using (OleDbDataReader reader = cmd.ExecuteReader()) {
                            if (reader.Read()) {
                                var version = reader[0] as string;
                                if (!"2.0".Equals(version, StringComparison.CurrentCultureIgnoreCase)) {
                                    ErrorMessage.Show("Unexpected version number found in source file: " + version + ". Converstion aborted.");
                                    return;
                                }
                            } else {
                                ErrorMessage.Show("Invalid gazetteer file. Could not select from settings table. Conversion aborted.");
                            }
                        }

                        // If we get here we have a valid gaz file, so create the new file...
                        var service = new GazetteerService(destFile, true);

                        // begin a transaction for all the inserts...
                        service.BeginTransaction();

                        StatusMessage("Converting Settings...");
                        using (cmd = new OleDbCommand("SELECT SettingKey, SettingValue, LongData, UseLongData from tblSettings", con)) {
                            using (OleDbDataReader reader = cmd.ExecuteReader()) {
                                while (reader.Read()) {
                                    var key = reader[0] as string;
                                    var useLongData = (bool) reader[3];
                                    string value = null;
                                    if (!useLongData) {
                                        value = reader[1] as string;
                                    } else {
                                        value = reader[2] as string;
                                    }
                                    service.SetSetting(key, value, useLongData);
                                    settingsCount++;
                                }
                            }
                        }

                        service.SetSetting("DatabaseVersion", "3.0");

                        // Now copy over divisions...
                        StatusMessage("Converting Divisions...");
                        using (cmd = new OleDbCommand("SELECT tDatabase, tAbbreviation, tFull from tblDivisions", con)) {
                            using (OleDbDataReader reader = cmd.ExecuteReader()) {
                                while (reader.Read()) {
                                    service.AddDivision(reader[0] as string, reader[1] as string, reader[2] as string);
                                    divisionCount++;
                                }
                            }
                        }

                        // Now count the number of place names...
                        StatusMessage("Counting Place Names...");
                        int totalPlaceNames = 0;
                        using (cmd = new OleDbCommand("SELECT count(*) from tblGaz", con)) {
                            using (OleDbDataReader reader = cmd.ExecuteReader()) {
                                if (reader.Read()) {
                                    totalPlaceNames = (int)reader[0];
                                }
                            }
                        }

                        progressBar.InvokeIfRequired(() => {
                            progressBar.Value = 0;
                            progressBar.Maximum = totalPlaceNames;                            
                        });

                        if (totalPlaceNames > 0) {
                            StatusMessage("Converting Place Names...");
                            using (cmd = new OleDbCommand("SELECT tPlace, tType, tDivision, tLatitude, tLongitude, GazID, dblLatitude, dblLongitude from tblGaz", con)) {
                                using (OleDbDataReader reader = cmd.ExecuteReader()) {
                                    while (reader.Read()) {
                                        double lat = 0;
                                        if (!reader.IsDBNull(6)) {
                                            lat = (double)reader[6];
                                        }

                                        double lon = 0;
                                        if (!reader.IsDBNull(7)) {
                                            lon = (double)reader[7];
                                        }

                                        try {
                                            service.AddPlaceName(reader[0] as string, reader[1] as string, reader[2] as string, reader[3] as string, reader[4] as string, (int)reader[5], lat, lon);
                                        } catch (Exception) {
                                            errors++;
                                        }
                                        placeCount++;
                                        progressBar.InvokeIfRequired(() => {
                                            progressBar.Value = placeCount;
                                        });
                                    }
                                }
                            }
                        }

                        service.CommitTransaction();
                        StatusMessage("Complete.");
                        progressBar.InvokeIfRequired(() => {
                            progressBar.Value = 0;
                        });

                    }
                    
                }

                

                MessageBox.Show( string.Format("Conversion complete with {3} errors: \n{0} Settings\n{1} Divisions\n{2} Place names.", settingsCount, divisionCount, placeCount, errors), "Conversion complete", MessageBoxButton.OK, MessageBoxImage.Information);
            } catch (Exception ex) {
                ErrorMessage.Show(ex.ToString());
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e) {
            this.Close();
        }
    }

}
