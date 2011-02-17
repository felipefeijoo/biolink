using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Client.Utilities;
using System.Windows;
using System.Data;
using System.Data.SQLite;
using GenericParsing;
using BioLink.Data;
using System.IO;

namespace BioLink.Client.Extensibility.Import {

    public class CSVImporter : TabularDataImporter {

        private CSVImporterOptions _options;

        public override Object GetOptions(System.Windows.Window parentWindow) {
           
            var frm = new CSVImportOptionsWindow(_options);
            frm.Owner = parentWindow;
            if (frm.ShowDialog().GetValueOrDefault(false)) {
                _options = new CSVImporterOptions { Filename = frm.Filename, Delimiter = frm.Delimiter, FirstRowContainsNames = frm.IsFirstRowContainNames };
                return _options;
            }

            return null;
        }

        public override ImportRowSource CreateRowSource(Object options) {

            var csvOptions = options as CSVImporterOptions;

            if (csvOptions == null) {
                throw new Exception("Null or incorrect options type received!");
            }

            SQLiteReaderRowSource rowsource = null;

            using (var parser = new GenericParserAdapter(csvOptions.Filename)) {
                parser.ColumnDelimiter = csvOptions.Delimiter[0];
                parser.FirstRowHasHeader = csvOptions.FirstRowContainsNames;
                parser.TextQualifier = '\"';
                parser.FirstRowSetsExpectedColumnCount = true;

                var service = new ImportStagingService("c:\\zz\\tmp.sqlite");
                var columnNames = new List<String>();

                int rowCount = 0;
                service.BeginTransaction();
                var values = new List<string>();
                while (parser.Read()) {
                    if (rowCount == 0) {                        
                        for (int i = 0; i < parser.ColumnCount; ++i) {
                            if (csvOptions.FirstRowContainsNames) {
                                columnNames.Add(parser.GetColumnName(i));
                            } else {
                                columnNames.Add("Column" + i);
                            }
                        }
                        service.CreateImportTable(columnNames);
                    }

                    values.Clear();
                    for (int i = 0; i < parser.ColumnCount; ++i) {
                        values.Add(parser[i]);
                    }

                    service.InsertImportRow(values);

                    rowCount++;
                }

                service.CommitTransaction();

                var reader = service.GetImportReader();
                rowsource = new SQLiteReaderRowSource(reader, rowCount);
                
            }


            return rowsource;
            
        }

        public override string Name {
            get { return "Delimited text file"; }
        }

        public override string Description {
            get {
                return "Imports data from a flat text file delimited by specific characters (comma, tab etc.)"; 
            }
        }

        public override System.Windows.Media.Imaging.BitmapSource Icon {
            get { 
                return ImageCache.GetPackedImage("images/csv_exporter.png", GetType().Assembly.GetName().Name); 
            }
        }

    }

    public class CSVImporterOptions {
        public string Filename { get; set; }
        public string Delimiter { get; set; }
        public bool FirstRowContainsNames { get; set; }
    }

    public class SQLiteReaderRowSource : ImportRowSource {

        public SQLiteReaderRowSource(SQLiteDataReader reader, int? rowcount) {
            this.Reader = reader;
            this.RowCount = rowcount;
        }

        public bool MoveNext() {
            return Reader.Read();
        }

        public object this[int index] {
            get { return Reader[index]; }
        }

        public object this[string columnname] {
            get { return Reader[columnname]; }
        }

        public int? RowCount { get; private set; }

        public int? ColumnCount {
            get { return Reader.FieldCount; }
        }

        public string ColumnName(int index) {
            return Reader.GetName(index);
        }

        protected SQLiteDataReader Reader { get; private set; }
    }

    public class ImportStagingService : SQLiteServiceBase, IDisposable {

        public ImportStagingService(string filename) : base(filename, true) {
        }

        public void CreateImportTable(List<string> columnNames) {


            Command((cmd) => {
                cmd.CommandText = "DROP TABLE IF EXISTS [Import];";
                cmd.ExecuteNonQuery();
            });

            var columnsSpec = new StringBuilder();
            foreach (string col in columnNames) {
                columnsSpec.AppendFormat("[{0}] TEXT,", col);
            }
            columnsSpec.Remove(columnsSpec.Length - 1, 1);

            Command((cmd) => {
                cmd.CommandText = String.Format("CREATE TABLE [Import] ({0})", columnsSpec.ToString());
                cmd.ExecuteNonQuery();
            });
        }

        public void InsertImportRow(List<string> values) {

            var parmSpec = new StringBuilder();
            for (int i = 0; i < values.Count; ++i) {
                parmSpec.Append("@param" + i).Append(",");
            }
            parmSpec.Remove(parmSpec.Length - 1, 1);

            Command((cmd) => {
                cmd.CommandText = String.Format(@"INSERT INTO [Import] VALUES ({0})", parmSpec.ToString());
                for (int i = 0; i < values.Count; ++i) {                    
                    cmd.Parameters.Add(new SQLiteParameter("@param" + i, values[i]));
                }                
                cmd.ExecuteNonQuery();
            });

        }

        public SQLiteDataReader GetImportReader() {
            SQLiteDataReader reader = null;
            Command((cmd) => {
                cmd.CommandText = "SELECT * from Import";
                reader = cmd.ExecuteReader();
            });
            return reader;
        }



    }

}
