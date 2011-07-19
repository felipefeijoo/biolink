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
using Microsoft.Win32;
using System.Collections.ObjectModel;

namespace BioLink.Client.Gazetteer {

    /// <summary>
    /// Interaction logic for GazetteerConverter.xaml
    /// </summary>
    public partial class GazetteerConverter : Window {

        private ObservableCollection<GazetteerConversionTarget> _jobList = new ObservableCollection<GazetteerConversionTarget>();
        public GazetteerConverter() {
            InitializeComponent();

            _jobList.Clear();
            lst.ItemsSource = _jobList;
        }

        private string CalculateTargetName(string filename) {            
            var f = new FileInfo(filename);
            string destName = f.Name;

            if (f.Name.Contains(".")) {
                destName = destName.Substring(0, destName.LastIndexOf('.'));
            }
            return string.Format("{0}\\{1}.gaz", f.DirectoryName, destName);                
        }

        private void btnConvert_Click(object sender, RoutedEventArgs e) {
            DoConvert();
        }

        private void DoConvert() {

            JobExecutor.QueueJob(() => {
                try {
                    this.InvokeIfRequired(() => {
                        btnCancel.IsEnabled = false;
                        btnConvert.IsEnabled = false;
                    });

                    this.InvokeIfRequired(() => {
                        totalProgressBar.Maximum = _jobList.Count;
                        totalProgressBar.Value = 0;
                    });

                    foreach (GazetteerConversionTarget job in _jobList) {
                        job.IsCurrent = true;
                        ConvertFile(job);
                        this.InvokeIfRequired(() => {
                            totalProgressBar.Value++;
                        });
                        job.IsComplete = true;
                        job.IsCurrent = false;
                    }
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

        private void ConvertFile(GazetteerConversionTarget job) {
            // First try and connect to the source file...
            try {
                job.SettingsCount = 0;
                job.DivisionCount = 0;
                job.PlaceCount = 0;
                job.Errors = 0;
                using (var con = new OleDbConnection(@"Provider=Microsoft.JET.OLEDB.4.0;data source=" + job.SourceFile + ";Jet OLEDB:Database Password=123456")) {
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
                        var service = new GazetteerService(job.TargetFile, true);

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
                                    job.SettingsCount++;
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
                                    job.DivisionCount++;
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
                            using (cmd = new OleDbCommand("SELECT tPlace, tType, tDivision, tLatitude, tLongitude, dblLatitude, dblLongitude from tblGaz", con)) {                            
                                using (OleDbDataReader reader = cmd.ExecuteReader()) {
                                    while (reader.Read()) {
                                        double lat = 0;
                                        if (!reader.IsDBNull(5)) {
                                            lat = (double)reader[5];
                                        }

                                        double lon = 0;
                                        if (!reader.IsDBNull(6)) {
                                            lon = (double)reader[6];
                                        }

                                        try {
                                            service.AddPlaceName(reader[0] as string, reader[1] as string, reader[2] as string, reader[3] as string, reader[4] as string, lat, lon);
                                        } catch (Exception) {
                                            job.Errors++;
                                        }
                                        job.PlaceCount++;
                                        progressBar.InvokeIfRequired(() => {
                                            progressBar.Value = job.PlaceCount;
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

                // MessageBox.Show( string.Format("Conversion complete with {3} errors: \n{0} Settings\n{1} Divisions\n{2} Place names.", job.SettingsCount, job.DivisionCount, job.PlaceCount, job.Errors), "Conversion complete", MessageBoxButton.OK, MessageBoxImage.Information);
            } catch (Exception ex) {
                ErrorMessage.Show(ex.ToString());
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e) {
            this.Close();
        }

        private void btnAddFiles_Click(object sender, RoutedEventArgs e) {
            AddFiles();
        }

        private void AddFiles() {
            var dlg = new OpenFileDialog();
            dlg.Multiselect = true;
            dlg.Filter = "eGaz 2.0 files (*.mdb)|*.mdb|All files (*.*)|*.*";
            if (dlg.ShowDialog().ValueOrFalse()) {
                foreach (string filename in dlg.FileNames) {
                    var job = new GazetteerConversionTarget { SourceFile = filename, TargetFile = CalculateTargetName(filename) };
                    _jobList.Add(job);
                }
            }
        }

        private void btnRemoveFile_Click(object sender, RoutedEventArgs e) {
            var selected = lst.SelectedItem as GazetteerConversionTarget;
            if (selected != null) {
                _jobList.Remove(selected);
            }
        }
    }

    public class GazetteerConversionTarget : ViewModelBase {

        private bool _isCurrent;
        private bool _isComplete;

        public string SourceFile { get; set; }
        public string TargetFile { get; set; }
        public int SettingsCount { get; set; }
        public int DivisionCount { get; set; }
        public int PlaceCount { get; set; }
        public int Errors { get; set; }

        public bool IsCurrent {
            get { return _isCurrent; }
            set { 
                SetProperty("IsCurrent", ref _isCurrent, value);
                RaisePropertyChanged("Icon");
            }
        }

        public bool IsComplete {
            get { return _isComplete; }
            set {
                SetProperty("IsComplete", ref _isComplete, value);
                RaisePropertyChanged("Icon");
            }
        }

        public override string DisplayLabel {
            get {
                if (IsComplete) {
                    return string.Format("{0} ({1} Settings {2} Divisions {3} Places {4} Errors)", SourceFile, SettingsCount, DivisionCount, PlaceCount, Errors);
                }

                return string.Format("{0}", SourceFile);             

            }
        }

        public override ImageSource Icon {
            get {
                if (IsCurrent) {
                    return ImageCache.GetImage("pack://application:,,,/BioLink.Client.Extensibility;component/images/RightArrowSmall.png");
                } else if (IsComplete) {
                    return ImageCache.GetImage("pack://application:,,,/BioLink.Client.Extensibility;component/images/Tick.png");
                }
                return  null; // ImageCache.GetImage("pack://application:,,,/BioLink.Client.Extensibility;component/images/AddNewSmall.png");
            }
            set {
                base.Icon = value;
            }
        }

        public override int? ObjectID {
            get { return null; }
        }


    }

}
