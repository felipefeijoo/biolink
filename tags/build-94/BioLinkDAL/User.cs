using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using BioLink.Client.Utilities;
using BioLink.Data.Model;

namespace BioLink.Data {

    public class User {

        private String Password;
        private SqlTransaction _transaction;
        private string _username;

        public bool Authenticate(out String message) {

                SqlConnection connection = null;
                try {
                    Logger.Debug("Attemping to connect to {0} (Database {1}) with username '{2}'...", ConnectionProfile.Server, ConnectionProfile.Database, Username);
                    using (GetConnection()) {
                        message = "Ok";
                        return true;
                    }                    
                } catch (SqlException sqlex) {
                    message = sqlex.Message;
                    Logger.Warn("SQL Exception: {0}", sqlex.Message);                    
                } catch (Exception ex) {
                    message = ex.Message;
                    Logger.Error("Unexpected error connecting to database!", ex);                                        
                } finally {
                    if (connection != null) {
                        connection.Close();
                        connection.Dispose();
                    }
                }

            return false;
        }

        public void BeginTransaction() {
            if (_transaction != null) {
                throw new Exception("A pending transaction already exists!");
            }

            SqlConnection conn = GetConnection();
            _transaction = conn.BeginTransaction();
        }

        public void RollbackTransaction() {
            if (_transaction != null && _transaction.Connection != null) {
                _transaction.Rollback();
                _transaction.Dispose();
                _transaction = null;
            }
        }

        public void CommitTransaction() {
            if (_transaction != null && _transaction.Connection != null) {
                _transaction.Commit();
                _transaction.Dispose();
                _transaction = null;
            }
        }

        public SqlConnection GetConnection() {
            if (_transaction != null) {
                Logger.Debug("Retrieving existing transaction connection to {0} (Database {1}) with username '{2}'...", ConnectionProfile.Server, ConnectionProfile.Database, Username);
                return _transaction.Connection;
            } 

            Logger.Debug("Opening new connection to {0} (Database {1}) with username '{2}'...", ConnectionProfile.Server, ConnectionProfile.Database, Username);
            SqlConnection connection = new SqlConnection(this.ConnectionString);
            connection.Open();            
            return connection;
        }

        #region Properties

        public Boolean InTransaction {
            get { return _transaction != null; }
        }

        public SqlTransaction CurrentTransaction {
            get { return _transaction; }
        }

        public String Username {
            get {
                if (ConnectionProfile.IntegratedSecurity || String.IsNullOrEmpty(_username)) {
                    return Environment.UserName;
                } else {
                    return _username;
                }
            }
            private set {
                _username = value;
            }
        }

        public ConnectionProfile ConnectionProfile { get; private set; }

        public User(string username, string password, ConnectionProfile profile) {
            this.Username = username;
            this.Password = password;
            this.ConnectionProfile = profile;
        }

        private string ConnectionString {
            get {
                StringBuilder s = new StringBuilder();
                if (ConnectionProfile.IntegratedSecurity) {
                    s.Append(String.Format("Data Source={0};Initial Catalog={1};Integrated Security=SSPI;", ConnectionProfile.Server, ConnectionProfile.Database));
                } else {
                    s.Append(String.Format("Data Source={0};Initial Catalog={1};User Id={2};Password={3}", ConnectionProfile.Server, ConnectionProfile.Database, Username, Password));
                }

                return s.ToString();

            }

        }

        #endregion

    }

}
