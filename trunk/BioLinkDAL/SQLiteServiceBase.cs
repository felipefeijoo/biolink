using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SQLite;


namespace BioLink.Data {

    public class SQLiteServiceBase {

        public String FileName { get; set; }
        private bool PermanentConnection { get; set; }
        private SQLiteConnection _connection;

        public SQLiteServiceBase(string filename, bool permanentConnection) {
            this.FileName = filename;
            this.PermanentConnection = permanentConnection;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected SQLiteConnection getConnection() {
            if (PermanentConnection && _connection != null) {
                // TODO check connection state to see if we need reconnect....
                return _connection;
            }

            SQLiteConnection conn = new SQLiteConnection(String.Format("Data Source={0}", FileName));
            if (PermanentConnection) {
                _connection = conn;
            }
            return conn;
        }

        protected void SelectReader(string sql, SqliteReaderDelegate action, params SQLiteParameter[] @params) {
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

        protected void Command(SqliteCommandDelegate action) {
            if (action == null) {
                return;
            }
            using (SQLiteConnection conn = getConnection()) {
                conn.Open();
                using (SQLiteCommand cmd = conn.CreateCommand()) {
                    action(cmd);
                }
            }
        }

    }

    public delegate void SqliteCommandDelegate(SQLiteCommand command);

    public delegate void SqliteReaderDelegate(SQLiteDataReader reader);

}
