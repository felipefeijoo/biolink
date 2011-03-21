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
using System.Data.OleDb;
using System.Data;
using BioLink.Client.Utilities;
using System.IO;

namespace BioLink.Client.Extensibility {
    /// <summary>
    /// Interaction logic for ExcelImporterOptionsWindow.xaml
    /// </summary>
    public partial class ExcelImporterOptionsWindow : Window {

        public ExcelImporterOptionsWindow(ExcelImporterOptions options) {
            InitializeComponent();            
            if (options != null) {
                txtFilename.Text = options.Filename;
                cmbSheet.Text = options.Worksheet;
            }
            txtFilename.TextChanged += new TextChangedHandler(txtFilename_TextChanged);
        }

        void txtFilename_TextChanged(object source, string value) {
            ListSheetNames(txtFilename.Text, true);
        }

        private void ListSheetNames(string filename, bool suppressErrorMessages) {
            if (!string.IsNullOrEmpty(filename)) {
                List<String> sheetNames = null;
                JobExecutor.QueueJob(() => {
                    sheetNames = GetExcelSheetNames(filename, true);
                    cmbSheet.InvokeIfRequired(() => {
                        cmbSheet.ItemsSource = sheetNames;
                        if (sheetNames != null && sheetNames.Count > 0) {
                            cmbSheet.Text = sheetNames[0];
                        }
                    });
                });

            } else {
                cmbSheet.ItemsSource = null;
            }
        }

        public String Filename {
            get { return txtFilename.Text; }            
        }

        public String Worksheet {
            get { return cmbSheet.Text; }
        }

        private List<string> GetExcelSheetNames(string filename, bool suppressException = false) {

            OleDbConnection objConn = null;
            DataTable dt = null;

            try {
                String connString = string.Format("Provider=Microsoft.Jet.OLEDB.4.0;Data Source={0};Extended Properties=Excel 8.0;", filename);

                objConn = new OleDbConnection(connString);
                objConn.Open();
                dt = objConn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);

                if (dt == null) {
                    return null;
                }

                var sheetNames = new List<string>();                
                foreach (DataRow row in dt.Rows) {
                    sheetNames.Add(row["TABLE_NAME"].ToString());
                }

                return sheetNames;
            } catch (Exception ex) {
                if (!suppressException) {
                    ErrorMessage.Show("An error occurred open the selected file: {0}", ex.Message);
                }
                return null;
            } finally {
                if (objConn != null) {
                    objConn.Close();
                    objConn.Dispose();
                }
                if (dt != null) {
                    dt.Dispose();
                }
            }
        }

        private void btnPreview_Click(object sender, RoutedEventArgs e) {
            Preview();
        }

        private void Preview() {

            OleDbConnection conn = null;
            DataTable dt = null;

            
            try {
                String connString = string.Format("Provider=Microsoft.Jet.OLEDB.4.0;Data Source={0};Extended Properties=Excel 8.0;", Filename);

                conn = new OleDbConnection(connString);
                conn.Open();
                OleDbCommand cmd = new OleDbCommand(string.Format("SELECT TOP 50 * FROM [{0}]", Worksheet), conn);
                OleDbDataAdapter dbAdapter = new OleDbDataAdapter();

                dbAdapter.SelectCommand = cmd;

                DataSet ds = new DataSet();
                dbAdapter.Fill(ds);
                if (ds.Tables.Count == 0) {
                    return;
                }
                dt = ds.Tables[0];
                conn.Close(); 

                if (dt == null) {
                    return;
                }

                previewGrid.ItemsSource = dt.DefaultView;
                
            } catch (Exception ex) {                
                ErrorMessage.Show("An error occurred open the selected file: {0}", ex.Message);
            } finally {
                if (conn != null) {
                    conn.Close();
                    conn.Dispose();
                }
                if (dt != null) {
                    dt.Dispose();
                }
            }

        }

        private void button2_Click(object sender, RoutedEventArgs e) {
            ListSheetNames(txtFilename.Text, false);
        }

        private bool Validate() {

            if (string.IsNullOrWhiteSpace(Filename)) {
                ErrorMessage.Show("You must supply a filename!");
                return false;
            }

            if (string.IsNullOrWhiteSpace(Worksheet)) {
                ErrorMessage.Show("You must supply a worksheet name!");
                return false;
            }

            if (!File.Exists(Filename)) {
                ErrorMessage.Show("File does not exist!");
                return false;
            }

            return true;
        }

        private void button4_Click(object sender, RoutedEventArgs e) {
            if (Validate()) {
                this.DialogResult = true;
                this.Close();
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e) {
            this.DialogResult = false;
            Close();
        }

        private void button1_Click(object sender, RoutedEventArgs e) {
            SystemUtils.ShellExecute(txtFilename.Text);
        }

    }
}
