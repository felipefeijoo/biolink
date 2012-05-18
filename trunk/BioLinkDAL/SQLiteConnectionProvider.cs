using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SQLite;
using BioLink.Client.Utilities;
using System.IO;
using System.Data.Common;

namespace BioLink.Data {

    class SQLiteConnectionProvider : IConnectionProvider {

        public System.Data.Common.DbConnection GetConnection(Model.ConnectionProfile profile, string username, string password) {
            if (!File.Exists(profile.Database)) {
                SQLiteConnection.CreateFile(profile.Database);                
                IsNew = true;
            }
            var conn = new SQLiteConnection(String.Format("Data Source={0}", profile.Database));
            try {
                conn.Open();
            } catch (Exception ex) {                
                throw ex;
            }

            if (IsNew) {
                CreateSchema(conn);
            }

            return conn;
        }

        private void CreateSchema(DbConnection connection) {

        }

        public bool IsNew { get; private set; }


        public void StoredProcReaderForEach(User user, System.Data.Common.DbCommand command, string proc, ServiceReaderAction action, Action<string> message, params System.Data.Common.DbParameter[] @params) {
            
        }


        public void StoredProcReaderFirst(User user, System.Data.Common.DbCommand command, string proc, ServiceReaderAction action, Action<string> message, params System.Data.Common.DbParameter[] @params) {
            
        }

        public bool IsSysAdmin(User user) {
            return true;
        }


        public int StoredProcUpdate(User user, System.Data.Common.DbCommand command, string proc, params System.Data.Common.DbParameter[] @params) {
            return 0;
        }
    }
}
