using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SQLite;
using BioLink.Client.Utilities;

namespace BioLink.Data {

    public class SQLiteServiceBase {

        public String FileName { get; set; }

        public SQLiteServiceBase(string filename) {
            this.FileName = filename;            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected SQLiteConnection getConnection() {
            SQLiteConnection conn = new SQLiteConnection(String.Format("Data Source={0}", FileName));
            return conn;
        }

        protected void SelectReader(string sql, SqliteReaderDelegate action, params SQLiteParameter[] @params) {
            try {
                Logger.Debug("Executing SQLite SelectReader: {0}", sql);
                using (new CodeTimer("SQLite SelectReader")) {
                    Command((cmd) => {
                        cmd.CommandText = sql;
                        foreach (SQLiteParameter param in @params) {
                            cmd.Parameters.Add(param);
                        }
                        using (var reader = cmd.ExecuteReader()) {
                            while (reader.Read()) {
                                if (action != null) {
                                    action(reader);
                                }
                            }
                        }
                    });
                }
            } catch (Exception ex) {
                GlobalExceptionHandler.Handle(ex);
            }

        }

        protected void Command(SqliteCommandDelegate action) {
            if (action == null) {
                return;
            }
            try {
                using (SQLiteConnection conn = getConnection()) {
                    conn.Open();
                    using (SQLiteCommand cmd = conn.CreateCommand()) {
                        action(cmd);
                    }
                }
            } catch (Exception ex) {
                GlobalExceptionHandler.Handle(ex);
            }
        }

    }

    public delegate void SqliteCommandDelegate(SQLiteCommand command);

    public delegate void SqliteReaderDelegate(SQLiteDataReader reader);

}
