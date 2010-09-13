using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SQLite;
using BioLink.Client.Utilities;
using System.IO;

namespace BioLink.Data {

    public class SQLiteServiceBase : IDisposable {

        public String FileName { get; set; }

        private SQLiteConnection _connection;
        private bool _persistConnection;
        private SQLiteTransaction _transaction;

        public SQLiteServiceBase(string filename, bool persistConnection = false) {
            this.FileName = filename;
            this._persistConnection = persistConnection;

            if (!File.Exists(filename)) {
                SQLiteConnection.CreateFile(filename);
            }

            if (persistConnection) {
                _connection = getConnection();
                _connection.Open();
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected SQLiteConnection getConnection() {
            if (_persistConnection && _connection != null) {
                return _connection;                
            }            
            return new SQLiteConnection(String.Format("Data Source={0}", FileName));            
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

        public void BeginTransaction() {
            if (!_persistConnection) {
                throw new Exception("Cannot begin a transaction unless the connection is persistent!");
            }

            if (_transaction != null) {
                throw new Exception("Cannot begin a transaction because there is already one outstanding!");
            }

            _transaction = _connection.BeginTransaction();
        }

        public void RollbackTransaction() {
            if (!_persistConnection) {
                throw new Exception("Cannot rollback a transaction unless the connection is persistent!");
            }

            if (_transaction == null) {
                throw new Exception("Cannot rollback because there is already no transaction outstanding!");
            }

            _transaction.Rollback();
            _transaction.Dispose();
            _transaction = null;
        }

        public void CommitTransaction() {
            if (!_persistConnection) {
                throw new Exception("Cannot commit a transaction unless the connection is persistent!");
            }

            if (_transaction == null) {
                throw new Exception("Cannot commit because there is already no transaction outstanding!");
            }

            _transaction.Commit();
            _transaction.Dispose();
            _transaction = null;

        }

        protected void Command(SqliteCommandDelegate action) {
            if (action == null) {
                return;
            }
            SQLiteConnection conn = getConnection();
            try {
                if (conn.State == System.Data.ConnectionState.Closed) {
                    conn.Open();
                }
                using (SQLiteCommand cmd = conn.CreateCommand()) {
                    action(cmd);
                }

            } catch (Exception ex) {
                GlobalExceptionHandler.Handle(ex);
            } finally {
                if (!_persistConnection && conn != null) {
                    conn.Dispose();
                }
            }
        }

        public void Dispose() {
            if (_persistConnection && _connection != null) {
                if (_transaction != null) {
                    _transaction.Rollback();
                    _transaction.Dispose();
                    _transaction = null;
                }

                _connection.Close();
                _connection.Dispose();
                _connection = null;
            }
        }
    }

    public delegate void SqliteCommandDelegate(SQLiteCommand command);

    public delegate void SqliteReaderDelegate(SQLiteDataReader reader);

}
