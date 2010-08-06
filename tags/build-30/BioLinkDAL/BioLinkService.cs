using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using System.Reflection;
using BioLink.Client.Utilities;
using System.Data;

namespace BioLink.Data {

    /// <summary>
    /// Base class for BioLink data services. It provides helper routines to ease the burden of
    /// extracting and mapping data from the BioLink database.
    /// </summary>
    public abstract class BioLinkService {

        private SqlTransaction _transaction = null;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="user">The User instance used to connect to the database</param>
        public BioLinkService(User user) {
            this.User = user;
        }

        /// <summary>
        /// Constructs a new SqlCommand instance, and invokes the supplied ServiceCommandAction delegate with it.
        /// The command instance is automatically cleaned up once the action has been executed
        /// </summary>
        /// <param name="commandfunc"></param>
        protected void Command(ServiceCommandAction action) {
            // If no action dont' bother with the connection or command
            if (action == null) {
                return;
            }

            // Get a connection
            bool cleanupConnection = false;
            SqlConnection connection = null;
            if (_transaction == null || _transaction.Connection == null) {
                connection = User.GetConnection();
                cleanupConnection = true;
            } else {
                connection = _transaction.Connection;
            }

            
            // and create a command instance
            try {
                using (SqlCommand command = connection.CreateCommand()) {
                    // invoke the action with the command
                    action(connection, command);
                }
            } finally {
                if (cleanupConnection) {
                    connection.Dispose();
                }
            }
            
        }

        private SqlConnection GetConnection() {
            if (_transaction == null || _transaction.Connection == null) {
                return User.GetConnection();
            } else {
                return _transaction.Connection;
            }
        }

        /// <summary>
        /// Convenience method for calling a stored procedure and iterating over each row retrieved. The supplied
        /// ServiceReaderAction is invoked for each row, and is expected to NOT advance the reader itself, but rather
        /// allow this method to act as a controller over the rowset
        /// </summary>
        /// <param name="proc">The name of the stored procedure</param>
        /// <param name="func">The action to be called for each row</param>
        /// <param name="params">A params array for the arguments of the stored proc</param>
        protected void StoredProcReaderForEach(string proc, ServiceReaderAction action, params SqlParameter[] @params) {
            using (new CodeTimer(String.Format("StoredProcReaderForEach '{0}'", proc))) {
                Logger.Debug("Calling stored procedure (reader): {0}", proc);
                Command((con, cmd) => {
                    cmd.CommandText = proc;
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    if (_transaction != null) {
                        cmd.Transaction = _transaction;
                    }
                    foreach (SqlParameter param in @params) {
                        cmd.Parameters.Add(param);
                    }

                    using (SqlDataReader reader = cmd.ExecuteReader()) {
                        while (reader.Read()) {
                            if (action != null) {
                                action(reader);
                            }
                        }
                    }
                });
            }
        }

        protected void StoredProcReaderFirst(string proc, ServiceReaderAction action, params SqlParameter[] @params) {
            using (new CodeTimer(String.Format("StoredProcReaderFirst '{0}'", proc))) {
                Logger.Debug("Calling stored procedure (reader): {0}", proc);
                Command((con, cmd) => {
                    cmd.CommandText = proc;
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    if (_transaction != null) {
                        cmd.Transaction = _transaction;
                    }
                    foreach (SqlParameter param in @params) {
                        cmd.Parameters.Add(param);
                    }

                    using (SqlDataReader reader = cmd.ExecuteReader()) {
                        if (reader.Read()) {
                            if (action != null) {
                                action(reader);
                            }
                        }
                    }
                });
            }
        }

        protected int StoredProcUpdate(string proc, params SqlParameter[] @params) {
            int rowsAffected = -1;
            using (new CodeTimer(String.Format("StoredProcUpdate '{0}'", proc))) {
                Logger.Debug("Calling stored procedure (update): {0}", proc);
                Command((con, cmd) => {
                    cmd.CommandText = proc;
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    if (_transaction != null) {
                        cmd.Transaction = _transaction;
                    }
                    foreach (SqlParameter param in @params) {
                        cmd.Parameters.Add(param);
                    }
                    rowsAffected = cmd.ExecuteNonQuery();                    
                });
            }
            return rowsAffected;
        }

        public void BeginTransaction() {
            if (_transaction != null) {
                throw new Exception("A pending transaction already exists!");
            }

            SqlConnection conn = User.GetConnection();
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

        protected SqlParameter _P(string name, object value, object defIfNull = null) {
            if (value == null) {
                value = defIfNull;
            }
            return new SqlParameter(name, value);
        }

        protected SqlParameter ReturnParam(string name, SqlDbType type) {
            SqlParameter param = new SqlParameter(name, type);
            param.Direction = ParameterDirection.ReturnValue;
            return param;
        }

        /// <summary>
        /// Holds user credentials, and is the conduit to gaining a Connection object
        /// </summary>
        public User User { get; private set; }

        public delegate void ServiceCommandAction(SqlConnection connection, SqlCommand command);

        public delegate void ServiceReaderAction(SqlDataReader reader);

    }

}
