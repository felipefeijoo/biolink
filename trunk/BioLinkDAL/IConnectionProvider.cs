using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;
using BioLink.Data.Model;
using BioLink.Client.Utilities;

namespace BioLink.Data {

    public interface IConnectionProvider {
        DbConnection GetConnection(ConnectionProfile profile, String username, String password);
        void StoredProcReaderFirst(User user, DbCommand command, string proc, ServiceReaderAction action, Action<String> message, params DbParameter[] @params);
        void StoredProcReaderForEach(User user, DbCommand command, string proc, ServiceReaderAction action, Action<String> message, params DbParameter[] @params);
        int StoredProcUpdate(User user, DbCommand command, string proc, params DbParameter[] @params);
        DbParameter CreateParameter(String name, Object value); 
        bool IsSysAdmin(User user);
    }

    public abstract class ConnectionProvider : IConnectionProvider {

        public ConnectionProvider(ConnectionProfile profile, String username, String password) {
            ConnectionProfile = profile;
            Username = username;
            Password = password;
        }

        protected DbConnection GetConnection() {
            return GetConnection(ConnectionProfile, Username, Password);
        }

        protected void Command(DbConnection conn, ServiceCommandAction action) {
            if (action == null) {
                return;
            }
            try {
                if (conn.State == System.Data.ConnectionState.Closed) {
                    conn.Open();
                }
                using (DbCommand cmd = conn.CreateCommand()) {
                    action(conn, cmd);
                }

            } finally {

            }
        }

        protected int ExecuteNonQuery(DbConnection conn, string sql, params DbParameter[] parameters) {
            int result = 0;
            Command(conn, (c, cmd) => {
                cmd.CommandText = sql;
                foreach (DbParameter p in parameters) {
                    cmd.Parameters.Add(p);
                }
                result = cmd.ExecuteNonQuery();
            });
            return result;
        }

        protected DbParameter _P(string name, object value) {
            return CreateParameter(name, value);
        }

        protected void SelectReader(DbConnection conn, string sql, ServiceReaderAction action, params DbParameter[] @params) {
            Logger.Debug("Executing SQLite SelectReader: {0}", sql);
            using (new CodeTimer("SQLite SelectReader")) {
                Command(conn, (c, cmd) => {
                    cmd.CommandText = sql;
                    foreach (DbParameter param in @params) {
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

        }


        public ConnectionProfile ConnectionProfile { get; private set; }

        public String Username { get; private set; }

        protected String Password { get; set; }

        public abstract DbConnection GetConnection(ConnectionProfile profile, string username, string password);

        public abstract void StoredProcReaderFirst(User user, DbCommand command, string proc, ServiceReaderAction action, Action<string> message, params DbParameter[] @params);

        public abstract void StoredProcReaderForEach(User user, DbCommand command, string proc, ServiceReaderAction action, Action<string> message, params DbParameter[] @params);

        public abstract int StoredProcUpdate(User user, DbCommand command, string proc, params DbParameter[] @params);

        public abstract DbParameter CreateParameter(String name, Object value);

        public abstract bool IsSysAdmin(User user);

    }
}
