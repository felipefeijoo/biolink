using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SQLite;
using BioLink.Client.Utilities;
using System.IO;
using System.Data.Common;

namespace BioLink.Data {

    class SQLiteConnectionProvider : ConnectionProvider {

        public SQLiteConnectionProvider(Model.ConnectionProfile profile, String username, String password) : base(profile, username, password) { }

        public override System.Data.Common.DbConnection GetConnection(Model.ConnectionProfile profile, string username, string password) {

            bool isNew = false;
            if (!File.Exists(profile.Database)) {
                SQLiteConnection.CreateFile(profile.Database);                
                isNew = true;
            }
            var conn = new SQLiteConnection(String.Format("Data Source={0}", profile.Database));
            try {
                conn.Open();
            } catch (Exception ex) {
                conn.Dispose();
                conn = null;
                throw ex;
            }

            if (isNew) {
                CreateSchema(conn);
            }

            return conn;
        }

        private void CreateSchema(DbConnection conn) {
            ExecuteNonQuery(conn,   "CREATE TABLE tblMultimedia (" + 
                                    "[intMultimediaID] INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL," +
	                                "[vchrName] TEXT NOT NULL," +
                                    "[vchrNumber] TEXT NULL," +
                                    "[vchrArtist] TEXT NULL," +
                                    "[vchrDateRecorded] TEXT NULL," +
                                    "[vchrOwner] TEXT NULL," +
                                    "[vchrFileExtension] TEXT NULL," +
                                    "[intSizeInBytes] INTEGER NULL," +
                                    "[imgMultimedia] BLOB NULL," +
                                    "[txtCopyright] TEXT NULL," +
                                    "[dtDateCreated] DATETIME NULL, " +
                                    "[vchrWhoCreated] TEXT NULL,"+
                                    "[dtDateLastUpdated] DATETIME NULL,"+
                                    "[vchrWhoLastUpdated] TEXT NULL,"+
                                    "[GUID] GUID NULL)");

        }

        public override void StoredProcReaderForEach(User user, System.Data.Common.DbCommand command, string proc, ServiceReaderAction action, Action<string> message, params System.Data.Common.DbParameter[] @params) {
            
        }


        public override void StoredProcReaderFirst(User user, System.Data.Common.DbCommand command, string proc, ServiceReaderAction action, Action<string> message, params System.Data.Common.DbParameter[] @params) {
            
        }

        public override bool IsSysAdmin(User user) {
            return true;
        }


        public override int StoredProcUpdate(User user, System.Data.Common.DbCommand command, string proc, params System.Data.Common.DbParameter[] @params) {
            return 0;
        }

        public override DbParameter CreateParameter(string name, object value) {
            return new SQLiteParameter(name, value);
        }
    }

    [SQLiteFunction(Name = "BLTest", Arguments = 1, FuncType = FunctionType.Scalar)]
    public class BLTest : SQLiteFunction {
        public override object Invoke(object[] args) {
            return Convert.ToString(args[0]) + "XXX";
        }
    }
}
