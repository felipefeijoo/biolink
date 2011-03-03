using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SQLite;
using BioLink.Data;

namespace BioLink.Client.Extensibility {

    public class ImportStagingService : SQLiteServiceBase, IDisposable {

        public ImportStagingService(string filename) : base(filename, true) { }

        public void CreateImportTable(List<string> columnNames) {


            Command((cmd) => {
                cmd.CommandText = "DROP TABLE IF EXISTS [Import];";                
                cmd.ExecuteNonQuery();
            });

            Command((cmd) => {
                cmd.CommandText = "DROP TABLE IF EXISTS [Errors];";
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

            columnsSpec.Append(", [ErrorMessage] TEXT");

            Command((cmd) => {
                cmd.CommandText = String.Format("CREATE TABLE [Errors] ({0})", columnsSpec.ToString());
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

    }
}
