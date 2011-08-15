using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SQLite;
using BioLink.Data;
using BioLink.Client.Utilities;
using System.Data;

namespace BioLink.Client.Extensibility {

    public class ImportStagingService : SQLiteServiceBase, IDisposable {

        public ImportStagingService() : base(TempFileManager.NewTempFilename(".sqlite"), true) { }

        public ImportStagingService(string filename) : base(filename, true) { }

        public void CreateImportTable(List<string> columnNames) {

            ExecuteNonQuery("DROP TABLE IF EXISTS [Import];");
            ExecuteNonQuery("DROP TABLE IF EXISTS [Errors];");

            var columnsSpec = new StringBuilder();
            foreach (string col in columnNames) {
                columnsSpec.AppendFormat("[{0}] TEXT,", col);
            }
            columnsSpec.Remove(columnsSpec.Length - 1, 1);

            ExecuteNonQuery(String.Format("CREATE TABLE [Import] ({0})", columnsSpec.ToString()));

            columnsSpec.Append(", [ErrorMessage] TEXT");

            ExecuteNonQuery(String.Format("CREATE TABLE [Errors] ({0})", columnsSpec.ToString()));

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
            return GetTableReader("Import");
        }

        public SQLiteDataReader GetErrorReader(bool includeRowID = false) {
            return GetTableReader("Errors", includeRowID);
        }

        protected SQLiteDataReader GetTableReader(string table, bool includeRowID = false) {

            SQLiteDataReader reader = null;
            Command((cmd) => {
                if (includeRowID) {
                    cmd.CommandText = "SELECT *, ROWID from " + table;
                } else {
                    cmd.CommandText = "SELECT * from " + table;
                }
                reader = cmd.ExecuteReader();
            });
            return reader;
        }


        public void CopyToErrorTable(ImportRowSource source, string message) {

            var parmSpec = new StringBuilder();
            for (int i = 0; i < source.ColumnCount + 1; ++i) {
                parmSpec.Append("@param" + i).Append(",");
            }

            parmSpec.Remove(parmSpec.Length - 1, 1);

            Command((cmd) => {
                cmd.CommandText = String.Format(@"INSERT INTO [Errors] VALUES ({0})", parmSpec.ToString());
                for (int i = 0; i < source.ColumnCount; ++i) {                
                    cmd.Parameters.Add(new SQLiteParameter("@param" + i, source[i]));
                }
                cmd.Parameters.Add(new SQLiteParameter("@param" + source.ColumnCount, message));
                cmd.ExecuteNonQuery();
            });
        }


        public void PurgeImportedRecords() {
            ExecuteNonQuery("DROP TABLE IF EXISTS [Import];");
        }

        public Int64 GetErrorCount() {
            return SelectScalar<Int64>("SELECT COUNT(*) from Errors");
        }

        public void SaveMappingInfo(TabularDataImporter Importer, IEnumerable<ImportFieldMapping> mappings) {
            
            ExecuteNonQuery("DROP TABLE IF EXISTS [Mappings];");

            ExecuteNonQuery("CREATE TABLE [Mappings] (SourceColumn, TargetColumn, IsFixed, DefaultValue)");
            foreach (ImportFieldMapping mapping in mappings) {
                ExecuteNonQuery("INSERT INTO [Mappings] VALUES (@source, @target, @fixed, @default)", 
                    _P("@source", mapping.SourceColumn),
                    _P("@target", mapping.TargetColumn),
                    _P("@fixed", mapping.IsFixed),
                    _P("@default", mapping.DefaultValue)
                );
            }

        }

        public void UpdateErrorRowField(int rowID, string columnName, string newValue) {
            ExecuteNonQuery("UPDATE Errors SET [" + columnName + "] = @newValue WHERE ROWID=@rowid",
                _P("@newValue", newValue),
                _P("@rowid", rowID)
            );
        }

        public List<ImportFieldMapping> GetMappings() {
            var list = new List<ImportFieldMapping>();
            SelectReader("SELECT * from Mappings", (reader) => {
                var mapping = new ImportFieldMapping { 
                    TargetColumn = (string) XIfNull(reader["TargetColumn"],""), 
                    SourceColumn = (string) XIfNull(reader["SourceColumn"], ""),
                    IsFixed = XIfNull<Int64>(reader["IsFixed"], (Int64)0) != 0,
                    DefaultValue = XIfNull(reader["DefaultValue"], "")
                };
                list.Add(mapping);
            });

            return list;
        }

        public DataSet GetErrorsDataSet() {
            var conn = getConnection();
            SQLiteCommand cmd = conn.CreateCommand();
            cmd.CommandText = "Select *, ROWID from Errors";
            var da = new SQLiteDataAdapter(cmd);
            var ds = new DataSet();
            da.Fill(ds, "Errors");
            return ds;
        }

    }
}
